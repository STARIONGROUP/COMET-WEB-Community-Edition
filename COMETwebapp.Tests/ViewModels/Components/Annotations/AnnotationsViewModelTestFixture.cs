// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AnnotationsViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.Annotations
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components;

    using COMETwebapp.ViewModels.Components.Annotations;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="AnnotationsViewModel" />
    /// </summary>
    [TestFixture]
    public class AnnotationsViewModelTestFixture
    {
        /// <summary>
        /// The reference date the <see cref="EngineeringModelDataAnnotation.CreatedOn" /> of the annotations is computed from
        /// </summary>
        private static readonly DateTime BaseDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// The <see cref="AnnotationsViewModel" /> under test
        /// </summary>
        private AnnotationsViewModel viewModel;

        /// <summary>
        /// The mocked <see cref="ISessionService" />
        /// </summary>
        private Mock<ISessionService> sessionService;

        /// <summary>
        /// The mocked <see cref="ISession" />
        /// </summary>
        private Mock<ISession> session;

        /// <summary>
        /// The mocked <see cref="IPermissionService" />
        /// </summary>
        private Mock<IPermissionService> permissionService;

        /// <summary>
        /// The mocked <see cref="ILogger" />
        /// </summary>
        private Mock<ILogger> logger;

        /// <summary>
        /// The <see cref="ConfirmCancelPopupViewModel" /> used to confirm deletions
        /// </summary>
        private ConfirmCancelPopupViewModel confirmCancelPopup;

        /// <summary>
        /// The things passed to <see cref="ISessionService.CreateOrUpdateThingsWithNotification(Thing, IReadOnlyCollection{Thing}, NotificationDescription)" />
        /// </summary>
        private List<(Thing TopContainer, List<Thing> Things)> writes;

        /// <summary>
        /// The things passed to <see cref="ISessionService.DeleteThingsWithNotification" />
        /// </summary>
        private List<(Thing TopContainer, List<Thing> Things)> deletes;

        /// <summary>
        /// The <see cref="NotificationDescription" />s passed along the writes and the deletes, in call order
        /// </summary>
        private List<NotificationDescription> notifications;

        /// <summary>
        /// The active <see cref="Person" />
        /// </summary>
        private Person person;

        /// <summary>
        /// The <see cref="Participant" /> of the active <see cref="Person" /> in the model
        /// </summary>
        private Participant participant;

        /// <summary>
        /// The <see cref="DomainOfExpertise" /> the active user works for
        /// </summary>
        private DomainOfExpertise domain;

        /// <summary>
        /// Another active <see cref="DomainOfExpertise" /> of the model, which an annotation can be handed over to
        /// </summary>
        private DomainOfExpertise engineeringDomain;

        /// <summary>
        /// A <see cref="DomainOfExpertise" /> that is no longer an active domain of the model
        /// </summary>
        private DomainOfExpertise archivedDomain;

        /// <summary>
        /// A non-deprecated <see cref="Category" /> of the site reference data library for change requests
        /// </summary>
        private Category alphaCategory;

        /// <summary>
        /// A non-deprecated <see cref="Category" /> of the site reference data library for change requests and review item discrepancies
        /// </summary>
        private Category betaCategory;

        /// <summary>
        /// A non-deprecated <see cref="Category" /> of the model reference data library for change requests
        /// </summary>
        private Category gammaCategory;

        /// <summary>
        /// A deprecated <see cref="Category" /> for change requests
        /// </summary>
        private Category deprecatedCategory;

        /// <summary>
        /// A non-deprecated <see cref="Category" /> that only applies to requests for waiver
        /// </summary>
        private Category waiverCategory;

        /// <summary>
        /// The <see cref="EngineeringModel" /> holding the annotations
        /// </summary>
        private EngineeringModel model;

        /// <summary>
        /// The <see cref="Iteration" /> of the model
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// The requirement carrying every annotation but <see cref="waiver" />
        /// </summary>
        private Requirement requirement;

        /// <summary>
        /// The requirement only carrying a closed <see cref="waiver" />
        /// </summary>
        private Requirement otherRequirement;

        /// <summary>
        /// A requirement without any annotation
        /// </summary>
        private Requirement bareRequirement;

        /// <summary>
        /// An <see cref="ElementDefinition" /> of the iteration, which is no requirement and carries no annotation
        /// </summary>
        private ElementDefinition element;

        /// <summary>
        /// The <see cref="RequirementsSpecification" /> containing the requirements
        /// </summary>
        private RequirementsSpecification specification;

        /// <summary>
        /// An open <see cref="ChangeRequest" /> about <see cref="requirement" />
        /// </summary>
        private ChangeRequest changeRequest;

        /// <summary>
        /// A <see cref="ChangeProposal" /> answering <see cref="changeRequest" />
        /// </summary>
        private ChangeProposal changeProposal;

        /// <summary>
        /// A <see cref="ReviewItemDiscrepancy" /> with status done about <see cref="requirement" />
        /// </summary>
        private ReviewItemDiscrepancy discrepancy;

        /// <summary>
        /// An <see cref="EngineeringModelDataNote" /> about <see cref="requirement" />
        /// </summary>
        private EngineeringModelDataNote note;

        /// <summary>
        /// An <see cref="ActionItem" /> about <see cref="requirement" />, which the view model does not handle
        /// </summary>
        private ActionItem actionItem;

        /// <summary>
        /// A closed <see cref="RequestForWaiver" /> about <see cref="otherRequirement" />
        /// </summary>
        private RequestForWaiver waiver;

        [SetUp]
        public void SetUp()
        {
            this.person = new Person { Iid = Guid.NewGuid(), ShortName = "author", GivenName = "Given", Surname = "Author" };
            this.participant = new Participant { Iid = Guid.NewGuid(), Person = this.person };
            this.domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };

            this.engineeringDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "ENG", Name = "Engineering" };
            this.archivedDomain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "ARC", Name = "Archive" };

            var setup = new EngineeringModelSetup { Iid = Guid.NewGuid(), ShortName = "model" };
            setup.Participant.Add(this.participant);
            setup.ActiveDomain.AddRange([this.domain, this.engineeringDomain]);

            this.alphaCategory = new Category { Iid = Guid.NewGuid(), ShortName = "ALPHA", Name = "Alpha", PermissibleClass = [ClassKind.ChangeRequest] };
            this.betaCategory = new Category { Iid = Guid.NewGuid(), ShortName = "BETA", Name = "Beta", PermissibleClass = [ClassKind.ChangeRequest, ClassKind.ReviewItemDiscrepancy] };
            this.gammaCategory = new Category { Iid = Guid.NewGuid(), ShortName = "GAMMA", Name = "Gamma", PermissibleClass = [ClassKind.ChangeRequest] };
            this.deprecatedCategory = new Category { Iid = Guid.NewGuid(), ShortName = "OLD", Name = "Aaa deprecated", IsDeprecated = true, PermissibleClass = [ClassKind.ChangeRequest] };
            this.waiverCategory = new Category { Iid = Guid.NewGuid(), ShortName = "WAIVER", Name = "Waiver only", PermissibleClass = [ClassKind.RequestForWaiver] };

            var siteLibrary = new SiteReferenceDataLibrary { Iid = Guid.NewGuid(), ShortName = "site" };
            siteLibrary.DefinedCategory.AddRange([this.betaCategory, this.alphaCategory, this.deprecatedCategory, this.waiverCategory]);

            // the same category defined again in the model library must be offered once
            var modelLibrary = new ModelReferenceDataLibrary { Iid = Guid.NewGuid(), ShortName = "mdl", RequiredRdl = siteLibrary };
            modelLibrary.DefinedCategory.AddRange([this.gammaCategory, new Category { Iid = this.alphaCategory.Iid, ShortName = "ALPHA", Name = "Alpha", PermissibleClass = [ClassKind.ChangeRequest] }]);
            setup.RequiredRdl.Add(modelLibrary);

            this.model = new EngineeringModel { Iid = Guid.NewGuid(), EngineeringModelSetup = setup };
            this.iteration = new Iteration { Iid = Guid.NewGuid() };
            this.model.Iteration.Add(this.iteration);

            this.requirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "REQ-1" };
            this.otherRequirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "REQ-2" };
            this.bareRequirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "REQ-3" };

            this.specification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "SPEC" };
            this.specification.Requirement.AddRange([this.requirement, this.otherRequirement, this.bareRequirement]);
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.element = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT", Name = "Satellite" };
            this.iteration.Element.Add(this.element);

            this.changeRequest = (ChangeRequest)this.AddAnnotation(AnnotationKind.ChangeRequest, this.requirement, "CR_001", AnnotationStatusKind.OPEN, 1);
            this.changeProposal = (ChangeProposal)this.AddAnnotation(AnnotationKind.ChangeProposal, this.requirement, "CP_001", AnnotationStatusKind.OPEN, 2);
            this.changeProposal.ChangeRequest = this.changeRequest;
            this.discrepancy = (ReviewItemDiscrepancy)this.AddAnnotation(AnnotationKind.ReviewItemDiscrepancy, this.requirement, "RID_001", AnnotationStatusKind.DONE, 3);
            this.note = (EngineeringModelDataNote)this.AddAnnotation(AnnotationKind.Note, this.requirement, null, AnnotationStatusKind.OPEN, 4);
            this.waiver = (RequestForWaiver)this.AddAnnotation(AnnotationKind.RequestForWaiver, this.otherRequirement, "RFW_001", AnnotationStatusKind.CLOSED, 5);

            this.actionItem = new ActionItem
            {
                Iid = Guid.NewGuid(),
                ShortName = "ACT_001",
                Status = AnnotationStatusKind.OPEN,
                PrimaryAnnotatedThing = new ModellingThingReference(this.requirement) { Iid = Guid.NewGuid() }
            };

            this.model.ModellingAnnotation.Add(this.actionItem);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.session = new Mock<ISession>();
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.writes = [];
            this.deletes = [];
            this.notifications = [];

            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.Session).Returns(this.session.Object);
            this.sessionService.Setup(x => x.IsReadOnly).Returns(false);

            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((top, things, notification) =>
                {
                    this.writes.Add((top, things.ToList()));
                    this.notifications.Add(notification);
                })
                .ReturnsAsync(Result.Ok());

            this.sessionService.Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((top, things, notification) =>
                {
                    this.deletes.Add((top, things.ToList()));
                    this.notifications.Add(notification);
                })
                .ReturnsAsync(Result.Ok());

            this.logger = new Mock<ILogger>();
            this.confirmCancelPopup = new ConfirmCancelPopupViewModel();

            this.viewModel = new AnnotationsViewModel(this.sessionService.Object, this.confirmCancelPopup, this.logger.Object);
            this.viewModel.SetIteration(this.iteration, this.domain);
        }

        [Test]
        public void VerifySetIteration()
        {
            this.viewModel.OpenPanel(this.requirement);
            this.viewModel.SetIteration(this.iteration, this.domain);

            Assert.That(this.viewModel.IsPanelOpen, Is.True, "the active requirement is still part of the iteration");

            this.viewModel.StartNew(AnnotationKind.Note);
            Assert.That(this.viewModel.Draft, Is.Not.Null);

            this.viewModel.SetIteration(null, this.domain);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.IsPanelOpen, Is.False);
                Assert.That(this.viewModel.ActiveThing, Is.Null);
                Assert.That(this.viewModel.Draft, Is.Null);
                Assert.That(this.viewModel.GetAnnotations(this.requirement), Is.Empty);
            }

            // an iteration without a model holds no annotation, so a thing of the former one is gone
            var orphanIteration = new Iteration { Iid = Guid.NewGuid() };

            this.viewModel.SetIteration(this.iteration, this.domain);
            this.viewModel.OpenPanel(this.requirement);
            this.viewModel.StartNew(AnnotationKind.Note);
            this.viewModel.SetIteration(orphanIteration, this.domain);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.IsPanelOpen, Is.False, "the other iteration has no model and does not contain the active requirement");
                Assert.That(this.viewModel.Draft, Is.Null);
                Assert.That(this.viewModel.GetAnnotations(this.requirement), Is.Empty);
            }

            // another iteration of the same model: the requirement is still in the model, but not in the open iteration
            var siblingIteration = new Iteration { Iid = Guid.NewGuid() };
            this.model.Iteration.Add(siblingIteration);

            this.viewModel.SetIteration(this.iteration, this.domain);
            this.viewModel.OpenPanel(this.requirement);
            this.viewModel.SetIteration(siblingIteration, this.domain);

            Assert.That(this.viewModel.IsPanelOpen, Is.False, "switching to another iteration of the model closes the panel on a thing of the former one");
        }

        [Test]
        public void VerifySetIterationClosesThePanelOfAThingRemovedFromItsContainer()
        {
            this.viewModel.OpenPanel(this.requirement);
            this.viewModel.StartNew(AnnotationKind.Note);

            // the requirement was deleted on the server: it is dropped from its specification but keeps its container
            this.specification.Requirement.Remove(this.requirement);
            Assert.That(this.requirement.Container, Is.SameAs(this.specification));

            this.viewModel.SetIteration(this.iteration, this.domain);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.IsPanelOpen, Is.False);
                Assert.That(this.viewModel.ActiveThing, Is.Null);
                Assert.That(this.viewModel.Draft, Is.Null);
            }

            this.viewModel.OpenPanel(this.otherRequirement);
            this.viewModel.SetIteration(this.iteration, this.domain);

            Assert.That(this.viewModel.IsPanelOpen, Is.True, "a requirement still in its specification keeps the panel open");
        }

        [Test]
        public void VerifySetIterationKeepsThePanelOfAnElementDefinition()
        {
            this.viewModel.OpenPanel(this.element, AnnotationKind.Note);
            this.viewModel.StartNew(AnnotationKind.Note);
            this.viewModel.SetIteration(this.iteration, this.domain);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.IsPanelOpen, Is.True);
                Assert.That(this.viewModel.ActiveThing, Is.SameAs(this.element));
                Assert.That(this.viewModel.KindFilter, Is.EqualTo(AnnotationKind.Note));
                Assert.That(this.viewModel.Draft, Is.Not.Null, "the composer survives a reload that keeps its thing");
            }

            this.iteration.Element.Remove(this.element);
            this.viewModel.SetIteration(this.iteration, this.domain);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.IsPanelOpen, Is.False, "the element was removed from the iteration");
                Assert.That(this.viewModel.Draft, Is.Null);
            }

            // a thing of the engineering model itself, outside the iteration, is still part of the model
            var book = new Book { Iid = Guid.NewGuid(), ShortName = "BOOK" };
            this.model.Book.Add(book);
            this.viewModel.OpenPanel(book);
            this.viewModel.SetIteration(this.iteration, this.domain);

            Assert.That(this.viewModel.IsPanelOpen, Is.True, "a book is contained by the model");
        }

        [Test]
        public void VerifySetIterationClearsADraftWhoseSubjectIsGone()
        {
            this.viewModel.OpenPanel(this.requirement);

            // the annotation being edited is deleted by another user
            this.viewModel.StartEdit(this.changeRequest);
            this.model.ModellingAnnotation.Remove(this.changeRequest);
            this.viewModel.SetIteration(this.iteration, this.domain);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.Draft, Is.Null, "the edited annotation is no longer in the model");
                Assert.That(this.viewModel.IsPanelOpen, Is.True, "the requirement itself is still there");
            }

            // the same goes for a note
            this.viewModel.StartEdit(this.note);
            this.model.GenericNote.Remove(this.note);
            this.viewModel.SetIteration(this.iteration, this.domain);

            Assert.That(this.viewModel.Draft, Is.Null, "the edited note is no longer in the model");

            // a new change proposal whose change request is deleted by another user
            this.model.ModellingAnnotation.Add(this.changeRequest);
            this.viewModel.SetIteration(this.iteration, this.domain);
            this.viewModel.StartChangeProposal(this.changeRequest);
            this.model.ModellingAnnotation.Remove(this.changeRequest);
            this.viewModel.SetIteration(this.iteration, this.domain);

            Assert.That(this.viewModel.Draft, Is.Null, "the change request the proposal answers is no longer in the model");

            // a draft whose subject is still there is kept
            this.viewModel.StartEdit(this.discrepancy);
            var kept = this.viewModel.Draft;
            this.viewModel.SetIteration(this.iteration, this.domain);

            Assert.That(this.viewModel.Draft, Is.SameAs(kept));

            this.viewModel.StartNew(AnnotationKind.Note);
            var newDraft = this.viewModel.Draft;
            this.viewModel.SetIteration(this.iteration, this.domain);

            Assert.That(this.viewModel.Draft, Is.SameAs(newDraft), "a new annotation has no subject to lose");
        }

        [Test]
        public void VerifyGetAnnotations()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.GetAnnotations(this.requirement), Is.EqualTo((EngineeringModelDataAnnotation[])[this.changeRequest, this.note, this.discrepancy]));
                Assert.That(this.viewModel.GetAnnotations(this.otherRequirement), Is.EqualTo((EngineeringModelDataAnnotation[])[this.waiver]));
                Assert.That(this.viewModel.GetAnnotations(this.bareRequirement), Is.Empty);
                Assert.That(this.viewModel.GetAnnotations(this.requirement), Does.Not.Contain(this.changeProposal));
                Assert.That(this.viewModel.GetAnnotations(this.requirement), Does.Not.Contain(this.actionItem));
                Assert.That(this.viewModel.GetAnnotations(null), Is.Empty);
            }

            var otherProposal = (ChangeProposal)this.AddAnnotation(AnnotationKind.ChangeProposal, this.otherRequirement, "CP_002", AnnotationStatusKind.OPEN, 6);
            otherProposal.ChangeRequest = this.changeRequest;

            this.viewModel.SetIteration(this.iteration, this.domain);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.GetAnnotations(this.otherRequirement), Does.Contain(otherProposal), "a change proposal answering a change request about another requirement is top-level");
                Assert.That(this.viewModel.GetChangeProposals(this.changeRequest), Is.EquivalentTo([this.changeProposal, otherProposal]));
            }
        }

        [Test]
        public void VerifyGetChangeProposals()
        {
            var secondProposal = (ChangeProposal)this.AddAnnotation(AnnotationKind.ChangeProposal, this.requirement, "CP_002", AnnotationStatusKind.OPEN, 7);
            secondProposal.ChangeRequest = this.changeRequest;

            // the proposals are indexed once per SetIteration: a proposal added to the model since is not known yet
            Assert.That(this.viewModel.GetChangeProposals(this.changeRequest), Is.EqualTo((ChangeProposal[])[this.changeProposal]));

            this.viewModel.SetIteration(this.iteration, this.domain);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.GetChangeProposals(this.changeRequest), Is.EqualTo((ChangeProposal[])[secondProposal, this.changeProposal]));
                Assert.That(this.viewModel.GetChangeProposals(new ChangeRequest { Iid = Guid.NewGuid() }), Is.Empty);
                Assert.That(this.viewModel.GetChangeProposals(null), Is.Empty);
            }

            this.viewModel.SetIteration(null, this.domain);

            Assert.That(this.viewModel.GetChangeProposals(this.changeRequest), Is.Empty, "without an iteration nothing is indexed");
        }

        [Test]
        public void VerifyGetChangeProposalsIgnoresAProposalWithoutChangeRequest()
        {
            var orphan = (ChangeProposal)this.AddAnnotation(AnnotationKind.ChangeProposal, this.requirement, "CP_003", AnnotationStatusKind.OPEN, 8);
            orphan.ChangeRequest = null;

            Assert.DoesNotThrow(() => this.viewModel.SetIteration(this.iteration, this.domain));
            Assert.That(this.viewModel.GetChangeProposals(this.changeRequest), Is.EqualTo((ChangeProposal[])[this.changeProposal]));
        }

        [Test]
        public void VerifyGetSummary()
        {
            // the change proposal answering the change request is nested under it, so it has no badge of its own
            var summary = this.viewModel.GetSummary(this.requirement);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(summary, Is.EqualTo((AnnotationKindSummary[])
                [
                    new AnnotationKindSummary(AnnotationKind.Note, 1, 0),
                    new AnnotationKindSummary(AnnotationKind.ChangeRequest, 1, 1),
                    new AnnotationKindSummary(AnnotationKind.ReviewItemDiscrepancy, 1, 0)
                ]));

                Assert.That(this.viewModel.GetSummary(this.otherRequirement), Is.EqualTo((AnnotationKindSummary[])[new AnnotationKindSummary(AnnotationKind.RequestForWaiver, 1, 0)]));
                Assert.That(this.viewModel.GetSummary(this.bareRequirement), Is.Empty);
            }

            // a closed change request still counts as open while its change proposal is
            this.changeRequest.Status = AnnotationStatusKind.CLOSED;

            var closedRequest = this.viewModel.GetSummary(this.requirement).Single(x => x.Kind == AnnotationKind.ChangeRequest);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(closedRequest, Is.EqualTo(new AnnotationKindSummary(AnnotationKind.ChangeRequest, 1, 1)));
                Assert.That(AnnotationHelper.GetSummaryTooltip(closedRequest), Is.EqualTo("1 Change Request, 1 needing attention"));
            }

            this.changeProposal.Status = AnnotationStatusKind.CLOSED;

            Assert.That(this.viewModel.GetSummary(this.requirement).Single(x => x.Kind == AnnotationKind.ChangeRequest), Is.EqualTo(new AnnotationKindSummary(AnnotationKind.ChangeRequest, 1, 0)));
        }

        [Test]
        public void VerifyHasOpenAnnotations()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.HasOpenAnnotations(this.requirement), Is.True);
                Assert.That(this.viewModel.HasOpenAnnotations(this.otherRequirement), Is.False);
                Assert.That(this.viewModel.HasOpenAnnotations(this.bareRequirement), Is.False);
            }

            // a closed change request whose nested change proposal is still open needs attention
            this.discrepancy.Status = AnnotationStatusKind.CLOSED;
            this.changeRequest.Status = AnnotationStatusKind.CLOSED;

            Assert.That(this.viewModel.HasOpenAnnotations(this.requirement), Is.True, "the change proposal is still open");

            this.changeProposal.Status = AnnotationStatusKind.CLOSED;
            this.viewModel.SetIteration(this.iteration, this.domain);

            Assert.That(this.viewModel.HasOpenAnnotations(this.requirement), Is.False, "notes are never open");
        }

        [Test]
        public void VerifyMatchesFilter()
        {
            var openOption = this.viewModel.FilterOptions.Single(x => x.Kind == null);
            var waiverOption = this.viewModel.FilterOptions.Single(x => x.Kind == AnnotationKind.RequestForWaiver);
            var discrepancyOption = this.viewModel.FilterOptions.Single(x => x.Kind == AnnotationKind.ReviewItemDiscrepancy);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.FilterOptions, Has.Count.EqualTo(6));
                Assert.That(this.viewModel.FilterOptions.Where(x => x.Kind != null).Select(x => x.Kind), Is.EqualTo(AnnotationHelper.AttachableKinds.Cast<AnnotationKind?>().ToArray()));

                Assert.That(this.viewModel.MatchesFilter(this.bareRequirement, []), Is.True);
                Assert.That(this.viewModel.MatchesFilter(this.requirement, [openOption]), Is.True);
                Assert.That(this.viewModel.MatchesFilter(this.otherRequirement, [openOption]), Is.False);
                Assert.That(this.viewModel.MatchesFilter(this.otherRequirement, [waiverOption]), Is.True);
                Assert.That(this.viewModel.MatchesFilter(this.requirement, [waiverOption]), Is.False);
                Assert.That(this.viewModel.MatchesFilter(this.requirement, [waiverOption, discrepancyOption]), Is.True);
                Assert.That(this.viewModel.MatchesFilter(this.bareRequirement, [openOption, waiverOption]), Is.False);
            }

            // a closed change request whose nested change proposal is still open passes the "Open items" filter
            this.discrepancy.Status = AnnotationStatusKind.CLOSED;
            this.changeRequest.Status = AnnotationStatusKind.CLOSED;

            Assert.That(this.viewModel.MatchesFilter(this.requirement, [openOption]), Is.True, "the change proposal is still open");

            this.changeProposal.Status = AnnotationStatusKind.CLOSED;

            Assert.That(this.viewModel.MatchesFilter(this.requirement, [openOption]), Is.False);
        }

        [Test]
        public void VerifyPanelAndVisibleAnnotations()
        {
            Assert.That(this.viewModel.VisibleAnnotations, Is.Empty);

            this.viewModel.ShowResolved = false;
            this.viewModel.OpenPanel(this.requirement, AnnotationKind.ReviewItemDiscrepancy);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.IsPanelOpen, Is.True);
                Assert.That(this.viewModel.ActiveThing, Is.SameAs(this.requirement));
                Assert.That(this.viewModel.KindFilter, Is.EqualTo(AnnotationKind.ReviewItemDiscrepancy));
                Assert.That(this.viewModel.VisibleAnnotations, Is.Empty, "the only discrepancy is done");
            }

            this.viewModel.ShowResolved = true;
            Assert.That(this.viewModel.VisibleAnnotations, Is.EqualTo((EngineeringModelDataAnnotation[])[this.discrepancy]));

            this.viewModel.ShowResolved = false;
            this.viewModel.KindFilter = null;
            Assert.That(this.viewModel.VisibleAnnotations, Is.EqualTo((EngineeringModelDataAnnotation[])[this.changeRequest, this.note]), "notes are kept, resolved items hidden");

            this.changeRequest.Status = AnnotationStatusKind.CLOSED;
            Assert.That(this.viewModel.VisibleAnnotations, Is.EqualTo((EngineeringModelDataAnnotation[])[this.changeRequest, this.note]), "a closed change request is kept while its change proposal is open");

            this.changeProposal.Status = AnnotationStatusKind.CLOSED;
            Assert.That(this.viewModel.VisibleAnnotations, Is.EqualTo((EngineeringModelDataAnnotation[])[this.note]), "the change request is hidden once its change proposal is closed too");

            this.changeRequest.Status = AnnotationStatusKind.OPEN;
            this.changeProposal.Status = AnnotationStatusKind.OPEN;

            this.viewModel.KindFilter = AnnotationKind.Note;
            Assert.That(this.viewModel.VisibleAnnotations, Is.EqualTo((EngineeringModelDataAnnotation[])[this.note]));

            this.viewModel.StartNew(AnnotationKind.Note);
            Assert.That(this.viewModel.Draft, Is.Not.Null);

            this.viewModel.ClosePanel();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.IsPanelOpen, Is.False);
                Assert.That(this.viewModel.ActiveThing, Is.Null);
                Assert.That(this.viewModel.Draft, Is.Null);
            }
        }

        [Test]
        public void VerifyCanCreate()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(AnnotationHelper.AttachableKinds.Select(this.viewModel.CanCreate), Is.All.True);
                Assert.That(this.viewModel.CreatableKinds, Is.Empty, "no panel is open, so there is no thing to annotate");
            }

            this.viewModel.OpenPanel(this.requirement);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.CreatableKinds, Is.EqualTo(AnnotationHelper.AttachableKinds));
                Assert.That(this.viewModel.GetCreatableKinds(this.requirement), Is.EqualTo(AnnotationHelper.AttachableKinds));
            }

            this.permissionService.Setup(x => x.CanWrite(ClassKind.ChangeRequest, It.IsAny<Thing>())).Returns(false);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.CanCreate(AnnotationKind.ChangeRequest), Is.False);
                Assert.That(this.viewModel.CanCreate(AnnotationKind.Note), Is.True);
                Assert.That(this.viewModel.CreatableKinds, Does.Not.Contain(AnnotationKind.ChangeRequest));
                Assert.That(this.viewModel.CreatableKinds, Does.Contain(AnnotationKind.Note));
            }

            this.permissionService.Setup(x => x.CanWrite(ClassKind.ChangeRequest, It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.ActivePerson).Returns(new Person { Iid = Guid.NewGuid() });
            this.viewModel.SetIteration(this.iteration, this.domain);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.CanCreate(AnnotationKind.Note), Is.False, "the active person is not a participant of the model");
                Assert.That(this.viewModel.CreatableKinds, Is.Empty);
            }

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.sessionService.Setup(x => x.IsReadOnly).Returns(true);
            this.viewModel.SetIteration(this.iteration, this.domain);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.CanCreate(AnnotationKind.Note), Is.False);
                Assert.That(this.viewModel.CreatableKinds, Is.Empty);
            }
        }

        [Test]
        public void VerifyGetCreatableKinds()
        {
            var book = new Book { Iid = Guid.NewGuid(), ShortName = "BOOK" };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.GetCreatableKinds(this.requirement), Is.EqualTo(AnnotationHelper.AttachableKinds));
                Assert.That(this.viewModel.GetCreatableKinds(this.element), Is.EqualTo(AnnotationHelper.AttachableKinds));
                Assert.That(this.viewModel.GetCreatableKinds(this.iteration), Is.EqualTo((AnnotationKind[])[AnnotationKind.Note]), "only a note applies to an iteration");
                Assert.That(this.viewModel.GetCreatableKinds(book), Is.EqualTo((AnnotationKind[])[AnnotationKind.Note]), "only a note applies to a book");
                Assert.That(this.viewModel.GetCreatableKinds(null), Is.Empty);
            }

            this.viewModel.OpenPanel(this.iteration);

            Assert.That(this.viewModel.CreatableKinds, Is.EqualTo((AnnotationKind[])[AnnotationKind.Note]), "the creatable kinds follow the active thing");

            this.viewModel.OpenPanel(this.element);

            Assert.That(this.viewModel.CreatableKinds, Is.EqualTo(AnnotationHelper.AttachableKinds));

            this.permissionService.Setup(x => x.CanWrite(ClassKind.EngineeringModelDataNote, It.IsAny<Thing>())).Returns(false);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.GetCreatableKinds(this.iteration), Is.Empty, "the note is the only applicable kind and it is not permitted");
                Assert.That(this.viewModel.GetCreatableKinds(this.element), Is.EqualTo((AnnotationKind[])
                [
                    AnnotationKind.ChangeRequest, AnnotationKind.ReviewItemDiscrepancy, AnnotationKind.RequestForDeviation, AnnotationKind.RequestForWaiver
                ]));
            }

            this.viewModel.ClosePanel();

            Assert.That(this.viewModel.CreatableKinds, Is.Empty);
        }

        [Test]
        public void VerifyCanEdit()
        {
            Assert.That(this.viewModel.CanEdit(this.changeRequest), Is.True);

            this.permissionService.Setup(x => x.CanWrite(this.changeRequest)).Returns(false);
            Assert.That(this.viewModel.CanEdit(this.changeRequest), Is.False);

            this.sessionService.Setup(x => x.IsReadOnly).Returns(true);
            Assert.That(this.viewModel.CanEdit(this.discrepancy), Is.False);
        }

        [Test]
        public void VerifyCanReplyApproveAddSolutionAndProposeChange()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.CanReply(this.changeRequest), Is.True);
                Assert.That(this.viewModel.CanReply(this.note), Is.True);
                Assert.That(this.viewModel.CanApprove(this.changeRequest), Is.True);
                Assert.That(this.viewModel.CanApprove(this.note), Is.False, "a note has no approvals");
                Assert.That(this.viewModel.CanAddSolution(this.discrepancy), Is.True);
                Assert.That(this.viewModel.CanAddSolution(this.changeRequest), Is.False, "only a discrepancy has solutions");
                Assert.That(this.viewModel.CanProposeChange(this.changeRequest), Is.True);
                Assert.That(this.viewModel.CanProposeChange(this.discrepancy), Is.False, "only a change request can be answered");
            }

            this.permissionService.Setup(x => x.CanWrite(ClassKind.EngineeringModelDataDiscussionItem, It.IsAny<Thing>())).Returns(false);
            this.permissionService.Setup(x => x.CanWrite(ClassKind.Approval, It.IsAny<Thing>())).Returns(false);
            this.permissionService.Setup(x => x.CanWrite(ClassKind.Solution, It.IsAny<Thing>())).Returns(false);
            this.permissionService.Setup(x => x.CanWrite(ClassKind.ChangeProposal, It.IsAny<Thing>())).Returns(false);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.CanReply(this.changeRequest), Is.False);
                Assert.That(this.viewModel.CanApprove(this.changeRequest), Is.False);
                Assert.That(this.viewModel.CanAddSolution(this.discrepancy), Is.False);
                Assert.That(this.viewModel.CanProposeChange(this.changeRequest), Is.False);
            }

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.sessionService.Setup(x => x.IsReadOnly).Returns(true);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.CanReply(this.changeRequest), Is.False);
                Assert.That(this.viewModel.CanApprove(this.changeRequest), Is.False);
                Assert.That(this.viewModel.CanAddSolution(this.discrepancy), Is.False);
                Assert.That(this.viewModel.CanProposeChange(this.changeRequest), Is.False);
            }
        }

        [Test]
        public async Task VerifySaveDraftAsync()
        {
            // a new annotation
            this.viewModel.OpenPanel(this.requirement);
            this.viewModel.StartNew(AnnotationKind.RequestForDeviation);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.Draft, Is.Not.Null);
                Assert.That(this.viewModel.Draft.Kind, Is.EqualTo(AnnotationKind.RequestForDeviation));
                Assert.That(this.viewModel.Draft.ShortName, Is.EqualTo("REQ-1-RFD-001"));
                Assert.That(this.viewModel.Draft.Owner, Is.SameAs(this.domain));
                Assert.That(this.viewModel.Draft.ShowsDetails, Is.False, "a new annotation does not offer its details");
            }

            await this.viewModel.SaveDraftAsync();

            Assert.That(this.writes, Is.Empty, "an invalid draft writes nothing");
            Assert.That(this.viewModel.Draft, Is.Not.Null);

            this.viewModel.Draft.Title = "a title";
            this.viewModel.Draft.Content = "a content";

            await this.viewModel.SaveDraftAsync();

            Assert.That(this.writes, Has.Count.EqualTo(1));
            var (top, things) = this.writes.Single();
            var created = things.OfType<RequestForDeviation>().Single();
            var reference = things.OfType<ModellingThingReference>().Single();
            var modelClone = (EngineeringModel)top;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(top, Is.TypeOf<EngineeringModel>());
                Assert.That(top.Iid, Is.EqualTo(this.model.Iid));
                Assert.That(top, Is.Not.SameAs(this.model));
                Assert.That(created.Author, Is.SameAs(this.participant));
                Assert.That(created.Owner, Is.SameAs(this.domain));
                Assert.That(created.ShortName, Is.EqualTo("REQ-1-RFD-001"));
                Assert.That(created.Title, Is.EqualTo("a title"));
                Assert.That(created.Content, Is.EqualTo("a content"));
                Assert.That(created.Status, Is.EqualTo(AnnotationStatusKind.OPEN));
                Assert.That(created.LanguageCode, Is.Not.Empty);
                Assert.That(created.PrimaryAnnotatedThing, Is.SameAs(reference));
                Assert.That(created.PrimaryAnnotatedThing.ReferencedThing, Is.SameAs(this.requirement));
                Assert.That(modelClone.ModellingAnnotation, Does.Contain(created));
                Assert.That(this.viewModel.Draft, Is.Null);
            }

            // the next short name follows the stored annotations
            this.model.ModellingAnnotation.Add(created);
            this.viewModel.SetIteration(this.iteration, this.domain);
            this.viewModel.StartNew(AnnotationKind.RequestForDeviation);
            this.viewModel.Draft.Title = "another title";
            this.viewModel.Draft.Content = "another content";

            await this.viewModel.SaveDraftAsync();

            Assert.That(this.writes.Last().Things.OfType<RequestForDeviation>().Single().ShortName, Is.EqualTo("REQ-1-RFD-002"));

            // a new note
            this.writes.Clear();
            this.viewModel.StartNew(AnnotationKind.Note);
            this.viewModel.Draft.Content = "a comment";

            await this.viewModel.SaveDraftAsync();

            var (noteTop, noteThings) = this.writes.Single();
            var createdNote = noteThings.OfType<EngineeringModelDataNote>().Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(noteTop.Iid, Is.EqualTo(this.model.Iid));
                Assert.That(((EngineeringModel)noteTop).GenericNote, Does.Contain(createdNote));
                Assert.That(createdNote.Author, Is.SameAs(this.participant));
                Assert.That(createdNote.Content, Is.EqualTo("a comment"));
                Assert.That(createdNote.PrimaryAnnotatedThing.ReferencedThing, Is.SameAs(this.requirement));
                Assert.That(noteThings.OfType<ModellingThingReference>(), Has.Exactly(1).Items);
                Assert.That(this.viewModel.Draft, Is.Null);
            }

            // an edited annotation
            this.writes.Clear();
            this.viewModel.StartEdit(this.changeRequest);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.Draft, Is.Not.Null);
                Assert.That(this.viewModel.Draft.EditedAnnotation, Is.SameAs(this.changeRequest));
                Assert.That(this.viewModel.Draft.Kind, Is.EqualTo(AnnotationKind.ChangeRequest));
                Assert.That(this.viewModel.Draft.Content, Is.EqualTo(this.changeRequest.Content));
                Assert.That(this.viewModel.Draft.Title, Is.EqualTo(this.changeRequest.Title));
            }

            this.viewModel.Draft.Content = "an updated content";

            await this.viewModel.SaveDraftAsync();

            var (editTop, editThings) = this.writes.Single();
            var edited = editThings.OfType<ChangeRequest>().Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(editTop, Is.TypeOf<EngineeringModel>());
                Assert.That(editTop.Iid, Is.EqualTo(this.model.Iid));
                Assert.That(edited.Iid, Is.EqualTo(this.changeRequest.Iid));
                Assert.That(edited, Is.Not.SameAs(this.changeRequest));
                Assert.That(edited.Content, Is.EqualTo("an updated content"));
                Assert.That(this.changeRequest.Content, Is.Not.EqualTo("an updated content"), "the original is not mutated");
                Assert.That(this.viewModel.Draft, Is.Null);
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(edited.ShortName, Is.EqualTo("CR_001"), "an untouched short name is kept");
                Assert.That(edited.Owner, Is.SameAs(this.domain));
            }

            // an edit of the short name, the owner and the categories
            this.writes.Clear();
            this.changeRequest.Category.Add(this.betaCategory);
            this.viewModel.StartEdit(this.changeRequest);
            this.viewModel.Draft.ShortName = "  CR_renamed  ";
            this.viewModel.Draft.Owner = this.engineeringDomain;
            this.viewModel.Draft.Categories = [this.alphaCategory, this.gammaCategory];

            await this.viewModel.SaveDraftAsync();

            var detailsEdited = this.writes.Single().Things.OfType<ChangeRequest>().Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(detailsEdited.Iid, Is.EqualTo(this.changeRequest.Iid));
                Assert.That(detailsEdited.ShortName, Is.EqualTo("CR_renamed"), "the short name is trimmed");
                Assert.That(detailsEdited.Owner, Is.SameAs(this.engineeringDomain));
                Assert.That(detailsEdited.Category, Is.EquivalentTo([this.alphaCategory, this.gammaCategory]), "the categories replace the old ones");
                Assert.That(this.changeRequest.ShortName, Is.EqualTo("CR_001"), "the original is not mutated");
                Assert.That(this.changeRequest.Owner, Is.SameAs(this.domain));
                Assert.That(this.changeRequest.Category, Is.EqualTo([this.betaCategory]));
                Assert.That(this.viewModel.Draft, Is.Null);
            }

            // clearing the owner keeps the current one, clearing the categories removes them
            this.writes.Clear();
            this.viewModel.StartEdit(this.changeRequest);
            this.viewModel.Draft.Owner = null;
            this.viewModel.Draft.Categories = [];

            await this.viewModel.SaveDraftAsync();

            var cleared = this.writes.Single().Things.OfType<ChangeRequest>().Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(cleared.Owner, Is.SameAs(this.domain));
                Assert.That(cleared.Category, Is.Empty);
                Assert.That(this.changeRequest.Category, Is.EqualTo([this.betaCategory]));
            }

            // a short name already used by another annotation, or an empty one, blocks the save
            this.writes.Clear();
            this.viewModel.StartEdit(this.changeRequest);

            foreach (var shortName in (string[])["rid_001", "ACT_001", " ", string.Empty])
            {
                this.viewModel.Draft.ShortName = shortName;

                await this.viewModel.SaveDraftAsync();

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(this.writes, Is.Empty, $"'{shortName}' must not be written");
                    Assert.That(this.viewModel.Draft, Is.Not.Null, $"the draft is kept for '{shortName}'");
                }
            }

            this.viewModel.Draft.ShortName = "cr_001";

            await this.viewModel.SaveDraftAsync();

            Assert.That(this.writes.Single().Things.OfType<ChangeRequest>().Single().ShortName, Is.EqualTo("cr_001"), "the edited annotation does not conflict with itself");

            // a change proposal answering a change request
            this.writes.Clear();
            this.viewModel.StartChangeProposal(this.changeRequest);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.Draft.Kind, Is.EqualTo(AnnotationKind.ChangeProposal));
                Assert.That(this.viewModel.Draft.ChangeRequest, Is.SameAs(this.changeRequest));
                Assert.That(this.viewModel.Draft.ShortName, Is.EqualTo("REQ-1-CP-001"));
                Assert.That(this.viewModel.Draft.Owner, Is.SameAs(this.domain));
            }

            this.viewModel.Draft.Title = "a proposal";
            this.viewModel.Draft.Content = "a proposed change";

            await this.viewModel.SaveDraftAsync();

            var (proposalTop, proposalThings) = this.writes.Single();
            var createdProposal = proposalThings.OfType<ChangeProposal>().Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(proposalTop.Iid, Is.EqualTo(this.model.Iid));
                Assert.That(createdProposal.ChangeRequest.Iid, Is.EqualTo(this.changeRequest.Iid));
                Assert.That(createdProposal.PrimaryAnnotatedThing.ReferencedThing, Is.SameAs(this.requirement));
                Assert.That(createdProposal.Author, Is.SameAs(this.participant));
                Assert.That(createdProposal.Owner, Is.SameAs(this.domain));
                Assert.That(createdProposal.ShortName, Is.EqualTo("REQ-1-CP-001"));
                Assert.That(((EngineeringModel)proposalTop).ModellingAnnotation, Does.Contain(createdProposal));
            }
        }

        [Test]
        public async Task VerifySaveDraftAsyncWritesTheShortNameAndOwnerOfTheDraft()
        {
            this.viewModel.OpenPanel(this.requirement);
            this.viewModel.StartNew(AnnotationKind.RequestForDeviation);
            this.viewModel.Draft.Title = "a title";
            this.viewModel.Draft.Content = "a content";
            this.viewModel.Draft.ShortName = "  CUSTOM-1  ";
            this.viewModel.Draft.Owner = this.engineeringDomain;

            await this.viewModel.SaveDraftAsync();

            var typed = this.writes.Single().Things.OfType<RequestForDeviation>().Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(typed.ShortName, Is.EqualTo("CUSTOM-1"), "the short name of the draft is written, trimmed");
                Assert.That(typed.Owner, Is.SameAs(this.engineeringDomain), "the owner of the draft is written");
                Assert.That(this.viewModel.Draft, Is.Null);
            }

            // an empty short name and no owner fall back to the generated name and the current domain
            this.writes.Clear();
            this.viewModel.StartNew(AnnotationKind.RequestForDeviation);
            this.viewModel.Draft.Title = "another title";
            this.viewModel.Draft.Content = "another content";
            this.viewModel.Draft.ShortName = "   ";
            this.viewModel.Draft.Owner = null;

            await this.viewModel.SaveDraftAsync();

            var fallback = this.writes.Single().Things.OfType<RequestForDeviation>().Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(fallback.ShortName, Is.EqualTo("REQ-1-RFD-001"));
                Assert.That(fallback.Owner, Is.SameAs(this.domain));
            }

            // the previewed short name is what is written when it is left alone
            this.writes.Clear();
            this.viewModel.StartNew(AnnotationKind.ReviewItemDiscrepancy);
            var preview = this.viewModel.Draft.ShortName;
            this.viewModel.Draft.Title = "a third title";
            this.viewModel.Draft.Content = "a third content";

            await this.viewModel.SaveDraftAsync();

            Assert.That(this.writes.Single().Things.OfType<ReviewItemDiscrepancy>().Single().ShortName, Is.EqualTo(preview));
        }

        [Test]
        public async Task VerifySaveDraftAsyncOnAnElementDefinition()
        {
            this.viewModel.OpenPanel(this.element);
            this.viewModel.StartNew(AnnotationKind.ChangeRequest);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.Draft.ShortName, Is.EqualTo("SAT-CR-001"), "the short name is built from the annotated thing");
                Assert.That(this.viewModel.Draft.Owner, Is.SameAs(this.domain));
            }

            this.viewModel.Draft.Title = "a title";
            this.viewModel.Draft.Content = "a content";

            await this.viewModel.SaveDraftAsync();

            var (top, things) = this.writes.Single();
            var created = things.OfType<ChangeRequest>().Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(top.Iid, Is.EqualTo(this.model.Iid));
                Assert.That(created.ShortName, Is.EqualTo("SAT-CR-001"));
                Assert.That(created.PrimaryAnnotatedThing.ReferencedThing, Is.SameAs(this.element));
                Assert.That(created.LanguageCode, Is.EqualTo("en"), "an element without definition has no language to borrow");
                Assert.That(this.notifications.Single().OnSuccess, Is.EqualTo("Element Definition 'SAT' annotated with a Change Request"));
                Assert.That(this.notifications.Single().OnError, Is.EqualTo("Could not add the Change Request to the Element Definition 'SAT'"));
            }

            // a new annotation is written in the language of the definition of the thing
            this.element.Definition.Add(new Definition { Iid = Guid.NewGuid(), LanguageCode = "fr", Content = "un satellite" });
            this.writes.Clear();
            this.viewModel.StartNew(AnnotationKind.Note);
            this.viewModel.Draft.Content = "a comment";

            await this.viewModel.SaveDraftAsync();

            Assert.That(this.writes.Single().Things.OfType<EngineeringModelDataNote>().Single().LanguageCode, Is.EqualTo("fr"));
        }

        [Test]
        public async Task VerifyNotificationsNameTheAnnotationAndTheAction()
        {
            await this.viewModel.ReplyAsync(this.changeRequest, "a reply");
            await this.viewModel.SetStatusAsync(this.changeRequest, AnnotationStatusKind.DONE);
            await this.viewModel.ApproveAsync(this.changeRequest, AnnotationApprovalKind.ACCEPTED, null);
            await this.viewModel.ApproveAsync(this.changeRequest, AnnotationApprovalKind.REJECTED, null);
            await this.viewModel.ApproveAsync(this.changeRequest, AnnotationApprovalKind.WITHDRAWN, null);
            await this.viewModel.AddSolutionAsync(this.discrepancy, "a solution");
            await this.viewModel.DeleteAsync(this.changeRequest);

            this.viewModel.StartEdit(this.changeRequest);
            this.viewModel.Draft.Content = "an updated content";
            await this.viewModel.SaveDraftAsync();

            this.viewModel.OpenPanel(this.requirement);
            this.viewModel.StartNew(AnnotationKind.ChangeRequest);
            this.viewModel.Draft.Title = "a title";
            this.viewModel.Draft.Content = "a content";
            await this.viewModel.SaveDraftAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.notifications.Select(x => x.OnSuccess), Is.EqualTo(
                [
                    "Change Request 'CR_001' received a reply",
                    "Change Request 'CR_001' set to Done",
                    "Change Request 'CR_001' accepted",
                    "Change Request 'CR_001' rejected",
                    "Change Request 'CR_001' withdrawn",
                    "Review Item Discrepancy 'RID_001' received a proposed solution",
                    "Change Request 'CR_001' deleted",
                    "Change Request 'CR_001' updated",
                    "Requirement 'REQ-1' annotated with a Change Request"
                ]));

                Assert.That(this.notifications.Select(x => x.OnError), Is.EqualTo(
                [
                    "Could not reply to the Change Request 'CR_001'",
                    "Could not change the status of the Change Request 'CR_001'",
                    "Could not record a decision on the Change Request 'CR_001'",
                    "Could not record a decision on the Change Request 'CR_001'",
                    "Could not record a decision on the Change Request 'CR_001'",
                    "Could not propose a solution for the Review Item Discrepancy 'RID_001'",
                    "Could not delete the Change Request 'CR_001'",
                    "Could not update the Change Request 'CR_001'",
                    "Could not add the Change Request to the Requirement 'REQ-1'"
                ]));
            }

            // a note is referred to by the start of its text
            this.notifications.Clear();
            this.note.Content = "  a short remark  ";
            await this.viewModel.DeleteAsync(this.note);

            this.note.Content = "This remark is much longer than thirty characters";
            await this.viewModel.DeleteAsync(this.note);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.notifications[0].OnSuccess, Is.EqualTo("Note 'a short remark' deleted"));
                Assert.That(this.notifications[0].OnError, Is.EqualTo("Could not delete the Note 'a short remark'"));
                Assert.That(this.notifications[1].OnSuccess, Is.EqualTo("Note 'This remark is much longer tha...' deleted"));
                Assert.That(this.notifications[1].OnError, Is.EqualTo("Could not delete the Note 'This remark is much longer tha...'"));
            }
        }

        [Test]
        public async Task VerifyDeleteAsyncDropsTheDraftOfTheDeletedAnnotation()
        {
            this.viewModel.OpenPanel(this.requirement);

            this.viewModel.StartEdit(this.changeRequest);
            await this.viewModel.DeleteAsync(this.changeRequest);

            Assert.That(this.viewModel.Draft, Is.Null, "the draft edits the deleted annotation");

            this.viewModel.StartEdit(this.changeProposal);
            await this.viewModel.DeleteAsync(this.changeRequest);

            Assert.That(this.viewModel.Draft, Is.Null, "the draft edits a change proposal deleted with its change request");

            this.viewModel.StartChangeProposal(this.changeRequest);
            await this.viewModel.DeleteAsync(this.changeRequest);

            Assert.That(this.viewModel.Draft, Is.Null, "the draft answers the deleted change request");

            this.viewModel.StartEdit(this.note);
            await this.viewModel.DeleteAsync(this.note);

            Assert.That(this.viewModel.Draft, Is.Null, "the draft edits the deleted note");

            this.viewModel.StartEdit(this.discrepancy);
            var unrelated = this.viewModel.Draft;
            await this.viewModel.DeleteAsync(this.changeRequest);

            Assert.That(this.viewModel.Draft, Is.SameAs(unrelated), "the draft edits another annotation");

            this.viewModel.StartNew(AnnotationKind.Note);
            var composing = this.viewModel.Draft;
            await this.viewModel.DeleteAsync(this.note);

            Assert.That(this.viewModel.Draft, Is.SameAs(composing), "a new annotation is not about the deleted one");

            // a deletion that failed leaves the annotation, so its draft stays
            this.sessionService.Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Fail("a failure"));

            this.viewModel.StartEdit(this.discrepancy);
            var failed = this.viewModel.Draft;
            await this.viewModel.DeleteAsync(this.discrepancy);

            Assert.That(this.viewModel.Draft, Is.SameAs(failed));
        }

        [Test]
        public void VerifyGetShortNameError()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.GetShortNameError(null), Is.Null);
                Assert.That(this.viewModel.GetShortNameError(new AnnotationDraft { Kind = AnnotationKind.ChangeRequest, ShortName = "CR-002" }), Is.Null, "a new annotation's short name is generated when written");
                Assert.That(this.viewModel.GetShortNameError(new AnnotationDraft { Kind = AnnotationKind.Note, EditedAnnotation = this.note }), Is.Null, "a note has no short name");

                Assert.That(this.viewModel.GetShortNameError(this.EditDraft(this.changeRequest, "CR_001")), Is.Null, "the edited annotation does not conflict with itself");
                Assert.That(this.viewModel.GetShortNameError(this.EditDraft(this.changeRequest, "  CR_002  ")), Is.Null, "the short name is trimmed");

                Assert.That(this.viewModel.GetShortNameError(this.EditDraft(this.changeRequest, string.Empty)), Is.EqualTo("A short name is required."));
                Assert.That(this.viewModel.GetShortNameError(this.EditDraft(this.changeRequest, "   ")), Is.EqualTo("A short name is required."));
                Assert.That(this.viewModel.GetShortNameError(this.EditDraft(this.changeRequest, null)), Is.EqualTo("A short name is required."));

                Assert.That(this.viewModel.GetShortNameError(this.EditDraft(this.changeRequest, "CR-002")), Is.Null, "no naming rule beyond uniqueness");
                Assert.That(this.viewModel.GetShortNameError(this.EditDraft(this.changeRequest, "1 abc.def")), Is.Null);

                Assert.That(this.viewModel.GetShortNameError(this.EditDraft(this.changeRequest, "RID_001")), Is.EqualTo("The short name 'RID_001' is already used by another annotation."));
                Assert.That(this.viewModel.GetShortNameError(this.EditDraft(this.changeRequest, "rid_001")), Does.Contain("already used"), "the comparison ignores the case");
                Assert.That(this.viewModel.GetShortNameError(this.EditDraft(this.changeRequest, " RFW_001 ")), Does.Contain("already used"), "an annotation about another requirement counts");
                Assert.That(this.viewModel.GetShortNameError(this.EditDraft(this.changeRequest, "ACT_001")), Does.Contain("already used"), "any annotation item of the model counts");
            }

            this.discrepancy.ShortName = "RID-001";

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.GetShortNameError(this.EditDraft(this.discrepancy, "RID-001")), Is.Null, "an annotation does not conflict with itself");
                Assert.That(this.viewModel.GetShortNameError(this.EditDraft(this.changeRequest, "rid-001")), Does.Contain("already used"), "a dashed short name conflicts case-insensitively too");
            }
        }

        [Test]
        public void VerifySetIterationRefreshesTheShortNamePreview()
        {
            this.viewModel.OpenPanel(this.requirement);
            this.viewModel.StartNew(AnnotationKind.RequestForDeviation);
            var preview = this.viewModel.Draft.ShortName;

            // another user writes an annotation with that very short name while the composer is open
            this.AddAnnotation(AnnotationKind.RequestForDeviation, this.requirement, preview, AnnotationStatusKind.OPEN, 9);
            this.viewModel.SetIteration(this.iteration, this.domain);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(preview, Is.EqualTo("REQ-1-RFD-001"));
                Assert.That(this.viewModel.Draft.ShortName, Is.EqualTo("REQ-1-RFD-002"));
            }
        }

        /// <summary>
        /// Builds a draft editing the given <paramref name="annotation" /> with the given <paramref name="shortName" />.
        /// </summary>
        /// <param name="annotation">The <see cref="ModellingAnnotationItem" /> being edited</param>
        /// <param name="shortName">The short name typed in the composer</param>
        /// <returns>The <see cref="AnnotationDraft" /></returns>
        private AnnotationDraft EditDraft(ModellingAnnotationItem annotation, string shortName)
        {
            return new AnnotationDraft { Kind = AnnotationHelper.GetKind(annotation).Value, ShortName = shortName, EditedAnnotation = annotation };
        }

        [Test]
        public void VerifyStartEdit()
        {
            this.changeRequest.Category.Add(this.betaCategory);
            this.discrepancy.Owner = this.archivedDomain;

            this.viewModel.StartEdit(this.changeRequest);
            var changeRequestDraft = this.viewModel.Draft;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(changeRequestDraft.ShowsDetails, Is.True);
                Assert.That(changeRequestDraft.Kind, Is.EqualTo(AnnotationKind.ChangeRequest));
                Assert.That(changeRequestDraft.EditedAnnotation, Is.SameAs(this.changeRequest));
                Assert.That(changeRequestDraft.ShortName, Is.EqualTo("CR_001"));
                Assert.That(changeRequestDraft.Owner, Is.SameAs(this.domain));
                Assert.That(changeRequestDraft.Categories, Is.EqualTo([this.betaCategory]));
                Assert.That(changeRequestDraft.Categories, Is.Not.SameAs(this.changeRequest.Category), "the draft edits a copy");
                Assert.That(changeRequestDraft.AvailableOwners, Is.EqualTo([this.engineeringDomain, this.domain]), "the active domains by name, once");
                Assert.That(changeRequestDraft.AvailableCategories.Select(x => x.Iid), Is.EqualTo([this.alphaCategory.Iid, this.betaCategory.Iid, this.gammaCategory.Iid]), "the non-deprecated categories of the library chain permissible for a change request, once, by name");
            }

            this.viewModel.StartEdit(this.discrepancy);
            var discrepancyDraft = this.viewModel.Draft;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(discrepancyDraft.Owner, Is.SameAs(this.archivedDomain));
                Assert.That(discrepancyDraft.Categories, Is.Empty);
                Assert.That(discrepancyDraft.AvailableOwners, Is.EqualTo([this.archivedDomain, this.engineeringDomain, this.domain]), "the current owner stays selectable when it is no longer active");
                Assert.That(discrepancyDraft.AvailableCategories, Is.EqualTo([this.betaCategory]));
            }

            this.viewModel.StartEdit(this.note);
            var noteDraft = this.viewModel.Draft;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(noteDraft.ShowsDetails, Is.False, "a note has no details");
                Assert.That(noteDraft.Kind, Is.EqualTo(AnnotationKind.Note));
                Assert.That(noteDraft.Content, Is.EqualTo(this.note.Content));
                Assert.That(noteDraft.ShortName, Is.Empty);
                Assert.That(noteDraft.Owner, Is.Null);
                Assert.That(noteDraft.Categories, Is.Empty);
                Assert.That(noteDraft.AvailableCategories, Is.Empty);
            }

            this.viewModel.CancelDraft();
            this.viewModel.StartEdit(this.actionItem);

            Assert.That(this.viewModel.Draft, Is.Null, "an annotation class the editor does not handle opens no composer");
        }

        [Test]
        public void VerifyCancelDraft()
        {
            this.viewModel.OpenPanel(this.requirement);
            this.viewModel.StartNew(AnnotationKind.Note);
            this.viewModel.CancelDraft();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.viewModel.Draft, Is.Null);
                Assert.That(this.writes, Is.Empty);
                Assert.That(this.viewModel.IsPanelOpen, Is.True);
            }
        }

        [Test]
        public void VerifyStartNewClearsAConflictingKindFilter()
        {
            this.viewModel.OpenPanel(this.requirement, AnnotationKind.Note);
            this.viewModel.StartNew(AnnotationKind.Note);

            Assert.That(this.viewModel.KindFilter, Is.EqualTo(AnnotationKind.Note));

            this.viewModel.StartNew(AnnotationKind.ReviewItemDiscrepancy);

            Assert.That(this.viewModel.KindFilter, Is.Null);
        }

        [Test]
        public async Task VerifyReplyAsync()
        {
            var empty = await this.viewModel.ReplyAsync(this.changeRequest, " ");

            Assert.That(empty, Is.False);
            Assert.That(this.writes, Is.Empty);

            var replied = await this.viewModel.ReplyAsync(this.changeRequest, "a reply");

            var (_, things) = this.writes.Single();
            var item = things.OfType<EngineeringModelDataDiscussionItem>().Single();
            var clone = things.OfType<ChangeRequest>().Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replied, Is.True);
                Assert.That(things, Has.Count.EqualTo(2));
                Assert.That(item.Content, Is.EqualTo("a reply"));
                Assert.That(item.Author, Is.SameAs(this.participant));
                Assert.That(item.LanguageCode, Is.Not.Empty);
                Assert.That(clone.Iid, Is.EqualTo(this.changeRequest.Iid));
                Assert.That(clone, Is.Not.SameAs(this.changeRequest));
                Assert.That(clone.Discussion, Does.Contain(item));
            }
        }

        [Test]
        public async Task VerifyIsBusyDuringAWrite()
        {
            var busyDuringWrite = false;

            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback(() => busyDuringWrite = this.viewModel.IsBusy)
                .ReturnsAsync(Result.Ok());

            Assert.That(this.viewModel.IsBusy, Is.False);

            await this.viewModel.ReplyAsync(this.changeRequest, "a reply");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(busyDuringWrite, Is.True);
                Assert.That(this.viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task VerifySetStatusAsync()
        {
            await this.viewModel.SetStatusAsync(this.changeRequest, AnnotationStatusKind.DONE);

            var (top, things) = this.writes.Single();
            var clone = things.OfType<ChangeRequest>().Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(top.Iid, Is.EqualTo(this.model.Iid));
                Assert.That(clone.Iid, Is.EqualTo(this.changeRequest.Iid));
                Assert.That(clone.Status, Is.EqualTo(AnnotationStatusKind.DONE));
                Assert.That(this.changeRequest.Status, Is.EqualTo(AnnotationStatusKind.OPEN), "the original is not mutated");
            }
        }

        [Test]
        public async Task VerifyApproveAsync()
        {
            var written = await this.viewModel.ApproveAsync(this.changeRequest, AnnotationApprovalKind.REJECTED, string.Empty);

            var (_, things) = this.writes.Single();
            var approval = things.OfType<Approval>().Single();
            var clone = things.OfType<ChangeRequest>().Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(written, Is.True);
                Assert.That(approval.Classification, Is.EqualTo(AnnotationApprovalKind.REJECTED));
                Assert.That(approval.Content, Is.Not.Empty);
                Assert.That(approval.Author, Is.SameAs(this.participant));
                Assert.That(approval.Owner, Is.SameAs(this.domain));
                Assert.That(clone.Iid, Is.EqualTo(this.changeRequest.Iid));
                Assert.That(clone.ApprovedBy, Does.Contain(approval));
            }
        }

        [Test]
        public async Task VerifyAddSolutionAsync()
        {
            var empty = await this.viewModel.AddSolutionAsync(this.discrepancy, string.Empty);

            Assert.That(empty, Is.False);
            Assert.That(this.writes, Is.Empty);

            var written = await this.viewModel.AddSolutionAsync(this.discrepancy, "a solution");

            var (_, things) = this.writes.Single();
            var solution = things.OfType<Solution>().Single();
            var clone = things.OfType<ReviewItemDiscrepancy>().Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(written, Is.True);
                Assert.That(solution.Content, Is.EqualTo("a solution"));
                Assert.That(solution.Author, Is.SameAs(this.participant));
                Assert.That(solution.Owner, Is.SameAs(this.domain));
                Assert.That(clone.Iid, Is.EqualTo(this.discrepancy.Iid));
                Assert.That(clone.Solution, Does.Contain(solution));
            }
        }

        [Test]
        public async Task VerifyDeleteAsync()
        {
            await this.viewModel.DeleteAsync(this.changeRequest);

            var (top, things) = this.deletes.Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(top.Iid, Is.EqualTo(this.model.Iid));
                Assert.That(things.Select(x => x.Iid), Is.EquivalentTo([this.changeRequest.Iid, this.changeProposal.Iid]));
                Assert.That(things, Does.Not.Contain(this.changeRequest));
                Assert.That(things, Does.Not.Contain(this.changeProposal));
                Assert.That(this.writes, Is.Empty);
            }

            await this.viewModel.DeleteAsync(this.note);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.deletes, Has.Count.EqualTo(2));
                Assert.That(this.deletes.Last().Things.Select(x => x.Iid), Is.EqualTo((Guid[])[this.note.Iid]));
            }
        }

        [Test]
        public async Task VerifyRequestDelete()
        {
            Assert.That(this.confirmCancelPopup.IsVisible, Is.False);

            this.viewModel.RequestDelete(this.note);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.confirmCancelPopup.IsVisible, Is.True);
                Assert.That(this.confirmCancelPopup.ContentText, Does.Not.Contain("change proposal(s)"));
                Assert.That(this.deletes, Is.Empty);
            }

            this.viewModel.RequestDelete(this.changeRequest);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.confirmCancelPopup.ContentText, Does.Contain("CR_001").And.Contain("1 change proposal(s)"));
                Assert.That(this.deletes, Is.Empty, "nothing is deleted before the user confirms");
            }

            await this.confirmCancelPopup.OnConfirm.InvokeAsync();

            var (top, things) = this.deletes.Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(top.Iid, Is.EqualTo(this.model.Iid));
                Assert.That(things.Select(x => x.Iid), Is.EquivalentTo([this.changeRequest.Iid, this.changeProposal.Iid]));
                Assert.That(this.confirmCancelPopup.IsVisible, Is.False);
            }
        }

        [Test]
        public void VerifyAFailingWriteIsLoggedAndDoesNotPropagate()
        {
            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ThrowsAsync(new InvalidOperationException("a failure"));

            var replied = false;

            Assert.DoesNotThrowAsync(async () => replied = await this.viewModel.ReplyAsync(this.changeRequest, "a reply"));
            Assert.DoesNotThrowAsync(() => this.viewModel.SetStatusAsync(this.changeRequest, AnnotationStatusKind.DONE));

            this.viewModel.OpenPanel(this.requirement);
            this.viewModel.StartNew(AnnotationKind.Note);
            this.viewModel.Draft.Content = "a comment";

            Assert.DoesNotThrowAsync(() => this.viewModel.SaveDraftAsync());

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replied, Is.False);
                Assert.That(this.viewModel.Draft, Is.Not.Null, "the draft is kept so the user can retry");

                this.logger.Verify(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
            }
        }

        /// <summary>
        /// Builds an annotation about <paramref name="target" /> and adds it to the model, like the server would store it
        /// </summary>
        /// <param name="kind">The <see cref="AnnotationKind" /></param>
        /// <param name="target">The annotated <see cref="Thing" /></param>
        /// <param name="shortName">The short name, ignored for a note</param>
        /// <param name="status">The <see cref="AnnotationStatusKind" />, ignored for a note</param>
        /// <param name="hours">The number of hours after <see cref="BaseDate" /> the annotation was created</param>
        /// <returns>The <see cref="EngineeringModelDataAnnotation" /></returns>
        private EngineeringModelDataAnnotation AddAnnotation(AnnotationKind kind, Thing target, string shortName, AnnotationStatusKind status, int hours)
        {
            var (annotation, _) = AnnotationHelper.CreateAnnotation(kind, target, this.participant, this.domain, "en", shortName, $"title {hours}", $"content {hours}", AnnotationClassificationKind.MINOR);
            annotation.CreatedOn = BaseDate.AddHours(hours);

            if (annotation is ModellingAnnotationItem item)
            {
                item.Status = status;
                this.model.ModellingAnnotation.Add(item);
            }
            else
            {
                this.model.GenericNote.Add((EngineeringModelDataNote)annotation);
            }

            return annotation;
        }
    }
}
