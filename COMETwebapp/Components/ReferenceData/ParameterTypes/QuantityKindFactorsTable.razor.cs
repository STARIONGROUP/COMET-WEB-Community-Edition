// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="QuantityKindFactorsTable.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ReferenceData.ParameterTypes
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="QuantityKindFactorsTable" />
    /// </summary>
    public partial class QuantityKindFactorsTable
    {
        /// <summary>
        /// The derived quantity kind parameter type
        /// </summary>
        [Parameter]
        public DerivedQuantityKind DerivedQuantityKindParameterType { get; set; }

        /// <summary>
        /// The callback for when the parameter type has changed
        /// </summary>
        [Parameter]
        public EventCallback<DerivedQuantityKind> DerivedQuantityKindParameterTypeChanged { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="ParameterType" />s of <see cref="QuantityKind" />
        /// </summary>
        [Parameter]
        public IEnumerable<QuantityKind> QuantityKindParameterTypes { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if a quantity kind factor should be created
        /// </summary>
        public bool ShouldCreate { get; private set; }

        /// <summary>
        /// The quantity kind factor that will be handled for both edit and add forms
        /// </summary>
        public QuantityKindFactor QuantityKindFactore { get; private set; } = new();

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Method that is invoked when the edit/add quantity kind factor form is being saved
        /// </summary>
        private void OnEditQuantityKindFactorSaving()
        {
            if (this.ShouldCreate)
            {
                this.DerivedQuantityKindParameterType.QuantityKindFactor.Add(this.QuantityKindFactore);
            }
            else
            {
                var indexToUpdate = this.DerivedQuantityKindParameterType.QuantityKindFactor.FindIndex(x => x.Iid == this.QuantityKindFactore.Iid);
                this.DerivedQuantityKindParameterType.QuantityKindFactor[indexToUpdate] = this.QuantityKindFactore;
            }

            this.DerivedQuantityKindParameterTypeChanged.InvokeAsync(this.DerivedQuantityKindParameterType);
        }

        /// <summary>
        /// Moves the selected row up
        /// </summary>
        /// <param name="row">The row to be moved</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task MoveUp(QuantityKindFactorRowViewModel row)
        {
            var currentIndex = this.DerivedQuantityKindParameterType.QuantityKindFactor.IndexOf(row.Thing);
            this.DerivedQuantityKindParameterType.QuantityKindFactor.Move(currentIndex, currentIndex - 1);
            await this.DerivedQuantityKindParameterTypeChanged.InvokeAsync(this.DerivedQuantityKindParameterType);
        }

        /// <summary>
        /// Moves the selected row down
        /// </summary>
        /// <param name="row">The row to be moved</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task MoveDown(QuantityKindFactorRowViewModel row)
        {
            var currentIndex = this.DerivedQuantityKindParameterType.QuantityKindFactor.IndexOf(row.Thing);
            this.DerivedQuantityKindParameterType.QuantityKindFactor.Move(currentIndex, currentIndex + 1);
            await this.DerivedQuantityKindParameterTypeChanged.InvokeAsync(this.DerivedQuantityKindParameterType);
        }

        /// <summary>
        /// Method that is invoked when a quantity kind factor row is being removed
        /// </summary>
        private void RemoveQuantityKindFactor(QuantityKindFactorRowViewModel row)
        {
            this.DerivedQuantityKindParameterType.QuantityKindFactor.Remove(row.Thing);
            this.DerivedQuantityKindParameterTypeChanged.InvokeAsync(this.DerivedQuantityKindParameterType);
        }

        /// <summary>
        /// Method invoked when creating a new quantity kind factor
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditQuantityKindFactor(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (QuantityKindFactorRowViewModel)e.DataItem;
            this.ShouldCreate = e.IsNew;

            this.QuantityKindFactore = dataItem == null
                ? new QuantityKindFactor { Iid = Guid.NewGuid() }
                : dataItem.Thing.Clone(true);

            e.EditModel = this.QuantityKindFactore;
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="CompoundParameterType" />
        /// </summary>
        /// <returns>A collection of <see cref="QuantityKindFactorRowViewModel" />s to display</returns>
        private List<QuantityKindFactorRowViewModel> GetRows()
        {
            return this.DerivedQuantityKindParameterType.QuantityKindFactor?
                .Select(x => new QuantityKindFactorRowViewModel(x))
                .OrderBy(x => x.Name)
                .ToList();
        }
    }
}
