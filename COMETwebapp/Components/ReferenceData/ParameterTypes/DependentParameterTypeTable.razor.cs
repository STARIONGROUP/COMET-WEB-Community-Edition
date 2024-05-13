// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DependentParameterTypeTable.razor.cs" company="Starion Group S.A.">
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
    /// Support class for the <see cref="DependentParameterTypeTable" />
    /// </summary>
    public partial class DependentParameterTypeTable
    {
        /// <summary>
        /// The compound parameter type
        /// </summary>
        [Parameter]
        public SampledFunctionParameterType SampledFunctionParameterType { get; set; }

        /// <summary>
        /// The callback for when the parameter type has changed
        /// </summary>
        [Parameter]
        public EventCallback<SampledFunctionParameterType> SampledFunctionParameterTypeChanged { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="ParameterType" />s
        /// </summary>
        [Parameter]
        public IEnumerable<ParameterType> ParameterTypes { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if a dependent parameter type should be created
        /// </summary>
        public bool ShouldCreate { get; private set; }

        /// <summary>
        /// The dependent parameter type that will be handled for both edit and add forms
        /// </summary>
        public DependentParameterTypeAssignment DependentParameterType { get; private set; } = new();

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Gets the available scales based on the <see cref="ParameterType" /> from <see cref="DependentParameterType" />
        /// </summary>
        /// <returns></returns>
        private IEnumerable<MeasurementScale> GetAvailableScales()
        {
            return this.DependentParameterType.ParameterType is not QuantityKind quantityKind ? Enumerable.Empty<MeasurementScale>() : quantityKind.AllPossibleScale.OrderBy(x => x.Name);
        }

        /// <summary>
        /// Method that is invoked when the edit/add dependent parameter type form is being saved
        /// </summary>
        private void OnEditDependentParameterTypeSaving()
        {
            if (this.ShouldCreate)
            {
                this.SampledFunctionParameterType.DependentParameterType.Add(this.DependentParameterType);
            }
            else
            {
                var indexToUpdate = this.SampledFunctionParameterType.DependentParameterType.FindIndex(x => x.Iid == this.DependentParameterType.Iid);
                this.SampledFunctionParameterType.DependentParameterType[indexToUpdate] = this.DependentParameterType;
            }

            this.SampledFunctionParameterTypeChanged.InvokeAsync(this.SampledFunctionParameterType);
        }

        /// <summary>
        /// Moves the selected row up
        /// </summary>
        /// <param name="row">The row to be moved</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task MoveUp(DependentParameterTypeRowViewModel row)
        {
            var currentIndex = this.SampledFunctionParameterType.DependentParameterType.IndexOf(row.Thing);
            this.SampledFunctionParameterType.DependentParameterType.Move(currentIndex, currentIndex - 1);
            await this.SampledFunctionParameterTypeChanged.InvokeAsync(this.SampledFunctionParameterType);
        }

        /// <summary>
        /// Moves the selected row down
        /// </summary>
        /// <param name="row">The row to be moved</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task MoveDown(DependentParameterTypeRowViewModel row)
        {
            var currentIndex = this.SampledFunctionParameterType.DependentParameterType.IndexOf(row.Thing);
            this.SampledFunctionParameterType.DependentParameterType.Move(currentIndex, currentIndex + 1);
            await this.SampledFunctionParameterTypeChanged.InvokeAsync(this.SampledFunctionParameterType);
        }

        /// <summary>
        /// Method that is invoked when a dependent parameter type row is being removed
        /// </summary>
        private void RemoveDependentParameterType(DependentParameterTypeRowViewModel row)
        {
            this.SampledFunctionParameterType.DependentParameterType.Remove(row.Thing);
            this.SampledFunctionParameterTypeChanged.InvokeAsync(this.SampledFunctionParameterType);
        }

        /// <summary>
        /// Method invoked when creating a new dependent parameter type
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditDependentParameterType(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (DependentParameterTypeRowViewModel)e.DataItem;
            this.ShouldCreate = e.IsNew;

            this.DependentParameterType = dataItem == null
                ? new DependentParameterTypeAssignment { Iid = Guid.NewGuid() }
                : dataItem.Thing.Clone(true);

            e.EditModel = this.DependentParameterType;
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="CompoundParameterType" />
        /// </summary>
        /// <returns>A collection of <see cref="EnumerationValueDefinitionRowViewModel" />s to display</returns>
        private List<DependentParameterTypeRowViewModel> GetRows()
        {
            return this.SampledFunctionParameterType.DependentParameterType?
                .Select(x => new DependentParameterTypeRowViewModel(x))
                .OrderBy(x => x.Name)
                .ToList();
        }
    }
}
