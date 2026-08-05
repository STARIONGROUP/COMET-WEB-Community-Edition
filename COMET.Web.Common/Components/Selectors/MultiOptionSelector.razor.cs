// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MultiOptionSelector.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMET.Web.Common.Components.Selectors
{
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component used to select one or more <see cref="CDP4Common.EngineeringModelData.Option" />s.
    /// </summary>
    public partial class MultiOptionSelector
    {
        /// <summary>
        /// Gets or sets the heading rendered above the selector. Set to an empty string to hide the heading.
        /// </summary>
        [Parameter]
        public string DisplayText { get; set; } = "Filter on Option:";
    }
}
