// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleIterationApplicationBaseViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.ViewModels.Components.Applications
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;

    /// <summary>
    /// Base view model for any application that will need only one <see cref="Iteration" />
    /// </summary>
    public abstract class SingleIterationApplicationBaseViewModel : SingleThingApplicationBaseViewModel<Iteration>, ISingleIterationApplicationBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleIterationApplicationBaseViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        protected SingleIterationApplicationBaseViewModel(ISessionService sessionService, ICDPMessageBus messageBus) : base(sessionService, messageBus)
        {
            this.Disposables.Add(this.MessageBus.Listen<DomainChangedEvent>().SubscribeAsync(_ => this.OnDomainChanged()));
        }

        /// <summary>
        /// Gets the current <see cref="DomainOfExpertise" />
        /// </summary>
        public DomainOfExpertise CurrentDomain { get; protected set; }

        /// <summary>
        /// Handles the change of <see cref="DomainOfExpertise" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected virtual Task OnDomainChanged()
        {
            this.CurrentDomain = this.CurrentThing == null ? null : this.SessionService.GetDomainOfExpertise(this.CurrentThing);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Update this view model properties when the <see cref="Iteration" /> has changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            this.IsLoading = true;
            this.CurrentDomain = this.CurrentThing == null ? null : this.SessionService.GetDomainOfExpertise(this.CurrentThing);
            await Task.CompletedTask;
        }

        /// <summary>
        /// The logic used to check if a change should be recorded an <see cref="ObjectChangedEvent" />
        /// </summary>
        /// <param name="objectChangedEvent">The <see cref="ObjectChangedEvent" /></param>
        /// <returns>true if the change should be recorded, false otherwise</returns>
        protected override bool ShouldRecordChange(ObjectChangedEvent objectChangedEvent)
        {
            return this.CurrentThing != null && objectChangedEvent.ChangedThing.GetContainerOfType<Iteration>()?.Iid == this.CurrentThing.Iid;
        }
    }
}
