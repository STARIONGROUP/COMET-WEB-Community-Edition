﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ApplicationTemplate.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMET.Web.Common.Components.Applications
{
    using COMET.Web.Common.Extensions;

    using Microsoft.AspNetCore.WebUtilities;

    /// <summary>
	/// Shared component that will englobe any applications that on need to be connected to a 10-25 datasource
	/// </summary>
	public partial class ApplicationTemplate
	{
		/// <summary>
		/// Method invoked when the component is ready to start, having received its
		/// initial parameters from its parent in the render tree.
		/// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.SetCorrectUrl();
        }

        /// <summary>
        /// Sets the correct url based on the context
        /// <para>If the current url already has all target options, it doesnt navigate</para>
        /// </summary>
        internal void SetCorrectUrl()
        {
            var urlPathAndQueries = this.NavigationManager.Uri.Replace(this.NavigationManager.BaseUri, string.Empty);
            var urlPage = urlPathAndQueries.Split('?')[0];
            var currentOptions = this.NavigationManager.Uri.GetParametersFromUrl();
            this.SetUrlParameters(currentOptions);
            var targetOptions = new Dictionary<string, string>();

            foreach (var currentOption in currentOptions.Where(x => !string.IsNullOrEmpty(x.Value)))
            {
                targetOptions[currentOption.Key] = currentOption.Value;
            }

            var targetUrl = QueryHelpers.AddQueryString(urlPage, targetOptions);

            if (urlPathAndQueries == targetUrl)
            {
                return;
            }

            this.NavigationManager.NavigateTo(targetUrl);
        }
    }
}
