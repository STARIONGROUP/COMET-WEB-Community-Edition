// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IConfirmCancelPopupViewModel.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.ViewModels.Components
{
    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model that enables the user to confirm his last choice before performing it
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
