// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ScaleValueDefinitionsTable.razor.cs" company="Starion Group S.A.">
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
    /// Support class for the <see cref="ScaleValueDefinitionsTable" />
    /// </summary>
    public partial class ScaleValueDefinitionsTable
    {
        /// <summary>
        /// The measurement scale that contains scale value definitions to display for selection
        /// </summary>
        [Parameter]
        public MeasurementScale MeasurementScale { get; set; }

        /// <summary>
        /// The method that is executed when the scale value definitions change
        /// </summary>
        [Parameter]
        public EventCallback<MeasurementScale> MeasurementScaleChanged { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if a scale value definition should be created
        /// </summary>
        public bool ShouldCreate { get; private set; }

        /// <summary>
        /// The scale value definition that will be handled for both edit and add forms
        /// </summary>
        public ScaleValueDefinition ScaleValueDefinition { get; private set; } = new();

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Method that is invoked when the edit/add scale value definition form is being saved
        /// </summary>
        private void OnEditScaleValueDefinitionSaving()
        {
            if (this.ShouldCreate)
            {
                this.MeasurementScale.ValueDefinition.Add(this.ScaleValueDefinition);
            }
            else
            {
                var indexToUpdate = this.MeasurementScale.ValueDefinition.FindIndex(x => x.Iid == this.ScaleValueDefinition.Iid);
                this.MeasurementScale.ValueDefinition[indexToUpdate] = this.ScaleValueDefinition;
            }

            this.MeasurementScaleChanged.InvokeAsync(this.MeasurementScale);
        }

        /// <summary>
        /// Method that is invoked when a scale value definition row is being removed
        /// </summary>
        private void RemoveScaleValueDefinition(ScaleValueDefinitionRowViewModel row)
        {
            this.MeasurementScale.ValueDefinition.Remove(row.Thing);
            this.MeasurementScaleChanged.InvokeAsync(this.MeasurementScale);
        }

        /// <summary>
        /// Method invoked when creating a new scale value definition
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditScaleValueDefinition(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (ScaleValueDefinitionRowViewModel)e.DataItem;
            this.ShouldCreate = e.IsNew;

            this.ScaleValueDefinition = dataItem == null
                ? new ScaleValueDefinition { Iid = Guid.NewGuid() }
                : dataItem.Thing.Clone(true);

            e.EditModel = this.ScaleValueDefinition;
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="MeasurementScale" />
        /// </summary>
        /// <returns>A collection of <see cref="ScaleValueDefinitionRowViewModel" />s to display</returns>
        private List<ScaleValueDefinitionRowViewModel> GetRows()
        {
            return this.MeasurementScale.ValueDefinition?.Select(x => new ScaleValueDefinitionRowViewModel(x)).ToList();
        }
    }
}
