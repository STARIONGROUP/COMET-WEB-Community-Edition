// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Callback.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2025 Starion Group S.A.
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

namespace COMET.Web.Common.Pages
{
    using CDP4DalCommon.Authentication;

    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Provides callback supports authentication callback
    /// </summary>
    public partial class Callback
    {
        /// <summary>
        /// Gets or sets the injected <see cref="IAuthenticationService" />
        /// </summary>
        [Inject]
        public IAuthenticationService AuthenticationService { get; set; }
        
        /// <summary>
        /// Gets or sets the injected <see cref="IConfigurationService" />
        /// </summary>
        [Inject]
        public IConfigurationService ConfigurationService { get; set; }
        
        /// <summary>
        /// Gets or sets the injected <see cref="NavigationManager"/>
        /// </summary>
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        
        /// <summary>
        /// Gets or sets the Aceess code provides by the authentication provider
        /// </summary>
        [SupplyParameterFromQuery(Name = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the session state provides by the authentication provider
        /// </summary>
        [SupplyParameterFromQuery(Name = "session_state")]
        public string SessionState { get; set; }

        /// <summary>
        /// Gets or sets the Issuer information provided by the authentication provider
        /// </summary>
        [SupplyParameterFromQuery(Name = "iss")]
        public string Iss { get; set; }

        /// <summary>
        /// Method invoked after each time the component has been rendered interactively and the UI has finished
        /// updating (for example, after elements have been added to the browser DOM). Any <see cref="T:Microsoft.AspNetCore.Components.ElementReference" />
        /// fields will be populated by the time this runs.
        /// This method is not invoked during prerendering or server-side rendering, because those processes
        /// are not attached to any live browser DOM and are already complete before the DOM is updated.
        /// Note that the component does not automatically re-render after the completion of any returned <see cref="T:System.Threading.Tasks.Task" />,
        /// because that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            
            if (string.IsNullOrEmpty(this.Code))
            {
                this.NavigationManager.NavigateTo("/");
                return;
            }

            var serverUrl = await this.AuthenticationService.RetrieveLastUsedServerUrlAsync();

            if (string.IsNullOrEmpty(serverUrl))
            {
                this.NavigationManager.NavigateTo("/");
                return;
            }
            
            var possibleSchemes = await this.AuthenticationService.RequestAvailableAuthenticationSchemeAsync(serverUrl);

            if (possibleSchemes.IsFailed || !possibleSchemes.Value.Schemes.Contains(AuthenticationSchemeKind.ExternalJwtBearer))
            {
                this.NavigationManager.NavigateTo("/");
                return;
            }
            
            await this.AuthenticationService.ExchangeOpenIdConnectCode(this.Code, possibleSchemes.Value, $"{this.NavigationManager.BaseUri.TrimEnd('/')}/callback",
                this.ConfigurationService.ServerConfiguration.ExternalAuthorizationClientSecret);

            this.NavigationManager.NavigateTo("/");
        }
    }
}
