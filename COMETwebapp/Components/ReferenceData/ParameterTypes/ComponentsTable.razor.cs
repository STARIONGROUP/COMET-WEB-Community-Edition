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
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="ComponentsTable" />
    /// </summary>
    public partial class ComponentsTable : ThingOrderedItemsTable<CompoundParameterType, ParameterTypeComponent, ParameterTypeComponentRowViewModel>
    {
        /// <summary>
        /// Gets or sets the collection of <see cref="ParameterType" />s
        /// </summary>
        [Parameter]
        public IEnumerable<ParameterType> ParameterTypes { get; set; }

        /// <summary>
        /// Gets or sets the ordered list of items from the current
        /// <see cref="ThingOrderedItemsTable{T,TItem,TItemRow}.Thing" />
        /// </summary>
        public override OrderedItemList<ParameterTypeComponent> OrderedItemsList => this.Thing.Component;

        /// <summary>
        /// Gets the component dimension for the <see cref="ArrayParameterType" />
        /// </summary>
        public string Dimension { get; private set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (this.Thing is ArrayParameterType arrayParameterType)
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
            return this.Item.ParameterType is not QuantityKind quantityKind ? Enumerable.Empty<MeasurementScale>() : quantityKind.AllPossibleScale.OrderBy(x => x.Name);
        }

        /// <summary>
        /// Method invoked when creating a new parameter type component
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditParameterTypeComponent(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (ParameterTypeComponentRowViewModel)e.DataItem;
            this.ShouldCreate = e.IsNew;

            this.Item = dataItem == null
                ? new ParameterTypeComponent { Iid = Guid.NewGuid() }
                : dataItem.Thing.Clone(true);

            e.EditModel = this.Item;
        }

        /// <summary>
        /// Method executed every time the dimension field text has changed
        /// </summary>
        /// <param name="text">The new text</param>
        private void OnTextChanged(string text)
        {
            this.Dimension = text;
            var dimensions = text.Split(",").Select(int.Parse).ToList();
            var arrayParameterType = (ArrayParameterType)this.Thing;
            arrayParameterType.Component.Clear();
            var dimensionIndex = 0;

            if (arrayParameterType.Dimension.Count > dimensions.Count)
            {
                foreach (var dimension in arrayParameterType.Dimension.ToList())
                {
                    if (dimensionIndex  >= dimensions.Count)
                    {
                        arrayParameterType.Dimension.Remove(dimension);
                    }

                    dimensionIndex++;
                }
            }

            if (arrayParameterType.Dimension.Count < dimensions.Count)
            {
                dimensionIndex = 0;

                foreach (var dimension in dimensions)
                {
                    if (dimensionIndex >= arrayParameterType.Dimension.Count)
                    {
                        arrayParameterType.Dimension.Add(dimension);
                    }

                    dimensionIndex++;
                }
            }

            dimensionIndex = 0;

            foreach (var dimension in dimensions)
            {
                arrayParameterType.Dimension[dimensionIndex] = dimension;
                dimensionIndex++;
            }

            this.GenerateRows(dimensions, 0);

            // TODO: #602 (https://github.com/STARIONGROUP/COMET-WEB-Community-Edition/issues/602)
        }

        private void GenerateRows(List<int> dimensions, int currentDimension)
        {
            var arrayParameterType = (ArrayParameterType)this.Thing;

            for (var dimensionIndex = 0; dimensionIndex < dimensions[currentDimension]; dimensionIndex++)
            {
                if (currentDimension == dimensions.Count - 1)
                {
                    arrayParameterType.Component.Add(new ParameterTypeComponent());
                }
                else if(currentDimension < dimensions.Count)
                {
                    this.GenerateRows(dimensions, currentDimension + 1);
                }
            }
        }
    }
}
