// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelMenuViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.ViewModels.Shared.TopMenuEntry
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Extensions;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.ViewModels.Components.Shared;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View model that handles the menu entry related to the opened <see cref="EngineeringModel" />
    /// </summary>
    public class ModelMenuViewModel : DisposableViewModel, IModelMenuViewModel
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
        /// The <see cref="ISessionService" />
        /// </summary>
        public ISessionService SessionService { get; }

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
        /// Switch the <see cref="DomainOfExpertise" /> for the currently selected <see cref="Iteration" />
        /// </summary>
        /// <param name="domainOfExpertise">The selected <see cref="DomainOfExpertise" /></param>
        private void SwitchDomain(DomainOfExpertise domainOfExpertise)
        {
            this.SessionService.SwitchDomain(this.selectedIteration, domainOfExpertise);
            this.IsOnSwitchDomainMode = false;
        }

        /// <summary>
        /// Closes the selected <see cref="CDP4Common.EngineeringModelData.Iteration" />
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
            this.SwitchDomainViewModel.SelectedDomainOfExpertise = this.SessionService.CurrentDomainOfExpertise;
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
