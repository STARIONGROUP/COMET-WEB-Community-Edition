// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RegistrationService.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Services.RegistrationService
{
    using System.Reflection;

    using COMET.Web.Common.Model;

    using Microsoft.Extensions.Options;

    /// <summary>
    /// The <see cref="RegistrationService" /> provides capabilities to register <see cref="Application" /> and
    /// <see cref="Assembly" /> that should be registered inside a COMET Web Application
    /// </summary>
    internal class RegistrationService : IRegistrationService
    {
        /// <summary>
        /// A collection of registered <see cref="Application" />
        /// </summary>
        private readonly List<Application> registeredApplications = new();

        /// <summary>
        /// A collection of <see cref="Assembly" /> that should be used to resolve pages
        /// </summary>
        private readonly List<Assembly> registeredAssemblies = new();

        /// <summary>
        /// A collection of <see cref="Type" /> that should be used on the TopMenu
        /// </summary>
        private readonly List<Type> registeredAuthorizedTopMenuEntries = new();

        /// <summary>
        /// Initializes a new <see cref="RegistrationService" />
        /// </summary>
        /// <param name="options">The for <see cref="GlobalOptions" /></param>
        public RegistrationService(IOptions<GlobalOptions> options)
        {
            this.registeredApplications.AddRange(options.Value.Applications);
            this.registeredAssemblies.AddRange(options.Value.AdditionalAssemblies);
            this.registeredAuthorizedTopMenuEntries.AddRange(options.Value.AdditionalMenuEntries);
            this.CustomHeader = options.Value.CustomHeaderTitle;
            this.MainLayoutType = options.Value.MainLayoutType;
        }

        /// <summary>
        /// Gets the <see cref="Type" /> to use as MainLayout for the application
        /// </summary>
        public Type MainLayoutType { get; }

        /// <summary>
        /// Gets the <see cref="IReadOnlyList{T}" /> of registered <see cref="Assembly" />
        /// </summary>
        public IReadOnlyList<Assembly> RegisteredAssemblies => this.registeredAssemblies.AsReadOnly();

        /// <summary>
        /// Gets the <see cref="IReadOnlyList{T}" /> of registered <see cref="Application" />
        /// </summary>
        public IReadOnlyList<Application> RegisteredApplications => this.registeredApplications.AsReadOnly();

        /// <summary>
        /// Gets the <see cref="IReadOnlyList{T}" /> of registered <see cref="Type" />
        /// </summary>
        public IReadOnlyList<Type> RegisteredAuthorizedMenuEntries => this.registeredAuthorizedTopMenuEntries.AsReadOnly();

        /// <summary>
        /// Gets the custom header <see cref="Type" />, if applicable
        /// </summary>
        public Type CustomHeader { get; }
    }
}
