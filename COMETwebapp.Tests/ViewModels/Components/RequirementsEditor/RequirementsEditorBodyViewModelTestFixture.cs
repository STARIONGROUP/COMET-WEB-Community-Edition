// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsEditorBodyViewModelTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using DynamicData;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementsEditorBodyViewModelTestFixture
    {
        private RequirementsEditorBodyViewModel viewModel;
        private CDPMessageBus messageBus;
        private Mock<ISession> session;
        private Iteration iteration;
        private RequirementsSpecification specification;
        private RequirementsGroup operateGroup;
        private RequirementsGroup c4iGroup;
        private Requirement topRequirement;
        private Requirement operateRequirement;
        private Requirement c4iRequirement;
        private DomainOfExpertise systemDomain;
        private DomainOfExpertise thermalDomain;
        private Category keyUserCategory;
        private RequirementsSpecification deprecatedSpecification;
        private Requirement deprecatedRequirement;
        private ShowHideDeprecatedThingsService showHideService;
        private Mock<ISessionService> sessionService;
        private SimpleQuantityKind massParameterType;
        private SimpleQuantityKind lengthParameterType;
        private SimpleParameterValue massValue;

        [SetUp]
        public void SetUp()
        {
            this.systemDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System Engineering" };
            this.thermalDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "THE", Name = "Thermal" };
            var parentCategory = new Category { Iid = Guid.NewGuid(), ShortName = "REQ", Name = "Requirement" };
            this.keyUserCategory = new Category { Iid = Guid.NewGuid(), ShortName = "KUR", Name = "Key-User Requirements", SuperCategory = { parentCategory } };

            this.c4iGroup = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "C4I", Name = "C4I", Owner = this.systemDomain };
            this.operateGroup = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "OPERATE", Name = "Operate", Owner = this.systemDomain };
            this.operateGroup.Group.Add(this.c4iGroup);

            this.topRequirement = new Requirement
            {
                Iid = Guid.NewGuid(), ShortName = "R01", Name = "Top level", Owner = this.thermalDomain,
                Definition = { new Definition { LanguageCode = "en", Content = "The system shall exist." } }
            };

            this.operateRequirement = new Requirement
            {
                Iid = Guid.NewGuid(), ShortName = "R25", Name = "Minimize user cognitive load", Owner = this.systemDomain,
                Definition = { new Definition { LanguageCode = "en", Content = "The USV SHALL have minimal user cognitive load." } }
            };

            this.c4iRequirement = new Requirement
            {
                Iid = Guid.NewGuid(), ShortName = "R24", Name = "Provide user with sensor information", Owner = this.systemDomain,
                Definition = { new Definition { LanguageCode = "en", Content = "The USV SHALL provide the required sensor information." } },
                Category = { this.keyUserCategory }
            };

            this.operateRequirement.Group = this.operateGroup;
            this.c4iRequirement.Group = this.c4iGroup;

            this.deprecatedRequirement = new Requirement
            {
                Iid = Guid.NewGuid(), ShortName = "R00", Name = "Retired requirement", Owner = this.systemDomain, IsDeprecated = true,
                Definition = { new Definition { LanguageCode = "en", Content = "This requirement is retired." } }
            };

            this.specification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "KUR", Name = "Key-User Requirements", Owner = this.systemDomain };
            this.specification.Group.Add(this.operateGroup);
            this.specification.Requirement.AddRange([this.topRequirement, this.operateRequirement, this.c4iRequirement, this.deprecatedRequirement]);

            this.deprecatedSpecification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "OLD", Name = "Deprecated", Owner = this.systemDomain, IsDeprecated = true };

            this.massParameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), ShortName = "m", Name = "mass" };
            this.lengthParameterType = new SimpleQuantityKind { Iid = Guid.NewGuid(), ShortName = "l", Name = "length" };
            this.massValue = new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = this.massParameterType, Value = new ValueArray<string>(["100"]) };
            this.topRequirement.ParameterValue.Add(this.massValue);
            this.c4iRequirement.ParameterValue.Add(new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = this.lengthParameterType, Value = new ValueArray<string>(["2"]) });
            this.c4iRequirement.ParameterValue.Add(new SimpleParameterValue { Iid = Guid.NewGuid(), ParameterType = this.massParameterType, Value = new ValueArray<string>(["5"]) });

            this.iteration = new Iteration { Iid = Guid.NewGuid() };
            this.iteration.RequirementsSpecification.AddRange([this.specification, this.deprecatedSpecification]);

            this.sessionService = new Mock<ISessionService>();
            var openIterations = new SourceList<Iteration>();
            openIterations.Add(this.iteration);
            this.sessionService.Setup(x => x.OpenIterations).Returns(openIterations);
            this.session = new Mock<ISession>();
            var permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(permissionService.Object);
            this.sessionService.Setup(x => x.Session).Returns(this.session.Object);
            this.sessionService.Setup(x => x.GetDomainOfExpertise(this.iteration)).Returns(this.systemDomain);
            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>())).ReturnsAsync(Result.Ok());
            this.messageBus = new CDPMessageBus();
            this.showHideService = new ShowHideDeprecatedThingsService();

            this.viewModel = new RequirementsEditorBodyViewModel(this.sessionService.Object, this.messageBus, this.showHideService, new Mock<ILogger<RequirementsEditorBodyViewModel>>().Object)
            {
                CurrentThing = this.iteration
            };
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
            this.messageBus.ClearSubscriptions();
            this.messageBus.Dispose();
        }

        [Test]
        public async Task VerifyInitialisation()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableSpecifications, Has.Exactly(1).Items);
                Assert.That(this.viewModel.AvailableSpecifications, Does.Contain(this.specification));
                Assert.That(this.viewModel.SelectedSpecification, Is.EqualTo(this.specification));
                Assert.That(this.viewModel.AvailableOwners, Is.EquivalentTo(new[] { this.systemDomain, this.thermalDomain }));
                Assert.That(this.viewModel.AvailableCategories, Is.EquivalentTo(new[] { this.keyUserCategory }));
                Assert.That(this.viewModel.DisplayMode, Is.EqualTo(RequirementRowDisplayMode.ShortNameNameAndDefinition));
                Assert.That(this.viewModel.IsTocCollapsed, Is.False);
            });
        }

        [Test]
        public async Task VerifyDocumentStructure()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GetGroups(this.specification), Is.EqualTo(new[] { this.operateGroup }));
                Assert.That(this.viewModel.GetGroups(this.operateGroup), Is.EqualTo(new[] { this.c4iGroup }));
                Assert.That(this.viewModel.GetRequirements(this.specification), Is.EqualTo(new[] { this.topRequirement }));
                Assert.That(this.viewModel.GetRequirements(this.operateGroup), Is.EqualTo(new[] { this.operateRequirement }));
                Assert.That(this.viewModel.GetRequirements(this.c4iGroup), Is.EqualTo(new[] { this.c4iRequirement }));
            });
        }

        [Test]
        public async Task VerifySearchFiltersOnDefinition()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.SearchText = "cognitive";

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GetRequirements(this.operateGroup), Is.EqualTo(new[] { this.operateRequirement }));
                Assert.That(this.viewModel.GetRequirements(this.c4iGroup), Is.Empty);
                Assert.That(this.viewModel.GetRequirements(this.specification), Is.Empty);
                Assert.That(this.viewModel.ShouldDisplayGroup(this.c4iGroup), Is.False);
                Assert.That(this.viewModel.GetGroups(this.operateGroup), Is.Empty);
            });

            this.viewModel.SearchText = string.Empty;
            Assert.That(this.viewModel.GetRequirements(this.c4iGroup), Is.EqualTo(new[] { this.c4iRequirement }));
        }

        [Test]
        public async Task VerifyOwnerAndCategoryFilters()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.SelectedOwners = [this.thermalDomain];

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GetRequirements(this.specification), Is.EqualTo(new[] { this.topRequirement }));
                Assert.That(this.viewModel.GetRequirements(this.c4iGroup), Is.Empty);
            });

            this.viewModel.SelectedOwners = [];
            this.viewModel.SelectedCategories = [this.keyUserCategory];

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GetRequirements(this.c4iGroup), Is.EqualTo(new[] { this.c4iRequirement }));
                Assert.That(this.viewModel.GetRequirements(this.operateGroup), Is.Empty);
                Assert.That(this.viewModel.ShouldDisplayGroup(this.operateGroup), Is.True);
            });
        }

        [Test]
        public async Task VerifyCollapseToggle()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.That(this.viewModel.IsTocCollapsed, Is.False);
            this.viewModel.IsTocCollapsed = true;
            Assert.That(this.viewModel.IsTocCollapsed, Is.True);
        }

        [Test]
        public async Task VerifyDirectCategoriesOnly()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            // KUR has super-category REQ; only the directly-assigned KUR must surface, not REQ.
            Assert.That(this.viewModel.AvailableCategories.Select(x => x.ShortName), Is.EqualTo(new[] { "KUR" }));
        }

        [Test]
        public async Task VerifyTogglesAndCollapseState()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ShowOwner, Is.True);
                Assert.That(this.viewModel.ShowCategory, Is.True);
                Assert.That(this.viewModel.IsTreeNodeCollapsed(this.operateGroup.Iid), Is.False);
                Assert.That(this.viewModel.IsDocumentGroupCollapsed(this.operateGroup.Iid), Is.False);
            });

            this.viewModel.ToggleTreeNode(this.operateGroup.Iid);
            this.viewModel.ToggleDocumentGroup(this.operateGroup.Iid);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsTreeNodeCollapsed(this.operateGroup.Iid), Is.True);
                Assert.That(this.viewModel.IsDocumentGroupCollapsed(this.operateGroup.Iid), Is.True);
            });

            this.viewModel.ToggleTreeNode(this.operateGroup.Iid);
            this.viewModel.ToggleDocumentGroup(this.operateGroup.Iid);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsTreeNodeCollapsed(this.operateGroup.Iid), Is.False);
                Assert.That(this.viewModel.IsDocumentGroupCollapsed(this.operateGroup.Iid), Is.False);
            });
        }

        [Test]
        public async Task VerifySessionRefreshPreservesOrResetsSelection()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.session.Object);
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.That(this.viewModel.SelectedSpecification, Is.EqualTo(this.specification));

            this.viewModel.SelectedSpecification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "FOREIGN" };
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.session.Object);
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.That(this.viewModel.SelectedSpecification, Is.EqualTo(this.specification));
        }

        [Test]
        public async Task VerifySessionRefreshWithNullIterationDoesNotThrow()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.CurrentThing = null;

            Assert.DoesNotThrow(() => this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.session.Object));

            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedSpecification, Is.Null);
                Assert.That(this.viewModel.AvailableSpecifications, Is.Empty);
            });
        }

        [Test]
        public async Task VerifyIndentGroupsToggle()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.That(this.viewModel.IndentGroups, Is.True);
            this.viewModel.IndentGroups = false;
            Assert.That(this.viewModel.IndentGroups, Is.False);
        }

        [Test]
        public async Task VerifyDeprecatedThingsRespectGlobalToggle()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableSpecifications, Does.Not.Contain(this.deprecatedSpecification));
                Assert.That(this.viewModel.GetRequirements(this.specification), Does.Not.Contain(this.deprecatedRequirement));
            });

            this.showHideService.ShowDeprecatedThings = true;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableSpecifications, Does.Contain(this.deprecatedSpecification));
                Assert.That(this.viewModel.GetRequirements(this.specification), Does.Contain(this.deprecatedRequirement));
            });
        }

        [Test]
        public async Task VerifyDetailToggleDefaults()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ShowSimpleParameterValues, Is.False);
                Assert.That(this.viewModel.ShowParametricConstraints, Is.False);
                Assert.That(this.viewModel.ShowTraceability, Is.False);
            });

            this.viewModel.ShowSimpleParameterValues = true;
            this.viewModel.ShowParametricConstraints = true;
            this.viewModel.ShowTraceability = true;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ShowSimpleParameterValues, Is.True);
                Assert.That(this.viewModel.ShowParametricConstraints, Is.True);
                Assert.That(this.viewModel.ShowTraceability, Is.True);
            });
        }

        [Test]
        public async Task VerifyGetSpecificationParameterTypes()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            // distinct over all requirements of the selected specification, ordered by short name
            Assert.That(this.viewModel.GetSpecificationParameterTypes(), Is.EqualTo(new ParameterType[] { this.lengthParameterType, this.massParameterType }));

            this.viewModel.SelectedSpecification = null;
            Assert.That(this.viewModel.GetSpecificationParameterTypes(), Is.Empty);
        }

        [Test]
        public async Task VerifyGetSimpleParameterValue()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GetSimpleParameterValue(this.topRequirement, this.massParameterType), Is.EqualTo(this.massValue));
                Assert.That(this.viewModel.GetSimpleParameterValue(this.topRequirement, this.lengthParameterType), Is.Null);
                Assert.That(this.viewModel.GetSimpleParameterValue(this.operateRequirement, this.massParameterType), Is.Null);
            });
        }

        [Test]
        public async Task VerifyUpdateSimpleParameterValue()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            await this.viewModel.UpdateSimpleParameterValue(this.massValue, ["100"]);
            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Never);

            await this.viewModel.UpdateSimpleParameterValue(this.massValue, ["250"]);

            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(
                It.IsAny<Thing>(),
                It.Is<IReadOnlyCollection<Thing>>(things => things.OfType<SimpleParameterValue>().Single().Value.Single() == "250"),
                It.IsAny<NotificationDescription>()), Times.Once);

            Assert.That(this.massValue.Value.Single(), Is.EqualTo("100"), "the original must not be mutated, only its clone");
        }

        [Test]
        public async Task VerifyCreateSimpleParameterValue()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            await this.viewModel.CreateSimpleParameterValue(this.operateRequirement, this.massParameterType);

            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(
                It.IsAny<Thing>(),
                It.Is<IReadOnlyCollection<Thing>>(things =>
                    things.OfType<SimpleParameterValue>().Single().ParameterType == this.massParameterType
                    && things.OfType<SimpleParameterValue>().Single().Value.Single() == "-"
                    && things.OfType<Requirement>().Single().ParameterValue.Count == 1),
                It.IsAny<NotificationDescription>()), Times.Once);

            Assert.That(this.operateRequirement.ParameterValue, Is.Empty, "the original requirement must not be mutated, only its clone");
        }

        [Test]
        public async Task VerifyGetVisibleParameterTypes()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.That(this.viewModel.GetVisibleParameterTypes(), Is.EqualTo(new ParameterType[] { this.lengthParameterType, this.massParameterType }), "no picker selection shows all");

            this.viewModel.SelectedParameterTypeColumns = [this.massParameterType];
            Assert.That(this.viewModel.GetVisibleParameterTypes(), Is.EqualTo(new ParameterType[] { this.massParameterType }));
        }

        [Test]
        public async Task VerifyExpressionCollapseState()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            var iid = Guid.NewGuid();
            Assert.That(this.viewModel.IsExpressionCollapsed(iid), Is.False);

            this.viewModel.ToggleExpression(iid);
            Assert.That(this.viewModel.IsExpressionCollapsed(iid), Is.True);

            this.viewModel.ToggleExpression(iid);
            Assert.That(this.viewModel.IsExpressionCollapsed(iid), Is.False);
        }

        [Test]
        public async Task VerifyGetTopExpressionsAndTerms()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            var firstRelational = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = this.massParameterType, RelationalOperator = RelationalOperatorKind.LE, Value = new ValueArray<string>(["100"]) };
            var secondRelational = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = this.lengthParameterType, RelationalOperator = RelationalOperatorKind.GT, Value = new ValueArray<string>(["2"]) };
            var andExpression = new AndExpression { Iid = Guid.NewGuid(), Term = { firstRelational, secondRelational } };
            var notExpression = new NotExpression { Iid = Guid.NewGuid(), Term = firstRelational };
            var constraint = new ParametricConstraint { Iid = Guid.NewGuid(), Expression = { andExpression, firstRelational, secondRelational } };

            Assert.Multiple(() =>
            {
                // no TopExpression set: fall back to the expressions that are not a term of another expression
                Assert.That(this.viewModel.GetTopExpressions(constraint), Is.EqualTo(new BooleanExpression[] { andExpression }));
                Assert.That(this.viewModel.GetTerms(andExpression), Is.EqualTo(new BooleanExpression[] { firstRelational, secondRelational }));
                Assert.That(this.viewModel.GetTerms(notExpression), Is.EqualTo(new BooleanExpression[] { firstRelational }));
                Assert.That(this.viewModel.GetTerms(firstRelational), Is.Empty);
            });

            constraint.TopExpression = firstRelational;
            Assert.That(this.viewModel.GetTopExpressions(constraint), Is.EqualTo(new BooleanExpression[] { firstRelational }));
        }

        [Test]
        public async Task VerifyGetBoundParameterModelCode()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            var relationalExpression = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = this.massParameterType };
            var elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT" };
            var parameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = this.massParameterType };
            parameter.ValueSet.Add(new ParameterValueSet { Iid = Guid.NewGuid(), Published = new ValueArray<string>(["42"]) });
            elementDefinition.Parameter.Add(parameter);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GetBoundParameter(relationalExpression), Is.Null);
                Assert.That(this.viewModel.GetBoundParameterModelCode(relationalExpression), Is.Null);
                Assert.That(this.viewModel.GetBoundParameterPublishedValue(relationalExpression), Is.Null);
            });

            this.iteration.Relationship.Add(new BinaryRelationship { Iid = Guid.NewGuid(), Source = parameter, Target = relationalExpression });

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GetBoundParameter(relationalExpression), Is.EqualTo(parameter));
                Assert.That(this.viewModel.GetBoundParameterModelCode(relationalExpression), Is.EqualTo(parameter.ModelCode()));
                Assert.That(this.viewModel.GetBoundParameterPublishedValue(relationalExpression), Is.EqualTo("42"));
            });

            // a second value set (e.g. option/state-dependent) makes the published value ambiguous, so none is shown
            parameter.ValueSet.Add(new ParameterValueSet { Iid = Guid.NewGuid(), Published = new ValueArray<string>(["7"]) });
            Assert.That(this.viewModel.GetBoundParameterPublishedValue(relationalExpression), Is.Null);
        }

        [Test]
        public async Task VerifyGetBoundParameters()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            var relationalExpression = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = this.massParameterType };
            Assert.That(this.viewModel.GetBoundParameters(relationalExpression), Is.Empty);

            var elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT" };
            var parameterA = new Parameter { Iid = Guid.NewGuid(), ParameterType = this.massParameterType };
            var parameterB = new Parameter { Iid = Guid.NewGuid(), ParameterType = this.massParameterType };
            elementDefinition.Parameter.AddRange([parameterA, parameterB]);

            this.iteration.Relationship.AddRange([
                new BinaryRelationship { Iid = Guid.NewGuid(), Source = relationalExpression, Target = parameterA },
                new BinaryRelationship { Iid = Guid.NewGuid(), Source = parameterB, Target = relationalExpression }
            ]);

            Assert.That(this.viewModel.GetBoundParameters(relationalExpression), Is.EquivalentTo(new ParameterOrOverrideBase[] { parameterA, parameterB }));

            this.viewModel.CurrentThing = null;
            Assert.That(this.viewModel.GetBoundParameters(relationalExpression), Is.Empty);
        }

        [Test]
        public async Task VerifyGetLinkableParameters()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            var relationalExpression = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = this.massParameterType };
            Assert.That(this.viewModel.GetLinkableParameters(new RelationalExpression { Iid = Guid.NewGuid() }), Is.Empty, "a null parameter type yields no candidates");

            var elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT" };
            var matchingParameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = this.massParameterType };
            var otherParameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = this.lengthParameterType };
            elementDefinition.Parameter.AddRange([matchingParameter, otherParameter]);

            var elementUsage = new ElementUsage { Iid = Guid.NewGuid(), ShortName = "sat_1", ElementDefinition = elementDefinition };
            var matchingOverride = new ParameterOverride { Iid = Guid.NewGuid(), Parameter = matchingParameter };
            elementUsage.ParameterOverride.Add(matchingOverride);
            elementDefinition.ContainedElement.Add(elementUsage);

            this.iteration.Element.Add(elementDefinition);

            Assert.That(this.viewModel.GetLinkableParameters(relationalExpression), Is.EqualTo(new ParameterOrOverrideBase[] { matchingParameter, matchingOverride }));

            this.viewModel.CurrentThing = null;
            Assert.That(this.viewModel.GetLinkableParameters(relationalExpression), Is.Empty);
        }

        [Test]
        public async Task VerifyUpdateParameterLinksAsync()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.sessionService.Setup(x => x.CreateUpdateAndDeleteThingsWithNotification(
                It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>())).ReturnsAsync(Result.Ok());

            var relationalExpression = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = this.massParameterType };
            var elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT" };
            var parameterA = new Parameter { Iid = Guid.NewGuid(), ParameterType = this.massParameterType };
            var parameterB = new Parameter { Iid = Guid.NewGuid(), ParameterType = this.massParameterType };
            elementDefinition.Parameter.AddRange([parameterA, parameterB]);
            this.iteration.Element.Add(elementDefinition);

            // Case A: expression currently unbound, select parameterA -> one create, no delete
            var resultA = await this.viewModel.UpdateParameterLinksAsync(relationalExpression, [parameterA]);

            Assert.Multiple(() =>
            {
                Assert.That(resultA.IsSuccess, Is.True);

                this.sessionService.Verify(x => x.CreateUpdateAndDeleteThingsWithNotification(
                    It.IsAny<Thing>(),
                    It.Is<IReadOnlyCollection<Thing>>(things => things.OfType<BinaryRelationship>().Count() == 1
                        && things.OfType<Iteration>().Count() == 1
                        && things.OfType<BinaryRelationship>().Single().Source == parameterA
                        && things.OfType<BinaryRelationship>().Single().Target == relationalExpression
                        && things.OfType<BinaryRelationship>().Single().Owner == this.systemDomain),
                    It.Is<IReadOnlyCollection<Thing>>(things => things.Count == 0),
                    It.IsAny<NotificationDescription>()), Times.Once);
            });

            // simulate the created relationship now existing so the next calls diff against it
            this.iteration.Relationship.Add(new BinaryRelationship { Iid = Guid.NewGuid(), Source = relationalExpression, Target = parameterA });
            this.sessionService.Invocations.Clear();

            // Case B: unchanged selection must not call the session
            var resultB = await this.viewModel.UpdateParameterLinksAsync(relationalExpression, [parameterA]);

            Assert.Multiple(() =>
            {
                Assert.That(resultB.IsSuccess, Is.True);

                this.sessionService.Verify(x => x.CreateUpdateAndDeleteThingsWithNotification(
                    It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Never);
            });

            // Case C: swap parameterA for parameterB -> one create, one delete
            var resultC = await this.viewModel.UpdateParameterLinksAsync(relationalExpression, [parameterB]);

            Assert.Multiple(() =>
            {
                Assert.That(resultC.IsSuccess, Is.True);

                this.sessionService.Verify(x => x.CreateUpdateAndDeleteThingsWithNotification(
                    It.IsAny<Thing>(),
                    It.Is<IReadOnlyCollection<Thing>>(things => things.OfType<BinaryRelationship>().Count() == 1
                        && things.OfType<BinaryRelationship>().Single().Source == parameterB),
                    It.Is<IReadOnlyCollection<Thing>>(things => things.Count == 1),
                    It.IsAny<NotificationDescription>()), Times.Once);
            });
        }

        [Test]
        public void VerifyCanMoveGroup()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CanMoveGroup(this.c4iGroup, this.specification), Is.True, "a nested group can be promoted to the specification");
                Assert.That(this.viewModel.CanMoveGroup(this.c4iGroup, this.operateGroup), Is.False, "moving onto the current parent is a no-op");
                Assert.That(this.viewModel.CanMoveGroup(this.operateGroup, this.specification), Is.False, "the group is already directly under the specification");
                Assert.That(this.viewModel.CanMoveGroup(this.operateGroup, this.c4iGroup), Is.False, "a group cannot be nested under its own descendant");
                Assert.That(this.viewModel.CanMoveGroup(this.c4iGroup, this.c4iGroup), Is.False, "a group cannot be nested under itself");
                Assert.That(this.viewModel.CanMoveGroup(null, this.specification), Is.False);
            });
        }

        [Test]
        public async Task VerifyMoveGroupAsync()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            var invalid = await this.viewModel.MoveGroupAsync(this.operateGroup, this.c4iGroup);

            Assert.Multiple(() =>
            {
                Assert.That(invalid.IsSuccess, Is.False, "a cyclic move is rejected");
                this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Never);
            });

            var result = await this.viewModel.MoveGroupAsync(this.c4iGroup, this.specification);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);

                // mirrors the IME: only the new container (the specification) is written, with the moved group added
                this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(
                    It.IsAny<Thing>(),
                    It.Is<IReadOnlyCollection<Thing>>(things =>
                        things.OfType<RequirementsSpecification>().Single(s => s.Iid == this.specification.Iid).Group.Any(g => g.Iid == this.c4iGroup.Iid)
                        && things.All(t => t.Iid != this.operateGroup.Iid)),
                    It.IsAny<NotificationDescription>()), Times.Once);
            });
        }

        [Test]
        public async Task VerifyGetExpressionSummary()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            var relational = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = this.massParameterType, RelationalOperator = RelationalOperatorKind.LE, Value = new ValueArray<string>(["100"]) };
            var notExpression = new NotExpression { Iid = Guid.NewGuid(), Term = relational };
            var andExpression = new AndExpression { Iid = Guid.NewGuid(), Term = { notExpression, relational } };

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GetExpressionSummary(relational), Is.EqualTo("m ≤ 100"));
                Assert.That(this.viewModel.GetExpressionSummary(notExpression), Does.Contain("NOT"), "the NOT operator must not be dropped from the summary");
                Assert.That(this.viewModel.GetExpressionSummary(andExpression), Does.Contain("NOT").And.Contain("AND"));
            });
        }

        [Test]
        public async Task VerifyTreeUsesShortNameToggle()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.That(this.viewModel.TreeUsesShortName, Is.True);
            this.viewModel.TreeUsesShortName = false;
            Assert.That(this.viewModel.TreeUsesShortName, Is.False);
        }

        [Test]
        public async Task VerifyGetTraceability()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            var verifiesCategory = new Category { Iid = Guid.NewGuid(), ShortName = "verifies", Name = "verifies" };
            var strictlyVerifiesCategory = new Category { Iid = Guid.NewGuid(), ShortName = "strictVerifies", Name = "strictly verifies", SuperCategory = { verifiesCategory } };
            var rule = new BinaryRelationshipRule { Iid = Guid.NewGuid(), Name = "Requirement verification", RelationshipCategory = verifiesCategory };
            var rdl = new SiteReferenceDataLibrary { Iid = Guid.NewGuid() };
            rdl.Rule.Add(rule);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns([rdl]);

            var elementDefinition = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT" };

            // the relationship carries a SUB-category of the rule's category; the rule must still apply
            var outgoing = new BinaryRelationship { Iid = Guid.NewGuid(), Source = this.topRequirement, Target = elementDefinition, Category = { strictlyVerifiesCategory } };
            var incoming = new BinaryRelationship { Iid = Guid.NewGuid(), Source = this.c4iRequirement, Target = this.topRequirement };
            var multi = new MultiRelationship { Iid = Guid.NewGuid(), RelatedThing = { this.topRequirement, this.operateRequirement, elementDefinition } };
            var unrelated = new BinaryRelationship { Iid = Guid.NewGuid(), Source = this.c4iRequirement, Target = elementDefinition };
            this.iteration.Relationship.AddRange([outgoing, incoming, multi, unrelated]);

            var rows = this.viewModel.GetTraceability(this.topRequirement);

            Assert.That(rows, Has.Count.EqualTo(3));

            var outgoingRow = rows.Single(x => x.Relationship == outgoing);
            var incomingRow = rows.Single(x => x.Relationship == incoming);
            var multiRow = rows.Single(x => x.Relationship == multi);

            Assert.Multiple(() =>
            {
                Assert.That(outgoingRow.Direction, Is.EqualTo(RelationshipDirection.Outgoing));
                Assert.That(outgoingRow.RelatedThings, Is.EqualTo(new Thing[] { elementDefinition }));
                Assert.That(outgoingRow.RuleNames, Is.EqualTo(new[] { "Requirement verification" }));
                Assert.That(incomingRow.Direction, Is.EqualTo(RelationshipDirection.Incoming));
                Assert.That(incomingRow.RelatedThings, Is.EqualTo(new Thing[] { this.c4iRequirement }));
                Assert.That(incomingRow.RuleNames, Is.Empty);
                Assert.That(multiRow.Direction, Is.EqualTo(RelationshipDirection.Bidirectional));
                Assert.That(multiRow.RelatedThings, Is.EqualTo(new Thing[] { this.operateRequirement, elementDefinition }));
            });
        }

        [Test]
        public async Task VerifyNavigateToRequirement()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.ToggleDocumentGroup(this.operateGroup.Iid);
            this.viewModel.ToggleDocumentGroup(this.c4iGroup.Iid);
            this.viewModel.SelectedSpecification = this.deprecatedSpecification;
            this.viewModel.SearchText = "nomatch";
            this.viewModel.SelectedOwners = [this.thermalDomain];
            this.viewModel.SelectedCategories = [this.keyUserCategory];

            this.viewModel.NavigateToRequirement(this.c4iRequirement);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedSpecification, Is.EqualTo(this.specification));
                Assert.That(this.viewModel.ScrollTarget, Is.EqualTo(this.c4iRequirement));
                Assert.That(this.viewModel.IsDocumentGroupCollapsed(this.c4iGroup.Iid), Is.False);
                Assert.That(this.viewModel.IsDocumentGroupCollapsed(this.operateGroup.Iid), Is.False);

                // filters are cleared so the target renders (its anchor exists to scroll to)
                Assert.That(this.viewModel.SearchText, Is.Null);
                Assert.That(this.viewModel.SelectedOwners, Is.Empty);
                Assert.That(this.viewModel.SelectedCategories, Is.Empty);
                Assert.That(this.viewModel.GetRequirements(this.c4iGroup), Does.Contain(this.c4iRequirement));
                Assert.That(this.showHideService.ShowDeprecatedThings, Is.False, "a non-deprecated target does not flip the global toggle");
            });
        }

        [Test]
        public async Task VerifySaveInlineDefinitionUsesDefaultLanguage()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            var requirementWithoutDefinition = new Requirement { Iid = Guid.NewGuid(), ShortName = "R99", Name = "No definition yet", Owner = this.systemDomain };
            this.specification.Requirement.Add(requirementWithoutDefinition);

            var siteDirectory = new SiteDirectory { Iid = Guid.NewGuid(), NaturalLanguage = { new NaturalLanguage { LanguageCode = "fr", Name = "French" } } };
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);

            Assert.That(requirementWithoutDefinition.Definition, Is.Empty);

            await this.viewModel.SaveInlineDefinitionAsync(requirementWithoutDefinition, "Some new definition text.");

            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(
                It.IsAny<Thing>(),
                It.Is<IReadOnlyCollection<Thing>>(things => things.OfType<Definition>().Any(d => d.LanguageCode == "fr" && d.Content == "Some new definition text.")),
                It.IsAny<NotificationDescription>()), Times.Once);
        }

        [Test]
        public async Task VerifySaveReloadsOnlyOnceViaEndUpdate()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            // a brand-new owner the cached AvailableOwners does not yet know about; it only appears when the document reloads
            var newOwner = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "PWR", Name = "Power" };
            this.specification.Requirement.Add(new Requirement { Iid = Guid.NewGuid(), ShortName = "R42", Name = "Added", Owner = newOwner });

            await this.viewModel.SaveInlineDefinitionAsync(this.topRequirement, "A changed definition.");

            // the save no longer reloads the document on its own — that redundant reload was the second refresh (GH897)
            Assert.That(this.viewModel.AvailableOwners, Does.Not.Contain(newOwner), "a save must not reload the document on its own");

            // the single reload is driven centrally by the EndUpdate session event the write raises
            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.EndUpdate));
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.That(this.viewModel.AvailableOwners, Does.Contain(newOwner), "OnEndUpdate reloads the document exactly once");
        }

        [Test]
        public async Task VerifyNavigateToGroup()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            // collapse the ancestor so the nested target would otherwise be hidden in the document
            this.viewModel.ToggleDocumentGroup(this.operateGroup.Iid);
            Assert.That(this.viewModel.IsDocumentGroupCollapsed(this.operateGroup.Iid), Is.True);

            this.viewModel.NavigateToGroup(this.c4iGroup);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ScrollTargetGroup, Is.EqualTo(this.c4iGroup), "the clicked group is flagged as the scroll target");
                Assert.That(this.viewModel.IsDocumentGroupCollapsed(this.operateGroup.Iid), Is.False, "the target's ancestor groups are expanded so it renders");
            });
        }

        [Test]
        public async Task VerifyNavigateToDeprecatedRequirementShowsDeprecated()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.That(this.showHideService.ShowDeprecatedThings, Is.False);

            this.viewModel.NavigateToRequirement(this.deprecatedRequirement);

            Assert.Multiple(() =>
            {
                Assert.That(this.showHideService.ShowDeprecatedThings, Is.True, "navigating to a deprecated target reveals deprecated things so its anchor renders");
                Assert.That(this.viewModel.SelectedSpecification, Is.EqualTo(this.specification));
                Assert.That(this.viewModel.ScrollTarget, Is.EqualTo(this.deprecatedRequirement));
            });
        }
    }
}
