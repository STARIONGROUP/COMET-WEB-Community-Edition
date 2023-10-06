// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Login.razor.cs" company="RHEA System S.A.">
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
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.ViewModels.Components;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;

    using ReactiveUI;

    /// <summary>
    /// This component enables the user to login against a COMET Server
    /// </summary>
    public partial class Login
    {
        /// <summary>
        /// The <see cref="ILoginViewModel" />
        /// </summary>
        [Inject]
        public ILoginViewModel ViewModel { get; set; }

        /// <summary>
        /// The url of the requested server
        /// </summary>
        [Parameter]
        public string RequestedServer { get; set; }

        /// <summary>
        /// The text of the login button
        /// </summary>
        public string LoginButtonDisplayText { get; private set; }

        /// <summary>
        /// An error message to display after a login failure
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Value indicating if the login button is enabled or not
        /// </summary>
        public bool LoginEnabled { get; set; } = true;

        /// <summary>
        /// The dictionary of focus status from the fields
        /// </summary>
        public Dictionary<string, bool> FieldsFocusedStatus;


        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.FieldsFocusedStatus = new Dictionary<string, bool>()
            {
                { "SourceAddress", false },
                { "UserName", false },
                { "Password", false }
            };

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.AuthenticationState)
                .Subscribe(_ => this.ComputeDisplayProperties()));
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
        /// Compute display properties based on the <see cref="ILoginViewModel.AuthenticationState" />
        /// </summary>
        private void ComputeDisplayProperties()
        {
            this.LoginButtonDisplayText = this.ViewModel.AuthenticationState switch
            {
                AuthenticationStateKind.None => "Connect",
                AuthenticationStateKind.Authenticating => "Connecting",
                AuthenticationStateKind.ServerFail or AuthenticationStateKind.Fail => "Retry",
                _ => this.LoginButtonDisplayText
            };

            this.ErrorMessage = this.ViewModel.AuthenticationState switch
            {
                AuthenticationStateKind.ServerFail => "The server could not be reached",
                AuthenticationStateKind.Fail => "Login failed.",
                _ => string.Empty
            };

            this.InvokeAsync(this.StateHasChanged);
        }

        /// <summary>
        /// Executes the login process
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task ExecuteLogin()
        {
            this.LoginEnabled = false;
            await this.ViewModel.ExecuteLogin();
            this.LoginEnabled = true;
        }

        /// <summary>
        /// Handles the focus event of the given fieldName
        /// </summary>
        /// <param name="fieldName">Form field name, as indexed in <see cref="FieldsFocusedStatus"/></param>
        public void HandleFieldFocus(string fieldName)
        {
            this.FieldsFocusedStatus[fieldName] = true; // Set the field as focused
        }

        /// <summary>
        /// Handles the blur event of the given fieldName
        /// </summary>
        /// <param name="fieldName">Form field name, as indexed in <see cref="FieldsFocusedStatus"/></param>
        public void HandleFieldBlur(string fieldName)
        {
            this.FieldsFocusedStatus[fieldName] = false; // Set the field as not focused when it loses focus
        }
    }
}
