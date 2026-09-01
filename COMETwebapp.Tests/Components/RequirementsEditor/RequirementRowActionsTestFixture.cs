// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementRowActionsTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementRowActionsTestFixture
    {
        private BunitContext context;
        private Mock<IRequirementsEditorBodyViewModel> viewModel;
        private Mock<ISessionService> sessionService;
        private Requirement requirement;
        private DomainOfExpertise owner;
        private Category category;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(this.sessionService.Object);

            this.owner = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            this.category = new Category { Iid = Guid.NewGuid(), ShortName = "KUR", Name = "Key-User Requirements" };

            this.requirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R1", Name = "First requirement", Owner = this.owner };
            this.requirement.Category.Add(this.category);

            this.viewModel = new Mock<IRequirementsEditorBodyViewModel>();
            this.viewModel.Setup(x => x.ShowOwner).Returns(true);
            this.viewModel.Setup(x => x.ShowCategory).Returns(true);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        private IRenderedComponent<RequirementRowActions> Render()
        {
            return this.context.Render<RequirementRowActions>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Requirement, this.requirement));
        }

        [Test]
        public void VerifyPillActionRowsRender()
        {
            var component = this.Render();

            Assert.Multiple(() =>
            {
                Assert.That(component.FindAll(".req-pill-action-row"), Has.Count.EqualTo(2));
                Assert.That(component.Markup, Does.Contain("SYS"), "the owner pill is shown");
                Assert.That(component.Markup, Does.Contain("KUR"), "the category pill is shown");
                Assert.That(component.Markup, Does.Not.Contain("req-deprecated-badge"), "a non-deprecated requirement has no badge");
            });
        }

        [Test]
        public void VerifyInlineLayoutRendersPillsAndBothActionsOnOneRow()
        {
            var component = this.context.Render<RequirementRowActions>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Requirement, this.requirement)
                .Add(p => p.Inline, true));

            Assert.Multiple(() =>
            {
                Assert.That(component.FindAll(".req-pill-action-row"), Is.Empty, "the inline layout does not use the stacked pill-action rows");
                Assert.That(component.FindAll(".req-pills"), Has.Count.EqualTo(1), "the inline layout uses a single RequirementPills block");
                Assert.That(component.Markup, Does.Contain("SYS").And.Contain("KUR"));
                Assert.That(component.FindComponents<DxButton>().Select(x => x.Instance.IconCssClass), Does.Contain(IconName.Edit.GetCssClass()).And.Contain(IconName.Ban.GetCssClass()));
            });
        }

        [Test]
        public void VerifyDeprecatedBadgeShowsWhenRequirementIsDeprecated()
        {
            this.requirement.IsDeprecated = true;

            var component = this.Render();

            Assert.That(component.Markup, Does.Contain("req-deprecated-badge"));
        }

        [Test]
        public void VerifyPillsAreHiddenWhenDisabled()
        {
            this.viewModel.Setup(x => x.ShowOwner).Returns(false);
            this.viewModel.Setup(x => x.ShowCategory).Returns(false);

            var component = this.Render();

            Assert.Multiple(() =>
            {
                Assert.That(component.Markup, Does.Not.Contain("SYS"));
                Assert.That(component.Markup, Does.Not.Contain("KUR"));
            });
        }

        [Test]
        public async Task VerifyEditButtonOpensEdit()
        {
            var component = this.Render();
            var editButton = component.FindComponents<DxButton>().First(x => x.Instance.IconCssClass == IconName.Edit.GetCssClass());

            await component.InvokeAsync(editButton.Instance.Click.InvokeAsync);

            this.viewModel.Verify(x => x.OpenEdit(this.requirement), Times.Once);
        }

        [Test]
        public async Task VerifyDeprecateButtonConfirmsDeprecation()
        {
            var component = this.Render();
            var deprecateButton = component.FindComponents<DxButton>().First(x => x.Instance.IconCssClass == IconName.Ban.GetCssClass());

            await component.InvokeAsync(deprecateButton.Instance.Click.InvokeAsync);

            this.viewModel.Verify(x => x.ConfirmDeprecation(this.requirement), Times.Once);
        }
    }
}
