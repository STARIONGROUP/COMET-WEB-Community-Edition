// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DependentParameterTypeTable.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.ReferenceData.ParameterTypes
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="DependentParameterTypeTable" />
    /// </summary>
    public partial class DependentParameterTypeTable : ThingOrderedItemsTable<SampledFunctionParameterType, DependentParameterTypeAssignment, DependentParameterTypeRowViewModel>
    {
        /// <summary>
        /// Gets or sets the collection of <see cref="ParameterType" />s
        /// </summary>
        [Parameter]
        public IEnumerable<ParameterType> ParameterTypes { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if the edit/creation of this component is enabled
        /// </summary>
        [Parameter]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the ordered list of items from the current <see cref="ThingOrderedItemsTable{T,TItem,TItemRow}.Thing" />
        /// </summary>
        public override OrderedItemList<DependentParameterTypeAssignment> OrderedItemsList => this.Thing.DependentParameterType;
    }
}
