// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FormButtons.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Components.Common
{
    using COMET.Web.Common.Components;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;

    /// <summary>
    /// Support class for the <see cref="FormButtons" />
    /// </summary>
    public partial class FormButtons : DisposableComponent
    {
        /// <summary>
        /// Gets or sets the condition to check if the save button is enabled
        /// </summary>
        [Parameter]
        public bool SaveButtonEnabled { get; set; }

        /// <summary>
        /// Gets or sets the callback for when the cancel button is selected
        /// </summary>
        [Parameter]
        public EventCallback<Task> OnCancel { get; set; }

        /// <summary>
        /// Gets or sets the validation messages. If not set, the <see cref="ValidationSummary" /> will be used to display the
        /// validation messages
        /// </summary>
        [Parameter]
        public IEnumerable<string> ValidationMessages { get; set; }

        /// <summary>
        /// Gets or sets the value to check if the save button should be set to loading state
        /// </summary>
        [Parameter]
        public bool IsLoading { get; set; }

        /// <summary>
        /// Gets or sets the callback for when the delete button is clicked
        /// </summary>
        [Parameter]
        public EventCallback<Task> OnDelete { get; set; }

        /// <summary>
        /// Gets or sets the value to check if the delete button should be displayed
        /// </summary>
        [Parameter]
        public bool DeleteButtonVisible { get; set; }
    }
}
