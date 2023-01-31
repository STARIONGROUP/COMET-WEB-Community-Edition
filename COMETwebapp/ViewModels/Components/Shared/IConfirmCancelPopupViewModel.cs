// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IConfirmCancelPopupViewModel.cs" company="RHEA System S.A.">
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

    /// <summary>
    /// Interface definition for <see cref="ConfirmCancelPopupViewModel"/>
    /// </summary>
    public interface IConfirmCancelPopupViewModel
    {
        /// <summary>
        /// Value indicating is this popup is visible or not
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// The <see cref="EventCallback" /> to call when the user cancels the action
        /// </summary>
        EventCallback OnCancel { get; set; }

        /// <summary>
        /// The <see cref="EventCallback" /> to call when the user confirms the action
        /// </summary>
        EventCallback OnConfirm { get; set; }

        /// <summary>
        /// The <see cref="ButtonRenderStyle" /> to apply for the Cancel button
        /// </summary>
        ButtonRenderStyle CancelRenderStyle { get; set; }

        /// <summary>
        /// The <see cref="ButtonRenderStyle" /> to apply for the Confirm button
        /// </summary>
        ButtonRenderStyle ConfirmRenderStyle { get; set; }

        /// <summary>
        /// The content of the header of the popup
        /// </summary>
        string HeaderText { get; set; }

        /// <summary>
        /// The content of the body of the popup
        /// </summary>
        string ContentText { get; set; }
    }
}
