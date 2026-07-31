// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IndependentParameterTypeTable.razor.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMET.Web.Common.Components;

    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="IndependentParameterTypeTable" />
    /// </summary>
    public partial class IndependentParameterTypeTable : DisposableComponent
    {
        /// <summary>
        /// The compound parameter type
        /// </summary>
        [Parameter]
        public SampledFunctionParameterType Thing { get; set; }

        /// <summary>
        /// The callback for when the parameter type has changed
        /// </summary>
        [Parameter]
        public EventCallback<SampledFunctionParameterType> ThingChanged { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="CDP4Common.SiteDirectoryData.ParameterType" />s
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
        /// Gets or sets a value indicating whether the form popup is visible for editing or creating.
        /// </summary>
        public bool IsOnEditMode { get; set; }

        /// <summary>
        /// The independent parameter type that will be handled for both edit and add forms
        /// </summary>
        public IndependentParameterTypeRowViewModel Item { get; private set; }

        /// <summary>
        /// Starts the creation flow for a new independent parameter type.
        /// </summary>
        public void StartCreate()
        {
            this.ShouldCreate = true;
            this.Item = new IndependentParameterTypeRowViewModel(new IndependentParameterTypeAssignment { Iid = Guid.NewGuid() }, string.Empty);
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Starts the edit flow for the specified <paramref name="row" />.
        /// </summary>
        /// <param name="row">The selected row to edit.</param>
        public void StartEdit(IndependentParameterTypeRowViewModel row)
        {
            if (row == null)
            {
                return;
            }

            this.ShouldCreate = false;
            this.Item = new IndependentParameterTypeRowViewModel(row.Thing.Clone(true), row.InterpolationPeriod);
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Handles saving of the form popup, updating or adding the item.
        /// </summary>
        public void OnSaved()
        {
            if (this.ShouldCreate)
            {
                this.Thing.IndependentParameterType.Add(this.Item.Thing);
                this.Thing.InterpolationPeriod = new ValueArray<string>(this.Thing.InterpolationPeriod.Append(this.Item.InterpolationPeriod));
            }
            else
            {
                var indexToUpdate = this.Thing.IndependentParameterType.FindIndex(x => x.Iid == this.Item.Thing.Iid);
                this.Thing.InterpolationPeriod[indexToUpdate] = this.Item.InterpolationPeriod;
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
        /// Moves the selected row up
        /// </summary>
        /// <param name="row">The row to be moved</param>
        /// <returns>A <see cref="Task" /></returns>    
        private async Task MoveUp(IndependentParameterTypeRowViewModel row)
        {
            var currentIndex = this.Thing.IndependentParameterType.IndexOf(row.Thing);
            this.Thing.IndependentParameterType.Move(currentIndex, currentIndex - 1);
            (this.Thing.InterpolationPeriod[currentIndex], this.Thing.InterpolationPeriod[currentIndex - 1]) = (this.Thing.InterpolationPeriod[currentIndex - 1], this.Thing.InterpolationPeriod[currentIndex]);
            await this.ThingChanged.InvokeAsync(this.Thing);
        }

        /// <summary>
        /// Moves the selected row down
        /// </summary>
        /// <param name="row">The row to be moved</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task MoveDown(IndependentParameterTypeRowViewModel row)
        {
            var currentIndex = this.Thing.IndependentParameterType.IndexOf(row.Thing);
            this.Thing.IndependentParameterType.Move(currentIndex, currentIndex + 1);
            (this.Thing.InterpolationPeriod[currentIndex], this.Thing.InterpolationPeriod[currentIndex + 1]) = (this.Thing.InterpolationPeriod[currentIndex + 1], this.Thing.InterpolationPeriod[currentIndex]);
            await this.ThingChanged.InvokeAsync(this.Thing);
        }

        /// <summary>
        /// Method that is invoked when a independent parameter type row is being removed
        /// </summary>
        private void RemoveIndependentParameterType(IndependentParameterTypeRowViewModel row)
        {
            this.Thing.IndependentParameterType.Remove(row.Thing);
            this.ThingChanged.InvokeAsync(this.Thing);
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="SampledFunctionParameterType" />
        /// </summary>
        /// <returns>A collection of <see cref="IndependentParameterTypeRowViewModel" />s to display</returns>
        private List<IndependentParameterTypeRowViewModel> GetRows()
        {
            var degreesOfInterpolation = this.Thing.InterpolationPeriod;
            var rows = new List<IndependentParameterTypeRowViewModel>();
            var i = 0;

            foreach (var independentParameterType in this.Thing.IndependentParameterType.ToList())
            {
                var degreeOfInterpolation = degreesOfInterpolation.ElementAtOrDefault(i) ?? string.Empty;
                rows.Add(new IndependentParameterTypeRowViewModel(independentParameterType, degreeOfInterpolation));
                i++;
            }

            return [.. rows.OrderBy(x => x.Name)];
        }
    }
}
