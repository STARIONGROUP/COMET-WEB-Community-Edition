// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IHaveValueSetViewModel.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.ViewModels.Components
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Base view model that have a <see cref="IValueSet" />
    /// </summary>
    public interface IHaveValueSetViewModel
    {
        /// <summary>
        /// Gets the <see cref="IValueSet" />
        /// </summary>
        IValueSet ValueSet { get; }

        /// <summary>
        /// Gets the current <see cref="ParameterSwitchKind" />
        /// </summary>
        ParameterSwitchKind CurrentParameterSwitchKind { get; }

        /// <summary>
        /// Sets the <see cref="ParameterSwitchKind" />
        /// </summary>
        /// <param name="parameterSwitchKind">The <see cref="ParameterSwitchKind" /></param>
        void UpdateParameterSwitchKind(ParameterSwitchKind parameterSwitchKind);

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="readOnly">The readonly state</param>
        void UpdateProperties( bool readOnly);
    }
}
