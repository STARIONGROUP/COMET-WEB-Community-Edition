// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="enumerationValueDefinitionsTable.razor.cs" company="Starion Group S.A.">
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
    /// Support class for the <see cref="EnumerationValueDefinitionsTable" />
    /// </summary>
    public partial class EnumerationValueDefinitionsTable
    {
        /// <summary>
        /// A collection of value definitions to display for selection
        /// </summary>
        [Parameter]
        public IEnumerable<EnumerationValueDefinition> EnumerationValueDefinitions { get; set; }

        /// <summary>
        /// The method that is executed when the enumeration value definitions change
        /// </summary>
        [Parameter]
        public EventCallback<IEnumerable<EnumerationValueDefinition>> EnumerationValueDefinitionsChanged { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if a enumeration value definition should be created
        /// </summary>
        public bool ShouldCreate { get; private set; }

        /// <summary>
        /// The enumeration value definition that will be handled for both edit and add forms
        /// </summary>
        public EnumerationValueDefinition EnumerationValueDefinition { get; private set; } = new();

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Method that is invoked when the edit/add enumeration value definition form is being saved
        /// </summary>
        private void OnEditEnumerationValueDefinitionSaving()
        {
            var valueDefinitionsList = this.EnumerationValueDefinitions.ToList();

            if (this.ShouldCreate)
            {
                valueDefinitionsList.Add(this.EnumerationValueDefinition);
                this.EnumerationValueDefinitions = valueDefinitionsList;
            }
            else
            {
                var indexToUpdate = valueDefinitionsList.FindIndex(x => x.Iid == this.EnumerationValueDefinition.Iid);
                valueDefinitionsList[indexToUpdate] = this.EnumerationValueDefinition;
            }

            this.EnumerationValueDefinitions = valueDefinitionsList;
            this.EnumerationValueDefinitionsChanged.InvokeAsync(this.EnumerationValueDefinitions);
        }

        /// <summary>
        /// Method that is invoked when a enumeration value definition row is being removed
        /// </summary>
        private void RemoveEnumerationValueDefinition(EnumerationValueDefinitionRowViewModel row)
        {
            var valueDefinitionsList = this.EnumerationValueDefinitions.ToList();
            valueDefinitionsList.Remove(row.Thing);

            this.EnumerationValueDefinitions = valueDefinitionsList;
            this.EnumerationValueDefinitionsChanged.InvokeAsync(this.EnumerationValueDefinitions);
        }

        /// <summary>
        /// Method invoked when creating a new enumeration value definition
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditEnumerationValueDefinition(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (EnumerationValueDefinitionRowViewModel)e.DataItem;
            this.ShouldCreate = e.IsNew;

            this.EnumerationValueDefinition = dataItem == null
                ? new EnumerationValueDefinition() { Iid = Guid.NewGuid() }
                : dataItem.Thing.Clone(true);

            e.EditModel = this.EnumerationValueDefinition;
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="EnumerationValueDefinitions" />
        /// </summary>
        /// <returns>A collection of <see cref="EnumerationValueDefinitionRowViewModel" />s to display</returns>
        private List<EnumerationValueDefinitionRowViewModel> GetRows()
        {
            return this.EnumerationValueDefinitions?.Select(x => new EnumerationValueDefinitionRowViewModel(x)).ToList();
        }
    }
}
