// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelMenuRow.razor.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.Shared.TopMenuEntry
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Shared.TopMenuEntry;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component that handle an open <see cref="Iteration" /> into the <see cref="ModelMenu" />
    /// </summary>
    public partial class ModelMenuRow
    {
        /// <summary>
        /// The <see cref="ModelMenuRowViewModel" />
        /// </summary>
        [Parameter]
        public ModelMenuRowViewModel ViewModel { get; set; }

        /// <summary>
        /// The current index of the <see cref="ModelMenuRow" />
        /// </summary>
        [Parameter]
        public int RowIndex { get; set; }

        /// <summary>
        /// The unique id of the <see cref="ModelMenuRow" />
        /// </summary>
        private string RowId => $"model-entry-row-{this.RowIndex}";

        /// <summary>
        /// The unique id of the close model row
        /// </summary>
        private string CloseModelId => $"{this.RowId}-close";

        /// <summary>
        /// The unique id of the switch domain model row
        /// </summary>
        private string SwitchModelId => $"{this.RowId}-switch";

        /// <summary>
        /// Value asserting if the current <see cref="Iteration" /> is the default one
        /// </summary>
        [Parameter]
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets the css class of the Icon
        /// </summary>
        /// <returns>The css class</returns>
        private string GetIconCssClass()
        {
            return this.IsDefault ? "icon icon-check" : string.Empty;
        }
    }
}
