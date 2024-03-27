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
    using CDP4Common;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    ///  Support class for the <see cref="UnitFactorsTable"/>
    /// </summary>
    public partial class UnitFactorsTable
    {
        [Parameter]
        public OrderedItemList<UnitFactor> UnitFactors { get; set; }

        [Parameter]
        public EventCallback<UnitFactor> OnUnitFactorCreated { get; set; }

        [Parameter]
        public EventCallback<UnitFactor> OnUnitFactorEdited { get; set; }

        [Parameter]
        public EventCallback<UnitFactor> OnUnitFactorRemoved { get; set; }

        [Parameter]
        public IEnumerable<MeasurementUnit> MeasurementUnits { get; set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if a unit factor should be created
        /// </summary>
        public bool ShouldCreateUnitFactor { get; protected set; }

        private List<UnitFactorRowViewModel> Rows => this.UnitFactors.Select(x => new UnitFactorRowViewModel(x)).ToList();

        private UnitFactor UnitFactor { get; set; } = new();

        /// <summary>
        /// Method that is invoked when the edit/add unit factor form is being saved
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task OnEditUnitFactorSaving()
        {
            if (this.ShouldCreateUnitFactor)
            {
                await this.OnUnitFactorCreated.InvokeAsync(this.UnitFactor);
                return;
            }

            await this.OnUnitFactorEdited.InvokeAsync(this.UnitFactor);
        }

        /// <summary>
        /// Method that is invoked when a unit factor row is being removed
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task RemoveUnitFactor(UnitFactorRowViewModel row)
        {
            await this.OnUnitFactorRemoved.InvokeAsync(row.UnitFactor);
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
                this.UnitFactor = new UnitFactor();
                this.UnitFactor.ChangeKind = ChangeKind.Create;
                e.EditModel = this.UnitFactor;
                return;
            }

            e.EditModel = dataItem;
            this.UnitFactor = dataItem.UnitFactor;
        }
    }
}
