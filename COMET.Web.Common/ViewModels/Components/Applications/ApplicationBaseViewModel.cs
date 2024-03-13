// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ApplicationBaseViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Miguel Serra
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

    using COMET.Web.Common.Enumerations;
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
        /// Backing field for <see cref="IsRefreshing" />
        /// </summary>
        private bool isRefreshing;

        /// <summary>
        /// Initialize a new instance of <see cref="ApplicationBaseViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        protected ApplicationBaseViewModel(ISessionService sessionService, ICDPMessageBus messageBus) : base(messageBus)
        {
            this.SessionService = sessionService;

            this.Disposables.Add(this.MessageBus.Listen<SessionStateKind>()
                .SubscribeAsync(this.HandleSessionStateKind));

            this.Disposables.Add(this.MessageBus.Listen<SessionEvent>()
                .Where(x => x.Status == SessionStatus.EndUpdate)
                .SubscribeAsync(_ => this.OnEndUpdate()));
        }

        /// <summary>
        /// Gets the assert that the current session is refreshing
        /// </summary>
        public bool IsRefreshing
        {
            get => this.isRefreshing;
            private set => this.RaiseAndSetIfChanged(ref this.isRefreshing, value);
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
        /// Gets or sets a collection of the registered view models with reusable rows
        /// </summary>
        private List<IHaveReusableRows> RegisteredViewModelsWithReusableRows { get; set; } = [];

        /// <summary>
        /// Registers a collection of view models with reusable rows
        /// </summary>
        /// <param name="viewModels">The view models that implement <see cref="IHaveReusableRows"/></param>
        protected void RegisterViewModelsWithReusableRows(IEnumerable<IHaveReusableRows> viewModels)
        {
            this.RegisteredViewModelsWithReusableRows.AddRange(viewModels.ToList());
        }

        /// <summary>
        /// Registers viewmodels with reusable rows
        /// </summary>
        /// <param name="viewModels">The view models that implement <see cref="IHaveReusableRows"/></param>
        protected void RegisterViewModelsWithReusableRows(params IHaveReusableRows[] viewModels)
        {
            this.RegisterViewModelsWithReusableRows(viewModels.ToList());
        }

        /// <summary>
        /// Method used to update the view model inner components that recycle rows
        /// </summary>
        protected void UpdateInnerComponents()
        {
            foreach (var viewModel in this.RegisteredViewModelsWithReusableRows)
            {
                viewModel.AddRows(this.AddedThings.ToList());
                viewModel.UpdateRows(this.UpdatedThings.ToList());
                viewModel.RemoveRows(this.DeletedThings.ToList());
            }
        }

        /// <summary>
        /// Handles the <see cref="SessionStatus.EndUpdate" /> message received
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected virtual Task OnEndUpdate()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected abstract Task OnSessionRefreshed();

        /// <summary>
        /// Handles the change of the <see cref="SessionStateKind" />
        /// </summary>
        /// <param name="sessionState">The new <see cref="SessionStateKind" /></param>
        /// <returns>A <see cref="Task" /></returns>
        /// <exception cref="ArgumentOutOfRangeException">If the <see cref="SessionStateKind"/> is unknowned</exception>
        private Task HandleSessionStateKind(SessionStateKind sessionState)
        {
            switch (sessionState)
            {
                case SessionStateKind.RefreshEnded:
                    this.IsRefreshing = false;
                    return this.OnSessionRefreshed();
                case SessionStateKind.Refreshing:
                    this.IsRefreshing = true;
                    break;
                case SessionStateKind.IterationClosed:
                case SessionStateKind.IterationOpened:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sessionState), $"Unknowned SessionStateKind {sessionState}");
            }

            return Task.CompletedTask;
        }
    }
}
