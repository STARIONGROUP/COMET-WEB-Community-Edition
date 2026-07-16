// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementDefinitionCellTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.RequirementsEditor
{
    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components.Web;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementDefinitionCellTestFixture
    {
        private BunitContext context;
        private Mock<IRequirementsEditorBodyViewModel> viewModel;
        private Requirement requirement;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.requirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R1" };
            this.requirement.Definition.Add(new Definition { Iid = Guid.NewGuid(), LanguageCode = "en", Content = "Initial definition" });

            this.viewModel = new Mock<IRequirementsEditorBodyViewModel>();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        private IRenderedComponent<RequirementDefinitionCell> Render()
        {
            return this.context.Render<RequirementDefinitionCell>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Requirement, this.requirement));
        }

        [Test]
        public void VerifyDisplayShowsDefinitionContent()
        {
            var cell = this.Render();

            Assert.Multiple(() =>
            {
                Assert.That(cell.Markup, Does.Contain("req-def-display"));
                Assert.That(cell.Markup, Does.Contain("Initial definition"));
                Assert.That(cell.Markup, Does.Not.Contain("req-def-editor"));
            });
        }

        [Test]
        public void VerifyDisplayShowsPlaceholderWhenNoDefinition()
        {
            var undefinedRequirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R2" };

            var cell = this.context.Render<RequirementDefinitionCell>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Requirement, undefinedRequirement));

            Assert.That(cell.Markup, Does.Contain("Click to add a definition"));
        }

        [Test]
        public async Task VerifyClickingDisplayEntersEditModeAndShowsDirtyOnChange()
        {
            var cell = this.Render();

            cell.Find(".req-def-display").Click();

            Assert.Multiple(() =>
            {
                Assert.That(cell.Markup, Does.Contain("req-def-editor"));
                Assert.That(cell.Markup, Does.Not.Contain("req-def-editor-dirty"), "the editor is not dirty right after opening");
            });

            var memo = cell.FindComponent<DxMemo>();
            await cell.InvokeAsync(() => memo.Instance.TextChanged.InvokeAsync("Updated definition"));

            Assert.That(cell.Markup, Does.Contain("req-def-editor-dirty"), "an unsaved change marks the editor dirty");
        }

        [Test]
        public async Task VerifySaveCommitsAndLeavesEditMode()
        {
            var cell = this.Render();

            cell.Find(".req-def-display").Click();
            var saveButton = cell.Find(".req-def-save");

            await cell.InvokeAsync(() => saveButton.Click());

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.SaveInlineDefinitionAsync(this.requirement, "Initial definition"), Times.Once);
                Assert.That(cell.Markup, Does.Not.Contain("req-def-editor"), "edit mode is left after saving");
            });
        }

        [Test]
        public void VerifyEscapeCancelsEdit()
        {
            var cell = this.Render();

            cell.Find(".req-def-display").Click();
            Assert.That(cell.Markup, Does.Contain("req-def-editor"));

            cell.Find(".req-def-editor").KeyDown(new KeyboardEventArgs { Key = "Escape" });

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.SaveInlineDefinitionAsync(It.IsAny<Requirement>(), It.IsAny<string>()), Times.Never);
                Assert.That(cell.Markup, Does.Not.Contain("req-def-editor"));
            });
        }

        [Test]
        public void VerifyCancelButtonLeavesEditModeWithoutSaving()
        {
            var cell = this.Render();

            cell.Find(".req-def-display").Click();
            cell.Find(".req-def-cancel").Click();

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.SaveInlineDefinitionAsync(It.IsAny<Requirement>(), It.IsAny<string>()), Times.Never);
                Assert.That(cell.Markup, Does.Not.Contain("req-def-editor"));
            });
        }
    }
}
