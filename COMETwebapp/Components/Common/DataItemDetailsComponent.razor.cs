// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataItemDetailsComponent.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// The <see cref="DataItemDetailsComponent" /> is used to display the details of a selected data item
    /// </summary>
    public partial class DataItemDetailsComponent
    {
        /// <summary>
        /// Value asserting that the <see cref="DataItemDetailsComponent" /> is selected or not
        /// </summary>
        [Parameter]
        public bool IsSelected { get; set; }

        /// <summary>
        /// The child content of the component
        /// </summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Gets or sets the text to be displayed in the option button
        /// </summary>
        [Parameter]
        public string ButtonDisplayText { get; set; } = "Add Item";

        /// <summary>
        /// Gets or sets the action to be executed when the option button is clicked. If not set, the button will not be displayed
        /// </summary>
        [Parameter]
        public Action OnButtonClick { get; set; }
    }
}
