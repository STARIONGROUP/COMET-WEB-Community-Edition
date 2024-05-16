// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ApplicationBase.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Components.Applications
{
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Configuration;

    using ReactiveUI;

    /// <summary>
    /// Base component for any application
    /// </summary>
    /// <typeparam name="TViewModel">An <see cref="IApplicationBaseViewModel" /></typeparam>
    public abstract partial class ApplicationBase<TViewModel>
    {
        /// <summary>
        /// The <typeparamref name="TViewModel" />
        /// </summary>
        [Inject]
        public TViewModel ViewModel { get; set; }

        /// <summary>
        /// The <see cref="NavigationManager" />
        /// </summary>
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IConfiguration" />
        /// </summary>
        [Inject]
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the number of required URL Parameters
        /// </summary>
        protected int NumberOfUrlRequiredParameters { get; set; } = 1;

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.ViewModel);

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsLoading)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (this.ViewModel.HasSetInitialValuesOnce)
            {
                return;
            }

            this.InitializeValues(this.NavigationManager.Uri.GetParametersFromUrl());
            this.ViewModel.HasSetInitialValuesOnce = true;
        }

        /// <summary>
        /// Sets the URL parameters that are required for this application
        /// </summary>
        /// <param name="currentOptions">A <see cref="Dictionary{TKey,TValue}" /> of current URL parameters that comes form the URI</param>
        /// <param name="urlParameters">A <see cref="Dictionary{TKey,TValue}" /> of parameters that have to be included</param>
        protected virtual void SetUrlParameters(Dictionary<string, string> currentOptions, Dictionary<string, string> urlParameters)
        {
            if (currentOptions.TryGetValue(QueryKeys.ServerKey, out var serverValue))
            {
                urlParameters[QueryKeys.ServerKey] = serverValue;
            }
        }

        /// <summary>
        /// Updates the url of the <see cref="Microsoft.AspNetCore.Components.NavigationManager" /> with the
        /// <paramref name="additionalParameters" />
        /// </summary>
        /// <param name="additionalParameters">A <see cref="Dictionary{TKey,TValue}" /> of additional parameters</param>
        /// <param name="pageName">The name of the ucrrent page</param>
        protected void UpdateUrlWithParameters(Dictionary<string, string> additionalParameters, string pageName)
        {
            var currentOptions = this.NavigationManager.Uri.GetParametersFromUrl();
            var requiredOptions = new Dictionary<string, string>();

            this.SetUrlParameters(currentOptions, requiredOptions);

            if (!string.IsNullOrEmpty(this.Configuration.GetSection(ConfigurationKeys.ServerConfigurationKey).Get<ServerConfiguration>().ServerAddress))
            {
                if (requiredOptions.TryGetValue(QueryKeys.ServerKey, out _) || requiredOptions.Count != this.NumberOfUrlRequiredParameters - 1)
                {
                    return;
                }
            }
            else
            {
                if (requiredOptions.Count != this.NumberOfUrlRequiredParameters)
                {
                    return;
                }
            }

            foreach (var additionalParameter in additionalParameters)
            {
                requiredOptions.Add(additionalParameter.Key, additionalParameter.Value);
            }

            this.NavigationManager.NavigateTo(QueryHelpers.AddQueryString($"/{pageName}", requiredOptions));
        }

        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected abstract void InitializeValues(Dictionary<string, string> parameters);
    }
}
