// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ApplicationTemplate.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMET.Web.Common.Components.Applications
{
    using System.ComponentModel;

    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Shared component that will englobe all applications
    /// </summary>
    /// <typeparam name="TViewModel">Any <see cref="IApplicationTemplateViewModel"/></typeparam>
    public partial class ApplicationTemplate<TViewModel>
    {
        /// <summary>
        /// Body of the application
        /// </summary>
        [Parameter]
        public RenderFragment Body { get; set; }

        /// <summary>
        /// Gets or Sets the <typeparamref name="TViewModel" />
        /// </summary>
        [Inject]
        public TViewModel ViewModel { get; set; }

        /// <summary>
        /// The <see cref="NavigationManager" />
        /// </summary>
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        /// <summary>
        /// The <see cref="IConfigurationService" />
        /// </summary>
        [Inject]
        public IConfigurationService ConfigurationService { get; set; }

        /// <summary>
        /// Set URL parameters based on the current context
        /// </summary>
        /// <param name="currentOptions">A <see cref="Dictionary{TKey,TValue}" /> of URL parameters</param>
        protected virtual void SetUrlParameters(Dictionary<string, string> currentOptions)
        {
            if (string.IsNullOrEmpty(this.ConfigurationService.ServerConfiguration.ServerAddress))
            {
                currentOptions[QueryKeys.ServerKey] = this.ViewModel.SessionService.Session.DataSourceUri;
            }
        }
    }
}
