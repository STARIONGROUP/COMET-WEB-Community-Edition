// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVSetupDialogTestFixture.cs" company="Starion Group S.A.">
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

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.VandV;
    using COMETwebapp.Tests.Utilities.VandV;
    using COMETwebapp.Utilities.VandV;
    using COMETwebapp.ViewModels.Components.VandV;

    using DevExpress.Blazor;

    using FluentResults;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class VandVSetupDialogTestFixture
    {
        private static readonly string[] MissingLines = ["Parameter type vnv_stage (V&V Stage Gate): enumeration of the stage gates defined below", "Category VnVItem (VnV Item) on Requirement"];

        private static readonly string[] Gates = ["PDR", "CDR"];

        private BunitContext context;
        private Mock<IVandVSetupViewModel> viewModel;
        private ModelReferenceDataLibrary library;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.library = new ModelReferenceDataLibrary { Iid = Guid.NewGuid(), ShortName = "MRDL", Name = "Model library" };
            this.viewModel = new Mock<IVandVSetupViewModel>();
            this.viewModel.SetupGet(x => x.IsVisible).Returns(true);
            this.viewModel.SetupGet(x => x.TargetLibrary).Returns(this.library);
            this.viewModel.SetupGet(x => x.Check).Returns(VandVManifest.Check([this.library]));
            this.viewModel.SetupGet(x => x.NeedsSetup).Returns(true);
            this.viewModel.SetupGet(x => x.CanSeed).Returns(true);
            this.viewModel.SetupGet(x => x.NeedsStageGates).Returns(true);
            this.viewModel.SetupGet(x => x.MissingItems).Returns(MissingLines);
            this.viewModel.SetupGet(x => x.StageGates).Returns(Gates);
            this.viewModel.Setup(x => x.SeedAsync()).ReturnsAsync(Result.Ok());
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        private IRenderedComponent<VandVSetupDialog> Render()
        {
            return this.context.Render<VandVSetupDialog>(parameters => parameters.Add(p => p.ViewModel, this.viewModel.Object));
        }

        [Test]
        public void VerifyMessages()
        {
            this.viewModel.SetupGet(x => x.TargetLibrary).Returns((ModelReferenceDataLibrary)null);
            this.viewModel.SetupGet(x => x.NeedsSetup).Returns(false);
            var noLibrary = this.Render();

            this.viewModel.SetupGet(x => x.TargetLibrary).Returns(this.library);
            this.viewModel.SetupGet(x => x.Check).Returns(VandVManifest.Check([CreateCompleteLibrary()]));
            this.viewModel.SetupGet(x => x.NeedsSetup).Returns(false);
            var complete = this.Render();

            this.viewModel.SetupGet(x => x.Check).Returns(VandVManifest.Check([this.library]));
            this.viewModel.SetupGet(x => x.NeedsSetup).Returns(true);
            this.viewModel.SetupGet(x => x.CanSeed).Returns(false);
            var notAllowed = this.Render();

            Assert.Multiple(() =>
            {
                Assert.That(noLibrary.Markup, Does.Contain("no model reference data library"));
                Assert.That(noLibrary.FindAll("#vandv-setup-confirm"), Is.Empty);
                Assert.That(complete.Markup, Does.Contain("complete in 'Model library'"));
                Assert.That(complete.FindAll("#vandv-setup-confirm"), Is.Empty);
                Assert.That(notAllowed.Find("#vandv-setup-no-permission").TextContent, Does.Contain("administrator"));
                Assert.That(notAllowed.FindAll("#vandv-setup-missing li"), Has.Count.EqualTo(2), "the list an administrator must create");
                Assert.That(notAllowed.FindAll("#vandv-setup-stage-gates"), Is.Empty);
                Assert.That(notAllowed.FindAll("#vandv-setup-confirm"), Is.Empty);
                Assert.That(notAllowed.Markup, Does.Contain("verifies and validates rules are skipped"), "the library has no requirement category");
            });
        }

        [Test]
        public async Task VerifyStageGateEditor()
        {
            var component = this.Render();
            var rows = component.FindAll(".vandv-stage-gate");
            var textBoxes = component.FindComponents<DxTextBox>();

            await component.InvokeAsync(() => textBoxes[1].Instance.TextChanged.InvokeAsync("FAT"));
            await component.InvokeAsync(component.FindComponents<DxButton>().First(x => x.Instance.Text == "Stage gate").Instance.Click.InvokeAsync);

            var buttons = component.FindComponents<DxButton>();
            var upButtons = buttons.Where(x => x.Instance.IconCssClass == IconName.ArrowUp.GetCssClass()).ToList();
            var downButtons = buttons.Where(x => x.Instance.IconCssClass == IconName.ArrowDown.GetCssClass()).ToList();
            var removeButtons = buttons.Where(x => x.Instance.IconCssClass == IconName.Delete.GetCssClass()).ToList();
            await component.InvokeAsync(downButtons[0].Instance.Click.InvokeAsync);
            await component.InvokeAsync(removeButtons[0].Instance.Click.InvokeAsync);
            await component.InvokeAsync(upButtons[1].Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(rows, Has.Count.EqualTo(2));
                Assert.That(upButtons[0].Instance.Enabled, Is.False, "the first gate cannot move up");
                Assert.That(downButtons[^1].Instance.Enabled, Is.False, "the last gate cannot move down");
                this.viewModel.Verify(x => x.SetStageGate(1, "FAT"), Times.Once);
                this.viewModel.Verify(x => x.AddStageGate(), Times.Once);
                this.viewModel.Verify(x => x.MoveStageGate(0, 1), Times.Once);
                this.viewModel.Verify(x => x.RemoveStageGate(0), Times.Once);
                this.viewModel.Verify(x => x.MoveStageGate(1, -1), Times.Once);
            });
        }

        [Test]
        public async Task VerifyConfirm()
        {
            this.viewModel.SetupGet(x => x.StageGateError).Returns("Define at least one stage gate.");
            var invalid = this.Render();
            var invalidConfirm = invalid.FindComponents<DxButton>().Single(x => x.Instance.Id == "vandv-setup-confirm");

            this.viewModel.SetupGet(x => x.StageGateError).Returns((string)null);
            var valid = this.Render();
            var confirm = valid.FindComponents<DxButton>().Single(x => x.Instance.Id == "vandv-setup-confirm");
            await valid.InvokeAsync(confirm.Instance.Click.InvokeAsync);

            this.viewModel.SetupGet(x => x.LastError).Returns("server says no");
            var failed = this.Render();

            this.viewModel.SetupGet(x => x.IsBusy).Returns(true);
            var busy = this.Render();
            var busyButtons = busy.FindComponents<DxButton>();

            Assert.Multiple(() =>
            {
                Assert.That(invalid.Find("#vandv-setup-stage-gate-error").TextContent, Is.EqualTo("Define at least one stage gate."));
                Assert.That(invalidConfirm.Instance.Enabled, Is.False);
                Assert.That(confirm.Instance.Enabled, Is.True);
                this.viewModel.Verify(x => x.SeedAsync(), Times.Once);
                Assert.That(valid.FindAll("#vandv-setup-error"), Is.Empty);
                Assert.That(failed.Find("#vandv-setup-error").TextContent, Is.EqualTo("server says no"));
                Assert.That(busyButtons.Single(x => x.Instance.Id == "vandv-setup-confirm").Instance.Enabled, Is.False);
                Assert.That(busyButtons.Single(x => x.Instance.Text == "Cancel").Instance.Enabled, Is.False, "the dialog cannot be left half way through a write");
                Assert.That(busy.FindComponent<DxPopup>().Instance.ShowCloseButton, Is.False);
            });
        }

        private static ReferenceDataLibrary CreateCompleteLibrary()
        {
            var library = new VandVTestData().Library;
            library.Rule.AddRange(VandVManifest.ParameterizedCategoryRules.Select(x => new ParameterizedCategoryRule { Iid = Guid.NewGuid(), ShortName = x.ShortName }));
            library.Rule.AddRange(VandVManifest.BinaryRelationshipRules.Select(x => new BinaryRelationshipRule { Iid = Guid.NewGuid(), ShortName = x.ShortName }));
            return library;
        }
    }
}
