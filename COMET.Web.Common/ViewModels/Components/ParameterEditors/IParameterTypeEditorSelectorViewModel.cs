// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterTypeEditorSelectorViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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
        EventCallback<IValueSet> ParameterValueChanged { get; set; }

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
        /// Updates the associated <see cref="IParameterEditorBaseViewModel{T}"/> properties
        /// </summary>
        /// <param name="readOnly">The readonly state</param>
        void UpdateProperties(bool readOnly);
    }
}
