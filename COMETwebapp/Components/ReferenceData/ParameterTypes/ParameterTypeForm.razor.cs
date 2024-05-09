// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeForm.razor.cs" company="Starion Group S.A.">
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
    using System.ComponentModel.DataAnnotations;

    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.ReferenceData.ParameterTypes;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="ParameterTypeForm" />
    /// </summary>
    public partial class ParameterTypeForm : SelectedDataItemForm
    {
        /// <summary>
        /// The <see cref="IParameterTypeTableViewModel" /> for this component
        /// </summary>
        [Parameter]
        [Required]
        public IParameterTypeTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Method that is executed when there is a valid submit
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnValidSubmit()
        {
            await this.ViewModel.CreateOrEditParameterType(this.ShouldCreate);
            await base.OnValidSubmit();
        }

        /// <summary>
        /// Method executed when the selected enumeration value definitions changed
        /// </summary>
        /// <param name="enumerationValueDefinitions">A collection of enumeration value definitions</param>
        private void OnEnumerationValueDefinitionsChanged(IEnumerable<EnumerationValueDefinition> enumerationValueDefinitions)
        {
            if (this.ViewModel.Thing is not EnumerationParameterType enumerationParameterType)
            {
                return;
            }

            var enumerationValueDefinitionsList = enumerationValueDefinitions.ToList();

            var valueDefinitionsToCreate = enumerationValueDefinitionsList.Where(x => !enumerationParameterType.ValueDefinition.Contains(x)).ToList();
            var valueDefinitionsToRemove = enumerationParameterType.ValueDefinition.Where(x => !enumerationValueDefinitionsList.Contains(x)).ToList();

            enumerationParameterType.ValueDefinition.AddRange(valueDefinitionsToCreate);
            valueDefinitionsToRemove.ForEach(x => enumerationParameterType.ValueDefinition.Remove(x));

            this.ViewModel.SelectedEnumerationValueDefinitions = enumerationValueDefinitionsList;
            this.InvokeAsync(this.StateHasChanged);
        }

        /// <summary>
        /// Method executed when the selected parameter type components changed
        /// </summary>
        /// <param name="parameterTypeComponents">A collection of parameter type components</param>
        private void OnParameterTypeComponentsChanged(SortedList<long, ParameterTypeComponent> parameterTypeComponents)
        {
            if (this.ViewModel.Thing is not ArrayParameterType arrayParameterType)
            {
                return;
            }

            var enumerationValueDefinitionsList = parameterTypeComponents;

            var valueDefinitionsToCreate = enumerationValueDefinitionsList.Where(x => !arrayParameterType.Component.Contains(x.Value)).ToList();
            var valueDefinitionsToRemove = arrayParameterType.Component.Where(x => !enumerationValueDefinitionsList.Select(x => x.Value).Contains(x)).ToList();

            arrayParameterType.Component.AddRange(valueDefinitionsToCreate.Select(x => x.Value));
            valueDefinitionsToRemove.ForEach(x => arrayParameterType.Component.Remove(x));

            this.ViewModel.SelectedParameterTypeComponents = enumerationValueDefinitionsList;
            this.InvokeAsync(this.StateHasChanged);
        }
    }
}
