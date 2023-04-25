// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IndexComponent.razor.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Components
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.RegistrationService;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components;

    using Microsoft.AspNetCore.Components;

    public partial class IndexComponent
    {
        /// <summary>
        /// The <see cref="Guid" /> of the requested <see cref="DomainOfExpertise" />
        /// </summary>
        private Guid requestedDomainOfExpertise;

        /// <summary>
        /// The <see cref="Guid" /> of the requested <see cref="Iteration" />
        /// </summary>
        private Guid requestedIteration;

        /// <summary>
        /// The <see cref="Guid" /> of the requested <see cref="EngineeringModel" />
        /// </summary>
        private Guid requestedModel;

        /// <summary>
        /// The value that has been requested
        /// </summary>
        private string requestedServer;

        /// <summary>
        /// The <see cref="IIndexViewModel" />
        /// </summary>
        [Inject]
        public IIndexViewModel ViewModel { get; set; }

        /// <summary>
        /// The redirection url
        /// </summary>
        [Parameter]
        public string Redirect { get; set; }

        /// <summary>
        /// The <see cref="NavigationManager" />
        /// </summary>
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.ViewModel.SessionService.OpenIterations.CountChanged
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (!string.IsNullOrEmpty(this.Redirect))
            {
                var options = this.Redirect.GetParametersFromUrl();

                if (options.TryGetValue(QueryKeys.ServerKey, out var server))
                {
                    this.requestedServer = server;
                }

                if (options.TryGetValue(QueryKeys.ModelKey, out var model))
                {
                    this.requestedModel = model.FromShortGuid();
                }

                if (options.TryGetValue(QueryKeys.DomainKey, out var domain))
                {
                    this.requestedDomainOfExpertise = domain.FromShortGuid();
                }

                if (options.TryGetValue(QueryKeys.IterationKey, out var iteration))
                {
                    this.requestedIteration = iteration.FromShortGuid();
                }
            }
        }

        /// <summary>
        /// Redirects to the correct page after opening a model
        /// </summary>
        private void RedirectTo()
        {
            if (!string.IsNullOrEmpty(this.Redirect))
            {
                this.NavigationManager.NavigateTo(this.Redirect);
            }
        }
    }
}
