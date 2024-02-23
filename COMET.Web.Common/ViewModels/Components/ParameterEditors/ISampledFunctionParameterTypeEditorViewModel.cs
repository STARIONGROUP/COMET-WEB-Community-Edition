// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ISampledFunctionParameterTypeEditorViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    /// <summary>
    /// View model used to edit <see cref="SampledFunctionParameterType"/> 
    /// </summary>
    public interface ISampledFunctionParameterTypeEditorViewModel: IHaveComponentParameterTypeEditor
    {
        /// <summary>
        /// Add a new row of default values
        /// </summary>
        void AddRow();

        /// <summary>
        /// Remove a row of values
        /// </summary>
        void RemoveRow();

        /// <summary>
        /// Value asserting that it is allowed to remove a row
        /// </summary>
        bool CanRemoveRow { get; }

        /// <summary>
        /// A collection of all <see cref="IParameterTypeAssignment" />
        /// </summary>
        IReadOnlyList<IParameterTypeAssignment> ParameterTypeAssignments { get; }

        /// <summary>
        /// Creates a <see cref="IParameterTypeEditorSelectorViewModel"/> 
        /// </summary>
        /// <param name="valueArrayIndex">
        /// the index of the inside the value array
        /// </param>
        /// <returns>the <see cref="IParameterTypeEditorSelectorViewModel"/></returns>
        IParameterTypeEditorSelectorViewModel CreateParameterTypeEditorSelectorViewModel(int valueArrayIndex);

        /// <summary>
        /// Reset changes that could have been set to the <see cref="ValueArray{T}" />
        /// </summary>
        void ResetChanges();
    }
}
