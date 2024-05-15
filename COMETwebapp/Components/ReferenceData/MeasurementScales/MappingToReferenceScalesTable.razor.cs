// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MappingToReferenceScalesTable.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ReferenceData.MeasurementScales
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="MappingToReferenceScalesTable" />
    /// </summary>
    public partial class MappingToReferenceScalesTable
    {
        /// <summary>
        /// The measurement scale that contains the mapping to reference scale to display for selection
        /// </summary>
        [Parameter]
        public MeasurementScale MeasurementScale { get; set; }

        /// <summary>
        /// The method that is executed when the mapping to reference scales change
        /// </summary>
        [Parameter]
        public EventCallback<MeasurementScale> MeasurementScaleChanged { get; set; }

        /// <summary>
        /// A collection of dependent scale value definitions to display for selection
        /// </summary>
        [Parameter]
        public IEnumerable<ScaleValueDefinition> DependentScaleValueDefinitions { get; set; }

        /// <summary>
        /// A collection of reference scale value definitions to display for selection
        /// </summary>
        [Parameter]
        public IEnumerable<ScaleValueDefinition> ReferenceScaleValueDefinitions { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if a mapping to reference scale should be created
        /// </summary>
        public bool ShouldCreate { get; private set; }

        /// <summary>
        /// The mapping to reference scale that will be handled for both edit and add forms
        /// </summary>
        private MappingToReferenceScale MappingToReferenceScale { get; set; } = new();

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Method that is invoked when the edit/add mapping to reference scale form is being saved
        /// </summary>
        private void OnEditMappingToReferenceScaleSaving()
        {
            if (this.ShouldCreate)
            {
                this.MeasurementScale.MappingToReferenceScale.Add(this.MappingToReferenceScale);
            }
            else
            {
                var indexToUpdate = this.MeasurementScale.MappingToReferenceScale.FindIndex(x => x.Iid == this.MappingToReferenceScale.Iid);
                this.MeasurementScale.MappingToReferenceScale[indexToUpdate] = this.MappingToReferenceScale;
            }

            this.MeasurementScaleChanged.InvokeAsync(this.MeasurementScale);
        }

        /// <summary>
        /// Method that is invoked when a mapping to reference scale row is being removed
        /// </summary>
        private void RemoveMappingToReferenceScale(MappingToReferenceScaleRowViewModel row)
        {
            this.MeasurementScale.MappingToReferenceScale.Remove(row.MappingToReferenceScale);
            this.MeasurementScaleChanged.InvokeAsync(this.MeasurementScale);
        }

        /// <summary>
        /// Method invoked when creating a new Mapping To Reference Scale
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditMappingToReferenceScale(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (MappingToReferenceScaleRowViewModel)e.DataItem;
            this.ShouldCreate = e.IsNew;

            this.MappingToReferenceScale = dataItem == null
                ? new MappingToReferenceScale { Iid = Guid.NewGuid() }
                : dataItem.MappingToReferenceScale.Clone(true);

            e.EditModel = this.MappingToReferenceScale;
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="MeasurementScale" />
        /// </summary>
        /// <returns>A collection of <see cref="MappingToReferenceScaleRowViewModel" />s to display</returns>
        private List<MappingToReferenceScaleRowViewModel> GetRows()
        {
            return this.MeasurementScale.MappingToReferenceScale?.Select(x => new MappingToReferenceScaleRowViewModel(x)).ToList();
        }
    }
}
