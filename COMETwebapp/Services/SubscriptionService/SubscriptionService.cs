// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SubscriptionService.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Services.SubscriptionService
{
    using System.Reactive.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Services.NotificationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;

    using COMETwebapp.Model;

    using ReactiveUI;

    /// <summary>
    /// This service tracks change one <see cref="ParameterSubscription" /> for all opened <see cref="Iteration" />
    /// </summary>
    public class SubscriptionService : DisposableObject, ISubscriptionService
    {
        /// <summary>
        /// The <see cref="INotificationService" />
        /// </summary>
        private readonly INotificationService notificationService;

        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// A  <see cref="Dictionary{TKey,TValue}" /> of <see cref="Guid" /> for <see cref="ParameterSubscriptionValueSet" /> that changed
        /// </summary>
        private readonly Dictionary<Guid, List<Guid>> subscriptionsWithUpdate = new();

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}" /> that tracks the <see cref="ParameterSubscription" /> inside an
        /// <see cref="Iteration" />
        /// </summary>
        private readonly Dictionary<Guid, List<TrackedParameterSubscription>> trackedSubscriptions = new();

        /// <summary>
        /// Backing field for the <see cref="SubscriptionUpdateCount" />
        /// </summary>
        private int subscriptionUpdateCount;

        /// <summary>
        /// Initializes a new <see cref="SubscriptionService" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="notificationService">The <see cref="INotificationService" /></param>
        /// <param name="messageBus">The <see cref="IMessageBus"/></param>
        public SubscriptionService(ISessionService sessionService, INotificationService notificationService, ICDPMessageBus messageBus)
        {
            this.sessionService = sessionService;
            this.notificationService = notificationService;
            this.Disposables.Add(this.sessionService.OpenIterations.CountChanged.Subscribe(_ => this.ComputeUpdateSinceLastTracking()));

            this.Disposables.Add(messageBus.Listen<DomainChangedEvent>().Subscribe(x =>
                {
                    this.UpdateTrackedSubscriptions(x.Iteration);
                    this.ComputeUpdateSinceLastTracking();
                }
            ));

            this.Disposables.Add(messageBus.Listen<SessionEvent>().Where(x => x.Status == SessionStatus.EndUpdate)
                .Subscribe(_ => this.ComputeUpdateSinceLastTracking()));
        }

        /// <summary>
        /// A <see cref="IReadOnlyDictionary{TKey,TValue}" /> to provide access to <see cref="ParameterSubscriptionValueSet" />
        /// that has changed
        /// </summary>
        public IReadOnlyDictionary<Guid, List<Guid>> SubscriptionsWithUpdate => this.subscriptionsWithUpdate.AsReadOnly();

        /// <summary>
        /// A <see cref="IReadOnlyDictionary{TKey,TValue}" /> to provide access to the tracked subscriptions
        /// </summary>
        public IReadOnlyDictionary<Guid, List<TrackedParameterSubscription>> TrackedSubscriptions => this.trackedSubscriptions.AsReadOnly();

        /// <summary>
        /// The current number of new <see cref="ParameterSubscription" /> updates
        /// </summary>
        public int SubscriptionUpdateCount
        {
            get => this.subscriptionUpdateCount;
            set => this.RaiseAndSetIfChanged(ref this.subscriptionUpdateCount, value);
        }

        /// <summary>
        /// Updates the tracked subscriptions for all open <see cref="Iteration" />
        /// </summary>
        public void UpdateTrackedSubscriptions()
        {
            this.RemoveClosedIterations();

            foreach (var openIteration in this.sessionService.OpenIterations.Items)
            {
                this.UpdateTrackedSubscriptions(openIteration);
            }
        }

        /// <summary>
        /// Updates the tracked subscriptions for an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        public void UpdateTrackedSubscriptions(Iteration iteration)
        {
            var domain = this.sessionService.GetDomainOfExpertise(iteration);

            this.trackedSubscriptions[iteration.Iid] = new List<TrackedParameterSubscription>(iteration.QueryOwnedParameterSubscriptions(domain)
                .Select(x => new TrackedParameterSubscription(x)));
        }

        /// <summary>
        /// Computes the number of updates of <see cref="ParameterSubscription" /> since the last tracking
        /// </summary>
        public void ComputeUpdateSinceLastTracking()
        {
            this.RemoveClosedIterations();

            var updates = new Dictionary<Guid, int>();

            foreach (var iteration in this.sessionService.OpenIterations.Items)
            {
                var domain = this.sessionService.GetDomainOfExpertise(iteration);
                updates[iteration.Iid] = this.ComputeUpdateSinceLastTracking(iteration, domain);
            }

            var newSubscriptionCount = updates.Values.Sum();
            var difference = newSubscriptionCount - this.SubscriptionUpdateCount;

            if (difference > 0)
            {
                this.notificationService.AddNotifications(difference);
            }
            else
            {
                this.notificationService.RemoveNotifications(difference);
            }

            this.SubscriptionUpdateCount = newSubscriptionCount;
        }

        /// <summary>
        /// Removes all closed <see cref="Iteration" /> from the tracked subscriptions
        /// </summary>
        private void RemoveClosedIterations()
        {
            foreach (var closedIterationId in this.trackedSubscriptions.Keys.Where(x => this.sessionService.OpenIterations.Items.All(i => i.Iid != x)).ToList())
            {
                this.trackedSubscriptions.Remove(closedIterationId);
                this.subscriptionsWithUpdate.Remove(closedIterationId);
            }
        }

        /// <summary>
        /// Compute the number of updates in an <see cref="Iteration" /> after a session refresh related to
        /// <see cref="ParameterSubscription" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <param name="domainOfExpertise">The <see cref="DomainOfExpertise" /></param>
        /// <returns>The number of new updates</returns>
        private int ComputeUpdateSinceLastTracking(Iteration iteration, DomainOfExpertise domainOfExpertise)
        {
            var newSubscriptions = iteration.QueryOwnedParameterSubscriptions(domainOfExpertise)
                .Select(x => new TrackedParameterSubscription(x)).ToList();

            if (!this.trackedSubscriptions.ContainsKey(iteration.Iid))
            {
                this.subscriptionsWithUpdate[iteration.Iid] = new List<Guid>();
                this.UpdateTrackedSubscriptions(iteration);
                return 0;
            }

            this.subscriptionsWithUpdate[iteration.Iid].Clear();

            var oldSubcriptions = this.trackedSubscriptions[iteration.Iid];

            foreach (var subscription in newSubscriptions)
            {
                var existingSubscription = oldSubcriptions.Find(x => x.ParameterSubscriptionId == subscription.ParameterSubscriptionId);
                this.subscriptionsWithUpdate[iteration.Iid].AddRange(existingSubscription?.QueryChangedValueSet(subscription) ?? subscription.CountChanges.Keys);
            }

            return this.subscriptionsWithUpdate[iteration.Iid].Count;
        }
    }
}
