// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterTypeEditorSelectorViewModel.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.ViewModels.Components.ParameterEditors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// ViewModel used to handle creation of <see cref="IParameterEditorBaseViewModel{T}" />
    /// </summary>
    public interface IParameterTypeEditorSelectorViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="CDP4Common.SiteDirectoryData.ParameterType" /> for this
        /// <see cref="IParameterEditorBaseViewModel{T}" />
        /// </summary>
        public ParameterType ParameterType { get; }

        /// <summary>
        /// Gets or sets the <see cref="EventCallback{T}" /> for when the parameter value has changed
        /// </summary>
        EventCallback<(IValueSet, int)> ParameterValueChanged { get; set; }

        /// <summary>
        /// Gets the <see cref="MeasurementScale" /> to use
        /// </summary>
        MeasurementScale Scale { get; set; }

        /// <summary>
        /// Gets the value set of this <see cref="ParameterType" />
        /// </summary>
        IValueSet ValueSet { get; }

        /// <summary>
        /// Creates a view model for the corresponding editor
        /// </summary>
        /// <typeparam name="T">the parameter type</typeparam>
        /// <returns>the view model</returns>
        IParameterEditorBaseViewModel<T> CreateParameterEditorViewModel<T>() where T : ParameterType;

        /// <summary>
        /// Updates the <see cref="ParameterSwitchKind" /> value
        /// </summary>
        /// <param name="switchValue">The <see cref="ParameterSwitchKind" /></param>
        void UpdateSwitchKind(ParameterSwitchKind switchValue);

        /// <summary>
        /// Updates the associated <see cref="IParameterEditorBaseViewModel{T}" /> properties
        /// </summary>
        /// <param name="readOnly">The readonly state</param>
        void UpdateProperties(bool readOnly);
    }
}
