// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVTabTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.VandV
{
    using Bunit;

    using CDP4Common.CommonData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.VandV;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Tests.ViewModels.Components.VandV;
    using COMETwebapp.Utilities.VandV;
    using COMETwebapp.ViewModels.Components.VandV;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class VandVTabTestFixture
    {
        private BunitContext context;
        private Mock<IVandVViewModel> viewModel;
        private Mock<IVandVSetupViewModel> setupViewModel;
        private Mock<ISessionService> sessionService;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddSingleton(new Mock<IDomDataService>().Object);
            this.sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(this.sessionService.Object);

            var model = VandVModel.Build(null, null);
            var registerViewModel = new Mock<IVandVRegisterViewModel>();
            registerViewModel.SetupGet(x => x.Model).Returns(model);
            registerViewModel.SetupGet(x => x.Rows).Returns([]);
            registerViewModel.SetupGet(x => x.AvailableSpecifications).Returns([]);

            this.setupViewModel = new Mock<IVandVSetupViewModel>();
            this.setupViewModel.SetupGet(x => x.Check).Returns(VandVManifest.Check(null));
            this.setupViewModel.SetupGet(x => x.MissingItems).Returns([]);
            this.setupViewModel.SetupGet(x => x.StageGates).Returns([]);

            this.viewModel = new Mock<IVandVViewModel>();
            this.viewModel.SetupGet(x => x.Model).Returns(model);
            this.viewModel.SetupGet(x => x.SetupViewModel).Returns(this.setupViewModel.Object);
            this.viewModel.SetupGet(x => x.RegisterViewModel).Returns(registerViewModel.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyEmptyState()
        {
            this.viewModel.SetupGet(x => x.ShowEmptyState).Returns(true);
            this.setupViewModel.SetupGet(x => x.NeedsSetup).Returns(true);
            var component = this.Render();

            var setupButton = component.FindComponents<DxButton>().Single(x => x.Instance.Text == "Set up V&V...");
            await component.InvokeAsync(setupButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(component.Find(".vandv-empty").TextContent, Does.Contain("not set up"));
                Assert.That(component.FindAll(".vandv-register"), Is.Empty);
                this.setupViewModel.Verify(x => x.Open(), Times.Once);
            });
        }

        [Test]
        public async Task VerifySetupBanner()
        {
            this.setupViewModel.SetupGet(x => x.NeedsSetup).Returns(true);
            var withBanner = this.Render();
            var bannerButton = withBanner.FindComponents<DxButton>().Single(x => x.Instance.Text == "Set up V&V...");
            await withBanner.InvokeAsync(bannerButton.Instance.Click.InvokeAsync);

            this.setupViewModel.SetupGet(x => x.NeedsSetup).Returns(false);
            var complete = this.Render();

            Assert.Multiple(() =>
            {
                Assert.That(withBanner.Find(".vandv-banner").TextContent, Does.Contain("missing"));
                Assert.That(withBanner.FindAll(".vandv-register"), Has.Count.EqualTo(1), "the register is shown next to the banner");
                this.setupViewModel.Verify(x => x.Open(), Times.Once);
                Assert.That(complete.FindAll(".vandv-banner"), Is.Empty);
                Assert.That(complete.FindAll(".vandv-empty"), Is.Empty);
                Assert.That(complete.FindAll(".vandv-register"), Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void VerifyReadOnlySession()
        {
            this.viewModel.SetupGet(x => x.ShowEmptyState).Returns(true);
            this.setupViewModel.SetupGet(x => x.NeedsSetup).Returns(true);
            var writable = this.Render();
            var writableButton = writable.FindComponents<DxButton>().Single(x => x.Instance.Text == "Set up V&V...");

            this.sessionService.Setup(x => x.IsReadOnly).Returns(true);
            var readOnly = this.Render();

            this.viewModel.SetupGet(x => x.ShowEmptyState).Returns(false);
            var banner = this.Render();

            Assert.Multiple(() =>
            {
                Assert.That(writableButton.Instance.Enabled, Is.True);
                Assert.That(writable.FindAll(".vandv-read-only"), Is.Empty);
                Assert.That(readOnly.FindComponents<DxButton>().Single(x => x.Instance.Text == "Set up V&V...").Instance.Enabled, Is.False);
                Assert.That(readOnly.Find(".vandv-read-only").TextContent, Does.Contain("read-only"));
                Assert.That(banner.FindComponents<DxButton>().Single(x => x.Instance.Text == "Set up V&V...").Instance.Enabled, Is.False, "nor from the banner");
            });
        }

        [Test]
        public void VerifyLoadError()
        {
            var withoutError = this.Render();

            this.viewModel.SetupGet(x => x.LoadError).Returns("could not be read");
            var withError = this.Render();

            Assert.Multiple(() =>
            {
                Assert.That(withoutError.FindAll(".vandv-load-error"), Is.Empty);
                Assert.That(withError.Find(".vandv-load-error").TextContent, Is.EqualTo("could not be read"));
                Assert.That(withError.FindAll(".vandv-register"), Has.Count.EqualTo(1), "what was read stays visible");
            });
        }

        [Test]
        public void VerifyNewSnapshotRerenders()
        {
            var permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(permissionService.Object);
            var sessionService = new Mock<ISessionService>();
            sessionService.Setup(x => x.Session).Returns(session.Object);

            using var realViewModel = new VandVViewModel(sessionService.Object, new Mock<ILogger>().Object);
            var component = this.context.Render<VandVTab>(parameters => parameters.Add(p => p.ViewModel, realViewModel));
            var rowsBefore = component.FindAll(".vandv-row").Count;

            component.InvokeAsync(() => realViewModel.SetIteration(new VandVRegisterScenario().Data.Iteration));

            component.WaitForAssertion(() => Assert.That(component.FindAll(".vandv-row"), Is.Not.Empty, "a reload shows without another interaction"));
            Assert.That(rowsBefore, Is.Zero);
        }

        private IRenderedComponent<VandVTab> Render()
        {
            return this.context.Render<VandVTab>(parameters => parameters.Add(p => p.ViewModel, this.viewModel.Object));
        }
    }
}
