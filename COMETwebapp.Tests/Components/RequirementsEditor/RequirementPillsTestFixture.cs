// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementPillsTestFixture.cs" company="Starion Group S.A.">
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

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementPillsTestFixture
    {
        private BunitContext context;
        private Mock<IRequirementsEditorBodyViewModel> viewModel;
        private DomainOfExpertise owner;
        private Category category;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.owner = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            this.category = new Category { Iid = Guid.NewGuid(), ShortName = "KUR", Name = "Key-User Requirements" };

            this.viewModel = new Mock<IRequirementsEditorBodyViewModel>();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        private IRenderedComponent<RequirementPills> Render()
        {
            return this.context.Render<RequirementPills>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Owner, this.owner)
                .Add(p => p.Categories, [this.category]));
        }

        [Test]
        public void VerifyPillsRenderWhenEnabled()
        {
            this.viewModel.Setup(x => x.ShowOwner).Returns(true);
            this.viewModel.Setup(x => x.ShowCategory).Returns(true);

            var component = this.Render();

            Assert.Multiple(() =>
            {
                Assert.That(component.Markup, Does.Contain("SYS"), "the owner pill renders");
                Assert.That(component.Markup, Does.Contain("KUR"), "the category pill renders");
            });
        }

        [Test]
        public void VerifyPillsAreHiddenWhenDisabled()
        {
            this.viewModel.Setup(x => x.ShowOwner).Returns(false);
            this.viewModel.Setup(x => x.ShowCategory).Returns(false);

            var component = this.Render();

            Assert.Multiple(() =>
            {
                Assert.That(component.Markup, Does.Not.Contain("SYS"), "the owner pill is hidden");
                Assert.That(component.Markup, Does.Not.Contain("KUR"), "the category pill is hidden");
            });
        }
    }
}
