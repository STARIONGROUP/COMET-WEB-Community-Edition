// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Login.razor.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Components.Shared
{
    using COMETwebapp.Enumerations;
    using COMETwebapp.ViewModels.Components.Shared.Login;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// This component enables the user to login against a COMET Server
    /// </summary>
    public partial class Login
    {
        /// <summary>
        /// The <see cref="ILoginViewModel"/>
        /// </summary>
        [Inject]
        public ILoginViewModel ViewModel { get; set; }

        /// <summary>
        /// The text of the login button
        /// </summary>
        public string LoginButtonDisplayText { get; private set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.AuthenticationState)
                .Subscribe(_ =>this.ComputeDisplayProperties()));
        }

        /// <summary>
        /// Compute display properties based on the <see cref="ILoginViewModel.AuthenticationState"/>
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
        /// An error message to display after a login failure
        /// </summary>
        public string ErrorMessage { get; private set; }
    }
}
