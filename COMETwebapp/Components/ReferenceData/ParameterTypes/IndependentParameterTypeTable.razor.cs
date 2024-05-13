// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IndependentParameterTypeTable.razor.cs" company="Starion Group S.A.">
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
    using CDP4Common.Types;

    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="IndependentParameterTypeTable" />
    /// </summary>
    public partial class IndependentParameterTypeTable
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
        /// Gets or sets the condition to check if the edit/creation of this component is enabled
        /// </summary>
        [Parameter]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if a independent parameter type should be created
        /// </summary>
        public bool ShouldCreate { get; private set; }

        /// <summary>
        /// The independent parameter type that will be handled for both edit and add forms
        /// </summary>
        public IndependentParameterTypeRowViewModel IndependentParameterType { get; private set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Gets the available scales based on the <see cref="ParameterType" /> from <see cref="IndependentParameterType" />
        /// </summary>
        /// <returns></returns>
        private IEnumerable<MeasurementScale> GetAvailableScales()
        {
            return this.IndependentParameterType.Thing.ParameterType is not QuantityKind quantityKind ? Enumerable.Empty<MeasurementScale>() : quantityKind.AllPossibleScale.OrderBy(x => x.Name);
        }

        /// <summary>
        /// Method that is invoked when the edit/add independent parameter type form is being saved
        /// </summary>
        private void OnEditIndependentParameterTypeSaving()
        {
            if (this.ShouldCreate)
            {
                this.SampledFunctionParameterType.IndependentParameterType.Add(this.IndependentParameterType.Thing);
                this.SampledFunctionParameterType.InterpolationPeriod = new ValueArray<string>(this.SampledFunctionParameterType.InterpolationPeriod.Append(this.IndependentParameterType.InterpolationPeriod));
            }
            else
            {
                var indexToUpdate = this.SampledFunctionParameterType.IndependentParameterType.FindIndex(x => x.Iid == this.IndependentParameterType.Thing.Iid);
                this.SampledFunctionParameterType.InterpolationPeriod[indexToUpdate] = this.IndependentParameterType.InterpolationPeriod;
            }

            this.SampledFunctionParameterTypeChanged.InvokeAsync(this.SampledFunctionParameterType);
        }

        /// <summary>
        /// Moves the selected row up
        /// </summary>
        /// <param name="row">The row to be moved</param>
        /// <returns>A <see cref="Task" /></returns>    
        private async Task MoveUp(IndependentParameterTypeRowViewModel row)
        {
            var currentIndex = this.SampledFunctionParameterType.IndependentParameterType.IndexOf(row.Thing);
            this.SampledFunctionParameterType.IndependentParameterType.Move(currentIndex, currentIndex - 1);
            (this.SampledFunctionParameterType.InterpolationPeriod[currentIndex], this.SampledFunctionParameterType.InterpolationPeriod[currentIndex - 1]) = (this.SampledFunctionParameterType.InterpolationPeriod[currentIndex - 1], this.SampledFunctionParameterType.InterpolationPeriod[currentIndex]);
            await this.SampledFunctionParameterTypeChanged.InvokeAsync(this.SampledFunctionParameterType);
        }

        /// <summary>
        /// Moves the selected row down
        /// </summary>
        /// <param name="row">The row to be moved</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task MoveDown(IndependentParameterTypeRowViewModel row)
        {
            var currentIndex = this.SampledFunctionParameterType.IndependentParameterType.IndexOf(row.Thing);
            this.SampledFunctionParameterType.IndependentParameterType.Move(currentIndex, currentIndex + 1);
            (this.SampledFunctionParameterType.InterpolationPeriod[currentIndex], this.SampledFunctionParameterType.InterpolationPeriod[currentIndex + 1]) = (this.SampledFunctionParameterType.InterpolationPeriod[currentIndex + 1], this.SampledFunctionParameterType.InterpolationPeriod[currentIndex]);
            await this.SampledFunctionParameterTypeChanged.InvokeAsync(this.SampledFunctionParameterType);
        }

        /// <summary>
        /// Method that is invoked when a independent parameter type row is being removed
        /// </summary>
        private void RemoveIndependentParameterType(IndependentParameterTypeRowViewModel row)
        {
            this.SampledFunctionParameterType.IndependentParameterType.Remove(row.Thing);
            this.SampledFunctionParameterTypeChanged.InvokeAsync(this.SampledFunctionParameterType);
        }

        /// <summary>
        /// Method invoked when creating a new independent parameter type
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditIndependentParameterType(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (IndependentParameterTypeRowViewModel)e.DataItem;
            this.ShouldCreate = e.IsNew;
            this.IndependentParameterType = dataItem ?? new IndependentParameterTypeRowViewModel(new IndependentParameterTypeAssignment { Iid = Guid.NewGuid() }, string.Empty);

            e.EditModel = this.IndependentParameterType;
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="CompoundParameterType" />
        /// </summary>
        /// <returns>A collection of <see cref="EnumerationValueDefinitionRowViewModel" />s to display</returns>
        private List<IndependentParameterTypeRowViewModel> GetRows()
        {
            var degreesOfInterpolation = this.SampledFunctionParameterType.InterpolationPeriod;
            var rows = new List<IndependentParameterTypeRowViewModel>();
            var i = 0;

            foreach (var independentParameterType in this.SampledFunctionParameterType.IndependentParameterType.ToList())
            {
                var degreeOfInterpolation = degreesOfInterpolation.ElementAtOrDefault(i) ?? string.Empty;
                rows.Add(new IndependentParameterTypeRowViewModel(independentParameterType, degreeOfInterpolation));
                i++;
            }

            return [.. rows.OrderBy(x => x.Name)];
        }
    }
}
