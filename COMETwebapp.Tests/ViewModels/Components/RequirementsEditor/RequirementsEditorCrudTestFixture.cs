// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsEditorCrudTestFixture.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor.ParametricConstraints;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementsEditorCrudTestFixture
    {
        private RequirementsEditorBodyViewModel viewModel;
        private CDPMessageBus messageBus;
        private Mock<ISessionService> sessionService;
        private Iteration iteration;
        private RequirementsSpecification specification;
        private RequirementsGroup group;
        private Requirement requirement;
        private DomainOfExpertise domain;
        private Category requirementCategory;
        private List<Thing> capturedCreateOrUpdate;
        private List<Thing> capturedDelete;
        private List<Thing> capturedDiscardedExpressions;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();
            var session = new Mock<ISession>();

            this.domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            this.requirementCategory = new Category { Iid = Guid.NewGuid(), ShortName = "REQ", Name = "Requirement", PermissibleClass = { ClassKind.Requirement } };

            var rdl = new SiteReferenceDataLibrary { ShortName = "siteRdl", Name = "Site RDL", DefinedCategory = { this.requirementCategory } };
            var siteDirectory = new SiteDirectory { Domain = { this.domain }, SiteReferenceDataLibrary = { rdl } };

            var modelSetup = new EngineeringModelSetup
            {
                Name = "Model", ShortName = "MOD", ActiveDomain = { this.domain },
                RequiredRdl = { new ModelReferenceDataLibrary { RequiredRdl = rdl } }
            };

            siteDirectory.Model.Add(modelSetup);

            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(this.domain);

            this.capturedCreateOrUpdate = null;
            this.capturedDelete = null;
            this.capturedDiscardedExpressions = null;

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((_, things, _) => this.capturedCreateOrUpdate = things.ToList())
                .ReturnsAsync(Result.Ok());

            this.sessionService
                .Setup(x => x.CreateUpdateAndDeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, IReadOnlyCollection<Thing>, NotificationDescription>((_, things, discarded, _) =>
                {
                    this.capturedCreateOrUpdate = things.ToList();
                    this.capturedDiscardedExpressions = discarded.ToList();
                })
                .ReturnsAsync(Result.Ok());

            this.sessionService
                .Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((_, things, _) => this.capturedDelete = things.ToList())
                .ReturnsAsync(Result.Ok());

            this.group = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "GRP", Name = "Group", Owner = this.domain };

            this.requirement = new Requirement
            {
                Iid = Guid.NewGuid(), ShortName = "R01", Name = "Requirement one", Owner = this.domain,
                Definition = { new Definition { LanguageCode = "en", Content = "Original content." } }
            };

            this.specification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "SPEC", Name = "Specification", Owner = this.domain };
            this.specification.Group.Add(this.group);
            this.specification.Requirement.Add(this.requirement);

            this.iteration = new Iteration { Iid = Guid.NewGuid(), Container = new EngineeringModel { EngineeringModelSetup = modelSetup } };
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.viewModel = new RequirementsEditorBodyViewModel(this.sessionService.Object, this.messageBus, new ShowHideDeprecatedThingsService(), new Mock<ILogger<RequirementsEditorBodyViewModel>>().Object)
            {
                CurrentThing = this.iteration
            };
        }

        [TearDown]
        public void TearDown()
        {
            this.viewModel.Dispose();
            this.messageBus.ClearSubscriptions();
            this.messageBus.Dispose();
        }

        [Test]
        public async Task VerifyOpenCreateSpecification()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.OpenCreateSpecification();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnEditMode, Is.True);
                Assert.That(this.viewModel.EditPopupHeader, Is.EqualTo("Create Specification"));
                Assert.That(this.viewModel.EditViewModel.Thing, Is.InstanceOf<RequirementsSpecification>());
                Assert.That(this.viewModel.EditViewModel.IsDeprecatable, Is.True);
                Assert.That(this.viewModel.EditViewModel.ShowGroupSelector, Is.False);
                Assert.That(((RequirementsSpecification)this.viewModel.EditViewModel.Thing).Owner, Is.EqualTo(this.domain));
            });
        }

        [Test]
        public async Task VerifyOpenCreateRequirementUnderGroup()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.SelectedSpecification = this.specification;
            this.viewModel.OpenCreateRequirement(this.group);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnEditMode, Is.True);
                Assert.That(this.viewModel.EditViewModel.Thing, Is.InstanceOf<Requirement>());
                Assert.That(this.viewModel.EditViewModel.ShowGroupSelector, Is.True);
                Assert.That(this.viewModel.EditViewModel.SelectedGroup, Is.EqualTo(this.group));
                Assert.That(this.viewModel.EditViewModel.AvailableGroups, Does.Contain(this.group));
                Assert.That(this.viewModel.EditViewModel.AvailableCategories, Does.Contain(this.requirementCategory));
            });
        }

        [Test]
        public async Task VerifyOpenEditClonesTarget()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.OpenEdit(this.specification);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsOnEditMode, Is.True);
                Assert.That(this.viewModel.EditPopupHeader, Is.EqualTo("Edit Specification"));
                Assert.That(this.viewModel.EditViewModel.Thing, Is.Not.SameAs(this.specification));
                Assert.That(this.viewModel.EditViewModel.Thing.Iid, Is.EqualTo(this.specification.Iid));
            });
        }

        [Test]
        public async Task VerifyCreateSpecificationSaves()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.OpenCreateSpecification();
            var newSpecification = (RequirementsSpecification)this.viewModel.EditViewModel.Thing;
            newSpecification.ShortName = "NEW";
            newSpecification.Name = "New specification";

            await this.viewModel.EditViewModel.OnValidSubmit.InvokeAsync();

            Assert.Multiple(() =>
            {
                this.sessionService.Verify(x => x.CreateUpdateAndDeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);
                Assert.That(this.capturedCreateOrUpdate, Does.Contain(newSpecification));
                Assert.That(this.capturedCreateOrUpdate.OfType<Iteration>().Single().RequirementsSpecification, Does.Contain(newSpecification));
                Assert.That(this.viewModel.IsOnEditMode, Is.False);
            });
        }

        [Test]
        public async Task VerifyCreateRequirementUnderGroupSaves()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.SelectedSpecification = this.specification;
            this.viewModel.OpenCreateRequirement(this.group);
            var newRequirement = (Requirement)this.viewModel.EditViewModel.Thing;
            newRequirement.ShortName = "R99";

            await this.viewModel.EditViewModel.OnValidSubmit.InvokeAsync();

            Assert.Multiple(() =>
            {
                Assert.That(this.capturedCreateOrUpdate, Does.Contain(newRequirement));
                Assert.That(newRequirement.Group, Is.EqualTo(this.group));
                Assert.That(this.capturedCreateOrUpdate.OfType<RequirementsSpecification>().Single().Requirement, Does.Contain(newRequirement));
            });
        }

        [Test]
        public async Task VerifyEditWritesSimpleParameterValues()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.OpenEdit(this.requirement);

            var parameterType = new TextParameterType { Iid = Guid.NewGuid(), ShortName = "txt", Name = "Text" };

            this.viewModel.EditViewModel.RequirementThing.ParameterValue.Add(new SimpleParameterValue
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterType,
                Value = new ValueArray<string>(["5"])
            });

            await this.viewModel.EditViewModel.OnValidSubmit.InvokeAsync();

            Assert.That(this.capturedCreateOrUpdate.OfType<SimpleParameterValue>().Select(x => x.ParameterType), Does.Contain(parameterType));
        }

        [Test]
        public async Task VerifyEditWritesParametricConstraints()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.OpenEdit(this.requirement);

            var parameterType = new TextParameterType { Iid = Guid.NewGuid(), ShortName = "a", Name = "Acceleration" };
            var constraintEditor = new EditParametricConstraintViewModel();
            constraintEditor.InitializeForNew();
            constraintEditor.AddNode(null, new RelationalExpressionRow { ParameterType = parameterType, Value = "5" });
            constraintEditor.AddNode(null, new RelationalExpressionRow { ParameterType = parameterType, Value = "6" });

            var constraint = new ParametricConstraint { Iid = Guid.NewGuid() };
            constraintEditor.BuildInto(constraint);
            this.viewModel.EditViewModel.RequirementThing.ParametricConstraint.Add(constraint);

            await this.viewModel.EditViewModel.OnValidSubmit.InvokeAsync();

            Assert.Multiple(() =>
            {
                Assert.That(this.capturedCreateOrUpdate.OfType<ParametricConstraint>().Select(x => x.Iid), Does.Contain(constraint.Iid));
                Assert.That(this.capturedCreateOrUpdate.OfType<RelationalExpression>().Count(), Is.EqualTo(2), "The constraint's expressions must be written too.");
                Assert.That(this.capturedCreateOrUpdate.OfType<AndExpression>().Count(), Is.EqualTo(1));
            });
        }

        [Test]
        public async Task VerifyNegatingAnExpressionOfAPersistedConstraintDiscardsTheReplacedExpressions()
        {
            var parameterType = new TextParameterType { Iid = Guid.NewGuid(), ShortName = "a", Name = "Acceleration" };

            // A constraint already on the server: (a = 5) AND (a = 6).
            var relationalA = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = parameterType, Value = new ValueArray<string>(["5"]) };
            var relationalB = new RelationalExpression { Iid = Guid.NewGuid(), ParameterType = parameterType, Value = new ValueArray<string>(["6"]) };
            var andExpression = new AndExpression { Iid = Guid.NewGuid() };
            andExpression.Term.Add(relationalA);
            andExpression.Term.Add(relationalB);

            var persistedConstraint = new ParametricConstraint { Iid = Guid.NewGuid() };
            persistedConstraint.Expression.AddRange([relationalA, relationalB, andExpression]);
            persistedConstraint.TopExpression = andExpression;
            this.requirement.ParametricConstraint.Add(persistedConstraint);

            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.OpenEdit(this.requirement);

            var constraintClone = this.viewModel.EditViewModel.RequirementThing.ParametricConstraint.Single();
            var constraintEditor = new EditParametricConstraintViewModel();
            constraintEditor.LoadFrom(constraintClone);

            // Negate one leaf: this mints a NotExpression, so the AndExpression that must reference it can no longer be
            // updated in place and is re-created too.
            var root = (CompositeExpressionRow)constraintEditor.RootExpression;
            constraintEditor.ToggleNot(root.Terms[0]);
            constraintEditor.BuildInto(constraintClone);

            await this.viewModel.EditViewModel.OnValidSubmit.InvokeAsync();

            Assert.Multiple(() =>
            {
                // The replaced AndExpression is deleted rather than left orphaned inside the constraint.
                Assert.That(this.capturedDiscardedExpressions.Select(x => x.Iid), Does.Contain(andExpression.Iid));

                // The relational expressions keep their identity, so their history is preserved.
                Assert.That(this.capturedDiscardedExpressions.Select(x => x.Iid), Does.Not.Contain(relationalA.Iid));
                Assert.That(this.capturedCreateOrUpdate.OfType<RelationalExpression>().Select(x => x.Iid), Does.Contain(relationalA.Iid));

                // Each discarded expression is a clone whose container is the constraint clone, so the transaction can route the delete.
                Assert.That(this.capturedDiscardedExpressions, Is.All.Matches<Thing>(x => ReferenceEquals(x.Container, constraintClone)));
            });
        }

        [Test]
        public async Task VerifyConfirmDeprecationTogglesAndSaves()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.ConfirmDeprecation(this.requirement);

            Assert.That(this.viewModel.ConfirmCancelPopupViewModel.IsVisible, Is.True);

            await this.viewModel.ConfirmCancelPopupViewModel.OnConfirm.InvokeAsync();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ConfirmCancelPopupViewModel.IsVisible, Is.False);
                Assert.That(this.capturedCreateOrUpdate.OfType<Requirement>().Single().IsDeprecated, Is.True);
            });
        }

        [Test]
        public async Task VerifyConfirmDeletionDeletes()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.ConfirmDeletion(this.group);

            Assert.That(this.viewModel.ConfirmCancelPopupViewModel.IsVisible, Is.True);

            await this.viewModel.ConfirmCancelPopupViewModel.OnConfirm.InvokeAsync();

            Assert.Multiple(() =>
            {
                this.sessionService.Verify(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);
                Assert.That(this.capturedDelete.Select(x => x.Iid), Does.Contain(this.group.Iid));
            });
        }

        [Test]
        public async Task VerifySaveInlineDefinition()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            await this.viewModel.SaveInlineDefinitionAsync(this.requirement, "Updated content.");

            Assert.Multiple(() =>
            {
                this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);
                Assert.That(this.capturedCreateOrUpdate.OfType<Requirement>().Single().Definition.First().Content, Is.EqualTo("Updated content."));
            });
        }

        [Test]
        public async Task VerifySaveInlineDefinitionSkipsWhenUnchanged()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            await this.viewModel.SaveInlineDefinitionAsync(this.requirement, "Original content.");

            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Never);
        }
    }
}
