// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ApplicationMenu.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Shared.TopMenuEntry
{
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.RegistrationService;
    using COMET.Web.Common.Services.StringTableService;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Menu entry to access <see cref="Application" />(s)
    /// </summary>
	public partial class ApplicationMenu
    {
        /// <summary>
        /// Gets or sets the <see cref="IRegistrationService"/>
        /// </summary>
        [Inject]
        internal IRegistrationService RegistrationService { get; set; }

        /// <summary>
        /// The <see cref="IStringTableService"/>
        /// </summary>
        [Inject]
        public IStringTableService ConfigurationService { get; set; }
    }
}
