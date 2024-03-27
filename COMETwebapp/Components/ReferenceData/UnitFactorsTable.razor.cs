// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitFactorsTable.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Components.ReferenceData
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    ///  Support class for the <see cref="UnitFactorsTable"/>
    /// </summary>
    public partial class UnitFactorsTable
    {
        /// <summary>
        /// The derived unit to be handled
        /// </summary>
        [Parameter]
        public DerivedUnit DerivedUnit { get; set; }

        /// <summary>
        /// A collection of measurement units to display for selection
        /// </summary>
        [Parameter]
        public IEnumerable<MeasurementUnit> MeasurementUnits { get; set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if a unit factor should be created
        /// </summary>
        public bool ShouldCreateUnitFactor { get; private set; }

        /// <summary>
        /// The unit factor that will be handled for both edit and add forms
        /// </summary>
        private UnitFactor UnitFactor { get; set; } = new();

        /// <summary>
        /// Method that is invoked when the edit/add unit factor form is being saved
        /// </summary>
        private void OnEditUnitFactorSaving()
        {
            if (this.ShouldCreateUnitFactor)
            {
                this.DerivedUnit.UnitFactor.Add(this.UnitFactor);
                return;
            }

            var indexToUpdate = this.DerivedUnit.UnitFactor.FindIndex(x => x.Iid == this.UnitFactor.Iid);
            this.DerivedUnit.UnitFactor.SortedItems.SetValueAtIndex(indexToUpdate, this.UnitFactor);
        }

        /// <summary>
        /// Method that is invoked when a unit factor row is being removed
        /// </summary>
        private void RemoveUnitFactor(UnitFactorRowViewModel row)
        {
            this.DerivedUnit.UnitFactor.Remove(row.UnitFactor);
        }

        /// <summary>
        /// Method invoked when creating a new measurement unit
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditUnitFactor(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (UnitFactorRowViewModel)e.DataItem;
            this.ShouldCreateUnitFactor = e.IsNew;

            if (dataItem == null)
            {
                this.UnitFactor = new UnitFactor()
                {
                    Iid = Guid.NewGuid()
                };

                e.EditModel = this.UnitFactor;
                return;
            }

            e.EditModel = dataItem;
            this.UnitFactor = dataItem.UnitFactor;
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="UnitFactor"/> from <see cref="DerivedUnit"/>
        /// </summary>
        /// <returns>A collection of <see cref="UnitFactorRowViewModel"/>s to display</returns>
        private List<UnitFactorRowViewModel> GetRows()
        {
            return this.DerivedUnit.UnitFactor.Select(x => new UnitFactorRowViewModel(x)).ToList();
        }
    }
}
