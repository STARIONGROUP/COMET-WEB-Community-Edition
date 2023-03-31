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

namespace COMET.Web.Common.ViewModels.Components
{
    using System.Reactive.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;

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
        /// <returns>A <see cref="Task" /></returns>
        protected abstract Task OnSessionRefreshed();

        /// <summary>
        /// Handles the change of <see cref="DomainOfExpertise" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
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
