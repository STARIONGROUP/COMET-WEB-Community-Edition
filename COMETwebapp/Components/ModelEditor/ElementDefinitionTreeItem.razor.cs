// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTreeItem.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Components.ModelEditor
{
    using COMETwebapp.ViewModels.Components.ModelEditor.Rows;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="ElementDefinitionTreeItem" /> component
    /// </summary>
    public partial class ElementDefinitionTreeItem
    {
        /// <summary>
        /// The css class string for the item
        /// </summary>
        [Parameter]
        public string CssClass { get; set; } = string.Empty;

        /// <summary>
        /// The <see cref="ElementBaseTreeRowViewModel"/> to show in the item
        /// </summary>
        [Parameter]
        public ElementBaseTreeRowViewModel ElementBaseTreeRowViewModel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node shows the element's full name (when <see langword="true" />)
        /// or its short name (when <see langword="false" />)
        /// </summary>
        [Parameter]
        public bool ShowName { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the owning domain of expertise pill is shown on the node
        /// </summary>
        [Parameter]
        public bool ShowOwner { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the category pills are shown
        /// </summary>
        [Parameter]
        public bool ShowCategories { get; set; } = true;

        /// <summary>
        /// Handle unmatched values, like "draggable" html attribute, so no error is thrown
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> AdditionalAttributes { get; set; }
    }
}
