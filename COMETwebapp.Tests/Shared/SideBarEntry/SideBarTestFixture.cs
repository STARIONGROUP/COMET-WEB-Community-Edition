// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SideBarTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Tests.Shared.SideBarEntry
{
    using AngleSharp.Html.Dom;

    using Bunit;
    using Bunit.TestDoubles;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.NotificationService;
    using COMET.Web.Common.Services.RegistrationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.Services.VersionService;
    using COMET.Web.Common.Shared.TopMenuEntry;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry;
    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry.PersonEdit;

    using COMETwebapp.Model;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.Shared;
    using COMETwebapp.Shared.SideBarEntry;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Pages;
    using COMETwebapp.ViewModels.Shared.TopMenuEntry;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;


    [TestFixture]
    public class SideBarTestFixture
    {
        private BunitContext context;
        private CometWebAuthStateProvider stateProvider;
        private Mock<ISessionService> sessionService;
        private Mock<IAutoRefreshService> autoRefreshService;
        private Mock<IAuthenticationService> authenticationService;
        private Mock<IRegistrationService> registrationService;
        private Mock<IVersionService> versionService;
        private Mock<IStringTableService> configurationService;
        private Mock<ISessionMenuViewModel> sessionMenuViewModel;
        private Mock<IShowHideDeprecatedThingsViewModel> showHideDeprecatedThingsViewModel;
        private Mock<IAuthorizedMenuEntryViewModel> authorizedMenuEntryViewModel;
        private SourceList<Iteration> sourceList;
        private List<Type> registeredSideBarEntries;
        private List<Application> registeredApplications;
        private Mock<ITabsViewModel> tabsViewModel;
        private CDPMessageBus messageBus;

        private class TestAuthorizedMenuEntry : AuthorizedMenuEntry
        {
        }

        [SetUp]
        public void Setup()
        {
            this.context = new BunitContext();
            this.sessionService = new Mock<ISessionService>();
            this.stateProvider = new CometWebAuthStateProvider(this.sessionService.Object);
            this.authenticationService = new Mock<IAuthenticationService>();
            this.autoRefreshService = new Mock<IAutoRefreshService>();
            this.showHideDeprecatedThingsViewModel = new Mock<IShowHideDeprecatedThingsViewModel>();
            this.showHideDeprecatedThingsViewModel.Setup(x => x.ShowHideDeprecatedThingsService).Returns(new Mock<IShowHideDeprecatedThingsService>().Object);
            this.sourceList = new SourceList<Iteration>();
            this.sessionService.Setup(x => x.OpenIterations).Returns(this.sourceList);
            this.sessionService.Setup(x => x.IsSessionOpen).Returns(true);
            this.sessionService.Setup(x => x.Session.ActivePerson).Returns(new Person { Role = new PersonRole() });

            this.registeredSideBarEntries =
            [
                typeof(ApplicationsSideBar),
                typeof(ModelSideBar),
                typeof(SessionSideBar),
                typeof(ShowHideDeprecatedThingsSideBar),
                typeof(AboutSideBar),
                typeof(SideBarFooter)
            ];

            this.registeredApplications = [];
            this.registrationService = new Mock<IRegistrationService>();
            this.registrationService.Setup(x => x.RegisteredAuthorizedMenuEntries).Returns(this.registeredSideBarEntries);
            this.registrationService.Setup(x => x.RegisteredApplications).Returns(this.registeredApplications);
            this.versionService = new Mock<IVersionService>();
            this.versionService.Setup(x => x.GetVersion()).Returns("1.1.2");
            this.sessionMenuViewModel = new Mock<ISessionMenuViewModel>();
            this.sessionMenuViewModel.Setup(x => x.SessionService).Returns(this.sessionService.Object);
            this.authorizedMenuEntryViewModel = new Mock<IAuthorizedMenuEntryViewModel>();
            this.authorizedMenuEntryViewModel.Setup(x => x.IsAuthenticated).Returns(true);
            this.messageBus = new CDPMessageBus();

            this.context.Services.AddSingleton<ICDPMessageBus>(this.messageBus);
            this.context.Services.AddSingleton(this.versionService.Object);
            this.context.Services.AddSingleton(this.registrationService.Object);
            this.context.Services.AddSingleton<AuthenticationStateProvider>(this.stateProvider);
            this.context.Services.AddSingleton(this.sessionService.Object);
            this.context.Services.AddSingleton(this.authenticationService.Object);
            this.context.Services.AddSingleton(this.autoRefreshService.Object);
            this.context.Services.AddSingleton(this.autoRefreshService.Object);
            this.context.Services.AddSingleton(this.showHideDeprecatedThingsViewModel.Object);
            this.context.Services.AddSingleton<IPersonEditViewModel>(new Mock<IPersonEditViewModel>().Object);
            this.context.Services.AddSingleton<ISessionMenuViewModel, SessionMenuViewModel>();
            this.context.Services.AddSingleton<IModelMenuViewModel, ModelMenuViewModel>();
            this.context.Services.AddSingleton(this.authorizedMenuEntryViewModel.Object);
            this.context.Services.AddSingleton<INotificationService, NotificationService>();
            this.tabsViewModel = new Mock<ITabsViewModel>();
            this.context.Services.AddSingleton(this.tabsViewModel.Object);
            this.configurationService = new Mock<IStringTableService>();
            this.context.Services.AddSingleton(this.configurationService.Object);
            this.context.ConfigureDevExpressBlazor();
            this.context.JSInterop.SetupVoid("cometKeyboard.focusFirstIn", _ => true);
            this.context.JSInterop.SetupVoid("cometKeyboard.focusElement", _ => true);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyAboutSideBar()
        {
            var renderer = this.context.Render<SideBar>();
            var aboutSideBar = renderer.FindComponent<AboutSideBar>();
            Assert.That(aboutSideBar.Instance.IsVisible, Is.EqualTo(false));

            var item = aboutSideBar.FindComponent<SideBarItem>();
            await aboutSideBar.InvokeAsync(item.Instance.OnClick);
            Assert.That(aboutSideBar.Instance.IsVisible, Is.EqualTo(true));
        }

        [Test]
        public void VerifyApplicationsRegistration()
        {
            var session = new Mock<ISession>();

            var activePerson = new Person
            {
                GivenName = "User",
                ShortName = "User",
                Role = new PersonRole
                {
                    ShortName = "PersonRole"
                }
            };

            session.Setup(x => x.ActivePerson).Returns(activePerson);

            this.sessionService.Setup(x => x.IsSessionOpen).Returns(true);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            var renderer = this.context.Render<SideBar>();
            var applicationSideBar = renderer.FindComponent<ApplicationsSideBar>();
            var sideBarItems = applicationSideBar.FindComponents<SideBarItem>();
            Assert.That(sideBarItems, Has.Count.EqualTo(0));

            var modelDashboardApplication = new Application
            {
                Name = "Model Dashboard",
                Url = "/ModelDashboard",
                IsDisabled = false,
                Description = "desc",
                Icon = IconName.PieChart
            };

            this.registeredApplications.Add(modelDashboardApplication);
            this.registrationService.Setup(x => x.RegisteredApplications).Returns(this.registeredApplications);

            renderer = this.context.Render<SideBar>();
            applicationSideBar = renderer.FindComponent<ApplicationsSideBar>();
            sideBarItems = applicationSideBar.FindComponents<SideBarItem>();

            Assert.Multiple(() =>
            {
                Assert.That(sideBarItems, Has.Count.EqualTo(1));
                Assert.That(sideBarItems[0].Instance.Text, Is.EqualTo(modelDashboardApplication.Name));
            });
        }

        /// <summary>
        /// Verifies the navigation landmark carries its accessible name and shortcut hint (issue #885), and that the
        /// active application entry is announced as current via <see cref="SideBarItem.Selected" />.
        /// </summary>
        [Test]
        public void VerifyNavigationLandmarkAndActiveApplicationAreAnnounced()
        {
            var application = new Application
            {
                Name = "Model Dashboard",
                Url = "ModelDashboard",
                IsDisabled = false,
                Description = "desc",
                Icon = IconName.PieChart
            };

            this.registeredApplications.Add(application);

            var renderer = this.context.Render<SideBar>();
            var navigationManager = this.context.Services.GetService<NavigationManager>()!;
            navigationManager.NavigateTo($"http://localhost/{application.Url}");

            var nav = renderer.Find("nav.main-side-bar");
            var applicationsSideBar = renderer.FindComponent<ApplicationsSideBar>();
            var activeItem = applicationsSideBar.FindComponents<SideBarItem>().First(x => x.Instance.Text == application.Name);

            Assert.Multiple(() =>
            {
                Assert.That(nav.GetAttribute("aria-label"), Is.EqualTo("Main navigation"));
                Assert.That(nav.GetAttribute("aria-keyshortcuts"), Is.EqualTo("Alt+Shift+1"));
                Assert.That(activeItem.Instance.Selected, Is.True);
                Assert.That(activeItem.Find(".application-item-container").GetAttribute("aria-current"), Is.EqualTo("page"));
            });
        }

        /// <summary>
        /// Verifies that the model side bar drop-down moves keyboard focus into itself when shown and back to its entry
        /// when closed, so a keyboard user can reach its menu (issue #885).
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        [Test]
        public async Task VerifyModelDropdownManagesKeyboardFocus()
        {
            var renderer = this.context.Render<SideBar>();
            var dropdown = renderer.FindComponent<ModelSideBar>().FindComponent<DxDropDown>();

            await renderer.InvokeAsync(() => dropdown.Instance.Shown.InvokeAsync(null));
            await renderer.InvokeAsync(() => dropdown.Instance.Closed.InvokeAsync(null));

            Assert.Multiple(() =>
            {
                this.context.JSInterop.VerifyInvoke("cometKeyboard.focusFirstIn");
                this.context.JSInterop.VerifyInvoke("cometKeyboard.focusElement");
            });
        }

        /// <summary>
        /// Verifies that the session side bar drop-down moves keyboard focus into itself when shown and back to its
        /// entry when closed, so a keyboard user can reach its buttons (issue #885).
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        [Test]
        public async Task VerifySessionDropdownManagesKeyboardFocus()
        {
            var renderer = this.context.Render<SideBar>();
            var dropdown = renderer.FindComponent<SessionSideBar>().FindComponent<DxDropDown>();

            await renderer.InvokeAsync(() => dropdown.Instance.Shown.InvokeAsync(null));
            await renderer.InvokeAsync(() => dropdown.Instance.Closed.InvokeAsync(null));

            Assert.Multiple(() =>
            {
                this.context.JSInterop.VerifyInvoke("cometKeyboard.focusFirstIn");
                this.context.JSInterop.VerifyInvoke("cometKeyboard.focusElement");
            });
        }

        [Test]
        public void VerifyReturnToHomeWithFooter()
        {
            var navigationManager = this.context.Services.GetService<NavigationManager>()!;
            navigationManager.NavigateTo("/AnUrl");
            var renderer = this.context.Render<SideBar>();
            var footer = renderer.FindComponent<SideBarFooter>();
            var link = (IHtmlAnchorElement)footer.Find("a");
            Assert.That(navigationManager.Uri, Does.EndWith("AnUrl"));
            navigationManager.NavigateTo(link.Href);
            Assert.That(navigationManager.Uri, Does.Not.EndWith("AnUrl"));
        }

        [Test]
        public async Task VerifySideBar()
        {
            var renderer = this.context.Render<SideBar>();
            var authorizedSideBarEntries = renderer.FindComponents<AuthorizedMenuEntry>();
            Assert.That(authorizedSideBarEntries.All(x => x.Instance.AuthorizedMenuEntryViewModel.IsAuthenticated), Is.EqualTo(true));

            var session = new Mock<ISession>();

            var activePerson = new Person
            {
                GivenName = "User",
                ShortName = "User",
                Role = new PersonRole
                {
                    ShortName = "PersonRole"
                }
            };

            session.Setup(x => x.ActivePerson).Returns(activePerson);

            this.sessionService.Setup(x => x.IsSessionOpen).Returns(true);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.stateProvider.NotifyAuthenticationStateChanged();
            Assert.That(authorizedSideBarEntries.All(x => x.Instance.AuthorizedMenuEntryViewModel.IsAuthenticated), Is.True);

            var sessionSideBarEntry = authorizedSideBarEntries[2];
            var sessionSideBarInstance = (SessionSideBar)sessionSideBarEntry.Instance;
            sessionSideBarInstance.ExpandDropdown();
            await renderer.InvokeAsync(sessionSideBarInstance.OnRefreshClick);
            await renderer.InvokeAsync(sessionSideBarInstance.Logout);
            var navigationManager = this.context.Services.GetService<NavigationManager>()!;

            Assert.Multiple(() =>
            {
                Assert.That(navigationManager.Uri, Does.EndWith("Logout"));
                Assert.That(sessionSideBarInstance.Expanded, Is.EqualTo(false));
                Assert.That(sessionSideBarInstance.IsRefreshing, Is.EqualTo(false));
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

            var modelSideBarEntry = authorizedSideBarEntries[1];
            var modelSideBarInstance = (ModelSideBar)modelSideBarEntry.Instance;
            var modelSideBarViewModel = modelSideBarInstance.ViewModel;
            var sideBarItem = modelSideBarEntry.FindComponent<SideBarItem>();
            await modelSideBarEntry.InvokeAsync(sideBarItem.Instance.OnClick.Invoke);

            var modelRow = renderer.FindComponent<ModelMenuRow>();
            await renderer.InvokeAsync(modelRow.Instance.ViewModel.SwitchDomain);
            Assert.That(modelSideBarViewModel.IsOnSwitchDomainMode, Is.True);
            var switchDomainComponent = renderer.FindComponent<SwitchDomain>();
            switchDomainComponent.Instance.ViewModel.SelectedDomainOfExpertise = switchDomainComponent.Instance.ViewModel.AvailableDomains.Last();
            await renderer.InvokeAsync(switchDomainComponent.Instance.ViewModel.OnSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(modelSideBarViewModel.IsOnSwitchDomainMode, Is.False);

                this.sessionService.Verify(x => x.SwitchDomain(It.IsAny<Iteration>(),
                    It.IsAny<DomainOfExpertise>()), Times.Once);
            });

            await renderer.InvokeAsync(modelRow.Instance.ViewModel.CloseIteration);
            Assert.That(modelSideBarViewModel.ConfirmCancelViewModel.IsVisible, Is.True);

            await renderer.InvokeAsync(modelSideBarViewModel.ConfirmCancelViewModel.OnCancel.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(modelSideBarViewModel.ConfirmCancelViewModel.IsVisible, Is.False);
                this.sessionService.Verify(x => x.CloseIteration(It.IsAny<Iteration>()), Times.Never);
            });

            await renderer.InvokeAsync(modelRow.Instance.ViewModel.CloseIteration);
            await renderer.InvokeAsync(modelSideBarViewModel.ConfirmCancelViewModel.OnConfirm.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(modelSideBarViewModel.ConfirmCancelViewModel.IsVisible, Is.False);
                this.sessionService.Verify(x => x.CloseIteration(It.IsAny<Iteration>()), Times.Once);
            });
        }

        /// <summary>
        /// Verifies that a narrow viewport collapses the side bar by default, that the user can still expand it again while the
        /// viewport stays narrow, and that resizing the window drops that manual override (issue GH742)
        /// </summary>
        [Test]
        public async Task VerifySideBarCollapsesOnNarrowViewport()
        {
            var renderer = this.context.Render<SideBar>();
            var sideBar = renderer.Instance;

            Assert.Multiple(() =>
            {
                Assert.That(sideBar.IsCollapsed, Is.False);
                Assert.That(renderer.Find("nav").ClassList, Does.Not.Contain("collapsed"));
            });

            // A narrow viewport collapses the side bar. In the running app the DxLayoutBreakpoint two-way binding both assigns
            // the property and triggers the re-render.
            await renderer.InvokeAsync(() => sideBar.IsNarrowViewport = true);
            renderer.Render();

            renderer.WaitForAssertion(() =>
            {
                Assert.That(sideBar.IsCollapsed, Is.True);
                Assert.That(renderer.Find("nav").ClassList, Does.Contain("collapsed"));
            });

            // The toggle remains available on a narrow viewport, so the user can expand the side bar anyway.
            await renderer.InvokeAsync(() => renderer.Find("#side-bar-collapse-button").Click());

            renderer.WaitForAssertion(() =>
            {
                Assert.That(sideBar.IsCollapsed, Is.False, "The user must be able to expand the side bar on a small screen.");
                Assert.That(renderer.Find("nav").ClassList, Does.Not.Contain("collapsed"));
            });

            // Widening the window drops the manual override, so the side bar follows the viewport again.
            await renderer.InvokeAsync(() => sideBar.IsNarrowViewport = false);
            renderer.Render();

            renderer.WaitForAssertion(() => Assert.That(sideBar.IsCollapsed, Is.False));

            await renderer.InvokeAsync(() => sideBar.IsNarrowViewport = true);
            renderer.Render();

            renderer.WaitForAssertion(() => Assert.That(sideBar.IsCollapsed, Is.True, "Shrinking the window collapses the side bar again."));

            // Re-assigning the same viewport state is a no-op, and must not drop a preference the user has meanwhile set.
            await renderer.InvokeAsync(sideBar.ToggleCollapsed);
            await renderer.InvokeAsync(() => sideBar.IsNarrowViewport = true);
            renderer.Render();

            renderer.WaitForAssertion(() => Assert.That(sideBar.IsCollapsed, Is.False, "Re-setting the same viewport state must keep the user's own choice."));
        }

        /// <summary>
        /// Verifies the markup hooks that keep the collapsed side bar narrow (issue GH806): the footer logo and both
        /// section-header labels are dropped on collapse, and the "General" header doubles as a horizontal divider.
        /// </summary>
        [Test]
        public void VerifyCollapsedSideBarIconOnlyLayout()
        {
            var renderer = this.context.Render<SideBar>();

            var footer = renderer.FindComponent<SideBarFooter>();
            var logo = footer.Find("img");

            var applicationsSideBar = renderer.FindComponent<ApplicationsSideBar>();
            var sectionHeaders = applicationsSideBar.FindAll(".side-bar-section-header");
            var dividers = applicationsSideBar.FindAll(".side-bar-section-divider");

            Assert.Multiple(() =>
            {
                Assert.That(logo.ClassList, Does.Contain("not-displayed-on-collapse"));
                Assert.That(sectionHeaders, Has.Count.EqualTo(2));
                Assert.That(dividers, Has.Count.EqualTo(1));
                Assert.That(sectionHeaders.All(header => header.QuerySelector(".not-displayed-on-collapse") is not null), Is.True);
            });
        }

        [Test]
        public void VerifySideBarFooterIsInFlow()
        {
            var renderer = this.context.Render<SideBar>();
            var footer = renderer.FindComponent<SideBarFooter>();
            var footerElement = footer.Find("div");

            Assert.Multiple(() =>
            {
                Assert.That(footerElement.ClassList, Does.Contain("side-bar-footer"));
                Assert.That(footerElement.GetAttribute("style"), Is.Null.Or.Empty);
            });
        }

        [Test]
        public void VerifySideBarEntryRegistration()
        {
            var renderer = this.context.Render<SideBar>();
            var sideBarEntries = renderer.FindComponents<AuthorizedMenuEntry>();
            Assert.That(sideBarEntries, Has.Count.EqualTo(4));
            this.registeredSideBarEntries.Add(typeof(Login));
            this.registeredSideBarEntries.Add(typeof(TestAuthorizedMenuEntry));
            renderer.Render();
            sideBarEntries = renderer.FindComponents<AuthorizedMenuEntry>();
            Assert.That(sideBarEntries, Has.Count.EqualTo(5));
        }

        [Test]
        public async Task VerifySideBarItemsBehavior()
        {
            var fakeNavigationManager = (BunitNavigationManager)this.context.Services.GetService(typeof(NavigationManager));
            Assert.That(fakeNavigationManager, Is.Not.Null);

            this.registeredApplications.AddRange([new TabbedApplication(), new Application { Url = "a" }, new Application { Url = WebAppConstantValues.TabsPage }]);
            var renderer = this.context.Render<SideBar>();
            var firstDataItem = renderer.FindComponent<SideBarItem>();
            await renderer.InvokeAsync(firstDataItem.Instance.OnClick.Invoke);
            Assert.That(fakeNavigationManager.Uri, Does.Contain(WebAppConstantValues.TabsPage));

            var secondDataItem = renderer.FindComponents<SideBarItem>()[2];
            await renderer.InvokeAsync(secondDataItem.Instance.OnClick.Invoke);
            Assert.That(fakeNavigationManager.Uri, Does.Contain(this.registeredApplications[2].Url));

            var thirdDataItem = renderer.FindComponents<SideBarItem>()[1];
            await renderer.InvokeAsync(thirdDataItem.Instance.OnClick.Invoke);

            Assert.Multiple(() =>
            {
                Assert.That(fakeNavigationManager.Uri, Does.Contain(this.registeredApplications[1].Url));
                Assert.That(renderer.Instance.IsCollapsed, Is.False);
            });

            var sideBarCollapseButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "side-bar-collapse-button");
            await renderer.InvokeAsync(sideBarCollapseButton.Instance.Click.InvokeAsync);
            Assert.That(renderer.Instance.IsCollapsed, Is.True);
        }
    }
}
