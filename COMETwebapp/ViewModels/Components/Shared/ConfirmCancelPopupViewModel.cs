// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ConfirmCancelPopupViewModel.cs" company="RHEA System S.A.">
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
    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View model that enables the user to confirm his last choice before performing it
    /// </summary>
    public class ConfirmCancelPopupViewModel : ReactiveObject, IConfirmCancelPopupViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsVisible" />
        /// </summary>
        private bool isVisible;

        /// <summary>
        /// Value indicating is this popup is visible or not
        /// </summary>
        public bool IsVisible
        {
            get => this.isVisible;
            set => this.RaiseAndSetIfChanged(ref this.isVisible, value);
        }

        /// <summary>
        /// The <see cref="EventCallback" /> to call when the user cancels the action
        /// </summary>
        public EventCallback OnCancel { get; set; }

        /// <summary>
        /// The <see cref="EventCallback" /> to call when the user confirms the action
        /// </summary>
        public EventCallback OnConfirm { get; set; }

        /// <summary>
        /// The <see cref="ButtonRenderStyle" /> to apply for the Cancel button
        /// </summary>
        public ButtonRenderStyle CancelRenderStyle { get; set; } = ButtonRenderStyle.Secondary;

        /// <summary>
        /// The <see cref="ButtonRenderStyle" /> to apply for the Confirm button
        /// </summary>
        public ButtonRenderStyle ConfirmRenderStyle { get; set; } = ButtonRenderStyle.Primary;

        /// <summary>
        /// The content of the header of the popup
        /// </summary>
        public string HeaderText { get; set; } = "Please confirm";

        /// <summary>
        /// The content of the body of the popup
        /// </summary>
        public string ContentText { get; set; }
    }
}
