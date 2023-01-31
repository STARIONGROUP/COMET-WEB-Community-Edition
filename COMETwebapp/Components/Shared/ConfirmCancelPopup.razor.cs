// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ConfirmCancelPopup.razor.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Components.Shared
{
    using COMETwebapp.ViewModels.Components.Shared;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Popup that ask the user to confirm his last choice
    /// </summary>
    public partial class ConfirmCancelPopup
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
