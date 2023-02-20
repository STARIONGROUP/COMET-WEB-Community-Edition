// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterTypeEditorSelectorViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.Shared.ParameterEditors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using Microsoft.AspNetCore.Components;

    public interface IParameterTypeEditorSelectorViewModel<T> where T : ParameterType
    {
        /// <summary>
        /// Gets or sets the <see cref="CDP4Common.SiteDirectoryData.ParameterType"/> for this <see cref="IParameterEditorBaseViewModel{T}"/>
        /// </summary>
        public T ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EventCallback{T}"/> for when the parameter value has changed
        /// </summary>
        EventCallback<IValueSet> ParameterValueChanged { get; set; }

        /// <summary>
        /// Gets or sets if the <i>ParameterTypeEditor</i> is readonly. For a <see cref="ParameterSwitchKind"/> with values MANUAL,REFERENCE the value of this
        /// property is false, for a value of COMPUTED the value of this property is true;
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the value set of this <see cref="T"/>
        /// </summary>
        IValueSet ValueSet { get; set; }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <returns>an asynchronous operation</returns>
        public abstract Task OnParameterValueChanged(IValueSet value);

        /// <summary>
        /// Creates a view model for the corresponding editor
        /// </summary>
        /// <typeparam name="T">the parameter type</typeparam>
        /// <returns>the view model</returns>
        public IParameterEditorBaseViewModel<T> CreateParameterEditorViewModel<T>() where T : ParameterType;
    }
}
