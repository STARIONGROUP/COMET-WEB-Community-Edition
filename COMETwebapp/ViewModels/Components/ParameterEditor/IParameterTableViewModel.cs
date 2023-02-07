// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterTableViewModel.cs" company="RHEA System S.A.">
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
    
    using DynamicData;

    /// <summary>
    /// Interface for the <see cref="ParameterTableViewModel"/>
    /// </summary>
    public interface IParameterTableViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ParameterBaseBaseRowViewModel"/> for this <see cref="ParameterTableViewModel"/>
        /// </summary>
        SourceList<ParameterBaseBaseRowViewModel> Rows { get; set; }

        /// <summary>
        /// Initializes this <see cref="IParameterTableViewModel"/>
        /// </summary>
        /// <param name="elements">the elements of the table</param>
        /// <param name="selectedOption">the selected option</param>
        /// <param name="selectedState">the selected state</param>
        /// <param name="isOwnedParameters">if true only parameters owned by the active domain are shown</param>
        void InitializeViewModel(SourceList<ElementBase> elements, Option selectedOption, ActualFiniteState selectedState, bool isOwnedParameters);

        /// <summary>
        /// Filters <see cref="Parameter"/> for the selected owner and the selected type
        /// </summary>
        /// <param name="parameters"><see cref="Parameter"/> to filter</param>
        IEnumerable<Parameter> FilterParameters(List<Parameter> parameters);

        /// <summary>
        /// Filters <see cref="ParameterOverride"/> for the selected owner and the selected type
        /// </summary>
        /// <param name="parameters"><see cref="ParameterOverride"/> to filter</param>
        IEnumerable<ParameterOverride> FilterParameterOverrides(List<ParameterOverride> parameters);

        /// <summary>
        /// Filters <see cref="ParameterValueSetBase"/>s for the selected option and the selected state
        /// </summary>
        /// <param name="isOptionDependent">if the <see cref="Parameter"/> is option dependant</param>
        /// <param name="parameterValueSets">the <see cref="ParameterValueSet"/> to filter</param>
        /// <returns>the filtered result</returns>
        IEnumerable<ParameterValueSetBase> FilterParameterValueSetBase(bool isOptionDependent, List<ParameterValueSetBase> parameterValueSets);
    }
}
