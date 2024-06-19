// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ApplicationBase.razor.cs" company="Starion Group S.A.">
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
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.WebUtilities;

    using ReactiveUI;

    /// <summary>
    /// Base component for any application
    /// </summary>
    /// <typeparam name="TViewModel">An <see cref="IApplicationBaseViewModel" /></typeparam>
    public abstract partial class ApplicationBase<TViewModel>
    {
        /// <summary>
        /// Gets the effective <typeparamref name="TViewModel" />
        /// </summary>
        public TViewModel ViewModel { get; private set; }

        /// <summary>
        /// Gets or sets the injected <typeparamref name="TViewModel" />
        /// </summary>
        [Inject]
        public TViewModel InjectedViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <typeparamref name="TViewModel" /> passed as Parameter
        /// </summary>
        [Parameter]
        public TViewModel ParameterizedViewModel { get; set; }

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
        /// Gets or sets the number of required URL Parameters
        /// </summary>
        protected int NumberOfUrlRequiredParameters { get; set; } = 1;

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (this.ViewModel != null && this.ParameterizedViewModel == null)
            {
                return;
            }

            this.ViewModel = this.ParameterizedViewModel ?? this.InjectedViewModel;

            if (this.ParameterizedViewModel != null && this.InjectedViewModel != null)
            {
                this.InjectedViewModel.Dispose();
                this.InjectedViewModel = default;
            }

            this.OnViewModelAssigned();
        }

        /// <summary>
        /// Handles the post-assignement flow of the <see cref="ViewModel" /> property
        /// </summary>
        protected virtual void OnViewModelAssigned()
        {
            this.Disposables.Add(this.ViewModel);

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsLoading)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
            
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

            if (!string.IsNullOrEmpty(this.ConfigurationService.ServerConfiguration.ServerAddress))
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
