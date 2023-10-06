// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="LoginTestFixture.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Tests.Components
{
    using Bunit;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Model.DTO;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components;

    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.AspNetCore.Components.Web;
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
        private Mock<IConfigurationService> serverConnectionService;

        [SetUp]
        public void Setup()
        {
            this.authenticationService = new Mock<IAuthenticationService>();
            this.serverConnectionService = new Mock<IConfigurationService>();
            this.serverConnectionService.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration { ServerAddress = "http://localhost.com" });
            this.context = new TestContext();
            this.viewModel = new LoginViewModel(this.authenticationService.Object, this.serverConnectionService.Object);
            this.context.Services.AddSingleton(this.viewModel);
            this.context.ConfigureDevExpressBlazor();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyFocusingAndBluring()
        {
            var renderer = this.context.RenderComponent<Login>();

            Assert.That(renderer.Instance.FieldsFocusedStatus, Is.EqualTo(new Dictionary<string, bool>()
            {
                { "SourceAddress", false },
                { "UserName", false },
                { "Password", false }
            }));

            const string fieldToFocusOn = "UserName";
            Assert.That(renderer.Instance.FieldsFocusedStatus[fieldToFocusOn], Is.False);
            renderer.Instance.HandleFieldFocus(fieldToFocusOn);
            
            Assert.Multiple(()=>
            {
                foreach (var fieldStatus in renderer.Instance.FieldsFocusedStatus)
                {
                    Assert.That(fieldStatus.Value, fieldStatus.Key == fieldToFocusOn ? Is.True : Is.False);
                }
            });

            renderer.Instance.HandleFieldBlur(fieldToFocusOn);

            Assert.Multiple(() =>
            {
                foreach (var fieldStatus in renderer.Instance.FieldsFocusedStatus)
                {
                    Assert.That(fieldStatus.Value, Is.False);
                }
            });
        }

        [Test]
        public async Task VerifyPerformLogin()
        {
            var renderer = this.context.RenderComponent<Login>();
            var editForm = renderer.FindComponent<EditForm>();

            this.authenticationService.Setup(x => x.Login(It.IsAny<AuthenticationDto>()))
                .ReturnsAsync(AuthenticationStateKind.ServerFail);

            Assert.That(renderer.Instance.FieldsFocusedStatus, Is.EqualTo(new Dictionary<string, bool>()
            {
                { "SourceAddress", false },
                { "UserName", false },
                { "Password", false }
            }));

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
