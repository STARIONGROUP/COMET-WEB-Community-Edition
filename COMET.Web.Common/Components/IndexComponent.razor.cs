// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IndexComponent.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

    using Blazored.SessionStorage;

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
        /// The value identifying the Annex C3 archive option of the connection-kind selector
        /// </summary>
        private const string ArchiveConnectionKind = "archive";

        /// <summary>
        /// The value identifying the default option of the connection-kind selector, connecting to a COMET server
        /// </summary>
        private const string ServerConnectionKind = "server";

        /// <summary>
        /// The connection kind that the user has selected on the landing page, which decides whether the server login or
        /// the Annex C3 archive login is presented. Connecting to a server is the default
        /// </summary>
        private string selectedConnectionKind = ServerConnectionKind;

        /// <summary>
        /// Gets the ways a user can connect, in the order they are offered
        /// </summary>
        private static IReadOnlyList<ConnectionKind> AvailableConnectionKinds { get; } =
        [
            new ConnectionKind(ServerConnectionKind, "Connect to a server"),
            new ConnectionKind(ArchiveConnectionKind, "Open a model archive (read-only)")
        ];

        /// <summary>
        /// The key under which the last selected connection kind is kept in the browser session storage
        /// </summary>
        private const string ConnectionKindKey = "cdp4-comet-connection-kind";

        /// <summary>
        /// The injected <see cref="ISessionStorageService" />, used to remember the selected connection kind across a
        /// page reload
        /// </summary>
        [Inject]
        public ISessionStorageService SessionStorageService { get; set; }

        /// <summary>
        /// Handles the selection of a connection kind by the user, remembering it so a page reload comes back to the
        /// same form
        /// </summary>
        /// <param name="eventArgs">The <see cref="ChangeEventArgs" /> carrying the selected value</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnConnectionKindChanged(ChangeEventArgs eventArgs)
        {
            this.selectedConnectionKind = eventArgs.Value?.ToString() ?? ServerConnectionKind;
            await this.SessionStorageService.SetItemAsync(ConnectionKindKey, this.selectedConnectionKind);
        }

        /// <summary>
        /// Method invoked after each time the component has been rendered. Restores the connection kind that the user
        /// last selected, because a reload starts a new circuit with a fresh component
        /// </summary>
        /// <param name="firstRender">A value indicating whether this is the first render of the component</param>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (!firstRender)
            {
                return;
            }

            var lastConnectionKind = await this.SessionStorageService.GetItemAsync<string>(ConnectionKindKey);

            if (!string.IsNullOrEmpty(lastConnectionKind) && lastConnectionKind != this.selectedConnectionKind)
            {
                this.selectedConnectionKind = lastConnectionKind;
                await this.InvokeAsync(this.StateHasChanged);
            }
        }

        /// <summary>
        /// Represents a way of connecting offered on the landing page
        /// </summary>
        /// <param name="Value">The value that identifies the option</param>
        /// <param name="Name">The text presented to the user</param>
        private sealed record ConnectionKind(string Value, string Name);

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
        /// Gets or sets the value to check if an open iteration is required for accessing the dashboard component
        /// </summary>
        [Parameter]
        public bool OpenIterationRequired { get; set; } = true;

        /// <summary>
        /// Gets or sets the component that will be rendered after the user logs in
        /// </summary>
        /// <remarks>Default value is <see cref="Dashboard" /></remarks>
        [Parameter]
        public Type HomePage { get; set; } = typeof(Dashboard);

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
