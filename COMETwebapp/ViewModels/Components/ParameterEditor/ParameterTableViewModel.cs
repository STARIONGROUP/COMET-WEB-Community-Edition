// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTableViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.ParameterEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Services.SessionManagement;

    using DynamicData;

    using Microsoft.AspNetCore.Components;
    
    using ReactiveUI;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Components.ParameterEditor.ParameterTable"/>
    /// </summary>
    public class ParameterTableViewModel : ReactiveObject, IParameterTableViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ISessionService"/>
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// All <see cref="ElementUsage"/> and the Top <see cref="ElementDefinition"/> of the iteration
        /// </summary>
        public SourceList<ElementBase> Elements { get; set; } = new();

        /// <summary>
        /// Sets if only parameters owned by the active domain are shown
        /// </summary>
        public bool IsOwnedParameters { get; set; }

        /// <summary>
        /// Name of the parameter type selected
        /// </summary>
        public ParameterType ParameterTypeSelected { get; set; }

        /// <summary>
        /// Name of the option selected
        /// </summary>
        public Option OptionSelected { get; set; }

        /// <summary>
        /// Name of the state selected
        /// </summary>
        public ActualFiniteState StateSelected { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterBaseBaseRowViewModel"/> for this <see cref="ParameterTableViewModel"/>
        /// </summary>
        public SourceList<ParameterBaseBaseRowViewModel> Rows { get; set; } = new();

        /// <summary>
        /// Creates a new instance of <see cref="ParameterTableViewModel"/>
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService"/></param>
        public ParameterTableViewModel(ISessionService sessionService)
        {
            this.SessionService = sessionService;
            this.WhenAnyValue(x=>x.Elements.CountChanged).Subscribe(_ => this.CreateParameterBaseRowViewModels(this.Elements.Items));
        }

        /// <summary>
        /// Initializes this <see cref="IParameterTableViewModel"/>
        /// </summary>
        /// <param name="elements">the elements of the table</param>
        /// <param name="selectedOption">the selected option</param>
        /// <param name="selectedState">the selected state</param>
        /// <param name="isOwnedParameters">if true only parameters owned by the active domain are shown</param>
        public void InitializeViewModel(SourceList<ElementBase> elements, Option selectedOption, ActualFiniteState selectedState, bool isOwnedParameters)
        {
            this.Elements.Clear();
            this.Elements.AddRange(elements.Items);
            this.OptionSelected = selectedOption;
            this.StateSelected = selectedState;
            this.IsOwnedParameters = isOwnedParameters;
  }

        /// <summary>
        /// Creates the <see cref="ParameterBaseBaseRowViewModel"/> for the <param name="elements"></param>
        /// </summary>
        /// <param name="elements">the elements used for creating each <see cref="ParameterBaseBaseRowViewModel"/></param>
        /// <returns>an <see cref="IEnumerable{T}"/> of <see cref="ParameterBaseBaseRowViewModel"/></returns>
        public void CreateParameterBaseRowViewModels(IEnumerable<ElementBase> elements)
        {
            foreach (var element in elements)
            {
                if (element is ElementDefinition elementDefinition)
                {
                    elementDefinition.Parameter.ForEach(parameter => this.Rows.Add(new ParameterBaseBaseRowViewModel(parameter)));
                }
                else if (element is ElementUsage elementUsage)
                {
                    if (elementUsage.ParameterOverride.Any())
                    {
                        elementUsage.ParameterOverride.ForEach(parameter => this.Rows.Add(new ParameterBaseBaseRowViewModel(parameter)));
                    }
                    else
                    {
                        elementUsage.ElementDefinition.Parameter.ForEach(parameter => this.Rows.Add(new ParameterBaseBaseRowViewModel(parameter)));
                    }
                }
            }
        }

        /// <summary>
        /// Filters <see cref="Parameter"/> for the selected owner and the selected type
        /// </summary>
        /// <param name="parameters"><see cref="Parameter"/> to filter</param>
        public IEnumerable<Parameter> FilterParameters(List<Parameter> parameters)
        {
            var filteredParameters = new List<Parameter>();

            if (this.IsOwnedParameters == true)
            {
                filteredParameters.AddRange(parameters.FindAll(p => p.Owner == this.SessionService.GetDomainOfExpertise(this.SessionService.DefaultIteration)));
            }
            else
            {
                filteredParameters.AddRange(parameters);
            }

            if (this.ParameterTypeSelected != null)
            {
                filteredParameters.RemoveAll(p => p.ParameterType != this.ParameterTypeSelected);
            }

            return filteredParameters.OrderBy(p => p.ParameterType.Name);
        }

        /// <summary>
        /// Filters <see cref="ParameterOverride"/> for the selected owner and the selected type
        /// </summary>
        /// <param name="parameters"><see cref="ParameterOverride"/> to filter</param>
        public IEnumerable<ParameterOverride> FilterParameterOverrides(List<ParameterOverride> parameters)
        {
            var filteredParameters = new List<ParameterOverride>();

            if (this.IsOwnedParameters == true)
            {
                filteredParameters.AddRange(parameters.FindAll(p => p.Owner == this.SessionService.GetDomainOfExpertise(this.SessionService.DefaultIteration)));
            }
            else
            {
                filteredParameters.AddRange(parameters);
            }

            if (this.ParameterTypeSelected != null)
            {
                filteredParameters.RemoveAll(p => p.ParameterType != this.ParameterTypeSelected);
            }

            return filteredParameters.OrderBy(p => p.ParameterType.Name);
        }

        /// <summary>
        /// Filters <see cref="ParameterValueSetBase"/>s for the selected option and the selected state
        /// </summary>
        /// <param name="isOptionDependent">if the <see cref="Parameter"/> is option dependant</param>
        /// <param name="parameterValueSets">the <see cref="ParameterValueSet"/> to filter</param>
        /// <returns>the filtered result</returns>
        public IEnumerable<ParameterValueSetBase> FilterParameterValueSetBase(bool isOptionDependent, List<ParameterValueSetBase> parameterValueSets)
        {
            var filteredParameterValueSets = new List<ParameterValueSetBase>();

            if (this.OptionSelected != null && isOptionDependent)
            {
                filteredParameterValueSets.AddRange(parameterValueSets.FindAll(p => p.ActualOption == this.OptionSelected));
            }
            else
            {
                filteredParameterValueSets.AddRange(parameterValueSets);
            }

            if (this.StateSelected != null)
            {
                var parameterValueSetsToRemove = new List<ParameterValueSetBase>();

                filteredParameterValueSets.ForEach(p =>
                {
                    if (p.ActualState is null || p.ActualState != this.StateSelected)
                    {
                        parameterValueSetsToRemove.Add(p);
                    }
                });

                filteredParameterValueSets.RemoveAll(p => parameterValueSetsToRemove.Contains(p));
            }

            return filteredParameterValueSets.OrderBy(p => p.ModelCode());
        }
    }
}
