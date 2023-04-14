// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelMenuViewModel.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.ViewModels.Shared.TopMenuEntry
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View model that handles the menu entry related to the opened <see cref="EngineeringModel" />
    /// </summary>
    public class ModelMenuViewModel : DisposableObject, IModelMenuViewModel
    {
        /// <summary>
        /// The <see cref="EventCallback{TValue}" /> for closing an <see cref="Iteration" />
        /// </summary>
        private readonly EventCallback<Iteration> closeIterationCallback;

        /// <summary>
        /// The <see cref="EventCallback{TValue}" /> to switch of <see cref="DomainOfExpertise" /> for an <see cref="Iteration" />
        /// </summary>
        private readonly EventCallback<Iteration> switchDomainCallback;

        /// <summary>
        /// Backing field <see cref="IsOnOpenIterationMode" />
        /// </summary>
        private bool isOnOpenIterationMode;

        /// <summary>
        /// Backing field for <see cref="IsOnSwitchDomainMode" />
        /// </summary>
        private bool isOnSwitchDomainMode;

        /// <summary>
        /// The <see cref="Iteration" /> that has been selected for closing or switching <see cref="DomainOfExpertise" />
        /// </summary>
        private Iteration selectedIteration;

        /// <summary>
        /// Initializes a new <see cref="ModelMenuViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        public ModelMenuViewModel(ISessionService sessionService)
        {
            this.SessionService = sessionService;
            this.closeIterationCallback = new EventCallbackFactory().Create<Iteration>(this, this.AskToCloseIteration);
            this.switchDomainCallback = new EventCallbackFactory().Create<Iteration>(this, this.AskToSwitchDomain);

            this.ConfirmCancelViewModel = new ConfirmCancelPopupViewModel
            {
                CancelRenderStyle = ButtonRenderStyle.Secondary,
                ConfirmRenderStyle = ButtonRenderStyle.Danger,
                HeaderText = "Close Iteration",
                OnCancel = new EventCallbackFactory().Create(this, () => { this.ConfirmCancelViewModel.IsVisible = false; }),
                OnConfirm = new EventCallbackFactory().Create(this, this.CloseIteration)
            };

            this.SwitchDomainViewModel = new SwitchDomainViewModel
            {
                OnSubmit = new EventCallbackFactory().Create<DomainOfExpertise>(this, this.SwitchDomain)
            };

            this.Disposables.Add(this.SessionService.OpenIterations.CountChanged.Subscribe(_ => { this.IsOnOpenIterationMode = false; }));
        }

        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        public ISessionService SessionService { get; }

        /// <summary>
        /// Value asserting that the user is asked to open a new <see cref="Iteration" />
        /// </summary>
        public bool IsOnOpenIterationMode
        {
            get => this.isOnOpenIterationMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnOpenIterationMode, value);
        }

        /// <summary>
        /// The <see cref="ISwitchDomainViewModel" />
        /// </summary>
        public ISwitchDomainViewModel SwitchDomainViewModel { get; }

        /// <summary>
        /// Value asserting that the user is asked to select a <see cref="DomainOfExpertise" />
        /// </summary>
        public bool IsOnSwitchDomainMode
        {
            get => this.isOnSwitchDomainMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnSwitchDomainMode, value);
        }

        /// <summary>
        /// The <see cref="IConfirmCancelPopupViewModel" /> for closing an <see cref="Iteration" />
        /// </summary>
        public IConfirmCancelPopupViewModel ConfirmCancelViewModel { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="ModelMenuRowViewModel" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> for the <see cref="ModelMenuRowViewModel" /></param>
        /// <returns>The newly created <see cref="ModelMenuRowViewModel" /></returns>
        public ModelMenuRowViewModel CreateRowViewModel(Iteration iteration)
        {
            return new ModelMenuRowViewModel(iteration, this.closeIterationCallback, this.switchDomainCallback);
        }

        /// <summary>
        /// Asks the user to open a new <see cref="Iteration" />
        /// </summary>
        public void AskToOpenIteration()
        {
            this.IsOnOpenIterationMode = true;
        }

        /// <summary>
        /// Switch the <see cref="DomainOfExpertise" /> for the currently selected <see cref="Iteration" />
        /// </summary>
        /// <param name="domainOfExpertise">The selected <see cref="DomainOfExpertise" /></param>
        private void SwitchDomain(DomainOfExpertise domainOfExpertise)
        {
            this.SessionService.SwitchDomain(this.selectedIteration, domainOfExpertise);
            this.IsOnSwitchDomainMode = false;
        }

        /// <summary>
        /// Closes the selected <see cref="Iteration" />
        /// </summary>
        private void CloseIteration()
        {
            this.SessionService.CloseIteration(this.selectedIteration);
            this.selectedIteration = null;
            this.ConfirmCancelViewModel.IsVisible = false;
        }

        /// <summary>
        /// Ask to switch the current <see cref="DomainOfExpertise" /> for an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> to switch <see cref="DomainOfExpertise" /></param>
        private void AskToSwitchDomain(Iteration iteration)
        {
            this.selectedIteration = iteration;
            this.SwitchDomainViewModel.AvailableDomains = this.SessionService.GetModelDomains((EngineeringModelSetup)iteration.IterationSetup.Container);
            this.SwitchDomainViewModel.SelectedDomainOfExpertise = this.SessionService.GetDomainOfExpertise(this.selectedIteration);
            this.IsOnSwitchDomainMode = true;
        }

        /// <summary>
        /// Ask to close an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /> to close</param>
        private void AskToCloseIteration(Iteration iteration)
        {
            this.selectedIteration = iteration;
            this.ConfirmCancelViewModel.ContentText = $"Are you sure to want to close the Iteration {iteration.GetName()} ?";
            this.ConfirmCancelViewModel.IsVisible = true;
        }
    }
}
