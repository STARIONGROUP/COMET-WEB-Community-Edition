// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsExportViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.RequirementsEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using CDP4Web.Enumerations;

    using ClosedXML.Excel;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.Export;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementsExportViewModelTestFixture
    {
        private RequirementsExportViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<ISession> session;
        private Mock<IExportService> exportService;
        private ShowHideDeprecatedThingsService showHideService;
        private Iteration iteration;
        private RequirementsSpecification specification;
        private Requirement topRequirement;
        private SimpleQuantityKind massParameterType;
        private SimpleQuantityKind lengthParameterType;

        [SetUp]
        public void SetUp()
        {
            this.massParameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), ShortName = "m", Name = "mass" };
            this.lengthParameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), ShortName = "l", Name = "length" };

            this.topRequirement = new Requirement
            {
                Iid = Guid.NewGuid(), ShortName = "R01", Name = "Top level",
                Definition = { new Definition { LanguageCode = "en", Content = "The system shall exist." } }
            };

            this.topRequirement.ParameterValue.Add(new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = this.massParameterType, Value = new ValueArray<string>(["100"]) });
            this.topRequirement.ParameterValue.Add(new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = this.lengthParameterType, Value = new ValueArray<string>(["2"]) });

            this.specification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "KUR", Name = "Key-User Requirements" };
            this.specification.Requirement.Add(this.topRequirement);

            this.iteration = new Iteration { Iid = Guid.NewGuid() };
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.session = new Mock<ISession>();
            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.Session).Returns(this.session.Object);
            this.exportService = new Mock<IExportService>();
            this.showHideService = new ShowHideDeprecatedThingsService();

            this.viewModel = new RequirementsExportViewModel(this.sessionService.Object, this.exportService.Object, this.showHideService, new Mock<ILogger>().Object);
            this.viewModel.SetIteration(this.iteration);
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
        }

        [Test]
        public async Task VerifyExportAsync()
        {
            var relationalExpression = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = this.massParameterType, RelationalOperator = RelationalOperatorKind.LE, Value = new ValueArray<string>(["100"]) };
            this.topRequirement.ParametricConstraint.Add(new ParametricConstraint { Iid = Guid.NewGuid(), Expression = { relationalExpression }, TopExpression = relationalExpression });

            var elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT" };
            var boundParameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = this.massParameterType };
            boundParameter.ValueSet.Add(new ParameterValueSet { Iid = Guid.NewGuid(), Published = new ValueArray<string>(["100"]) });
            elementDefinition.Parameter.Add(boundParameter);
            this.iteration.Relationship.Add(new BinaryRelationship { Iid = Guid.NewGuid(), Source = boundParameter, Target = relationalExpression });
            this.iteration.Element.Add(elementDefinition);

            this.viewModel.ExportConfiguration.IncludeParametricConstraints = true;

            Stream exported = null;
            this.exportService.Setup(x => x.ExportAndDownloadAsync(It.IsAny<IExporter>()))
                .Returns<IExporter>(exporter =>
                {
                    exported = exporter.Export();
                    return Task.CompletedTask;
                });

            this.viewModel.ExportConfiguration.IncludeConstraintLinkedElementAndValue = true;
            this.viewModel.IsVisible = true;
            await this.viewModel.ExportAsync();
            var dialogClosedAfterExport = this.viewModel.IsVisible;

            using var withLinkWorkbook = new XLWorkbook(exported);
            var withLinkCells = withLinkWorkbook.Worksheets.SelectMany(sheet => sheet.CellsUsed()).Select(cell => cell.GetString()).ToList();

            this.viewModel.ExportConfiguration.IncludeConstraintLinkedElementAndValue = false;
            await this.viewModel.ExportAsync();

            using var withoutLinkWorkbook = new XLWorkbook(exported);
            var withoutLinkCells = withoutLinkWorkbook.Worksheets.SelectMany(sheet => sheet.CellsUsed()).Select(cell => cell.GetString()).ToList();

            this.exportService.Verify(x => x.ExportAndDownloadAsync(It.IsAny<IExporter>()), Times.Exactly(2));

            Assert.Multiple(() =>
            {
                Assert.That(dialogClosedAfterExport, Is.False, "a successful export closes the dialog");
                Assert.That(withLinkCells, Has.Some.Contains("mass"), "the constraint summary renders the parameter type's short name");
                Assert.That(withLinkCells, Has.Some.Contains(boundParameter.ModelCode()), "the linked element's model code is rendered when configured");
                Assert.That(withoutLinkCells, Has.None.Contains("linked:"), "the linked element and value are omitted when not configured");
            });
        }

        [Test]
        public void VerifyGetExportableParameterTypes()
        {
            var expectedParameterTypes = new[] { this.lengthParameterType, this.massParameterType };

            Assert.That(this.viewModel.GetExportableParameterTypes(), Is.EquivalentTo(expectedParameterTypes));
        }

        [Test]
        public void VerifyGetExportableDefinitionLanguages()
        {
            this.topRequirement.Definition.Add(new Definition { LanguageCode = "fr", Content = "Le systeme doit exister." });
            var expectedLanguages = new[] { "en", "fr" };

            Assert.That(this.viewModel.GetExportableDefinitionLanguages(), Is.EqualTo(expectedLanguages));
        }

        [Test]
        public void VerifyGetRelationshipDetails()
        {
            var verifiesCategory = new Category { Iid = Guid.NewGuid(), ShortName = "verifies", Name = "verifies" };
            var rule = new BinaryRelationshipRule { Iid = Guid.NewGuid(), Name = "Requirement verification", ForwardRelationshipName = "verifies", InverseRelationshipName = "is verified by", RelationshipCategory = verifiesCategory };
            var rdl = new SiteReferenceDataLibrary { Iid = Guid.NewGuid() };
            rdl.Rule.Add(rule);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns([rdl]);

            var elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT" };
            var otherRequirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R02" };

            var ruled = new BinaryRelationship { Iid = Guid.NewGuid(), Source = this.topRequirement, Target = elementDefinition, Category = { verifiesCategory } };
            var ruleless = new BinaryRelationship { Iid = Guid.NewGuid(), Source = otherRequirement, Target = this.topRequirement };
            var multi = new MultiRelationship { Iid = Guid.NewGuid(), RelatedThing = { this.topRequirement, otherRequirement, elementDefinition } };
            var unrelated = new BinaryRelationship { Iid = Guid.NewGuid(), Source = otherRequirement, Target = elementDefinition };
            this.iteration.Relationship.AddRange([ruled, ruleless, multi, unrelated]);

            var details = this.viewModel.GetRelationshipDetails(this.topRequirement);
            var ruledDetail = details.Single(x => x.Rule != null);
            var rulelessDetail = details.Single(x => x.Rule == null && x.Direction == RelationshipDirection.Incoming);
            var multiDetail = details.Single(x => x.Direction == RelationshipDirection.Bidirectional);

            this.viewModel.SetIteration(null);

            Assert.Multiple(() =>
            {
                Assert.That(details, Has.Count.EqualTo(3), "the unrelated relationship is skipped and the ruleless binary and multi relationship each yield one detail");
                Assert.That(ruledDetail.Direction, Is.EqualTo(RelationshipDirection.Outgoing));
                Assert.That(ruledDetail.RelatedThings, Is.EqualTo(new Thing[] { elementDefinition }));
                Assert.That(ruledDetail.Rule.ForwardName, Is.EqualTo("verifies"));
                Assert.That(ruledDetail.Rule.InverseName, Is.EqualTo("is verified by"));
                Assert.That(ruledDetail.Rule.IsDirectional, Is.True);
                Assert.That(rulelessDetail.RelatedThings, Is.EqualTo(new Thing[] { otherRequirement }));
                Assert.That(multiDetail.RelatedThings, Is.EqualTo(new Thing[] { otherRequirement, elementDefinition }));
                Assert.That(this.viewModel.GetRelationshipDetails(this.topRequirement), Is.Empty, "no open iteration yields no relationship details");
            });
        }

        [Test]
        public void VerifyGetExportableRelationshipCategories()
        {
            var traceCategory = new Category { Iid = Guid.NewGuid(), ShortName = "trace", Name = "Traces" };
            var verifiesCategory = new Category { Iid = Guid.NewGuid(), ShortName = "verifies", Name = "Verifies" };
            var elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT" };
            var expectedCategories = new[] { traceCategory, verifiesCategory };

            this.iteration.Relationship.AddRange(
            [
                new BinaryRelationship { Iid = Guid.NewGuid(), Source = this.topRequirement, Target = elementDefinition, Category = { verifiesCategory } },
                new BinaryRelationship { Iid = Guid.NewGuid(), Source = this.topRequirement, Target = elementDefinition, Category = { traceCategory, verifiesCategory } }
            ]);

            var categories = this.viewModel.GetExportableRelationshipCategories();

            this.viewModel.SetIteration(null);

            Assert.Multiple(() =>
            {
                Assert.That(categories, Is.EqualTo(expectedCategories), "the distinct categories are returned sorted by name");
                Assert.That(this.viewModel.GetExportableRelationshipCategories(), Is.Empty, "no open iteration yields no categories");
            });
        }
    }
}
