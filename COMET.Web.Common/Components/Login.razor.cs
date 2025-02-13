// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Login.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMET.Web.Common.Components
{
    using System.Web;

    using CDP4Dal.DAL;

    using CDP4DalCommon.Authentication;

    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// This component enables the user to login against a COMET Server
    /// </summary>
    public partial class Login
    {
        /// <summary>
        /// Provides <see cref="AuthenticationSchemeKind" /> that requires userName and password inputs
        /// </summary>
        private static AuthenticationSchemeKind[] SchemesWithUserNameAndPassword = [AuthenticationSchemeKind.Basic, AuthenticationSchemeKind.LocalJwtBearer];

        /// <summary>
        /// The <see cref="ILoginViewModel" />
        /// </summary>
        [Inject]
        public ILoginViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the injected <see cref="NavigationManager"/>
        /// </summary>
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        /// <summary>
        /// Gets or sets the injected <see cref="IAuthenticationService" />
        /// </summary>
        [Inject]
        public IAuthenticationService AuthenticationService { get; set; }

        /// <summary>
        /// The url of the requested server
        /// </summary>
        [Parameter]
        public string RequestedServer { get; set; }

        /// <summary>
        /// The label for the username input field
        /// </summary>
        [Parameter]
        public string UsernameLabel { get; set; } = "UserName:";

        /// <summary>
        /// The label for the password input field
        /// </summary>
        [Parameter]
        public string PasswordLabel { get; set; } = "Password:";

        /// <summary>
        /// The label for the full trust checkbox field
        /// </summary>
        [Parameter]
        public string FullTrustLabel { get; set; } = "Full Trust:";
        
        /// <summary>
        /// The text of the login button
        /// </summary>
        public string LoginButtonDisplayText { get; private set; }

        /// <summary>
        /// The error messages to display after a login failure
        /// </summary>
        public IEnumerable<string> ErrorMessages { get; private set; }

        /// <summary>
        /// Value indicating if the login button is enabled or not
        /// </summary>
        public bool LoginEnabled { get; set; } = true;

        /// <summary>
        /// The dictionary of focus status from the form fields
        /// </summary>
        public Dictionary<string, bool> FieldsFocusedStatus { get; private set; }

        /// <summary>
        /// Gets the server configuration
        /// </summary>
        private ServerConfiguration ServerConfiguration => this.ViewModel.ServerConnectionService.ServerConfiguration;

        /// <summary>
        /// Asserts that the we attempt to restore a previous session
        /// </summary>
        private bool checkingRestoreSession = true;

        /// <summary>
        /// Handles the focus event of the given fieldName
        /// </summary>
        /// <param name="fieldName">Form field name, as indexed in <see cref="FieldsFocusedStatus" /></param>
        public void HandleFieldFocus(string fieldName)
        {
            this.FieldsFocusedStatus[fieldName] = true; // Set the field as focused
        }

        /// <summary>
        /// Handles the blur event of the given fieldName
        /// </summary>
        /// <param name="fieldName">Form field name, as indexed in <see cref="FieldsFocusedStatus" /></param>
        public void HandleFieldBlur(string fieldName)
        {
            this.FieldsFocusedStatus[fieldName] = false; // Set the field as not focused when it loses focus
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.FieldsFocusedStatus = new Dictionary<string, bool>
            {
                { "SourceAddress", false },
                { "UserName", false },
                { "Password", false }
            };

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.AuthenticationResult,
                x => x.ViewModel.IsLoading,
                x => x.ViewModel.AuthenticationSchemeResponseResult,
                x=> x.ViewModel.AuthenticationDto
            ).Subscribe(_ => this.ComputeDisplayProperties()));
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// Override this method if you will perform an asynchronous operation and
        /// want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing any asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            if (!string.IsNullOrEmpty(this.ServerConfiguration.ServerAddress) && this.ServerConfiguration.AllowMultipleStepsAuthentication)
            {
                await this.ViewModel.RequestAvailableAuthenticationScheme();
            }
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            this.ViewModel.AuthenticationDto.SourceAddress = this.RequestedServer;
        }

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

            if (firstRender)
            {
                await this.AuthenticationService.TryRestoreLastSession();
                this.checkingRestoreSession = false;
                await this.InvokeAsync(this.StateHasChanged);
            }
        }

        /// <summary>
        /// Compute display properties based on the <see cref="ILoginViewModel.AuthenticationResult" />
        /// </summary>
        private void ComputeDisplayProperties()
        {
            this.LoginButtonDisplayText = this.ViewModel.IsLoading switch
            {
                true => "Connecting",
                false when this.ViewModel.AuthenticationResult.IsSuccess => "Connect",
                false when this.ViewModel.AuthenticationResult.IsFailed => "Retry",
                _ => this.LoginButtonDisplayText
            };

            var errors = new List<string>();

            if (this.ViewModel.AuthenticationResult != null)
            {
                errors.AddRange(this.ViewModel.AuthenticationResult.Errors.Select(x => x.Message));
            }

            if (this.ViewModel.AuthenticationSchemeResponseResult != null)
            {
                errors.AddRange(this.ViewModel.AuthenticationSchemeResponseResult.Errors.Select(x => x.Message));
            }

            this.ErrorMessages = errors;
            this.ViewModel.AuthenticationDto.ShouldValidateCredentials = this.RequiresUserNameAndPasswordInput();
            this.InvokeAsync(this.StateHasChanged);
        }

        /// <summary>
        /// Executes the login process
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task ExecuteLogin()
        {
            this.LoginEnabled = false;

            if (this.ServerConfiguration.AllowMultipleStepsAuthentication)
            {
                if (this.ViewModel.AuthenticationSchemeResponseResult.Value.Schemes.Intersect(SchemesWithUserNameAndPassword).Any())
                {
                    var authenticationInformation = new AuthenticationInformation(this.ViewModel.AuthenticationDto.UserName, this.ViewModel.AuthenticationDto.Password);

                    var scheme = this.ViewModel.AuthenticationSchemeResponseResult.Value.Schemes.Contains(AuthenticationSchemeKind.LocalJwtBearer)
                        ? AuthenticationSchemeKind.LocalJwtBearer
                        : AuthenticationSchemeKind.Basic;

                    await this.ViewModel.ExecuteLogin(scheme, authenticationInformation);
                }
            }
            else
            {
                await this.ViewModel.ExecuteLogin();
            }

            this.LoginEnabled = true;
        }

        /// <summary>
        /// Asserts that the user requires to input username and password information
        /// </summary>
        /// <returns>Asserts that the user have to provides userName and password information</returns>
        private bool RequiresUserNameAndPasswordInput()
        {
            if (!this.ServerConfiguration.AllowMultipleStepsAuthentication)
            {
                return true;
            }

            if (this.ViewModel.AuthenticationSchemeResponseResult == null || this.ViewModel.AuthenticationSchemeResponseResult.IsFailed)
            {
                return false;
            }

            return this.ViewModel.AuthenticationSchemeResponseResult.Value.Schemes.Intersect(SchemesWithUserNameAndPassword).Any();
        }

        /// <summary>
        /// Asserts that the user requires to provide server information (url and FullTrust)
        /// </summary>
        /// <returns>The assert</returns>
        private bool ShouldProvideServerInformationInput()
        {
            return this.ViewModel.AuthenticationSchemeResponseResult == null
                   || this.ViewModel.AuthenticationSchemeResponseResult.IsFailed;
        }

        /// <summary>
        /// Provides server information to the session and request supported authentication scheme
        /// </summary>
        /// <returns>An awaitable <see cref="Task" /></returns>
        private Task ProvideServerInformation()
        {
            return this.ViewModel.RequestAvailableAuthenticationScheme();
        }

        /// <summary>
        /// Handle the valid submission
        /// </summary>
        /// <returns>An awaitable <see cref="Task" /></returns>
        private async Task HandleSubmit()
        {
            if (this.ServerConfiguration.AllowMultipleStepsAuthentication && this.ShouldProvideServerInformationInput())
            {
                await this.ProvideServerInformation();

                if (this.ViewModel.AuthenticationSchemeResponseResult.IsSuccess && this.ViewModel.AuthenticationSchemeResponseResult.Value.Schemes.Contains(AuthenticationSchemeKind.ExternalJwtBearer))
                {
                    var uri = new UriBuilder($"{this.ViewModel.AuthenticationSchemeResponseResult.Value.Authority}/protocol/openid-connect/auth");
                    var queryParameters = HttpUtility.ParseQueryString(uri.Query);
                    queryParameters["response_type"] = "code";
                    queryParameters["client_id"] = this.ViewModel.AuthenticationSchemeResponseResult.Value.ClientId;
                    queryParameters["redirect_uri"] = $"{this.NavigationManager.BaseUri.TrimEnd('/')}/callback";
                    uri.Query = string.Join("&", queryParameters.AllKeys.Select(key => $"{key}={queryParameters[key]!}"));

                    this.NavigationManager.NavigateTo(uri.ToString(), forceLoad: true);
                }
            }
            else
            {
                await this.ExecuteLogin();
            }
        }

        /// <summary>
        /// Go back to the first step of the authentication process, if the MultipleStep is supported
        /// </summary>
        private void HandleBack()
        {
            this.ViewModel.AuthenticationSchemeResponseResult = null;
            this.ViewModel.AuthenticationDto.UserName = null;
            this.ViewModel.AuthenticationDto.Password = null;
        }
    }
}
