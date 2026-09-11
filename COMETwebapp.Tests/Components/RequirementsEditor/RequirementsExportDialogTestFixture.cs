// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsExportDialogTestFixture.cs" company="Starion Group S.A.">
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

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.Model.RequirementsEditor.Export;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementsExportDialogTestFixture
    {
        private BunitContext context;
        private Mock<IRequirementsEditorBodyViewModel> viewModel;
        private RequirementsExportConfiguration configuration;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            var specification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "KUR", Name = "Key-User Requirements" };
            this.configuration = new RequirementsExportConfiguration();

            this.viewModel = new Mock<IRequirementsEditorBodyViewModel>();
            this.viewModel.SetupProperty(x => x.IsExportDialogVisible, true);
            this.viewModel.Setup(x => x.ExportConfiguration).Returns(this.configuration);
            this.viewModel.Setup(x => x.AvailableSpecifications).Returns([specification]);
            this.viewModel.Setup(x => x.GetExportableDefinitionLanguages()).Returns([]);
            this.viewModel.Setup(x => x.GetExportableParameterTypes()).Returns([]);
            this.viewModel.Setup(x => x.GetExportableRelationshipCategories()).Returns([]);
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        private IRenderedComponent<RequirementsExportDialog> RenderDialog()
        {
            return this.context.Render<RequirementsExportDialog>(parameters => parameters.Add(p => p.ViewModel, this.viewModel.Object));
        }

        [Test]
        public void VerifyDialogRendersConfigurationControls()
        {
            var renderedComponent = this.RenderDialog();

            Assert.Multiple(() =>
            {
                Assert.That(renderedComponent.Markup, Does.Contain("File"));
                Assert.That(renderedComponent.Markup, Does.Contain("Specifications"));
                Assert.That(renderedComponent.Markup, Does.Contain("Columns"));
                Assert.That(renderedComponent.Markup, Does.Contain("Definitions"));
                Assert.That(renderedComponent.Markup, Does.Contain("Relationships"));
                Assert.That(renderedComponent.Find("#requirement-export-confirm"), Is.Not.Null);
            });
        }

        [Test]
        public void VerifyExportButtonInvokesViewModelExportAsync()
        {
            var renderedComponent = this.RenderDialog();

            renderedComponent.Find("#requirement-export-confirm").Click();

            this.viewModel.Verify(x => x.ExportAsync(), Times.Once);
        }

        [Test]
        public void VerifyCancelButtonClosesDialogWithoutExporting()
        {
            var renderedComponent = this.RenderDialog();

            var cancelButton = renderedComponent.FindAll("button").Single(x => x.TextContent.Contains("Cancel"));
            cancelButton.Click();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Object.IsExportDialogVisible, Is.False);
                this.viewModel.Verify(x => x.ExportAsync(), Times.Never);
            });
        }
    }
}
