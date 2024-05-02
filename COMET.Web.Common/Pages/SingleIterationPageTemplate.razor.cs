// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleIterationPageTemplate.razor.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Pages
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Extensions;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Base abstract component for any page that should use only one <see cref="Iteration" />
    /// </summary>
    public abstract partial class SingleIterationPageTemplate
    {
        /// <summary>
        /// The <see cref="Guid" /> of an <see cref="Iteration" /> as a short string
        /// </summary>
        [Parameter]
        [SupplyParameterFromQuery]
        public string IterationId { get; set; }

        /// <summary>
        /// The <see cref="Guid" /> of the requested <see cref="Iteration" />
        /// </summary>
        protected Guid RequestedIteration { get; private set; }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (!string.IsNullOrEmpty(this.IterationId))
            {
                this.RequestedIteration = this.IterationId.FromShortGuid();
            }
        }
    }
}
