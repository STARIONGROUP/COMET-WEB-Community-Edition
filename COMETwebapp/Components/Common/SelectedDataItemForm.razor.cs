// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectedDataItemForm.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.Common
{
    using COMET.Web.Common.Components;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;

    /// <summary>
    /// Support abstract class for the <see cref="SelectedDataItemForm"/>
    /// </summary>
    public abstract partial class SelectedDataItemForm : DisposableComponent
    {
        /// <summary>
        /// Gets or sets the value to check if the options form is visible
        /// </summary>
        [Parameter]
        public bool IsVisible { get; set; }

        /// <summary>
        /// The method that is executed when the <see cref="IsVisible"/> property changes
        /// </summary>
        [Parameter]
        public EventCallback<bool> IsVisibleChanged { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if a new option should be created
        /// </summary>
        [Parameter]
        public bool ShouldCreate { get; set; }

        /// <summary>
        /// Method that is executed after the cancel button is triggered
        /// </summary>
        [Parameter]
        public EventCallback<Task> OnCanceled { get; set; }

        /// <summary>
        /// Method that is executed after the save button is triggered
        /// </summary>
        [Parameter]
        public EventCallback<Task> OnSaved { get; set; }

        /// <summary>
        /// Method that is executed when the cancel button is clicked
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        protected virtual async Task OnCancel()
        {
            this.IsVisible = false;
            await this.IsVisibleChanged.InvokeAsync(this.IsVisible);
            await this.OnCanceled.InvokeAsync();
        }

        /// <summary>
        /// Method that is executed when there is a valid submit
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        protected virtual async Task OnValidSubmit()
        {
            await this.OnSaved.InvokeAsync();
        }

        /// <summary>
        /// Returns the condition to check if the save button should be enabled
        /// </summary>
        /// <param name="editFormContext">The <see cref="EditForm"/> context</param>
        /// <returns>The enabled value</returns>
        protected bool IsSaveButtonEnabled(EditContext editFormContext)
        {
            return editFormContext.Validate() && (editFormContext.IsModified() || !this.ShouldCreate);
        }
    }
}
