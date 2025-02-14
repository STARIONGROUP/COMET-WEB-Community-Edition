// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="LoginViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.ViewModels.Components
{
    using CDP4DalCommon.Authentication;

    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components;

    using FluentResults;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class LoginViewModelTestFixture
    {
        private LoginViewModel loginViewModel;
        private Mock<IAuthenticationService> authenticationService;
        private Mock<IConfigurationService> configurationService;

        [SetUp]
        public void Setup()
        {
            this.authenticationService = new Mock<IAuthenticationService>();
            this.configurationService = new Mock<IConfigurationService>();
            var serverConfiguration = new ServerConfiguration();
            
            this.configurationService.Setup(x => x.ServerConfiguration).Returns(serverConfiguration);
            
            this.loginViewModel = new LoginViewModel(this.authenticationService.Object, this.configurationService.Object);
        }

        [Test]
        public async Task VerifyRequestAvailableAuthenticationScheme()
        {
            this.loginViewModel.AuthenticationDto.SourceAddress = "http://localhost:5000";

            Assert.That(this.loginViewModel.AuthenticationSchemeResponseResult, Is.Null);
            
            this.authenticationService.Setup(x => x.RequestAvailableAuthenticationScheme(this.loginViewModel.AuthenticationDto.SourceAddress, false))
                .ReturnsAsync(Result.Fail("Unreachable server"));

            await this.loginViewModel.RequestAvailableAuthenticationScheme();

            Assert.Multiple(() =>
            {
                Assert.That(this.loginViewModel.AuthenticationSchemeResponseResult, Is.Not.Null);
                Assert.That(this.loginViewModel.AuthenticationSchemeResponseResult.IsSuccess, Is.False);
            });
            
            this.authenticationService.Setup(x => x.RequestAvailableAuthenticationScheme(this.loginViewModel.AuthenticationDto.SourceAddress, false))
                .ReturnsAsync(Result.Ok(new AuthenticationSchemeResponse()));
            
            await this.loginViewModel.RequestAvailableAuthenticationScheme();

            Assert.Multiple(() =>
            {
                Assert.That(this.loginViewModel.AuthenticationSchemeResponseResult, Is.Not.Null);
                Assert.That(this.loginViewModel.AuthenticationSchemeResponseResult.IsSuccess, Is.True);
            });
            
            this.loginViewModel.AuthenticationDto.SourceAddress = "http://localhost:50";
            this.configurationService.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration(){ServerAddress = "http://localhost:5000"});
            
            await this.loginViewModel.RequestAvailableAuthenticationScheme();

            Assert.Multiple(() =>
            {
                Assert.That(this.loginViewModel.AuthenticationSchemeResponseResult, Is.Not.Null);
                Assert.That(this.loginViewModel.AuthenticationSchemeResponseResult.IsSuccess, Is.True);
            });
        }
    }
}
