// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EngineeringModelSelector.razor.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Components.Selectors
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Selector for <see cref="EngineeringModel"/>
    /// </summary>
    public partial class EngineeringModelSelector
    {
        /// <summary>
        /// Gets or sets the <see cref="IEngineeringModelSelectorViewModel"/>
        /// </summary>
        [Parameter]
        public IEngineeringModelSelectorViewModel ViewModel { get; set; }
    }
}
