// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="UnitFactorsTable.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ReferenceData.MeasurementUnits
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="UnitFactorsTable" />
    /// </summary>
    public partial class UnitFactorsTable : ThingOrderedItemsTable<DerivedUnit, UnitFactor, UnitFactorRowViewModel>
    {
        /// <summary>
        /// A collection of measurement units to display for selection
        /// </summary>
        [Parameter]
        public IEnumerable<MeasurementUnit> MeasurementUnits { get; set; }

        /// <summary>
        /// Gets or sets the ordered list of items from the current <see cref="Thing" />
        /// </summary>
        public override OrderedItemList<UnitFactor> OrderedItemsList => this.Thing.UnitFactor;

        /// <summary>
        /// Method invoked when creating a new quantity kind factor
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditUnitFactor(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (UnitFactorRowViewModel)e.DataItem;
            this.ShouldCreate = e.IsNew;

            this.Item = dataItem == null
                ? new UnitFactor { Iid = Guid.NewGuid() }
                : dataItem.Thing.Clone(true);

            e.EditModel = this.Item;
        }
    }
}
