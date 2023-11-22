// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ApplicationBaseViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Miguel Serra
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
    using System.Reactive.Linq;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.HaveObjectChangedTracking;

    using ReactiveUI;

    /// <summary>
    /// Base view model for any application
    /// </summary>
    public abstract class ApplicationBaseViewModel : HaveObjectChangedTracking, IApplicationBaseViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsLoading" />
        /// </summary>
        private bool isLoading;

        /// <summary>
        /// Initialize a new instance of <see cref="ApplicationBaseViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        protected ApplicationBaseViewModel(ISessionService sessionService)
        {
            this.SessionService = sessionService;

            this.Disposables.Add(CDPMessageBus.Current.Listen<SessionEvent>()
                .Where(x => x.Session == this.SessionService?.Session && x.Status == SessionStatus.EndUpdate)
                .SubscribeAsync(_ => this.OnSessionRefreshed()));
        }

        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        protected ISessionService SessionService { get; private set; }

        /// <summary>
        /// Value asserting that the view model has set initial values at least once
        /// </summary>
        public bool HasSetInitialValuesOnce { get; set; }

        /// <summary>
        /// Value asserting that the current <see cref="ISingleThingApplicationBaseViewModel{TThing}" /> is loading
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.RaiseAndSetIfChanged(ref this.isLoading, value);
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected abstract Task OnSessionRefreshed();
    }
}
