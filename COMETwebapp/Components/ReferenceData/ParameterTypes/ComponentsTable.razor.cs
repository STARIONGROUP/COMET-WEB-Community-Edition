// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ComponentsTable.razor.cs" company="Starion Group S.A.">
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
    using System.Text;

    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="ComponentsTable" />
    /// </summary>
    public partial class ComponentsTable
    {
        /// <summary>
        /// The compound parameter type
        /// </summary>
        [Parameter]
        public CompoundParameterType CompoundParameterType { get; set; }

        /// <summary>
        /// The callback for when the parameter type has changed
        /// </summary>
        [Parameter]
        public EventCallback<CompoundParameterType> CompoundParameterTypeChanged { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="ParameterType" />s
        /// </summary>
        [Parameter]
        public IEnumerable<ParameterType> ParameterTypes { get; set; }

        /// <summary>
        /// Gets the component dimension for the <see cref="ArrayParameterType"/>
        /// </summary>
        public string Dimension { get; private set; }

        /// <summary>
        /// Gets or sets the condition to check if a parameter type component should be created
        /// </summary>
        public bool ShouldCreate { get; private set; }

        /// <summary>
        /// The parameter type component that will be handled for both edit and add forms
        /// </summary>
        public ParameterTypeComponent ParameterTypeComponent { get; private set; } = new();

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (this.CompoundParameterType is ArrayParameterType arrayParameterType)
            {
                this.Dimension = string.Join(",", arrayParameterType.Dimension.Select(x => x.ToString()));
            }
        }

        /// <summary>
        /// Gets the available scales based on the <see cref="ParameterType" /> from <see cref="ParameterTypeComponent" />
        /// </summary>
        /// <returns>A collection of the available scales</returns>
        private IEnumerable<MeasurementScale> GetAvailableScales()
        {
            return this.ParameterTypeComponent.ParameterType is not QuantityKind quantityKind ? Enumerable.Empty<MeasurementScale>() : quantityKind.AllPossibleScale.OrderBy(x => x.Name);
        }

        /// <summary>
        /// Method that is invoked when the edit/add parameter type component form is being saved
        /// </summary>
        private void OnEditParameterTypeComponentSaving()
        {
            if (this.ShouldCreate)
            {
                this.CompoundParameterType.Component.Add(this.ParameterTypeComponent);
            }
            else
            {
                var indexToUpdate = this.CompoundParameterType.Component.FindIndex(x => x.Iid == this.ParameterTypeComponent.Iid);
                this.CompoundParameterType.Component[indexToUpdate] = this.ParameterTypeComponent;
            }

            this.CompoundParameterTypeChanged.InvokeAsync(this.CompoundParameterType);
        }

        /// <summary>
        /// Moves the selected row up
        /// </summary>
        /// <param name="row">The row to be moved</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task MoveUp(ParameterTypeComponentRowViewModel row)
        {
            var currentIndex = this.CompoundParameterType.Component.IndexOf(row.Thing);
            this.CompoundParameterType.Component.Move(currentIndex, currentIndex - 1);
            await this.CompoundParameterTypeChanged.InvokeAsync(this.CompoundParameterType);
        }

        /// <summary>
        /// Moves the selected row down
        /// </summary>
        /// <param name="row">The row to be moved</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task MoveDown(ParameterTypeComponentRowViewModel row)
        {
            var currentIndex = this.CompoundParameterType.Component.IndexOf(row.Thing);
            this.CompoundParameterType.Component.Move(currentIndex, currentIndex + 1);
            await this.CompoundParameterTypeChanged.InvokeAsync(this.CompoundParameterType);
        }

        /// <summary>
        /// Method that is invoked when a parameter type component row is being removed
        /// </summary>
        private void RemoveParameterTypeComponent(ParameterTypeComponentRowViewModel row)
        {
            this.CompoundParameterType.Component.Remove(row.Thing);
            this.CompoundParameterTypeChanged.InvokeAsync(this.CompoundParameterType);
        }

        /// <summary>
        /// Method invoked when creating a new parameter type component
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditParameterTypeComponent(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (ParameterTypeComponentRowViewModel)e.DataItem;
            this.ShouldCreate = e.IsNew;

            this.ParameterTypeComponent = dataItem == null
                ? new ParameterTypeComponent { Iid = Guid.NewGuid() }
                : dataItem.Thing.Clone(true);

            e.EditModel = this.ParameterTypeComponent;
        }

        /// <summary>
        /// Method executed every time the dimension field text has changed
        /// </summary>
        /// <param name="text">The new text</param>
        private void OnTextChanged(string text)
        {
            this.Dimension = text;
            var dimensions = text.Split(",").Select(int.Parse).ToList();
            var i = 0;

            var arrayParameterType = (ArrayParameterType)this.CompoundParameterType;

            if (arrayParameterType.Dimension.Count > dimensions.Count)
            {
                var j = 0;

                foreach (var dimension in arrayParameterType.Dimension.ToList())
                {
                    if (j >= dimensions.Count)
                    {
                        arrayParameterType.Dimension.Remove(dimension);
                    }

                    j++;
                }
            }

            if (arrayParameterType.Dimension.Count < dimensions.Count)
            {
                var k = 0;

                foreach (var dimension in dimensions)
                {
                    if (k >= arrayParameterType.Dimension.Count)
                    {
                        arrayParameterType.Dimension.Add(dimension);
                    }

                    k++;
                }
            }

            var l = 0;

            foreach (var dimension in dimensions)
            {
                arrayParameterType.Dimension[l] = dimensions[l];

                for (var m = 1; m <= dimension; m++)
                {
                   var element = this.CompoundParameterType.Component.ElementAtOrDefault(m * dimension - 1);

                   if (element is null)
                   {
                       this.CompoundParameterType.Component.Add(new ParameterTypeComponent());
                   }
                }

                l++;
            }
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="CompoundParameterType" />
        /// </summary>
        /// <returns>A collection of <see cref="EnumerationValueDefinitionRowViewModel" />s to display</returns>
        private List<ParameterTypeComponentRowViewModel> GetRows()
        {
            return this.CompoundParameterType.Component?
                .Select(x => new ParameterTypeComponentRowViewModel(x))
                .OrderBy(x => x.Name)
                .ToList();
        }
    }
}
