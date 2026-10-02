// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementRowTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementRowTestFixture
    {
        private BunitContext context;
        private Mock<IRequirementsEditorBodyViewModel> viewModel;
        private Mock<ISessionService> sessionService;
        private Requirement requirement;
        private SimpleQuantityKind parameterType;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddSingleton<ICDPMessageBus>(new CDPMessageBus());
            this.sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(this.sessionService.Object);

            var domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            this.parameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), ShortName = "m", Name = "mass" };
            this.requirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R1", Name = "First requirement", Owner = domain };
            this.requirement.Definition.Add(new Definition { Iid = Guid.NewGuid(), LanguageCode = "en", Content = "The system shall do it." });

            this.viewModel = new Mock<IRequirementsEditorBodyViewModel>();
            this.viewModel.Setup(x => x.DisplayMode).Returns(RequirementRowDisplayMode.ShortNameNameAndDefinition);
            this.viewModel.Setup(x => x.ShowOwner).Returns(true);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        private IRenderedComponent<RequirementRow> Render(bool valuesBelow = false)
        {
            return this.context.Render<RequirementRow>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Requirement, this.requirement)
                .Add(p => p.VisibleParameterTypes, [this.parameterType])
                .Add(p => p.ValuesBelow, valuesBelow));
        }

        [Test]
        public void VerifyDisplayModesRenderTheIdentityAndDefinition()
        {
            Assert.Multiple(() =>
            {
                this.viewModel.Setup(x => x.DisplayMode).Returns(RequirementRowDisplayMode.ShortNameNameAndDefinition);
                Assert.That(this.Render().Markup, Does.Contain("req-row-header").And.Contain("The system shall do it."));

                this.viewModel.Setup(x => x.DisplayMode).Returns(RequirementRowDisplayMode.ShortNameAndDefinition);
                Assert.That(this.Render().Markup, Does.Contain("R1").And.Contain("req-def-display"));

                this.viewModel.Setup(x => x.DisplayMode).Returns(RequirementRowDisplayMode.NameAndDefinition);
                Assert.That(this.Render().Markup, Does.Contain("First requirement"));
            });
        }

        [Test]
        public void VerifyValueColumnsRenderInlineAndBelow()
        {
            this.viewModel.Setup(x => x.ShowSimpleParameterValues).Returns(true);

            var inline = this.Render();
            var below = this.Render(valuesBelow: true);

            Assert.Multiple(() =>
            {
                Assert.That(inline.FindAll(".req-value-cell"), Is.Not.Empty, "a value cell renders for the visible parameter type");
                Assert.That(inline.FindAll(".req-pills-cell"), Has.Count.EqualTo(1), "the pills trail the values inline");
                Assert.That(below.FindAll(".req-pills-corner"), Has.Count.EqualTo(1), "with values-below the pills move to the corner");
            });
        }

        [Test]
        public void VerifyPillsAndActionsRenderWhenValuesAreHidden()
        {
            var component = this.Render();

            Assert.Multiple(() =>
            {
                Assert.That(component.FindAll(".req-value-cell"), Is.Empty);
                Assert.That(component.Markup, Does.Contain("SYS"), "the inline owner pill renders");
                Assert.That(component.FindComponent<RequirementRowActions>().Instance.Inline, Is.True);
            });
        }

        [Test]
        public void VerifyConstraintsAndTraceabilitySectionRendersWhenEnabled()
        {
            this.viewModel.Setup(x => x.ShowTraceability).Returns(true);
            this.viewModel.Setup(x => x.GetTraceability(this.requirement)).Returns([]);

            Assert.That(this.Render().FindComponents<RequirementDetails>(), Has.Count.EqualTo(1));
        }
    }
}
