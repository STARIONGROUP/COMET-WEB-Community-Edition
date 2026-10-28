// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AnnotationsPanelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.Annotations
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

    using COMETwebapp.Components.Annotations;
    using COMETwebapp.ViewModels.Components.Annotations;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="AnnotationsPanel" /> component.
    /// </summary>
    [TestFixture]
    public class AnnotationsPanelTestFixture
    {
        /// <summary>
        /// The <see cref="BunitContext" /> the component is rendered in.
        /// </summary>
        private BunitContext context;

        /// <summary>
        /// The mocked <see cref="IAnnotationsViewModel" /> driving the panel.
        /// </summary>
        private Mock<IAnnotationsViewModel> annotationsViewModel;

        /// <summary>
        /// The mocked <see cref="IPermissionService" /> deciding whether the annotated thing may be edited.
        /// </summary>
        private Mock<IPermissionService> permissionService;

        /// <summary>
        /// The <see cref="DomainOfExpertise" /> owning the annotations.
        /// </summary>
        private DomainOfExpertise owner;

        /// <summary>
        /// The <see cref="Requirement" /> the panel is open on.
        /// </summary>
        private Requirement requirement;

        /// <summary>
        /// The annotations of the <see cref="requirement" />.
        /// </summary>
        private List<EngineeringModelDataAnnotation> annotations;

        /// <summary>
        /// Creates the context, the requirement, two annotations and the mocked view models, with the panel open and no draft.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;

            this.permissionService = new Mock<IPermissionService>();
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            var sessionService = new Mock<ISessionService>();
            sessionService.Setup(x => x.Session).Returns(session.Object);
            this.context.Services.AddSingleton(sessionService.Object);

            this.owner = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            var author = new Participant { Iid = Guid.NewGuid(), Person = new Person { Iid = Guid.NewGuid(), GivenName = "Author", Surname = "One" } };

            this.requirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R1", Name = "First requirement" };

            var changeRequest = AnnotationHelper.CreateAnnotation(AnnotationKind.ChangeRequest, this.requirement, author, this.owner, "en", "CR-001", "First change", "Change the wording", AnnotationClassificationKind.MINOR).Annotation;
            var note = AnnotationHelper.CreateAnnotation(AnnotationKind.Note, this.requirement, author, this.owner, "en", string.Empty, string.Empty, "A question", AnnotationClassificationKind.MINOR).Annotation;
            this.annotations = [changeRequest, note];

            this.annotationsViewModel = new Mock<IAnnotationsViewModel>();
            this.annotationsViewModel.Setup(x => x.ActiveThing).Returns(this.requirement);
            this.annotationsViewModel.Setup(x => x.GetAnnotations(this.requirement)).Returns(this.annotations);
            this.annotationsViewModel.Setup(x => x.VisibleAnnotations).Returns(this.annotations);
            this.annotationsViewModel.Setup(x => x.GetSummary(this.requirement)).Returns(
            [
                new AnnotationKindSummary(AnnotationKind.Note, 1, 0),
                new AnnotationKindSummary(AnnotationKind.ChangeRequest, 1, 1)
            ]);
            this.annotationsViewModel.Setup(x => x.GetChangeProposals(It.IsAny<ChangeRequest>())).Returns([]);
            this.annotationsViewModel.Setup(x => x.CreatableKinds).Returns(AnnotationHelper.AttachableKinds);
            this.annotationsViewModel.Setup(x => x.Draft).Returns((AnnotationDraft)null);
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
        /// Creates the draft editing the first annotation, a change request, with a title, a content, an owner and categories.
        /// </summary>
        /// <returns>The <see cref="AnnotationDraft" /></returns>
        private AnnotationDraft CreateEditedDraft()
        {
            var category = new Category { Iid = Guid.NewGuid(), ShortName = "ALPHA", Name = "Alpha" };

            return new AnnotationDraft
            {
                Kind = AnnotationKind.ChangeRequest,
                Title = "First change",
                Content = "Change the wording",
                ShortName = "CR_001",
                Owner = this.owner,
                Categories = [category],
                AvailableOwners = [this.owner],
                AvailableCategories = [category],
                EditedAnnotation = this.annotations[0]
            };
        }

        /// <summary>
        /// Removes every annotation of the <see cref="requirement" />, so that the panel offers one button per kind to add.
        /// </summary>
        private void ClearAnnotations()
        {
            this.annotations.Clear();
            this.annotationsViewModel.Setup(x => x.GetSummary(this.requirement)).Returns([]);
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <returns>The <see cref="IRenderedComponent{TComponent}" /></returns>
        private IRenderedComponent<AnnotationsPanel> Render()
        {
            return this.context.Render<AnnotationsPanel>(parameters => parameters
                .Add(p => p.ViewModel, this.annotationsViewModel.Object));
        }

        /// <summary>
        /// Verifies that nothing is rendered while the panel is closed.
        /// </summary>
        [Test]
        public void VerifyNothingRendersWithoutActiveThing()
        {
            this.annotationsViewModel.Setup(x => x.ActiveThing).Returns((Thing)null);

            var component = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.Markup.Trim(), Is.Empty);
                Assert.That(component.FindAll("#annotations-panel"), Is.Empty);
            }
        }

        /// <summary>
        /// Verifies that the header of a thing without a short name falls back to its name, or to its class, and that the
        /// name is not repeated when it equals the short name.
        /// </summary>
        [Test]
        public void VerifyHeaderOfAThingWithoutShortName()
        {
            var iteration = new Iteration { Iid = Guid.NewGuid() };
            var noShortName = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = string.Empty, Name = "Directory" };
            var sameName = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT", Name = "SAT" };

            this.annotationsViewModel.Setup(x => x.VisibleAnnotations).Returns([]);
            this.annotationsViewModel.Setup(x => x.GetAnnotations(It.IsAny<Thing>())).Returns([]);
            this.annotationsViewModel.Setup(x => x.GetSummary(It.IsAny<Thing>())).Returns([]);

            this.annotationsViewModel.Setup(x => x.ActiveThing).Returns(iteration);
            var iterationPanel = this.Render();

            this.annotationsViewModel.Setup(x => x.ActiveThing).Returns(noShortName);
            var namedPanel = this.Render();

            this.annotationsViewModel.Setup(x => x.ActiveThing).Returns(sameName);
            var samePanel = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(iterationPanel.Find("#annotations-panel").GetAttribute("aria-label"), Is.EqualTo("Annotations of Iteration"));
                Assert.That(iterationPanel.Find(".ann-panel-subject").TextContent.Trim(), Is.EqualTo("Iteration"));
                Assert.That(namedPanel.Find("#annotations-panel").GetAttribute("aria-label"), Is.EqualTo("Annotations of Directory"));
                Assert.That(namedPanel.Find(".ann-panel-subject").TextContent, Is.EqualTo("Directory"), "the name is the short name, so it is not repeated");
                Assert.That(samePanel.Find(".ann-panel-subject").TextContent, Is.EqualTo("SAT"), "the name equal to the short name is not repeated");
            }
        }

        /// <summary>
        /// Verifies that the action opening the annotated thing for editing reaches the cards with the text the host
        /// supplied, and calls the host back with the active thing.
        /// </summary>
        [Test]
        public async Task VerifyEditAnnotatedThingParametersReachTheCards()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            Thing edited = null;

            var component = this.context.Render<AnnotationsPanel>(parameters => parameters
                .Add(p => p.ViewModel, this.annotationsViewModel.Object)
                .Add(p => p.EditAnnotatedThingText, "Edit requirement")
                .Add(p => p.OnEditAnnotatedThing, (Thing thing) => edited = thing));

            var action = component.FindComponents<DxButton>().Single(x => x.Instance.CssClass != null && x.Instance.CssClass.Contains("ann-action-edit-thing"));

            await component.InvokeAsync(action.Instance.Click.InvokeAsync);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(action.Instance.Text, Is.EqualTo("Edit requirement"));
                Assert.That(edited, Is.SameAs(this.requirement));
            }

            var withoutHost = this.Render();

            Assert.That(withoutHost.FindAll(".ann-action-edit-thing"), Is.Empty, "without a host callback the action is not offered");
        }

        /// <summary>
        /// Verifies that the header, the filter chips and one card per visible annotation are rendered.
        /// </summary>
        [Test]
        public void VerifyHeaderChipsAndCardsRender()
        {
            var component = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.Find("#annotations-panel").GetAttribute("aria-label"), Is.EqualTo("Annotations of R1"));
                Assert.That(component.Find(".ann-panel-subject").TextContent, Does.Contain("R1").And.Contain("First requirement"));
                Assert.That(component.FindAll(".ann-card"), Has.Count.EqualTo(2));
                Assert.That(component.FindAll(".ann-list .ann-empty"), Is.Empty);

                var chips = component.FindAll(".ann-chip");
                Assert.That(chips, Has.Count.EqualTo(3), "one chip for all kinds and one per kind present");
                Assert.That(chips[0].TextContent, Does.Contain("All").And.Contain("2"));
                Assert.That(chips[1].ClassList, Does.Contain("ann-kind-note"));
                Assert.That(chips[2].ClassList, Does.Contain("ann-kind-cr"));
                Assert.That(chips[0].ClassList, Does.Contain("is-selected"), "no kind filter selects the all chip");
            }
        }

        /// <summary>
        /// Verifies that only the visible annotations are rendered as cards.
        /// </summary>
        [Test]
        public void VerifyOnlyVisibleAnnotationsRenderAsCards()
        {
            this.annotationsViewModel.Setup(x => x.VisibleAnnotations).Returns([this.annotations[0]]);

            var component = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll(".ann-card"), Has.Count.EqualTo(1));
                Assert.That(component.FindAll(".ann-card.ann-kind-cr"), Has.Count.EqualTo(1));
            }
        }

        /// <summary>
        /// Verifies that a filter that hides every annotation shows its own empty message.
        /// </summary>
        [Test]
        public void VerifyFilterEmptyStateRenders()
        {
            this.annotationsViewModel.Setup(x => x.VisibleAnnotations).Returns([]);

            var component = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll(".ann-card"), Is.Empty);
                Assert.That(component.Find(".ann-empty").TextContent, Does.Contain("No annotation matches the current filter."));
            }
        }

        /// <summary>
        /// Verifies that a requirement without annotations also offers the single "+ Annotation" dropdown, listing the
        /// creatable kinds only, and no separate button per kind.
        /// </summary>
        [Test]
        public void VerifyAddDropdownRendersWithoutAnnotations()
        {
            this.ClearAnnotations();
            this.annotationsViewModel.Setup(x => x.CreatableKinds).Returns([AnnotationKind.Note, AnnotationKind.ChangeRequest, AnnotationKind.RequestForWaiver]);

            var component = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll("#annotation-add"), Has.Count.EqualTo(1), "the dropdown is offered even when there is nothing to read");
                Assert.That(component.FindAll("button[id^='annotation-new-']"), Is.Empty, "the kinds stay folded until the dropdown is opened");
                Assert.That(component.Find(".ann-empty").TextContent, Does.Contain("No annotations yet."));
            }

            component.Find("#annotation-add").Click();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll(".ann-add-item"), Has.Count.EqualTo(3));
                Assert.That(component.FindAll("#annotation-new-note"), Has.Count.EqualTo(1));
                Assert.That(component.FindAll("#annotation-new-cr"), Has.Count.EqualTo(1));
                Assert.That(component.FindAll("#annotation-new-rfw"), Has.Count.EqualTo(1));
                Assert.That(component.FindAll("#annotation-new-rid"), Is.Empty);
                Assert.That(component.FindAll("#annotation-new-rfd"), Is.Empty);
            }
        }

        /// <summary>
        /// Verifies that choosing a kind in the dropdown of a requirement without annotations starts a new annotation of
        /// that kind.
        /// </summary>
        [Test]
        public void VerifyChoosingAKindWithoutAnnotationsStartsNewAnnotation()
        {
            this.ClearAnnotations();

            var component = this.Render();

            component.Find("#annotation-add").Click();
            component.Find("#annotation-new-cr").Click();

            this.annotationsViewModel.Verify(x => x.StartNew(AnnotationKind.ChangeRequest), Times.Once);
            this.annotationsViewModel.Verify(x => x.StartNew(AnnotationKind.Note), Times.Never);
        }

        /// <summary>
        /// Verifies that a requirement that already has annotations offers the single, closed "+ Annotation" dropdown.
        /// </summary>
        [Test]
        public void VerifyAddDropdownRendersWithAnnotations()
        {
            var component = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll("#annotation-add"), Has.Count.EqualTo(1));
                Assert.That(component.Find("#annotation-add").GetAttribute("aria-expanded"), Is.EqualTo("false"));
                Assert.That(component.FindAll(".ann-add-list"), Is.Empty, "the kinds stay folded until the toggle is clicked");
                Assert.That(component.FindAll(".ann-add-item"), Is.Empty);
                Assert.That(component.Instance.IsAddMenuOpen, Is.False);
            }
        }

        /// <summary>
        /// Verifies that the toggle opens the dropdown listing the creatable kinds only, and closes it again.
        /// </summary>
        [Test]
        public void VerifyAddToggleOpensAndClosesTheDropdown()
        {
            this.annotationsViewModel.Setup(x => x.CreatableKinds).Returns([AnnotationKind.Note, AnnotationKind.ChangeRequest, AnnotationKind.RequestForWaiver]);

            var component = this.Render();

            component.Find("#annotation-add").Click();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.Instance.IsAddMenuOpen, Is.True);
                Assert.That(component.Find("#annotation-add").GetAttribute("aria-expanded"), Is.EqualTo("true"));
                Assert.That(component.FindAll(".ann-add-list .ann-add-item"), Has.Count.EqualTo(3));
                Assert.That(component.FindAll("#annotation-new-note"), Has.Count.EqualTo(1));
                Assert.That(component.FindAll("#annotation-new-cr"), Has.Count.EqualTo(1));
                Assert.That(component.FindAll("#annotation-new-rfw"), Has.Count.EqualTo(1));
                Assert.That(component.FindAll("#annotation-new-rid"), Is.Empty);
                Assert.That(component.FindAll("#annotation-new-rfd"), Is.Empty);
                Assert.That(component.FindAll(".ann-add-item.ann-kind-cr"), Has.Count.EqualTo(1));
                Assert.That(component.FindAll(".ann-add-backdrop"), Has.Count.EqualTo(1));
            }

            component.Find("#annotation-add").Click();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.Instance.IsAddMenuOpen, Is.False);
                Assert.That(component.Find("#annotation-add").GetAttribute("aria-expanded"), Is.EqualTo("false"));
                Assert.That(component.FindAll(".ann-add-list"), Is.Empty);
            }
        }

        /// <summary>
        /// Verifies that choosing a kind in the dropdown starts a new annotation of that kind and closes the dropdown.
        /// </summary>
        [Test]
        public void VerifyClickingDropdownItemStartsNewAnnotationAndClosesMenu()
        {
            var component = this.Render();

            component.Find("#annotation-add").Click();
            component.Find("#annotation-new-rid").Click();

            this.annotationsViewModel.Verify(x => x.StartNew(AnnotationKind.ReviewItemDiscrepancy), Times.Once);
            this.annotationsViewModel.Verify(x => x.StartNew(It.Is<AnnotationKind>(kind => kind != AnnotationKind.ReviewItemDiscrepancy)), Times.Never);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.Instance.IsAddMenuOpen, Is.False);
                Assert.That(component.FindAll(".ann-add-list"), Is.Empty);
                Assert.That(component.Find("#annotation-add").GetAttribute("aria-expanded"), Is.EqualTo("false"));
            }
        }

        /// <summary>
        /// Verifies that clicking the backdrop closes the dropdown without starting an annotation.
        /// </summary>
        [Test]
        public void VerifyBackdropClosesTheDropdown()
        {
            var component = this.Render();

            component.Find("#annotation-add").Click();
            Assert.That(component.FindAll(".ann-add-list"), Has.Count.EqualTo(1));

            component.Find(".ann-add-backdrop").Click();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.Instance.IsAddMenuOpen, Is.False);
                Assert.That(component.FindAll(".ann-add-list"), Is.Empty);
                Assert.That(component.FindAll(".ann-add-backdrop"), Is.Empty);
            }

            this.annotationsViewModel.Verify(x => x.StartNew(It.IsAny<AnnotationKind>()), Times.Never);
        }

        /// <summary>
        /// Verifies that Escape closes the dropdown while any other key leaves it open.
        /// </summary>
        [Test]
        public void VerifyEscapeClosesTheDropdown()
        {
            var component = this.Render();

            component.Find("#annotation-add").Click();
            component.Find(".ann-add-list").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

            Assert.That(component.FindAll(".ann-add-list"), Has.Count.EqualTo(1), "another key does not close the dropdown");

            component.Find(".ann-add-list").KeyDown(new KeyboardEventArgs { Key = "Escape" });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.Instance.IsAddMenuOpen, Is.False);
                Assert.That(component.FindAll(".ann-add-list"), Is.Empty);
            }
        }

        /// <summary>
        /// Verifies that neither the dropdown toggle nor the add buttons are offered while a draft is open or when no kind
        /// can be created.
        /// </summary>
        [Test]
        public void VerifyNoAddControlsWhileComposingOrWithoutCreatableKinds()
        {
            this.annotationsViewModel.Setup(x => x.CreatableKinds).Returns([]);

            var readOnly = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(readOnly.FindAll("#annotation-add"), Is.Empty, "nothing is creatable");
                Assert.That(readOnly.FindAll(".ann-add-menu"), Is.Empty);
            }

            this.annotationsViewModel.Setup(x => x.CreatableKinds).Returns(AnnotationHelper.AttachableKinds);
            this.annotationsViewModel.Setup(x => x.Draft).Returns(new AnnotationDraft { Kind = AnnotationKind.Note });

            var composing = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(composing.FindAll("#annotation-add"), Is.Empty, "a draft is open");
                Assert.That(composing.FindAll(".ann-add-menu"), Is.Empty);
                Assert.That(composing.FindAll("#annotation-composer"), Has.Count.EqualTo(1));
            }
        }

        /// <summary>
        /// Verifies that a requirement without annotation shows the empty state, which invites to annotate when the user
        /// may and explains the missing permission otherwise.
        /// </summary>
        [Test]
        public void VerifyEmptyStateRenders()
        {
            this.annotations.Clear();
            this.annotationsViewModel.Setup(x => x.GetSummary(this.requirement)).Returns([]);

            var component = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll(".ann-card"), Is.Empty);
                Assert.That(component.Find(".ann-empty").TextContent, Does.Contain("No annotations yet.").And.Contain("Leave a note"));
            }

            this.annotationsViewModel.Setup(x => x.CreatableKinds).Returns([]);

            var readOnlyComponent = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(readOnlyComponent.Find(".ann-empty").TextContent, Does.Contain("You do not have permission to annotate this item."));
                Assert.That(readOnlyComponent.FindAll("#annotation-add"), Is.Empty, "no add button without a creatable kind");
            }
        }

        /// <summary>
        /// Verifies that clicking a kind chip, or the all chip, sets the kind filter.
        /// </summary>
        [Test]
        public void VerifyClickingChipsSetsKindFilter()
        {
            var component = this.Render();

            component.FindAll(".ann-chip")[2].Click();
            this.annotationsViewModel.VerifySet(x => x.KindFilter = AnnotationKind.ChangeRequest, Times.Once);

            component.FindAll(".ann-chip")[0].Click();
            this.annotationsViewModel.VerifySet(x => x.KindFilter = null, Times.Once);
        }

        /// <summary>
        /// Verifies that the close button closes the panel.
        /// </summary>
        [Test]
        public async Task VerifyCloseButtonClosesPanel()
        {
            var component = this.Render();
            var closeButton = component.FindComponents<DxButton>().First(x => x.Instance.Id == "annotations-close");

            await component.InvokeAsync(closeButton.Instance.Click.InvokeAsync);

            this.annotationsViewModel.Verify(x => x.ClosePanel(), Times.Once);
        }

        /// <summary>
        /// Verifies that the composer of a new modelling annotation item renders its title, its hint and a Save button
        /// that stays disabled until the required values are filled in, and replaces the "Add" buttons.
        /// </summary>
        [Test]
        public void VerifyComposerRendersForNewDraft()
        {
            var draft = new AnnotationDraft { Kind = AnnotationKind.ChangeRequest, ShortName = "CR-002" };
            this.annotationsViewModel.Setup(x => x.Draft).Returns(draft);

            var component = this.Render();
            var saveButton = component.FindComponents<DxButton>().First(x => x.Instance.Id == "annotation-save");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll("#annotation-composer"), Has.Count.EqualTo(1));
                Assert.That(component.Find(".ann-composer-header strong").TextContent, Is.EqualTo("New Change Request"));
                Assert.That(component.Find(".ann-composer-shortname").TextContent, Is.EqualTo("CR-002"));
                Assert.That(component.Find(".ann-composer-hint").TextContent, Is.EqualTo(AnnotationsPanel.GetKindHint(AnnotationKind.ChangeRequest)));
                Assert.That(component.FindComponents<DxTextBox>().Select(x => x.Instance.Id), Does.Contain("annotation-title"));
                Assert.That(component.FindComponents<DxMemo>().Select(x => x.Instance.Id), Does.Contain("annotation-content"));
                Assert.That(component.FindAll(".ann-segmented button"), Has.Count.EqualTo(2), "the classification offers minor and major");
                Assert.That(saveButton.Instance.Text, Is.EqualTo("Add"));
                Assert.That(saveButton.Instance.Enabled, Is.False, "the content and the title are empty");
                Assert.That(component.FindAll("#annotation-add"), Is.Empty, "the add button is hidden while composing");
                Assert.That(component.FindComponents<DxTextBox>().Select(x => x.Instance.Id), Does.Not.Contain("annotation-shortname"), "a new annotation does not offer its details");
                Assert.That(component.FindComponents<DxComboBox<DomainOfExpertise, DomainOfExpertise>>(), Is.Empty);
                Assert.That(component.FindComponents<DxTagBox<Category, Category>>(), Is.Empty);
                Assert.That(component.FindAll(".ann-field-error"), Is.Empty);
            }
        }

        /// <summary>
        /// Verifies that the composer of an edited modelling annotation item also offers its short name, owner and
        /// categories, without error while the short name is fine.
        /// </summary>
        [Test]
        public void VerifyComposerOfEditedItemRendersDetails()
        {
            var draft = this.CreateEditedDraft();
            this.annotationsViewModel.Setup(x => x.Draft).Returns(draft);
            this.annotationsViewModel.Setup(x => x.GetShortNameError(draft)).Returns((string)null);

            var component = this.Render();
            var saveButton = component.FindComponents<DxButton>().First(x => x.Instance.Id == "annotation-save");
            var shortName = component.FindComponents<DxTextBox>().Single(x => x.Instance.Id == "annotation-shortname");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(shortName.Instance.Text, Is.EqualTo("CR_001"));
                Assert.That(component.FindAll("#annotation-shortname"), Has.Count.EqualTo(1));
                Assert.That(component.FindAll("label[for='annotation-shortname']"), Has.Count.EqualTo(1));
                Assert.That(component.FindComponents<DxComboBox<DomainOfExpertise, DomainOfExpertise>>().Select(x => x.Instance.Id), Is.EqualTo(["annotation-owner"]));
                Assert.That(component.FindComponents<DxTagBox<Category, Category>>().Select(x => x.Instance.Id), Is.EqualTo(["annotation-categories"]));
                Assert.That(component.FindAll(".ann-field-error"), Is.Empty);
                Assert.That(saveButton.Instance.Text, Is.EqualTo("Save"));
                Assert.That(saveButton.Instance.Enabled, Is.True);
            }
        }

        /// <summary>
        /// Verifies that the short name error is shown and the Save button disabled while the view model reports one.
        /// </summary>
        [Test]
        public void VerifyShortNameErrorIsShownAndDisablesSave()
        {
            var draft = this.CreateEditedDraft();
            this.annotationsViewModel.Setup(x => x.Draft).Returns(draft);
            this.annotationsViewModel.Setup(x => x.GetShortNameError(draft)).Returns("The short name 'CR_001' is already used by another annotation.");

            var component = this.Render();
            var saveButton = component.FindComponents<DxButton>().First(x => x.Instance.Id == "annotation-save");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll(".ann-field-error"), Has.Count.EqualTo(1));
                Assert.That(component.Find(".ann-field-error").TextContent, Is.EqualTo("The short name 'CR_001' is already used by another annotation."));
                Assert.That(draft.IsValid, Is.True, "the draft is otherwise valid");
                Assert.That(saveButton.Instance.Enabled, Is.False);
            }

            this.annotationsViewModel.Setup(x => x.GetShortNameError(draft)).Returns((string)null);

            var fixedComponent = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(fixedComponent.FindAll(".ann-field-error"), Is.Empty);
                Assert.That(fixedComponent.FindComponents<DxButton>().First(x => x.Instance.Id == "annotation-save").Instance.Enabled, Is.True);
            }
        }

        /// <summary>
        /// Verifies the Save button's binding to <c>GetShortNameError</c> for a new draft too. The real view model never
        /// reports an error for a new draft (its short name is generated when written); the mock forces one to cover the
        /// binding.
        /// </summary>
        [Test]
        public void VerifyShortNameErrorDisablesSaveOfNewDraft()
        {
            var draft = new AnnotationDraft { Kind = AnnotationKind.ChangeRequest, ShortName = "CR_002", Title = "A title", Content = "Some content" };
            this.annotationsViewModel.Setup(x => x.Draft).Returns(draft);
            this.annotationsViewModel.Setup(x => x.GetShortNameError(draft)).Returns("A short name is required.");

            var component = this.Render();

            Assert.That(component.FindComponents<DxButton>().First(x => x.Instance.Id == "annotation-save").Instance.Enabled, Is.False);
        }

        /// <summary>
        /// Verifies that the composer of an edited note offers no details.
        /// </summary>
        [Test]
        public void VerifyComposerOfEditedNoteHasNoDetails()
        {
            this.annotationsViewModel.Setup(x => x.Draft).Returns(new AnnotationDraft { Kind = AnnotationKind.Note, Content = "A comment", EditedAnnotation = this.annotations[1] });

            var component = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll("#annotation-composer"), Has.Count.EqualTo(1));
                Assert.That(component.FindComponents<DxTextBox>(), Is.Empty);
                Assert.That(component.FindComponents<DxComboBox<DomainOfExpertise, DomainOfExpertise>>(), Is.Empty);
                Assert.That(component.FindComponents<DxTagBox<Category, Category>>(), Is.Empty);
                Assert.That(component.FindAll(".ann-field-error"), Is.Empty);
            }
        }

        /// <summary>
        /// Verifies that the Save button of the composer is enabled once the draft is valid, and saves the draft.
        /// </summary>
        [Test]
        public async Task VerifySaveButtonIsEnabledForValidDraftAndSaves()
        {
            var draft = new AnnotationDraft { Kind = AnnotationKind.ChangeRequest, Title = "A title", Content = "Some content" };
            this.annotationsViewModel.Setup(x => x.Draft).Returns(draft);

            var component = this.Render();
            var saveButton = component.FindComponents<DxButton>().First(x => x.Instance.Id == "annotation-save");

            Assert.That(saveButton.Instance.Enabled, Is.True);

            await component.InvokeAsync(saveButton.Instance.Click.InvokeAsync);

            this.annotationsViewModel.Verify(x => x.SaveDraftAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that the Cancel button of the composer cancels the draft.
        /// </summary>
        [Test]
        public async Task VerifyCancelButtonCancelsDraft()
        {
            this.annotationsViewModel.Setup(x => x.Draft).Returns(new AnnotationDraft { Kind = AnnotationKind.Note });

            var component = this.Render();
            var cancelButton = component.FindComponents<DxButton>().First(x => x.Instance.Text == "Cancel");

            await component.InvokeAsync(cancelButton.Instance.Click.InvokeAsync);

            this.annotationsViewModel.Verify(x => x.CancelDraft(), Times.Once);
        }

        /// <summary>
        /// Verifies that the composer of a note has no title and no classification.
        /// </summary>
        [Test]
        public void VerifyComposerOfNoteHasNoTitle()
        {
            this.annotationsViewModel.Setup(x => x.Draft).Returns(new AnnotationDraft { Kind = AnnotationKind.Note, Content = "A comment" });

            var component = this.Render();
            var saveButton = component.FindComponents<DxButton>().First(x => x.Instance.Id == "annotation-save");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll("#annotation-composer"), Has.Count.EqualTo(1));
                Assert.That(component.FindComponents<DxTextBox>(), Is.Empty);
                Assert.That(component.FindAll(".ann-segmented"), Is.Empty);
                Assert.That(component.Find("label[for='annotation-content']").TextContent, Is.EqualTo("Comment"));
                Assert.That(saveButton.Instance.Enabled, Is.True, "a note only needs its content");
            }
        }

        /// <summary>
        /// Verifies the heading of the composer for a new annotation, an edit and a change proposal.
        /// </summary>
        [Test]
        public void VerifyGetComposerTitle()
        {
            var changeRequest = (ChangeRequest)this.annotations[0];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(AnnotationsPanel.GetComposerTitle(new AnnotationDraft { Kind = AnnotationKind.ChangeRequest }), Is.EqualTo("New Change Request"));
                Assert.That(AnnotationsPanel.GetComposerTitle(new AnnotationDraft { Kind = AnnotationKind.Note, EditedAnnotation = this.annotations[1] }), Is.EqualTo("Edit Note"));
                Assert.That(AnnotationsPanel.GetComposerTitle(new AnnotationDraft { Kind = AnnotationKind.ChangeProposal, ChangeRequest = changeRequest }), Is.EqualTo("Change Proposal for CR-001"));
            }
        }

        /// <summary>
        /// Verifies that every kind has a hint and a content placeholder.
        /// </summary>
        [Test]
        public void VerifyKindHintsAndPlaceholders()
        {
            using (Assert.EnterMultipleScope())
            {
                foreach (var kind in Enum.GetValues<AnnotationKind>())
                {
                    Assert.That(AnnotationsPanel.GetKindHint(kind), Is.Not.Empty, $"hint of {kind}");
                    Assert.That(AnnotationsPanel.GetContentPlaceholder(kind), Is.Not.Empty, $"placeholder of {kind}");
                }
            }
        }
    }
}
