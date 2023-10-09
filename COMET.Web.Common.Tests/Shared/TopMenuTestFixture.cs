// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TopMenuTestFixture.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Tests.Shared
{
    using AngleSharp.Html.Dom;

    using AngleSharpWrappers;

    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.NotificationService;
    using COMET.Web.Common.Services.RegistrationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.Services.VersionService;
    using COMET.Web.Common.Shared;
    using COMET.Web.Common.Shared.TopMenuEntry;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class TopMenuTestFixture
    {
        private TestContext context;
        private CometWebAuthStateProvider stateProvider;
        private Mock<ISessionService> sessionService;
        private Mock<IAutoRefreshService> autoRefreshService;
        private Mock<IAuthenticationService> authenticationService;
        private Mock<IRegistrationService> registrationService;
        private Mock<IVersionService> versionService;
        private Mock<IStringTableService> configurationService;
        private SourceList<Iteration> sourceList;
        private List<Type> registeredMenuEntries;
        private List<Application> registeredApplications;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.sessionService = new Mock<ISessionService>();
            this.stateProvider = new CometWebAuthStateProvider(this.sessionService.Object);
            this.authenticationService = new Mock<IAuthenticationService>();
            this.autoRefreshService = new Mock<IAutoRefreshService>();
            this.sourceList = new SourceList<Iteration>();
            this.sessionService.Setup(x => x.OpenIterations).Returns(this.sourceList);
            this.registeredMenuEntries = new List<Type>()
            {
                typeof(ApplicationMenu),
                typeof(ModelMenu),
                typeof(SessionMenu)
            };
            this.registeredApplications = new List<Application>();
            this.registrationService = new Mock<IRegistrationService>();
            this.registrationService.Setup(x => x.RegisteredAuthorizedMenuEntries).Returns(this.registeredMenuEntries);
            this.registrationService.Setup(x => x.RegisteredApplications).Returns(this.registeredApplications);
            this.versionService = new Mock<IVersionService>();
            this.versionService.Setup(x => x.GetVersion()).Returns("1.1.2");

            this.context.Services.AddSingleton(this.versionService.Object);
            this.context.Services.AddSingleton(this.registrationService.Object);
            this.context.Services.AddSingleton<AuthenticationStateProvider>(this.stateProvider);
            this.context.Services.AddSingleton(this.sessionService.Object);
            this.context.Services.AddSingleton(this.authenticationService.Object);
            this.context.Services.AddSingleton(this.autoRefreshService.Object);
            this.context.Services.AddSingleton<ISessionMenuViewModel, SessionMenuViewModel>();
            this.context.Services.AddSingleton<IModelMenuViewModel, ModelMenuViewModel>();
            this.context.Services.AddSingleton<IAuthorizedMenuEntryViewModel, AuthorizedMenuEntryViewModel>();
            this.context.Services.AddSingleton<INotificationService, NotificationService>();
            this.configurationService = new Mock<IStringTableService>();
            this.context.Services.AddSingleton(this.configurationService.Object);
            this.context.ConfigureDevExpressBlazor();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyTopMenu()
        {
            var renderer = this.context.RenderComponent<TopMenu>();
            var authorizedMenuEntries = renderer.FindComponents<AuthorizedMenuEntry>();

            Assert.That(authorizedMenuEntries.All(x => !x.Instance.AuthorizedMenuEntryViewModel.IsAuthenticated), Is.True);

            var session = new Mock<ISession>();
            session.Setup(x => x.ActivePerson).Returns(new Person() { GivenName = "User", ShortName = "User" });

            this.sessionService.Setup(x => x.IsSessionOpen).Returns(true);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.stateProvider.NotifyAuthenticationStateChanged();
            Assert.That(authorizedMenuEntries.All(x => x.Instance.AuthorizedMenuEntryViewModel.IsAuthenticated), Is.True);

            var sessionMenuEntry = authorizedMenuEntries[2];
            var sessionMenuInstance = (SessionMenu)sessionMenuEntry.Instance;
            sessionMenuInstance.Expanded = true;
            await renderer.InvokeAsync(sessionMenuInstance.Logout);
            var navigationManager = this.context.Services.GetService<NavigationManager>()!;

            Assert.Multiple(() =>
            {
                Assert.That(navigationManager.Uri, Does.EndWith("Logout"));
                Assert.That(sessionMenuInstance.Expanded, Is.False);
            });

            var iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup
                {
                    Iid = Guid.NewGuid(),
                    IterationNumber = 4,
                    Container = new EngineeringModelSetup
                    {
                        Name = "Envision",
                        Iid = Guid.NewGuid()
                    }
                }
            };

            this.sessionService.Setup(x => x.IsSessionOpen).Returns(true);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);

            var systemDomain = new DomainOfExpertise
            {
                Iid = Guid.NewGuid(),
                Name = "System"
            };

            var thermoDomain = new DomainOfExpertise
            {
                Iid = Guid.NewGuid(),
                Name = "Thermodynamic"
            };

            this.sessionService.Setup(x => x.GetModelDomains(It.IsAny<EngineeringModelSetup>()))
                .Returns(new List<DomainOfExpertise>
                {
                    systemDomain, thermoDomain
                });

            this.sessionService.Setup(x => x.GetDomainOfExpertise(iteration)).Returns(systemDomain);
            this.sourceList.Add(iteration);

            var modelMenuEntry = authorizedMenuEntries[1];
            var modelMenuInstance = (ModelMenu)modelMenuEntry.Instance;
            var modelMenuViewModel = modelMenuInstance.ViewModel;

            var modelRow = renderer.FindComponent<ModelMenuRow>();
            await renderer.InvokeAsync(modelRow.Instance.ViewModel.SwitchDomain);
            Assert.That(modelMenuViewModel.IsOnSwitchDomainMode, Is.True);
            var switchDomainComponent = renderer.FindComponent<SwitchDomain>();
            switchDomainComponent.Instance.ViewModel.SelectedDomainOfExpertise = switchDomainComponent.Instance.ViewModel.AvailableDomains.Last();
            await renderer.InvokeAsync(switchDomainComponent.Instance.ViewModel.OnSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(modelMenuViewModel.IsOnSwitchDomainMode, Is.False);

                this.sessionService.Verify(x => x.SwitchDomain(It.IsAny<Iteration>(),
                    It.IsAny<DomainOfExpertise>()), Times.Once);
            });

            await renderer.InvokeAsync(modelRow.Instance.ViewModel.CloseIteration);
            Assert.That(modelMenuViewModel.ConfirmCancelViewModel.IsVisible, Is.True);

            await renderer.InvokeAsync(modelMenuViewModel.ConfirmCancelViewModel.OnCancel.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(modelMenuViewModel.ConfirmCancelViewModel.IsVisible, Is.False);
                this.sessionService.Verify(x => x.CloseIteration(It.IsAny<Iteration>()), Times.Never);
            });

            await renderer.InvokeAsync(modelRow.Instance.ViewModel.CloseIteration);
            await renderer.InvokeAsync(modelMenuViewModel.ConfirmCancelViewModel.OnConfirm.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(modelMenuViewModel.ConfirmCancelViewModel.IsVisible, Is.False);
                this.sessionService.Verify(x => x.CloseIteration(It.IsAny<Iteration>()), Times.Once);
            });
        }

        [Test]
        public void VerifyApplicationsRegistration()
        {
            var session = new Mock<ISession>();
            session.Setup(x => x.ActivePerson).Returns(new Person() { GivenName = "User", ShortName = "User" });

            this.sessionService.Setup(x => x.IsSessionOpen).Returns(true);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            var renderer = this.context.RenderComponent<TopMenu>();
            var applicationMenuEntry = renderer.FindComponent<ApplicationMenu>();
            var dxMenus = applicationMenuEntry.FindComponents<DxMenuItem>();
            Assert.That(dxMenus, Has.Count.EqualTo(1));

            var modelDashboardApplication = new Application
            {
                Name = "Model Dashboard",
                Url = "/ModelDashboard"
            };

            this.registeredApplications.Add(modelDashboardApplication);

            renderer.Render();
            dxMenus = applicationMenuEntry.FindComponents<DxMenuItem>();

            Assert.Multiple(() =>
            {
                Assert.That(dxMenus, Has.Count.EqualTo(2));
                Assert.That(dxMenus[1].Instance.Text, Is.EqualTo(modelDashboardApplication.Name));
                Assert.That(dxMenus[1].Instance.NavigateUrl, Is.EqualTo(modelDashboardApplication.Url));
            });
        }

        [Test]
        public void VerifyMenuEntryRegistration()
        {
            var renderer = this.context.RenderComponent<TopMenu>();
            var menuEntries = renderer.FindComponents<AuthorizedMenuEntry>();
            Assert.That(menuEntries, Has.Count.EqualTo(3));
            this.registeredMenuEntries.Add(typeof(Login));
            this.registeredMenuEntries.Add(typeof(TestAuthorizedMenuEntry));
            renderer.Render();
            menuEntries = renderer.FindComponents<AuthorizedMenuEntry>();
            Assert.That(menuEntries, Has.Count.EqualTo(4));
        }

        [Test]
        public void VerifyReturnToHomeWithTitle()
        {
            var navigationManager = this.context.Services.GetService<NavigationManager>()!;
            navigationManager.NavigateTo("/AnUrl");
            var renderer = this.context.RenderComponent<TopMenu>();
            var topMenuTitle = renderer.FindComponent<TopMenuTitle>();
            var link = (ElementWrapper)topMenuTitle.Find("a");
            var htmlAnchor = (IHtmlAnchorElement)link.WrappedElement;
            Assert.That(navigationManager.Uri, Does.EndWith("AnUrl"));
            navigationManager.NavigateTo(htmlAnchor.Href);
            Assert.That(navigationManager.Uri, Does.Not.EndWith("AnUrl"));
        }

        [Test]
        public void VerifyCustomTitleHeader()
        {
            this.registrationService.Setup(x => x.CustomHeader).Returns(typeof(CustomHeader));
            var renderer = this.context.RenderComponent<TopMenu>();

            Assert.Multiple(() =>
            {
                Assert.That(() => renderer.FindComponent<TopMenuTitle>(), Throws.Exception);
                Assert.That(() => renderer.FindComponent<CustomHeader>(), Throws.Nothing);
            });
        }

        private class TestAuthorizedMenuEntry : AuthorizedMenuEntry
        {
        }

        private class CustomHeader : MenuEntryBase
        {
        }
    }
}
