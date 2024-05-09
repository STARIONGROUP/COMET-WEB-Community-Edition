// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ArrayComponentsTable.razor.cs" company="Starion Group S.A.">
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
    /// Support class for the <see cref="ArrayComponentsTable" />
    /// </summary>
    public partial class ArrayComponentsTable
    {
        /// <summary>
        /// A collection of parameter type component to display for selection
        /// </summary>
        [Parameter]
        public SortedList<long, ParameterTypeComponent> ParameterTypeComponents { get; set; }

        /// <summary>
        /// The method that is executed when the parameter type components change
        /// </summary>
        [Parameter]
        public EventCallback<SortedList<long, ParameterTypeComponent>> ParameterTypeComponentsChanged { get; set; }

        /// <summary>
        /// A collection of dimensions to display for selection
        /// </summary>
        [Parameter]
        public SortedList<long, int> Dimensions { get; set; }

        /// <summary>
        /// The method that is executed when the dimensions change
        /// </summary>
        [Parameter]
        public EventCallback<SortedList<long, int>> DimensionsChanged { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if a parameter type component should be created
        /// </summary>
        public bool ShouldCreate { get; private set; }

        /// <summary>
        /// The parameter type component that will be handled for both edit and add forms
        /// </summary>
        public ParameterTypeComponent ParameterTypeComponent { get; private set; } = new();

        /// <summary>
        /// The dimension value to be read and set the actual <see cref="Dimensions" />
        /// </summary>
        public string Dimension { get; private set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Method that is invoked when the edit/add parameter type component form is being saved
        /// </summary>
        private void OnEditEnumerationValueDefinitionSaving()
        {
            var valueDefinitionsList = this.ParameterTypeComponents;

            if (this.ShouldCreate)
            {
                var index = valueDefinitionsList.Count;
                valueDefinitionsList.Add(index + 1, this.ParameterTypeComponent);
                this.ParameterTypeComponents = valueDefinitionsList;
            }
            else
            {
                var valueToUpdate = valueDefinitionsList.First(x => x.Value.Iid == this.ParameterTypeComponent.Iid);
                valueDefinitionsList[valueToUpdate.Key] = this.ParameterTypeComponent;
            }

            this.ParameterTypeComponents = valueDefinitionsList;
            this.ParameterTypeComponentsChanged.InvokeAsync(this.ParameterTypeComponents);
        }

        /// <summary>
        /// Method that is invoked when a parameter type component row is being removed
        /// </summary>
        private void RemoveEnumerationValueDefinition(ParameterTypeComponentRowViewModel row)
        {
            var valueDefinitionsList = this.ParameterTypeComponents;
            valueDefinitionsList.RemoveAt(row.Thing.Index);

            this.ParameterTypeComponents = valueDefinitionsList;
            this.ParameterTypeComponentsChanged.InvokeAsync(this.ParameterTypeComponents);
        }

        /// <summary>
        /// Method invoked when creating a new parameter type component
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditEnumerationValueDefinition(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (ParameterTypeComponentRowViewModel)e.DataItem;
            this.ShouldCreate = e.IsNew;

            this.ParameterTypeComponent = dataItem == null
                ? new ParameterTypeComponent { Iid = Guid.NewGuid() }
                : dataItem.Thing.Clone(true);

            e.EditModel = this.ParameterTypeComponent;
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="ParameterTypeComponents" />
        /// </summary>
        /// <returns>A collection of <see cref="EnumerationValueDefinitionRowViewModel" />s to display</returns>
        private List<ParameterTypeComponentRowViewModel> GetRows()
        {
            return this.ParameterTypeComponents?.Select(x => new ParameterTypeComponentRowViewModel(x.Value)).ToList();
        }

        /// <summary>
        /// Method invoked everytime the dimension has changed
        /// </summary>
        /// <param name="newChangedDimension">The new changed dimension</param>
        private void OnDimensionChanged(string newChangedDimension)
        {
            var newDimension = newChangedDimension[1..^1];
            this.Dimension = newChangedDimension;
            var dimensions = newDimension.Split(";");
            var i = 0;

            if (this.Dimensions.Count < dimensions.Length)
            {
                for (var j = this.Dimension.Length - 1; j < dimensions.Length; j++)
                {
                    this.Dimensions.Add(j, int.Parse(dimensions[j]));
                }
            }

            foreach (var dimension in this.Dimensions.ToList().TakeWhile(_ => dimensions.Length + 1 != i))
            {
                this.Dimensions[dimension.Key] = int.Parse(dimensions[i]);
                i++;
            }

            this.InvokeAsync(this.StateHasChanged);
        }
    }
}
