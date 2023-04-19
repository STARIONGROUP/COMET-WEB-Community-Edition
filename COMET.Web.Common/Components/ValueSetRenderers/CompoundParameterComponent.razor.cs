﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CompoundParameterComponent.razor.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Components.ValueSetRenderers
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component that show <see cref="ParameterValueSet" /> values for an <see cref="CompoundParameterType" />
    /// </summary>
    public partial class CompoundParameterComponent
    {
        /// <summary>
        /// Index of the <see cref="ParameterTypeComponent" /> in the associated <see cref="ParameterValueSet" />
        /// </summary>
        [Parameter]
        public int IndexStartInParameterTypeComponent { get; set; }

        /// <summary>
        /// Values from <see cref="ParameterValueSet" /> assoicated to the <see cref="ParameterTypeComponent" />
        /// </summary>
        [Parameter]
        public ValueArray<string> Values { get; set; }

        /// <summary>
        /// The <see cref="ParameterTypeComponent" /> to show
        /// </summary>
        [Parameter]
        public ParameterTypeComponent Component { get; set; }

        /// <summary>
        /// Gets value to show when <see cref="ParameterTypeComponent" /> is a Scalar
        /// </summary>
        private string GetScalarValue()
        {
            return this.Values.ElementAt(this.IndexStartInParameterTypeComponent);
        }
    }
}