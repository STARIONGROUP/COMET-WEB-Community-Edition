// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleThingApplicationTemplate.razor.cs" company="RHEA System S.A.">
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
    using System.Reactive.Linq;

    using CDP4Common.CommonData;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.WebUtilities;

    using ReactiveUI;

    /// <summary>
    /// Shared component that will englobe all applications where only one <see cref="Thing" /> needs to be selected
    /// </summary>
    /// <typeparam name="TThing">Any <see cref="Thing" /></typeparam>
    /// <typeparam name="TViewModel">Any <see cref="ISingleThingApplicationTemplateViewModel{TThing}" /></typeparam>
    public abstract partial class SingleThingApplicationTemplate<TThing, TViewModel>
    {
        /// <summary>
        /// A property to show the active model
        /// </summary>
        [Parameter]
        public bool ShowActiveModel { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnSelectionMode)
                .Merge(this.ViewModel.SessionService.OpenIterations.CountChanged.Select(_ => true))
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.SelectedThing)
                .Subscribe(_ => this.SetCorrectUrl()));
        }

        /// <summary>
        /// Sets the correct url based on the selected <typeparamref name="TThing" />
        /// <para>If the current url already has all target options, it doesnt navigate</para>
        /// </summary>
        internal void SetCorrectUrl()
        {
            var urlPathAndQueries = this.NavigationManager.Uri.Replace(this.NavigationManager.BaseUri, string.Empty);
            var urlPage = urlPathAndQueries.Split('?')[0];
            var currentOptions = this.NavigationManager.Uri.GetParametersFromUrl();

            if (this.ViewModel.SelectedThing != null)
            {
                this.SetUrlParameters(currentOptions);
            }
            else
            {
                currentOptions.Clear();
            }

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
