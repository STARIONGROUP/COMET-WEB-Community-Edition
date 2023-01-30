// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="LoginTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Components.Shared
{
    using System.Threading.Tasks;

    using Bunit;

    using COMETwebapp.Components.Shared;
    using COMETwebapp.Enumerations;
    using COMETwebapp.Model.DTO;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.ViewModels.Components.Shared;

    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class LoginTestFixture
    {
        private ILoginViewModel viewModel;
        private TestContext context;
        private Mock<IAuthenticationService> authenticationService;

        [SetUp]
        public void Setup()
        {
            this.authenticationService = new Mock<IAuthenticationService>();
            this.context = new TestContext();
            this.viewModel = new LoginViewModel(this.authenticationService.Object);
            this.context.Services.AddSingleton(this.viewModel);
            this.context.ConfigureDevExpressBlazor();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyPerformLogin()
        {
            var renderer = this.context.RenderComponent<Login>();
            var editForm = renderer.FindComponent<EditForm>();

            this.authenticationService.Setup(x => x.Login(It.IsAny<AuthenticationDto>()))
                .ReturnsAsync(AuthenticationStateKind.ServerFail);

            await renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);
            
            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.LoginButtonDisplayText, Is.EqualTo("Retry"));
                Assert.That(renderer.Instance.ErrorMessage, Is.Not.Null);
            });

            this.authenticationService.Setup(x => x.Login(It.IsAny<AuthenticationDto>()))
                .ReturnsAsync(AuthenticationStateKind.Fail);

            await renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.LoginButtonDisplayText, Is.EqualTo("Retry"));
                Assert.That(renderer.Instance.ErrorMessage, Is.Not.Null);
            });

            this.authenticationService.Setup(x => x.Login(It.IsAny<AuthenticationDto>()))
                .ReturnsAsync(AuthenticationStateKind.Success);

            this.viewModel.AuthenticationDto.SourceAddress = "http://localhost.com";
            this.viewModel.AuthenticationDto.UserName = "user";
            this.viewModel.AuthenticationDto.Password = "user1";

            await renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.LoginButtonDisplayText, Is.EqualTo("Connecting"));
                Assert.That(renderer.Instance.ErrorMessage, Is.Empty);
            });
        }
    }
}
