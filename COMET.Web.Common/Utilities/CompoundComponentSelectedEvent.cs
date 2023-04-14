// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CompoundComponentSelectedEvent.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Utilities
{
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    /// <summary>
    /// Class used to notify an observer that the <see cref="CompoundParameterTypeEditorViewModel" /> is selected.
    /// </summary>
    public class CompoundComponentSelectedEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundComponentSelectedEvent" /> class.
        /// </summary>
        /// <param name="compoundParameterTypeEditorViewModel">The <see cref="CompoundParameterTypeEditorViewModel" /></param>
        public CompoundComponentSelectedEvent(CompoundParameterTypeEditorViewModel compoundParameterTypeEditorViewModel)
        {
            this.CompoundParameterTypeEditorViewModel = compoundParameterTypeEditorViewModel;
        }

        /// <summary>
        /// Gets or sets the <see cref="CompoundParameterTypeEditorViewModel" />
        /// </summary>
        public CompoundParameterTypeEditorViewModel CompoundParameterTypeEditorViewModel { get; set; }
    }
}
