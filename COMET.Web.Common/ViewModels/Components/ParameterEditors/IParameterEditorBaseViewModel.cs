// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterEditorBaseViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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
    using CDP4Common.Types;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Base interface for all the interfaces of type <i>ParameterTypeEditorViewModel</i>
    /// </summary>
    public interface IParameterEditorBaseViewModel<out T> : IHaveValueSetViewModel, IHaveReadOnlyStateViewModel where T : ParameterType
    {
        /// <summary>
        /// Gets or sets the <see cref="CDP4Common.SiteDirectoryData.ParameterType" /> for this
        /// <see cref="IParameterEditorBaseViewModel{T}" />
        /// </summary>
        T ParameterType { get; }

        /// <summary>
        /// Gets or sets the <see cref="EventCallback{T}" /> for when the parameter value has changed
        /// </summary>
        EventCallback<(IValueSet, int)> ParameterValueChanged { get; set; }

        /// <summary>
        /// The <see cref="ValueArray{T}" /> to work with
        /// </summary>
        ValueArray<string> ValueArray { get; set; }

        /// <summary>
        /// Gets the index of the value changed in the value sets
        /// </summary>
        int ValueArrayIndex { get; }

        /// <summary>
        /// Gets the associated <see cref="ParameterOrOverrideBase" />
        /// </summary>
        ParameterOrOverrideBase Parameter { get; }

        /// <summary>
        /// The validation messages to display
        /// </summary>
        string ValidationMessage { get; }

        /// <summary>
        /// Event for when a parameter's value has changed
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        Task OnParameterValueChanged(object value);
    }
}
