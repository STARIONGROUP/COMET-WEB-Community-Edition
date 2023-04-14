// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IEnumerationParameterTypeEditorViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Interface definition for <see cref="EnumerationParameterTypeEditorViewModel" />
    /// </summary>
    public interface IEnumerationParameterTypeEditorViewModel
    {
        /// <summary>
        /// The available <see cref="EnumerationValueDefinition" />s
        /// </summary>
        IEnumerable<EnumerationValueDefinition> EnumerationValueDefinitions { get; set; }

        /// <summary>
        /// Names of selected <see cref="EnumerationValueDefinition" />s
        /// </summary>
        IEnumerable<string> SelectedEnumerationValueDefinitions { get; set; }

        /// <summary>
        /// Indicates if all elements are checked
        /// </summary>
        bool SelectAllChecked { get; set; }

        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        bool IsOnEditMode { get; set; }

        /// <summary>
        /// Method invoked when select all is checked
        /// </summary>
        void OnSelectAllChanged(bool value);

        /// <summary>
        /// Method invoked when confirming selection of  <see cref="EnumerationValueDefinition" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task OnConfirmButtonClick();

        /// <summary>
        /// Method invoked when canceling the selection of <see cref="EnumerationValueDefinition" />
        /// </summary>
        void OnCancelButtonClick();
    }
}
