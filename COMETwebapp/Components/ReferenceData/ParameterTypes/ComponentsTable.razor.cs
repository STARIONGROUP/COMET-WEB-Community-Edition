// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ComponentsTable.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ReferenceData.ParameterTypes
{
    using System.Text.RegularExpressions;

    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

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
        /// Gets or sets a value indicating whether the form popup is visible for editing or creating.
        /// </summary>
        public bool IsOnEditMode { get; set; }

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
        /// Starts the creation flow for a new <see cref="ParameterTypeComponent" /> item.
        /// </summary>
        public void StartCreate()
        {
            this.ShouldCreate = true;
            this.Item = new ParameterTypeComponent { Iid = Guid.NewGuid() };
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Starts the edit flow for the specified <paramref name="row" />.
        /// </summary>
        /// <param name="row">The selected row to edit.</param>
        public void StartEdit(ParameterTypeComponentRowViewModel row)
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
        /// Handles saving of the form popup, updating or adding the item to <see cref="OrderedItemsList" />.
        /// </summary>
        public void OnSaved()
        {
            if (this.ShouldCreate)
            {
                this.OrderedItemsList.Add(this.Item);
            }
            else
            {
                var indexToUpdate = this.OrderedItemsList.FindIndex(x => x.Iid == this.Item.Iid);
                
                if (indexToUpdate >= 0)
                {
                    this.OrderedItemsList[indexToUpdate] = this.Item;
                }
            }

            this.ThingChanged.InvokeAsync(this.Thing);
            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Handles cancellation of the form popup.
        /// </summary>
        public void OnCanceled()
        {
            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (this.Thing is not ArrayParameterType arrayParameterType)
            {
                return;
            }

            if (arrayParameterType.Dimension.Count == 0)
            {
                arrayParameterType.Dimension.AddRange([1, 1, 1]);
                this.OnDimensionTextChanged("1,1,1");
                return;
            }

            this.Dimension = string.Join(",", arrayParameterType.Dimension.Select(x => x.ToString()));
        }

        /// <summary>
        /// Method executed every time the dimension field text has changed
        /// </summary>
        /// <param name="text">The new text</param>
        private void OnDimensionTextChanged(string text)
        {
            if (!DimensionTextPattern().IsMatch(text))
            {
                return;
            }

            this.Dimension = text;
            var dimensions = text.Split(",").Select(int.Parse).ToList();

            this.UpdateDimensions(dimensions);
            this.GenerateComponents(dimensions, [..new int[dimensions.Count]]);
            this.TrimComponents(dimensions);
        }

        /// <summary>
        /// Trims the not necessary components, if any
        /// </summary>
        /// <param name="dimensions">The matrix dimensions</param>
        private void TrimComponents(List<int> dimensions)
        {
            var numberOfElements = GetNumberOfElements(dimensions);
            var arrayParameterType = (ArrayParameterType)this.Thing;

            if (numberOfElements >= arrayParameterType.Component.Count)
            {
                return;
            }

            while (arrayParameterType.Component.ElementAtOrDefault(numberOfElements) != null)
            {
                arrayParameterType.Component.Remove(arrayParameterType.Component[numberOfElements]);
            }
        }

        /// <summary>
        /// Generates parameter type components based on the given dimensions
        /// </summary>
        /// <param name="dimensions">The dimensions to be used</param>
        /// <param name="currentIndices">The current coordinate indices</param>
        /// <param name="currentDimension">The current dimension parameter used by recursivity</param>
        private void GenerateComponents(List<int> dimensions, List<int> currentIndices, int currentDimension = 0)
        {
            var arrayParameterType = (ArrayParameterType)this.Thing;

            for (var dimensionIndex = 0; dimensionIndex < dimensions[currentDimension]; dimensionIndex++)
            {
                currentIndices[currentDimension] = dimensionIndex;

                if (currentDimension == dimensions.Count - 1)
                {
                    var flatIndex = GetFlatIndexFromCoordinates(dimensions, currentIndices);

                    if (flatIndex >= arrayParameterType.Component.Count || arrayParameterType.Component[flatIndex] == null)
                    {
                        arrayParameterType.Component.Add(new ParameterTypeComponent { Iid = Guid.NewGuid() });
                    }
                }
                else if (currentDimension < dimensions.Count)
                {
                    this.GenerateComponents(dimensions, currentIndices, currentDimension + 1);
                }
            }
        }

        /// <summary>
        /// Updates the dimensions of the <see cref="ArrayParameterType" /> given a list of dimensions
        /// </summary>
        /// <param name="dimensions">The dimensions</param>
        private void UpdateDimensions(List<int> dimensions)
        {
            var arrayParameterType = (ArrayParameterType)this.Thing;
            var dimensionCount = dimensions.Count;
            var currentDimensionCount = arrayParameterType.Dimension.Count;

            for (var dimensionIndex = 0; dimensionIndex < Math.Max(dimensionCount, currentDimensionCount); dimensionIndex++)
            {
                if (dimensionIndex < dimensionCount)
                {
                    if (dimensionIndex < currentDimensionCount)
                    {
                        arrayParameterType.Dimension[dimensionIndex] = dimensions[dimensionIndex];
                    }
                    else
                    {
                        arrayParameterType.Dimension.Add(dimensions[dimensionIndex]);
                    }
                }
                else
                {
                    arrayParameterType.Dimension.SortedItems.RemoveAt(dimensionCount);
                }
            }
        }

        /// <summary>
        /// Gets the flat index of a component based on its coordinates
        /// </summary>
        /// <param name="dimensions">The dimensions of the matrix</param>
        /// <param name="coordinates">The coordinates of the component</param>
        /// <returns>The flat index</returns>
        private static int GetFlatIndexFromCoordinates(IReadOnlyList<int> dimensions, IReadOnlyList<int> coordinates)
        {
            var flatIndex = 0;
            var multiplier = 1;

            for (var i = dimensions.Count - 1; i >= 0; i--)
            {
                flatIndex += coordinates[i] * multiplier;
                multiplier *= dimensions[i];
            }

            return flatIndex;
        }

        /// <summary>
        /// Gets the number of elements that a matrix with the given dimension has
        /// </summary>
        /// <param name="dimensions">The matrix dimensions</param>
        /// <returns>The number of elements of the matrix</returns>
        private static int GetNumberOfElements(List<int> dimensions)
        {
            return dimensions.Aggregate(1, (current, dimension) => current * dimension);
        }

        /// <summary>
        /// The regex used to validate the dimensions field
        /// </summary>
        /// <returns>The regex pattern</returns>
        [GeneratedRegex(@"^\d+(,\d+)*$")]
        private static partial Regex DimensionTextPattern();
    }
}
