// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="LoginTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMET.Web.Common.Tests.Components
{
    using Bunit;

    using CDP4Dal.DAL;

    using CDP4DalCommon.Authentication;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Model.DTO;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components;

    using DevExpress.Blazor;

    using FluentResults;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;


    [TestFixture]
    public class LoginTestFixture
    {
        private LoginViewModel viewModel;
        private BunitContext context;
        private Mock<IAuthenticationService> authenticationService;
        private Mock<IConfigurationService> serverConnectionService;
        private ServerConfiguration serverConfiguration;

        [SetUp]
        public void Setup()
        {
            this.authenticationService = new Mock<IAuthenticationService>();
            this.serverConnectionService = new Mock<IConfigurationService>();

            this.serverConfiguration = new ServerConfiguration
            {
                ServerAddress = "http://localhost.com",
                FullTrustConfiguration = new FullTrustConfiguration()
            };

            this.serverConnectionService.Setup(x => x.ServerConfiguration).Returns(this.serverConfiguration);
            this.context = new BunitContext();
            this.viewModel = new LoginViewModel(this.authenticationService.Object, this.serverConnectionService.Object);
            this.context.Services.AddSingleton<ILoginViewModel>(this.viewModel);
            this.context.Services.AddSingleton(this.authenticationService.Object);
            this.context.ConfigureDevExpressBlazor();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyReadinessMarkerIsPublished()
        {
            var renderer = this.context.Render<Login>();
            var form = renderer.Find("#login-form");
            Assert.That(form.GetAttribute("data-app-ready"), Is.EqualTo("true"));
        }

        [Test]
        public async Task VerifyErrorsShown()
        {
            var renderer = this.context.Render<Login>();
            var errorsElement = renderer.Find(".validation-errors");
            var numberOfRequiredFieldsInFirstLoginTry = renderer.Instance.FieldsFocusedStatus.Count - 1;

            Assert.That(errorsElement.InnerHtml, Is.Empty);

            await renderer.Find("button").ClickAsync(new MouseEventArgs());
            Assert.That(errorsElement.ChildElementCount, Is.EqualTo(numberOfRequiredFieldsInFirstLoginTry));

            // Username input field
            await renderer.Find("input").FocusAsync(new FocusEventArgs());

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.FieldsFocusedStatus["UserName"], Is.True);
                Assert.That(errorsElement.ChildElementCount, Is.EqualTo(numberOfRequiredFieldsInFirstLoginTry - 1));
            });

            await renderer.Find("input").BlurAsync(new FocusEventArgs());

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.FieldsFocusedStatus["UserName"], Is.False);
                Assert.That(errorsElement.ChildElementCount, Is.EqualTo(numberOfRequiredFieldsInFirstLoginTry));
            });
        }

        [Test]
        public void VerifyFocusingAndBluring()
        {
            var renderer = this.context.Render<Login>();

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
            var renderer = this.context.Render<Login>();
            var editForm = renderer.FindComponent<EditForm>();

            this.authenticationService.Setup(x => x.Login(It.IsAny<AuthenticationDto>())).ReturnsAsync(Result.Ok);

            Assert.That(renderer.Instance.FieldsFocusedStatus, Is.EqualTo(new Dictionary<string, bool>()
            {
                { "SourceAddress", false },
                { "UserName", false },
                { "Password", false }
            }));

            await renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.LoginButtonDisplayText, Is.EqualTo("Connect"));
                Assert.That(renderer.Instance.ErrorMessages, Is.Not.Null);
            });

            this.authenticationService.Setup(x => x.Login(It.IsAny<AuthenticationDto>())).ReturnsAsync(Result.Fail(["error"]));
            this.viewModel.AuthenticationDto.SourceAddress = "http://localhost.com";
            this.viewModel.AuthenticationDto.UserName = "user";
            this.viewModel.AuthenticationDto.Password = "user1";

            await renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.LoginButtonDisplayText, Is.EqualTo("Retry"));
                Assert.That(renderer.Instance.ErrorMessages, Is.Not.Null);
            });

            renderer.Render();

            this.authenticationService.Setup(x => x.Login(It.IsAny<AuthenticationDto>())).ReturnsAsync(Result.Ok);

            this.viewModel.AuthenticationDto.SourceAddress = "http://localhost.com";
            this.viewModel.AuthenticationDto.UserName = "user";
            this.viewModel.AuthenticationDto.Password = "user1";

            await renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);
            Assert.That(renderer.Instance.ErrorMessages, Is.Empty);
        }

        [Test]
        public void VerifyFullTrustHintIsShownAndSourceAddressNotRequiredWhenPreconfigured()
        {
            var renderer = this.context.Render<Login>();

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Find(".full-trust-hint").TextContent, Is.Not.Empty);
                Assert.That(this.viewModel.AuthenticationDto.ShouldValidateSourceAddress, Is.False);
            });
        }

        [Test]
        public void VerifySourceAddressRequiredWhenNotPreconfigured()
        {
            this.serverConfiguration.ServerAddress = null;

            this.context.Render<Login>();

            Assert.That(this.viewModel.AuthenticationDto.ShouldValidateSourceAddress, Is.True);
        }

        [Test]
        public async Task VerifySourceAddressRequiredMessageIsRendered()
        {
            this.serverConfiguration.ServerAddress = null;

            var renderer = this.context.Render<Login>();

            await renderer.Find("#connectbtn").ClickAsync(new MouseEventArgs());

            Assert.That(renderer.Find(".validation-errors").TextContent, Does.Contain("The Source Address is required."));
        }

        [Test]
        public async Task VerifyMultipleAuthenticationSchemeFlowWithInternalToken()
        {
            this.serverConfiguration.ServerAddress = null;
            this.serverConfiguration.AllowMultipleStepsAuthentication = true;

            var renderer = this.context.Render<Login>();
            var editForm = renderer.FindComponent<EditForm>();

            Assert.That(renderer.FindComponents<DxTextBox>(), Has.Count.EqualTo(1));
            
            const string sourceAddress = "http://localhost:5000"; 
            this.viewModel.AuthenticationDto.SourceAddress = sourceAddress;

            var authenticationSchemeResponse = new AuthenticationSchemeResponse()
            {
                Schemes = [AuthenticationSchemeKind.LocalJwtBearer, AuthenticationSchemeKind.Basic]
            };

            this.authenticationService.Setup(x => x.RequestAvailableAuthenticationSchemeAsync(sourceAddress, false))
                .ReturnsAsync(Result.Ok(authenticationSchemeResponse));
            
            await renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);
            Assert.That(renderer.FindComponents<DxTextBox>(), Has.Count.EqualTo(2));
            
            this.viewModel.AuthenticationDto.UserName = "admin";
            this.viewModel.AuthenticationDto.Password = "pass";

            this.authenticationService.Setup(x => x.LoginAsync(AuthenticationSchemeKind.LocalJwtBearer, It.IsAny<AuthenticationInformation>()))
                .ReturnsAsync(Result.Ok);
            
            await renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AuthenticationResult, Is.Not.Null);
                Assert.That(this.viewModel.AuthenticationResult.IsSuccess, Is.True);
            });
        }

        [Test]
        public async Task VerifyMultipleAuthenticationSchemeFlowWithExternalToken()
        {
            this.serverConfiguration.ServerAddress = null;
            this.serverConfiguration.AllowMultipleStepsAuthentication = true;
            
            var renderer = this.context.Render<Login>();
            var editForm = renderer.FindComponent<EditForm>();

            Assert.That(renderer.FindComponents<DxTextBox>(), Has.Count.EqualTo(1));
            
            const string sourceAddress = "http://localhost:5000"; 
            this.viewModel.AuthenticationDto.SourceAddress = sourceAddress;

            var authenticationSchemeResponse = new AuthenticationSchemeResponse()
            {
                Schemes = [AuthenticationSchemeKind.ExternalJwtBearer],
                Authority = "http://localhost:8080/realms/MyRealm",
                ClientId = "client"
            };

            this.authenticationService.Setup(x => x.RequestAvailableAuthenticationSchemeAsync(sourceAddress, false))
                .ReturnsAsync(Result.Ok(authenticationSchemeResponse));
            
            await renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);
            Assert.That(renderer.FindComponents<DxTextBox>(), Has.Count.EqualTo(0));

            var navigationManager = this.context.Services.GetService<NavigationManager>();
            Assert.That(navigationManager.Uri.StartsWith(authenticationSchemeResponse.Authority), Is.True);
        }
    }
}
