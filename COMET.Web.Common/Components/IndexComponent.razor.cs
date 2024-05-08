// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IndexComponent.razor.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Components
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Common component that can handle the home page of the application
    /// </summary>
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
        /// The condition to check if the full trust checkbox should be visible or not
        /// </summary>
        [Parameter]
        public bool FullTrustCheckboxVisible { get; set; }

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

            if (string.IsNullOrEmpty(this.Redirect))
            {
                return;
            }

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
