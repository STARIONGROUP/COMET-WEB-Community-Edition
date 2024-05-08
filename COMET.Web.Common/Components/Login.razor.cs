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
    using COMET.Web.Common.ViewModels.Components;

    using Microsoft.AspNetCore.Components;

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
        public string FullTrustLabel { get; set; } = "FullTrust:";

        /// <summary>
        /// The condition to check if the full trust checkbox should be visible or not
        /// </summary>
        [Parameter]
        public bool FullTrustCheckboxVisible { get; set; } = false;

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

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.AuthenticationResult).Subscribe(_ => this.ComputeDisplayProperties()));
            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsLoading).Subscribe(_ => this.ComputeDisplayProperties()));
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

            this.ErrorMessages = this.ViewModel.AuthenticationResult.Errors.Select(x => x.Message);

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
