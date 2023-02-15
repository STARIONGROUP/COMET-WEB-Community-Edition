// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterBaseRowViewModel.cs" company="RHEA System S.A.">
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

    using COMETwebapp.ViewModels.Components.Shared.ParameterEditors;

    /// <summary>
    /// Interface for the <see cref="ParameterBaseRowViewModel"/>
    /// </summary>
    public interface IParameterBaseRowViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ParameterBase"/> for this <see cref="ParameterBaseRowViewModel"/>
        /// </summary>
        ParameterBase Parameter { get; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterType"/> for this <see cref="ParameterBaseRowViewModel"/>
        /// </summary>
        public ParameterType ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ElementBase"/> used for grouping this <see cref="ParameterBaseRowViewModel"/>
        /// </summary>
        string ElementBaseName { get; }

        /// <summary>
        /// Gets the <see cref="Parameter"/> type name
        /// </summary>
        string ParameterName { get; }

        /// <summary>
        /// Gets the <see cref="Parameter"/> owner name
        /// </summary>
        string OwnerName { get; }

        /// <summary>
        /// Gets the switch for the published value
        /// </summary>
        ParameterSwitchKind Switch { get; }

        /// <summary>
        /// Gets the switch for the published value
        /// </summary>
        string ModelCode { get; }

        /// <summary>
        /// Gets the <see cref="Option"/> name this <see cref="Parameter"/> is dependant on
        /// </summary>
        string Option { get; }

        /// <summary>
        /// Gets the <see cref="ActualFiniteState"/> name this <see cref="Parameter"/> is dependant on
        /// </summary>
        string State { get; }

        /// <summary>
        /// Creates a <see cref="IParameterTypeEditorSelectorViewModel{T}"/> based on the data of this <see cref="IParameterBaseRowViewModel"/>
        /// </summary>
        /// <returns>a <see cref="IParameterTypeEditorSelectorViewModel{T}"/></returns>
        IParameterTypeEditorSelectorViewModel<T> CreateParameterTypeEditorSelectorViewModel<T>() where T : ParameterType;

        /// <summary>
        /// Creates a <see cref="IParameterSwitchKindComponentViewModel"/> based on the data of this <see cref="IParameterBaseRowViewModel"/>
        /// </summary>
        /// <returns>a <see cref="IParameterSwitchKindComponentViewModel"/></returns>
        IParameterSwitchKindComponentViewModel CreateParameterSwitchKindComponentViewModel();

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        Task OnParameterValueChanged(IValueSet value);
    }
}
