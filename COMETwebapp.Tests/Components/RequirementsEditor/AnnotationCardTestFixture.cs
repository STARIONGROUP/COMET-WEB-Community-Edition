// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AnnotationCardTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor.Annotations;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="AnnotationCard" /> component.
    /// </summary>
    [TestFixture]
    public class AnnotationCardTestFixture
    {
        /// <summary>
        /// The CSS selectors of the action buttons that depend on a permission, in the order the card renders them.
        /// </summary>
        private static readonly string[] PermissionActions =
        [
            ".ann-action-reply",
            ".ann-action-accept",
            ".ann-action-reject",
            ".ann-action-withdraw",
            ".ann-action-proposal",
            ".ann-action-edit-requirement",
            ".ann-action-edit",
            ".ann-action-delete"
        ];

        /// <summary>
        /// The <see cref="BunitContext" /> the component is rendered in.
        /// </summary>
        private BunitContext context;

        /// <summary>
        /// The mocked <see cref="IRequirementsEditorBodyViewModel" /> handed to the component.
        /// </summary>
        private Mock<IRequirementsEditorBodyViewModel> viewModel;

        /// <summary>
        /// The mocked <see cref="IRequirementAnnotationsViewModel" /> the card asks for permissions and writes through.
        /// </summary>
        private Mock<IRequirementAnnotationsViewModel> annotationsViewModel;

        /// <summary>
        /// The mocked <see cref="ISessionService" /> registered for the component.
        /// </summary>
        private Mock<ISessionService> sessionService;

        /// <summary>
        /// The mocked <see cref="IPermissionService" /> deciding whether the requirement may be edited.
        /// </summary>
        private Mock<IPermissionService> permissionService;

        /// <summary>
        /// The <see cref="Requirement" /> the annotations are about.
        /// </summary>
        private Requirement requirement;

        /// <summary>
        /// The <see cref="Participant" /> authoring the annotations.
        /// </summary>
        private Participant author;

        /// <summary>
        /// The <see cref="DomainOfExpertise" /> owning the annotations.
        /// </summary>
        private DomainOfExpertise owner;

        /// <summary>
        /// Creates the context, the requirement, the author and the mocked view models, with every permission granted.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.IsReadOnly).Returns(false);
            this.context.Services.AddSingleton(this.sessionService.Object);

            this.owner = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            this.author = new Participant { Iid = Guid.NewGuid(), Person = new Person { Iid = Guid.NewGuid(), GivenName = "Author", Surname = "One" } };
            this.requirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R1", Name = "First requirement" };

            this.annotationsViewModel = new Mock<IRequirementAnnotationsViewModel>();
            this.annotationsViewModel.Setup(x => x.ActiveRequirement).Returns(this.requirement);
            this.annotationsViewModel.Setup(x => x.GetChangeProposals(It.IsAny<ChangeRequest>())).Returns([]);
            this.annotationsViewModel.Setup(x => x.ReplyAsync(It.IsAny<EngineeringModelDataAnnotation>(), It.IsAny<string>())).ReturnsAsync(true);
            this.annotationsViewModel.Setup(x => x.ApproveAsync(It.IsAny<ModellingAnnotationItem>(), It.IsAny<AnnotationApprovalKind>(), It.IsAny<string>())).ReturnsAsync(true);
            this.SetPermissions(true);

            this.viewModel = new Mock<IRequirementsEditorBodyViewModel>();
            this.viewModel.Setup(x => x.AnnotationsViewModel).Returns(this.annotationsViewModel.Object);
        }

        /// <summary>
        /// Cleans the <see cref="BunitContext" />.
        /// </summary>
        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        /// <summary>
        /// Grants or denies every permission the card asks for, on the annotations view model and on the permission service.
        /// </summary>
        /// <param name="granted">True to grant every permission</param>
        private void SetPermissions(bool granted)
        {
            this.annotationsViewModel.Setup(x => x.CanReply(It.IsAny<EngineeringModelDataAnnotation>())).Returns(granted);
            this.annotationsViewModel.Setup(x => x.CanApprove(It.IsAny<EngineeringModelDataAnnotation>())).Returns(granted);
            this.annotationsViewModel.Setup(x => x.CanAddSolution(It.IsAny<EngineeringModelDataAnnotation>())).Returns(granted);
            this.annotationsViewModel.Setup(x => x.CanProposeChange(It.IsAny<EngineeringModelDataAnnotation>())).Returns(granted);
            this.annotationsViewModel.Setup(x => x.CanEdit(It.IsAny<EngineeringModelDataAnnotation>())).Returns(granted);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(granted);
        }

        /// <summary>
        /// Creates a real annotation of the given kind about the <see cref="requirement" />.
        /// </summary>
        /// <param name="kind">The <see cref="RequirementAnnotationKind" /></param>
        /// <returns>The new <see cref="EngineeringModelDataAnnotation" /></returns>
        private EngineeringModelDataAnnotation CreateAnnotation(RequirementAnnotationKind kind)
        {
            var annotation = RequirementAnnotationHelper.CreateAnnotation(kind, this.requirement, this.author, this.owner, "en", $"{RequirementAnnotationHelper.GetAcronym(kind)}-001", "A title", "Some content", AnnotationClassificationKind.MAJOR).Annotation;
            annotation.CreatedOn = new DateTime(2026, 3, 4, 14, 5, 59, DateTimeKind.Utc);

            return annotation;
        }

        /// <summary>
        /// Renders the card for an annotation.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" /> to render</param>
        /// <param name="isNested">A value indicating whether the card is nested in another one</param>
        /// <returns>The <see cref="IRenderedComponent{TComponent}" /></returns>
        private IRenderedComponent<AnnotationCard> Render(EngineeringModelDataAnnotation annotation, bool isNested = false)
        {
            return this.context.Render<AnnotationCard>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Annotation, annotation)
                .Add(p => p.IsNested, isNested));
        }

        /// <summary>
        /// Verifies the header, the title, the meta line and the content of a change request.
        /// </summary>
        [Test]
        public void VerifyChangeRequestRendersItsDetails()
        {
            var annotation = this.CreateAnnotation(RequirementAnnotationKind.ChangeRequest);

            var component = this.Render(annotation);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.Find("article").Id, Is.EqualTo($"annotation-{annotation.Iid}"));
                Assert.That(component.Find("article").ClassList, Does.Contain("ann-kind-cr").And.Contain("is-open").And.Not.Contain("is-nested"));
                Assert.That(component.Find(".ann-kind-pill").TextContent, Is.EqualTo("CR"));
                Assert.That(component.Find(".ann-card-id").TextContent, Is.EqualTo("CR-001"));
                Assert.That(component.Find(".ann-classification").TextContent, Is.EqualTo("Major"));
                Assert.That(component.Find(".ann-card-title").TextContent, Is.EqualTo("A title"));
                Assert.That(component.Find(".ann-card-meta").TextContent, Does.Contain("Author One").And.Contain("2026-03-04 14:05 UTC"));
                Assert.That(component.Find(".ann-card-content").TextContent, Is.EqualTo("Some content"));
            }
        }

        /// <summary>
        /// Verifies that each category of a modelling annotation item is rendered as a pill, without an owner pill.
        /// </summary>
        [Test]
        public void VerifyCategoryPillsRender()
        {
            var annotation = (ModellingAnnotationItem)this.CreateAnnotation(RequirementAnnotationKind.ChangeRequest);
            annotation.Category.AddRange(
            [
                new Category { Iid = Guid.NewGuid(), ShortName = "ALPHA", Name = "Alpha" },
                new Category { Iid = Guid.NewGuid(), ShortName = "BETA", Name = "Beta" }
            ]);

            var component = this.Render(annotation);
            var pills = component.FindAll(".ann-card-pills .starion-pill");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll(".ann-card-pills"), Has.Count.EqualTo(1));
                Assert.That(pills.Select(x => x.TextContent), Is.EqualTo(["ALPHA", "BETA"]), "one pill per category, no owner pill (the author is shown already)");
                Assert.That(pills.Select(x => x.GetAttribute("title")), Is.EqualTo(["Category: Alpha", "Category: Beta"]));
            }
        }

        /// <summary>
        /// Verifies that category pills render only when the item has categories.
        /// </summary>
        [Test]
        public void VerifyPillsOnlyRenderWhenThereIsACategory()
        {
            var withoutCategory = this.Render(this.CreateAnnotation(RequirementAnnotationKind.ChangeRequest));

            Assert.That(withoutCategory.FindAll(".ann-card-pills"), Is.Empty, "an owner alone gives no pill");

            var categoryOnly = (ModellingAnnotationItem)this.CreateAnnotation(RequirementAnnotationKind.ReviewItemDiscrepancy);
            categoryOnly.Owner = null;
            categoryOnly.Category.Add(new Category { Iid = Guid.NewGuid(), ShortName = "ALPHA", Name = "Alpha" });

            var withCategoryOnly = this.Render(categoryOnly);

            var bare = (ModellingAnnotationItem)this.CreateAnnotation(RequirementAnnotationKind.RequestForWaiver);
            bare.Owner = null;

            var withoutPills = this.Render(bare);
            var note = this.Render(this.CreateAnnotation(RequirementAnnotationKind.Note));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(withCategoryOnly.FindAll(".ann-card-pills .starion-pill").Select(x => x.TextContent), Is.EqualTo(["ALPHA"]), "the category alone");
                Assert.That(withoutPills.FindAll(".ann-card-pills"), Is.Empty);
                Assert.That(note.FindAll(".ann-card-pills"), Is.Empty, "a note has no category");
            }
        }

        /// <summary>
        /// Verifies that a resolved annotation is styled as resolved and that a nested card is marked as nested.
        /// </summary>
        [Test]
        public void VerifyResolvedAndNestedStyling()
        {
            var annotation = this.CreateAnnotation(RequirementAnnotationKind.ChangeRequest);
            ((ModellingAnnotationItem)annotation).Status = AnnotationStatusKind.CLOSED;
            this.SetPermissions(false);

            var component = this.Render(annotation, true);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.Find("article").ClassList, Does.Contain("is-resolved").And.Contain("is-nested"));
                Assert.That(component.Find("article").ClassList, Does.Not.Contain("is-open"));
                Assert.That(component.Find(".ann-status").TextContent, Is.EqualTo("Closed"));
            }
        }

        /// <summary>
        /// Verifies that every permission-dependent action is shown when every permission is granted.
        /// </summary>
        [Test]
        public void VerifyAllActionsRenderWhenPermissionsGranted()
        {
            var component = this.Render(this.CreateAnnotation(RequirementAnnotationKind.ChangeRequest));

            using (Assert.EnterMultipleScope())
            {
                foreach (var selector in PermissionActions)
                {
                    Assert.That(component.FindAll(selector), Has.Count.EqualTo(1), selector);
                }

                Assert.That(component.FindAll(".ann-status-select"), Has.Count.EqualTo(1), "the status is editable");
                Assert.That(component.FindAll("span.ann-status"), Is.Empty);
            }
        }

        /// <summary>
        /// Verifies that no permission-dependent action is shown when no permission is granted, and that the status is
        /// read-only.
        /// </summary>
        [Test]
        public void VerifyNoActionRendersWhenPermissionsDenied()
        {
            this.SetPermissions(false);

            var component = this.Render(this.CreateAnnotation(RequirementAnnotationKind.ChangeRequest));

            using (Assert.EnterMultipleScope())
            {
                foreach (var selector in PermissionActions)
                {
                    Assert.That(component.FindAll(selector), Is.Empty, selector);
                }

                Assert.That(component.FindAll(".ann-status-select"), Is.Empty);
                Assert.That(component.Find("span.ann-status").TextContent, Is.EqualTo("Open"));
            }
        }

        /// <summary>
        /// Verifies that the edit requirement action is hidden when the session is read-only, whatever the permissions.
        /// </summary>
        [Test]
        public void VerifyEditRequirementActionIsHiddenForReadOnlySession()
        {
            this.sessionService.Setup(x => x.IsReadOnly).Returns(true);

            var component = this.Render(this.CreateAnnotation(RequirementAnnotationKind.ChangeRequest));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll(".ann-action-edit-requirement"), Is.Empty);
                Assert.That(component.FindAll(".ann-action-reply"), Has.Count.EqualTo(1), "the view model permissions still apply");
            }
        }

        /// <summary>
        /// Verifies that a note shows neither a short name, a classification, a status nor a decision action.
        /// </summary>
        [Test]
        public void VerifyNoteRendersNoStatusNorDecisionActions()
        {
            this.annotationsViewModel.Setup(x => x.CanApprove(It.IsAny<EngineeringModelDataAnnotation>())).Returns(false);
            this.annotationsViewModel.Setup(x => x.CanProposeChange(It.IsAny<EngineeringModelDataAnnotation>())).Returns(false);
            this.annotationsViewModel.Setup(x => x.CanAddSolution(It.IsAny<EngineeringModelDataAnnotation>())).Returns(false);

            var component = this.Render(this.CreateAnnotation(RequirementAnnotationKind.Note));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.Find(".ann-kind-pill").TextContent, Is.EqualTo("Note"));
                Assert.That(component.FindAll(".ann-card-id"), Is.Empty);
                Assert.That(component.FindAll(".ann-classification"), Is.Empty);
                Assert.That(component.FindAll(".ann-card-title"), Is.Empty);
                Assert.That(component.FindAll(".ann-status"), Is.Empty);
                Assert.That(component.FindAll(".ann-status-select"), Is.Empty);
                Assert.That(component.FindAll(".ann-action-accept"), Is.Empty);
                Assert.That(component.FindAll(".ann-action-reject"), Is.Empty);
                Assert.That(component.FindAll(".ann-action-proposal"), Is.Empty);
                Assert.That(component.FindAll(".ann-action-edit-requirement"), Is.Empty, "a note does not ask for the requirement to change");
                Assert.That(component.FindAll(".ann-action-reply"), Has.Count.EqualTo(1));
                Assert.That(component.FindAll(".ann-action-edit"), Has.Count.EqualTo(1));
                Assert.That(component.FindAll(".ann-action-delete"), Has.Count.EqualTo(1));
            }
        }

        /// <summary>
        /// Verifies that the replies of the discussion are rendered with their author, oldest first.
        /// </summary>
        [Test]
        public void VerifyDiscussionRepliesRender()
        {
            var annotation = this.CreateAnnotation(RequirementAnnotationKind.Note);
            var replier = new Participant { Iid = Guid.NewGuid(), Person = new Person { Iid = Guid.NewGuid(), GivenName = "Replier", Surname = "Two" } };

            var later = RequirementAnnotationHelper.CreateDiscussionItem(replier, "en", "Second reply");
            later.CreatedOn = new DateTime(2026, 3, 6, 9, 0, 0, DateTimeKind.Utc);
            var earlier = RequirementAnnotationHelper.CreateDiscussionItem(this.author, "en", "First reply");
            earlier.CreatedOn = new DateTime(2026, 3, 5, 8, 30, 0, DateTimeKind.Utc);
            annotation.Discussion.AddRange([later, earlier]);

            var component = this.Render(annotation);
            var replies = component.FindAll(".ann-reply");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replies, Has.Count.EqualTo(2));
                Assert.That(replies[0].TextContent, Does.Contain("Author One").And.Contain("First reply").And.Contain("2026-03-05 08:30 UTC"));
                Assert.That(replies[1].TextContent, Does.Contain("Replier Two").And.Contain("Second reply"));
            }
        }

        /// <summary>
        /// Verifies that a card without discussion renders no thread.
        /// </summary>
        [Test]
        public void VerifyNoThreadRendersWithoutDiscussion()
        {
            var component = this.Render(this.CreateAnnotation(RequirementAnnotationKind.Note));

            Assert.That(component.FindAll(".ann-thread"), Is.Empty);
        }

        /// <summary>
        /// Verifies that the decisions and the proposed solutions are rendered.
        /// </summary>
        [Test]
        public void VerifyApprovalsAndSolutionsRender()
        {
            var annotation = (ReviewItemDiscrepancy)this.CreateAnnotation(RequirementAnnotationKind.ReviewItemDiscrepancy);
            annotation.ApprovedBy.Add(RequirementAnnotationHelper.CreateApproval(this.author, this.owner, "en", AnnotationApprovalKind.ACCEPTED, string.Empty));
            annotation.ApprovedBy.Add(RequirementAnnotationHelper.CreateApproval(this.author, this.owner, "en", AnnotationApprovalKind.REJECTED, "Not convincing"));
            annotation.Solution.Add(RequirementAnnotationHelper.CreateSolution(this.author, this.owner, "en", "Reword it"));

            var component = this.Render(annotation);
            var approvals = component.FindAll(".ann-approval");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(approvals, Has.Count.EqualTo(2));
                Assert.That(approvals.Select(x => x.TextContent).Count(x => x.Contains("Accepted") && x.Contains("Author One") && x.Contains("(SYS)")), Is.EqualTo(1));
                Assert.That(approvals.Select(x => x.TextContent).Count(x => x.Contains("Rejected") && x.Contains("Not convincing")), Is.EqualTo(1));
                Assert.That(component.FindAll(".ann-approval-comment"), Has.Count.EqualTo(1), "a comment equal to the decision label is not repeated");
                Assert.That(component.Find(".ann-solution").TextContent, Does.Contain("Reword it").And.Contain("Author One"));
            }
        }

        /// <summary>
        /// Verifies that the change proposals answering a change request are rendered as nested cards.
        /// </summary>
        [Test]
        public void VerifyChangeProposalsRenderAsNestedCards()
        {
            var changeRequest = (ChangeRequest)this.CreateAnnotation(RequirementAnnotationKind.ChangeRequest);
            var proposal = (ChangeProposal)this.CreateAnnotation(RequirementAnnotationKind.ChangeProposal);
            proposal.ChangeRequest = changeRequest;
            this.annotationsViewModel.Setup(x => x.GetChangeProposals(changeRequest)).Returns([proposal]);

            var component = this.Render(changeRequest);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll(".ann-card"), Has.Count.EqualTo(2));
                Assert.That(component.FindAll(".ann-card.is-nested"), Has.Count.EqualTo(1));
                Assert.That(component.Find(".ann-card.is-nested").ClassList, Does.Contain("ann-kind-cp"));
                Assert.That(component.Find("section[aria-label='Change proposals'] .ann-card.is-nested").Id, Is.EqualTo($"annotation-{proposal.Iid}"));
            }
        }

        /// <summary>
        /// Verifies that a change request without change proposal renders no proposals section.
        /// </summary>
        [Test]
        public void VerifyNoProposalsSectionWithoutChangeProposals()
        {
            var component = this.Render(this.CreateAnnotation(RequirementAnnotationKind.ChangeRequest));

            Assert.That(component.FindAll("section[aria-label='Change proposals']"), Is.Empty);
        }

        /// <summary>
        /// Verifies that the reply button opens the inline form, that the send button is disabled until a text is typed,
        /// and that sending writes the reply and closes the form.
        /// </summary>
        [Test]
        public async Task VerifyReplyFormSendsReply()
        {
            var annotation = this.CreateAnnotation(RequirementAnnotationKind.Note);
            var component = this.Render(annotation);

            Assert.That(component.FindComponents<DxMemo>(), Is.Empty, "no form is open yet");

            component.Find(".ann-action-reply").Click();

            var sendButton = component.FindComponents<DxButton>().First(x => x.Instance.CssClass == "ann-reply-send");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll(".ann-inline-form"), Has.Count.EqualTo(1));
                Assert.That(component.FindAll(".ann-card-actions"), Is.Empty, "the form replaces the actions");
                Assert.That(sendButton.Instance.Enabled, Is.False);
            }

            await component.InvokeAsync(() => component.FindComponent<DxMemo>().Instance.TextChanged.InvokeAsync("My reply"));

            sendButton = component.FindComponents<DxButton>().First(x => x.Instance.CssClass == "ann-reply-send");
            Assert.That(sendButton.Instance.Enabled, Is.True);

            await component.InvokeAsync(sendButton.Instance.Click.InvokeAsync);

            this.annotationsViewModel.Verify(x => x.ReplyAsync(annotation, "My reply"), Times.Once);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll(".ann-inline-form"), Is.Empty, "the form closes once the reply is written");
                Assert.That(component.FindAll(".ann-card-actions"), Has.Count.EqualTo(1));
            }
        }

        /// <summary>
        /// Verifies that the form stays open when the reply could not be written.
        /// </summary>
        [Test]
        public async Task VerifyReplyFormStaysOpenWhenReplyFails()
        {
            var annotation = this.CreateAnnotation(RequirementAnnotationKind.Note);
            this.annotationsViewModel.Setup(x => x.ReplyAsync(annotation, It.IsAny<string>())).ReturnsAsync(false);

            var component = this.Render(annotation);
            component.Find(".ann-action-reply").Click();
            await component.InvokeAsync(() => component.FindComponent<DxMemo>().Instance.TextChanged.InvokeAsync("My reply"));

            var sendButton = component.FindComponents<DxButton>().First(x => x.Instance.CssClass == "ann-reply-send");
            await component.InvokeAsync(sendButton.Instance.Click.InvokeAsync);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll(".ann-inline-form"), Has.Count.EqualTo(1));
                Assert.That(component.FindComponent<DxMemo>().Instance.Text, Is.EqualTo("My reply"));
            }
        }

        /// <summary>
        /// Verifies that cancelling the reply form brings the actions back without writing anything.
        /// </summary>
        [Test]
        public async Task VerifyCancelClosesReplyForm()
        {
            var component = this.Render(this.CreateAnnotation(RequirementAnnotationKind.Note));
            component.Find(".ann-action-reply").Click();

            var cancelButton = component.FindComponents<DxButton>().First(x => x.Instance.Text == "Cancel");
            await component.InvokeAsync(cancelButton.Instance.Click.InvokeAsync);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll(".ann-inline-form"), Is.Empty);
                Assert.That(component.FindAll(".ann-card-actions"), Has.Count.EqualTo(1));
            }

            this.annotationsViewModel.Verify(x => x.ReplyAsync(It.IsAny<EngineeringModelDataAnnotation>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Verifies that the accept button opens the decision form and that confirming records the decision with the comment.
        /// </summary>
        [Test]
        public async Task VerifyAcceptFormRecordsDecision()
        {
            var annotation = (ModellingAnnotationItem)this.CreateAnnotation(RequirementAnnotationKind.ChangeRequest);
            var component = this.Render(annotation);

            component.Find(".ann-action-accept").Click();

            Assert.That(component.Find(".ann-inline-title").TextContent, Does.Contain("Accepted").And.Contain("CR-001"));

            await component.InvokeAsync(() => component.FindComponent<DxMemo>().Instance.TextChanged.InvokeAsync("Agreed"));
            var confirmButton = component.FindComponents<DxButton>().First(x => x.Instance.Text == "Confirm accepted");
            await component.InvokeAsync(confirmButton.Instance.Click.InvokeAsync);

            this.annotationsViewModel.Verify(x => x.ApproveAsync(annotation, AnnotationApprovalKind.ACCEPTED, "Agreed"), Times.Once);
            Assert.That(component.FindAll(".ann-inline-form"), Is.Empty);
        }

        /// <summary>
        /// Verifies that the action buttons call the view model: change proposal, edit requirement, edit and delete.
        /// </summary>
        [Test]
        public async Task VerifyActionButtonsCallViewModel()
        {
            var annotation = this.CreateAnnotation(RequirementAnnotationKind.ChangeRequest);
            var component = this.Render(annotation);

            await component.InvokeAsync(this.FindActionButton(component, "ann-action-delete").Instance.Click.InvokeAsync);
            await component.InvokeAsync(this.FindActionButton(component, "ann-action-edit").Instance.Click.InvokeAsync);
            await component.InvokeAsync(this.FindActionButton(component, "ann-action-proposal").Instance.Click.InvokeAsync);
            await component.InvokeAsync(this.FindActionButton(component, "ann-action-edit-requirement").Instance.Click.InvokeAsync);

            this.annotationsViewModel.Verify(x => x.RequestDelete(annotation), Times.Once);
            this.annotationsViewModel.Verify(x => x.StartEdit(annotation), Times.Once);
            this.annotationsViewModel.Verify(x => x.StartChangeProposal((ChangeRequest)annotation), Times.Once);
            this.viewModel.Verify(x => x.OpenEdit(this.requirement), Times.Once);
        }

        /// <summary>
        /// Verifies that the solution button of a review item discrepancy opens the solution form.
        /// </summary>
        [Test]
        public void VerifySolutionButtonOpensSolutionForm()
        {
            var component = this.Render(this.CreateAnnotation(RequirementAnnotationKind.ReviewItemDiscrepancy));

            component.Find(".ann-action-solution").Click();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll(".ann-inline-form"), Has.Count.EqualTo(1));
                Assert.That(component.FindComponent<DxMemo>().Instance.NullText, Is.EqualTo("Describe the proposed solution..."));
            }
        }

        /// <summary>
        /// Verifies the formatting of a creation date.
        /// </summary>
        [Test]
        public void VerifyFormatDate()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(AnnotationCard.FormatDate(new DateTime(2026, 10, 21, 14, 5, 59, DateTimeKind.Utc)), Is.EqualTo("2026-10-21 14:05 UTC"));
                Assert.That(AnnotationCard.FormatDate(new DateTime(2026, 1, 2, 3, 4, 0, DateTimeKind.Utc)), Is.EqualTo("2026-01-02 03:04 UTC"));
            }
        }

        /// <summary>
        /// Verifies the display name of an author, which falls back to "Unknown" whenever it cannot be resolved.
        /// </summary>
        [Test]
        public void VerifyGetAuthorName()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(AnnotationCard.GetAuthorName(null), Is.EqualTo("Unknown"));
                Assert.That(AnnotationCard.GetAuthorName(new Participant()), Is.EqualTo("Unknown"));
                Assert.That(AnnotationCard.GetAuthorName(new Participant { Person = new Person { GivenName = " ", Surname = " " } }), Is.EqualTo("Unknown"));
                Assert.That(AnnotationCard.GetAuthorName(this.author), Is.EqualTo("Author One"));
            }
        }

        /// <summary>
        /// Finds the <see cref="DxButton" /> carrying the given CSS class.
        /// </summary>
        /// <param name="component">The rendered <see cref="AnnotationCard" /></param>
        /// <param name="cssClass">The CSS class of the button</param>
        /// <returns>The <see cref="IRenderedComponent{TComponent}" /> of the button</returns>
        private IRenderedComponent<DxButton> FindActionButton(IRenderedComponent<AnnotationCard> component, string cssClass)
        {
            return component.FindComponents<DxButton>().First(x => x.Instance.CssClass != null && x.Instance.CssClass.Split(' ').Contains(cssClass));
        }
    }
}
