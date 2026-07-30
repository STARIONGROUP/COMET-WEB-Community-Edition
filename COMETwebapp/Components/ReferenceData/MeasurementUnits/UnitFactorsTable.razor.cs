// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="UnitFactorsTable.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ReferenceData.MeasurementUnits
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

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
        public IEnumerable<MeasurementUnit> MeasurementUnits { get; set; } = [];

        /// <summary>
        /// Gets or sets the ordered list of items from the current Thing
        /// </summary>
        public override OrderedItemList<UnitFactor> OrderedItemsList => this.Thing.UnitFactor;

        /// <summary>
        /// Gets or sets a value indicating whether the form popup is visible for editing or creating.
        /// </summary>
        public bool IsOnEditMode { get; set; }

        /// <summary>
        /// Starts the creation flow for a new <see cref="UnitFactor" />.
        /// </summary>
        private void StartCreate()
        {
            this.ShouldCreate = true;
            this.Item = new UnitFactor { Iid = Guid.NewGuid() };
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Starts the edit flow for the specified <paramref name="row" />.
        /// </summary>
        /// <param name="row">The selected row to edit.</param>
        private void StartEdit(UnitFactorRowViewModel row)
        {
            if (row == null)
            {
                return;
            }

            this.ShouldCreate = false;
            this.Item = row.Thing.Clone(true);
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Method that is invoked when the edit/add unit factor form is being saved
        /// </summary>
        private void OnSaved()
        {
            this.OnEditItemSaving();
            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Handles cancellation of the form popup.
        /// </summary>
        private void OnCanceled()
        {
            this.IsOnEditMode = false;
        }
    }
}
