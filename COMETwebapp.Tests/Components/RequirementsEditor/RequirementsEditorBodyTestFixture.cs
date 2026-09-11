// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsEditorBodyTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using DynamicData;

    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.Services.Export;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementsEditorBodyTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<RequirementsEditorBody> renderedComponent;
        private CDPMessageBus messageBus;
        private RequirementsEditorBodyViewModel viewModel;
        private Mock<IDomDataService> domDataService;
        private Requirement requirement;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.messageBus = new CDPMessageBus();

            var domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System Engineering" };
            var category = new Category { Iid = Guid.NewGuid(), ShortName = "KUR", Name = "Key-User Requirements" };

            var group = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "OPERATE", Name = "Operate", Owner = domain };

            this.requirement = new Requirement
            {
                Iid = Guid.NewGuid(), ShortName = "R24", Name = "Provide user with sensor information", Owner = domain,
                Definition = { new Definition { LanguageCode = "en", Content = "The USV SHALL provide sensor information." } },
                Category = { category },
                Group = group
            };

            var massParameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), ShortName = "m", Name = "mass" };
            var kilogramScale = new RatioScale { Iid = Guid.NewGuid(), ShortName = "kg", Name = "kilogram" };
            this.requirement.ParameterValue.Add(new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = massParameterType, Scale = kilogramScale, Value = new ValueArray<string>(["100"]) });

            var relationalExpression = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = massParameterType, RelationalOperator = RelationalOperatorKind.LE, Scale = kilogramScale, Value = new ValueArray<string>(["100"]) };
            this.requirement.ParametricConstraint.Add(new ParametricConstraint { Iid = Guid.NewGuid(), Expression = { relationalExpression }, TopExpression = relationalExpression });

            var specification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "KUR", Name = "Key-User Requirements", Owner = domain };
            specification.Group.Add(group);
            specification.Requirement.Add(this.requirement);

            var iteration = new Iteration { Iid = Guid.NewGuid() };
            iteration.RequirementsSpecification.Add(specification);
            iteration.Relationship.Add(new BinaryRelationship { Iid = Guid.NewGuid(), Source = this.requirement, Target = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT", Name = "Satellite" } });

            var openIterations = new SourceList<Iteration>();
            openIterations.Add(iteration);
            var sessionService = new Mock<ISessionService>();
            sessionService.Setup(x => x.OpenIterations).Returns(openIterations);
            var session = new Mock<ISession>();
            var permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            session.Setup(x => x.PermissionService).Returns(permissionService.Object);
            session.Setup(x => x.OpenReferenceDataLibraries).Returns([]);
            sessionService.Setup(x => x.Session).Returns(session.Object);
            sessionService.Setup(x => x.GetDomainOfExpertise(iteration)).Returns(domain);

            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());

            this.viewModel = new RequirementsEditorBodyViewModel(sessionService.Object, this.messageBus, new ShowHideDeprecatedThingsService(), new Mock<ILogger<RequirementsEditorBodyViewModel>>().Object, new Mock<IExportService>().Object)
            {
                CurrentThing = iteration
            };

            this.domDataService = new Mock<IDomDataService>();

            this.context.Services.AddSingleton(configuration.Object);
            this.context.Services.AddSingleton(this.domDataService.Object);
            this.context.Services.AddSingleton<ICDPMessageBus>(this.messageBus);
            this.context.Services.AddSingleton<IRequirementsEditorBodyViewModel>(this.viewModel);
            this.context.Services.AddSingleton(new Mock<IExportService>().Object);

            this.renderedComponent = this.context.Render<RequirementsEditorBody>();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.viewModel.Dispose();
            this.messageBus.ClearSubscriptions();
            this.messageBus.Dispose();
        }

        [Test]
        public void VerifyDocumentRenders()
        {
            this.renderedComponent.WaitForAssertion(() => Assert.That(this.viewModel.IsLoading, Is.False));

            var tree = this.renderedComponent.FindComponent<RequirementsTree>();
            var document = this.renderedComponent.FindComponent<RequirementsDocument>();
            var markup = this.renderedComponent.Markup;

            Assert.Multiple(() =>
            {
                Assert.That(tree, Is.Not.Null);
                Assert.That(document, Is.Not.Null);
                Assert.That(markup, Does.Contain("R24"));
                Assert.That(markup, Does.Contain("The USV SHALL provide sensor information."));
                Assert.That(markup, Does.Contain("starion-pill"));
                Assert.That(markup, Does.Contain("KUR"));
            });
        }

        [Test]
        public void VerifyScrollTargetIsScrolledIntoViewAndCleared()
        {
            this.renderedComponent.WaitForAssertion(() => Assert.That(this.viewModel.IsLoading, Is.False));

            this.renderedComponent.InvokeAsync(() => this.viewModel.ScrollTarget = this.requirement);

            this.renderedComponent.WaitForAssertion(() =>
            {
                this.domDataService.Verify(x => x.ScrollElementIntoView(RequirementsDocument.RequirementAnchorId(this.requirement)), Times.Once);
                Assert.That(this.viewModel.ScrollTarget, Is.Null, "the target is cleared once scrolled");
            });
        }

        [Test]
        public void VerifyTreeNameToggle()
        {
            this.renderedComponent.WaitForAssertion(() => Assert.That(this.viewModel.IsLoading, Is.False));

            Assert.That(this.renderedComponent.Markup, Does.Contain("KUR"), "the tree labels by short name by default");

            this.renderedComponent.InvokeAsync(() => this.viewModel.TreeUsesShortName = false);

            this.renderedComponent.WaitForAssertion(() => Assert.That(this.renderedComponent.Markup, Does.Contain("Key-User Requirements"), "the tree labels by name when toggled"));
        }

        [Test]
        public void VerifyDisplayModeChangesLayout()
        {
            this.renderedComponent.WaitForAssertion(() => Assert.That(this.viewModel.IsLoading, Is.False));

            Assert.That(this.renderedComponent.Markup, Does.Contain("req-row-header"), "default mode shows the name on its own header line");

            this.renderedComponent.InvokeAsync(() => this.viewModel.DisplayMode = RequirementRowDisplayMode.ShortNameAndDefinition);

            this.renderedComponent.WaitForAssertion(() => Assert.That(this.renderedComponent.Markup, Does.Not.Contain("req-row-header")));
        }

        [Test]
        public void VerifyInlineDefinitionEdit()
        {
            this.renderedComponent.WaitForAssertion(() => Assert.That(this.viewModel.IsLoading, Is.False));

            this.renderedComponent.Find(".req-def-display").Click();

            Assert.That(this.renderedComponent.Markup, Does.Contain("req-def-editor"), "Clicking the definition opens the inline editor.");

            this.renderedComponent.Find(".req-def-cancel").Click();

            Assert.That(this.renderedComponent.Markup, Does.Not.Contain("req-def-editor"), "Cancel closes the inline editor without saving.");

            this.renderedComponent.Find(".req-def-display").Click();
            Assert.That(this.renderedComponent.Markup, Does.Contain("req-def-editor"));

            this.renderedComponent.Find(".req-def-editor").KeyDown(new KeyboardEventArgs { Key = "Escape" });

            Assert.That(this.renderedComponent.Markup, Does.Not.Contain("req-def-editor"), "Escape closes the inline editor without saving.");
        }

        [Test]
        public void VerifyDetailTogglesRevealSections()
        {
            this.renderedComponent.WaitForAssertion(() => Assert.That(this.viewModel.IsLoading, Is.False));

            Assert.Multiple(() =>
            {
                Assert.That(this.renderedComponent.Markup, Does.Not.Contain("req-row-values"));
                Assert.That(this.renderedComponent.Markup, Does.Not.Contain("req-section"));
            });

            this.viewModel.ShowSimpleParameterValues = true;
            this.viewModel.ShowParametricConstraints = true;
            this.viewModel.ShowTraceability = true;

            var document = this.context.Render<RequirementsDocument>(parameters => parameters.Add(p => p.ViewModel, this.viewModel));

            Assert.Multiple(() =>
            {
                Assert.That(document.Markup, Does.Contain("req-row-values"), "the value columns must be on the requirement row");
                Assert.That(document.Markup, Does.Contain("req-value-display"), "the existing value must be shown and clickable");
                Assert.That(document.Markup, Does.Contain("Constraints"));
                Assert.That(document.Markup, Does.Contain("req-expr-leaf"));
                Assert.That(document.Markup, Does.Contain("Traceability"));
                Assert.That(document.Markup, Does.Contain("Satellite"), "the related element definition must be listed");
            });
        }
    }
}
