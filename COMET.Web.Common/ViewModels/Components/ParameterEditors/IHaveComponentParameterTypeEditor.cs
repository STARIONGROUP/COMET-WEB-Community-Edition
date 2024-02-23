// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IHaveComponentParameterTypeEditor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.ViewModels.Components.ParameterEditors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Base class for <see cref="ParameterTypeEditorBaseViewModel{TParameterType}"/> that contains component <see cref="ParameterType"/>
    /// </summary>
    public interface IHaveComponentParameterTypeEditor
    {
        /// <summary>
        /// Gets the <see cref="ParameterOrOverrideBase"/>
        /// </summary>
        public ParameterOrOverrideBase Parameter { get; }

        /// <summary>
        /// Creates a <see cref="IParameterTypeEditorSelectorViewModel"/> 
        /// </summary>
        /// <param name="parameterType">the parameter type</param>
        /// <param name="valueArrayIndex">
        /// the index of the inside the value array
        /// </param>
        /// <param name="scale">The <see cref="MeasurementScale"/></param>
        /// <param name="onParameterUpdate">The <see cref="EventCallback{TValue}"/> to call on <see cref="IValueSet"/> update</param>
        /// <returns>the <see cref="IParameterTypeEditorSelectorViewModel"/></returns>
        IParameterTypeEditorSelectorViewModel CreateParameterTypeEditorSelectorViewModel(ParameterType parameterType, int valueArrayIndex, MeasurementScale scale, EventCallback<(IValueSet, int)> onParameterUpdate);

        /// <summary>
        /// Event for when the edit button is clicked
        /// </summary>
        void OnComponentSelected();
	}
}
