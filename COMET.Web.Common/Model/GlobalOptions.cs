// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="GlobalOptions.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Model
{
    using System.Reflection;

    using COMET.Web.Common.Shared;
    using COMET.Web.Common.Shared.TopMenuEntry;

    /// <summary>
    /// Handles the global options used to register this Library
    /// </summary>
    public class GlobalOptions
    {
        /// <summary>
        /// A collection of <see cref="Application" />, used to register <see cref="Application" />
        /// </summary>
        public List<Application> Applications { get; set; } = new();

        /// <summary>
        /// A collection of <see cref="Assembly" /> that should be used to resolve pages
        /// </summary>
        public List<Assembly> AdditionalAssemblies { get; set; } = new();

        /// <summary>
        /// A collection of <see cref="Type" /> that should be added inside the Top Menu.
        /// </summary>
        /// <remarks>Only subtypes of <see cref="MenuEntryBase" /> are compatible</remarks>
        public List<Type> AdditionalMenuEntries { get; set; } = new();

        /// <summary>
        /// Gets or sets the <see cref="Type"/> to have a custom header title
        /// </summary>
        public Type CustomHeaderTitle { get; set; }

        /// <summary>
        /// Gets or sets the configuration file used for the names in the app.
        /// </summary>
        public string JsonConfigurationFile { get; set; }

        /// <summary>
        /// Gets or sets the configuration file used to set server address.
        /// </summary>
        public string ServerConfigurationFile { get; set; }

        /// <summary>
        /// Defines the <see cref="Type" /> that should be used as MainLayout
        /// </summary>
        public Type MainLayoutType { get; set; } = typeof(MainLayout);
    }
}
 