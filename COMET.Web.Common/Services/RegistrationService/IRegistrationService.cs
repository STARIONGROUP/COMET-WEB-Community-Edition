// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IRegistrationService.cs" company="RHEA System S.A.">
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

    /// <summary>
    /// The <see cref="IRegistrationService" /> provides capabilities to register <see cref="Application" /> and
    /// <see cref="Assembly" /> that should be registered inside a COMET Web Application
    /// </summary>
    public interface IRegistrationService
    {
        /// <summary>
        /// Gets the <see cref="IReadOnlyList{T}"/> of registered <see cref="Application"/>
        /// </summary>
        IReadOnlyList<Application> RegisteredApplications { get; }

        /// <summary>
        /// Gets the <see cref="IReadOnlyList{T}" /> of registered <see cref="Assembly" />
        /// </summary>
        IReadOnlyList<Assembly> RegisteredAssemblies { get; }

        /// <summary>
        /// Gets the <see cref="IReadOnlyList{T}" /> of registered <see cref="Type" />
        /// </summary>
        IReadOnlyList<Type> RegisteredAuthorizedMenuEntries { get; }

        /// <summary>
        /// Gets the custom header <see cref="Type"/>, if applicable
        /// </summary>
        Type CustomHeader { get; }
    }
}
