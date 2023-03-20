// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleIterationApplicationBaseViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.ViewModels.Components.Shared
{
    using System.Reactive.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Utilities.DisposableObject;
    using COMETwebapp.Extensions;

    using DynamicData.Binding;

    using ReactiveUI;

    /// <summary>
    /// Base view model for any application that will need only one <see cref="Iteration" />
    /// </summary>
    public abstract class SingleIterationApplicationBaseViewModel : DisposableObject, ISingleIterationApplicationBaseViewModel
    {
        /// <summary>
        /// Backing field for <see cref="CurrentIteration" />
        /// </summary>
        private Iteration currentIteration;

        /// <summary>
        /// Backing field for <see cref="IsLoading" />
        /// </summary>
        private bool isLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleIterationApplicationBaseViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        protected SingleIterationApplicationBaseViewModel(ISessionService sessionService)
        {
            this.Disposables.Add(this.WhenAnyPropertyChanged(nameof(this.CurrentIteration))
                .SubscribeAsync(_ => this.OnIterationChanged()));

            this.Disposables.Add(CDPMessageBus.Current.Listen<DomainChangedEvent>().SubscribeAsync(_ => this.OnDomainChanged()));

            this.Disposables.Add(CDPMessageBus.Current.Listen<SessionEvent>()
                .Where(x => x.Status == SessionStatus.EndUpdate)
                .Subscribe(_ => this.OnSessionRefreshed()));

            this.SessionService = sessionService;
        }

        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        protected ISessionService SessionService { get; private set; }

        /// <summary>
        /// Gets the current <see cref="DomainOfExpertise" />
        /// </summary>
        public DomainOfExpertise CurrentDomain { get; protected set; }

        /// <summary>
        /// Value asserting that the current <see cref="ISingleIterationApplicationBaseViewModel" /> is loading
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.RaiseAndSetIfChanged(ref this.isLoading, value);
        }

        /// <summary>
        /// The current <see cref="Iteration" /> to work with
        /// </summary>
        public Iteration CurrentIteration
        {
            get => this.currentIteration;
            set => this.RaiseAndSetIfChanged(ref this.currentIteration, value);
        }

        /// <summary>
        /// Value asserting that the view model has set initial values at least once
        /// </summary>
        public bool HasSetInitialValuesOnce { get; set; }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        protected abstract Task OnSessionRefreshed();

        /// <summary>
        /// Handles the change of <see cref="DomainOfExpertise" />
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        protected virtual Task OnDomainChanged()
        {
            this.CurrentDomain = this.CurrentIteration == null ? null : this.SessionService.GetDomainOfExpertise(this.CurrentIteration);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Update this view model properties when the <see cref="Iteration" /> has changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected virtual async Task OnIterationChanged()
        {
            this.IsLoading = true;
            this.CurrentDomain = this.CurrentIteration == null ? null : this.SessionService.GetDomainOfExpertise(this.CurrentIteration);
            await Task.CompletedTask;
        }
    }
}
