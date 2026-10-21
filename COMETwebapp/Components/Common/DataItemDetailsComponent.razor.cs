// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataItemDetailsComponent.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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
    /// <remarks>
    /// This component deliberately has no read-only check of its own. <see cref="IsAllowedToCreate" /> is supplied by
    /// the hosting table from its view model, where the create permission is already refused for a read-only session,
    /// so a separate <c>IsReadOnly</c> conjunct here would only duplicate that gate
    /// </remarks>
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

        /// <summary>
        /// Gets or sets the text to be displayed when the property <see cref="IsSelected"/> is set to false
        /// </summary>
        [Parameter]
        public string NotSelectedText { get; set; } = "Select an item to view or edit, or click add to create";

        /// <summary>
        /// Gets or sets the width of the panel container
        /// </summary>
        [Parameter]
        public string Width { get; set; } = "50%";

        /// <summary>
        /// The custom css class to be used in the container component
        /// </summary>
        [Parameter]
        public string CssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the active user is allowed to create a new item in the hosted
        /// table. Defaults to false (fail-closed): every host that renders an Add button binds this from its
        /// ViewModel's own permission check, and a host that forgets to bind it should get a disabled button rather
        /// than one that silently ignores the user's actual write permission
        /// </summary>
        [Parameter]
        public bool IsAllowedToCreate { get; set; }
    }
}
