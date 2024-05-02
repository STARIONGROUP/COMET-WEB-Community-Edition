﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ConfirmCancelPopup.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Components
{
    using COMET.Web.Common.ViewModels.Components;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Popup that ask the user to confirm his last choice
    /// </summary>
    public partial class ConfirmCancelPopup: DisposableComponent
    {
        /// <summary>
        /// Value that indicates if buttons are enabled or not
        /// </summary>
        private bool buttonsEnabled = true;

        /// <summary>
        /// The <see cref="IConfirmCancelPopupViewModel" />
        /// </summary>
        [Parameter]
        public IConfirmCancelPopupViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsVisible)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Performs the OnConfirm <see cref="EventHandler" /> when the Confirm button has been pressed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnConfirmClicked()
        {
            this.buttonsEnabled = false;
            await this.ViewModel.OnConfirm.InvokeAsync();
            this.buttonsEnabled = true;
        }
    }
}
