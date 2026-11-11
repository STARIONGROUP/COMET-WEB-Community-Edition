// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AnnotationBadgesTestFixture.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Annotations;
    using COMETwebapp.ViewModels.Components.Annotations;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="AnnotationBadges" /> component.
    /// </summary>
    [TestFixture]
    public class AnnotationBadgesTestFixture
    {
        /// <summary>
        /// The <see cref="BunitContext" /> the component is rendered in.
        /// </summary>
        private BunitContext context;

        /// <summary>
        /// The mocked <see cref="IAnnotationsViewModel" /> the component reads its summary from.
        /// </summary>
        private Mock<IAnnotationsViewModel> annotationsViewModel;

        /// <summary>
        /// The <see cref="Requirement" /> the badges are rendered for.
        /// </summary>
        private Requirement requirement;

        /// <summary>
        /// Creates the context, the requirement and the mocked view model, with no annotation and no creatable kind.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.requirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R1", Name = "First requirement" };

            this.annotationsViewModel = new Mock<IAnnotationsViewModel>();
            this.annotationsViewModel.Setup(x => x.GetSummary(It.IsAny<Thing>())).Returns([]);
            this.annotationsViewModel.Setup(x => x.GetCreatableKinds(It.IsAny<Thing>())).Returns([]);
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
        /// Renders the component for the given <paramref name="thing" />, the <see cref="requirement" /> by default.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> to render the badges for</param>
        /// <returns>The <see cref="IRenderedComponent{TComponent}" /></returns>
        private IRenderedComponent<AnnotationBadges> Render(Thing thing = null)
        {
            return this.context.Render<AnnotationBadges>(parameters => parameters
                .Add(p => p.ViewModel, this.annotationsViewModel.Object)
                .Add(p => p.Thing, thing ?? this.requirement));
        }

        /// <summary>
        /// Verifies that a requirement without annotation offers the discreet "annotate" button when the user can create one.
        /// </summary>
        [Test]
        public void VerifyAddButtonRendersWhenNoAnnotationAndKindsCreatable()
        {
            this.annotationsViewModel.Setup(x => x.GetCreatableKinds(this.requirement)).Returns([AnnotationKind.Note, AnnotationKind.ChangeRequest]);

            var component = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll(".ann-badge-add"), Has.Count.EqualTo(1));
                Assert.That(component.Find(".ann-badge-add").GetAttribute("aria-label"), Is.EqualTo("Annotate R1"));
                Assert.That(component.FindAll(".ann-badge"), Is.Empty);
            }
        }

        /// <summary>
        /// Verifies that a thing that is not a requirement is annotated the same way, the button being labelled with the
        /// short name of that thing, or with its class when it has none.
        /// </summary>
        [Test]
        public void VerifyBadgesRenderForANonRequirementThing()
        {
            var element = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "SAT", Name = "Satellite" };
            var iteration = new Iteration { Iid = Guid.NewGuid() };

            this.annotationsViewModel.Setup(x => x.GetCreatableKinds(element)).Returns([AnnotationKind.Note, AnnotationKind.ChangeRequest]);
            this.annotationsViewModel.Setup(x => x.GetCreatableKinds(iteration)).Returns([AnnotationKind.Note]);
            this.annotationsViewModel.Setup(x => x.GetSummary(element)).Returns([]);

            var elementComponent = this.Render(element);
            var iterationComponent = this.Render(iteration);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(elementComponent.Find(".ann-badge-add").GetAttribute("aria-label"), Is.EqualTo("Annotate SAT"));
                Assert.That(iterationComponent.Find(".ann-badge-add").GetAttribute("aria-label"), Is.EqualTo("Annotate Iteration"));
            }

            elementComponent.Find(".ann-badge-add").Click();

            this.annotationsViewModel.Verify(x => x.OpenPanel(element, null), Times.Once);

            this.annotationsViewModel.Setup(x => x.GetSummary(element)).Returns([new AnnotationKindSummary(AnnotationKind.Note, 2, 0)]);

            var annotated = this.Render(element);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(annotated.FindAll(".ann-badge"), Has.Count.EqualTo(1));
                Assert.That(annotated.FindAll(".ann-badge-add"), Is.Empty);
            }
        }

        /// <summary>
        /// Verifies that a requirement without annotation renders no button when the user cannot create any kind.
        /// </summary>
        [Test]
        public void VerifyNoButtonRendersWhenNoAnnotationAndNoKindCreatable()
        {
            var component = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll("button"), Is.Empty);
                Assert.That(component.FindAll(".ann-badges"), Has.Count.EqualTo(1));
            }
        }

        /// <summary>
        /// Verifies that one badge is rendered per kind, solid while an annotation of the kind is open and muted otherwise.
        /// </summary>
        [Test]
        public void VerifyBadgesRenderPerKindWithOpenState()
        {
            this.annotationsViewModel.Setup(x => x.GetCreatableKinds(this.requirement)).Returns([AnnotationKind.Note]);

            this.annotationsViewModel.Setup(x => x.GetSummary(this.requirement)).Returns(
            [
                new AnnotationKindSummary(AnnotationKind.ChangeRequest, 2, 1),
                new AnnotationKindSummary(AnnotationKind.ReviewItemDiscrepancy, 1, 0)
            ]);

            var component = this.Render();
            var badges = component.FindAll(".ann-badge");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(badges, Has.Count.EqualTo(2));
                Assert.That(component.FindAll(".ann-badge-add"), Is.Empty, "the add button is replaced by the badges");
                Assert.That(badges[0].ClassList, Does.Contain("ann-kind-cr").And.Contain("has-open"));
                Assert.That(badges[0].ClassList, Does.Not.Contain("is-resolved"));
                Assert.That(badges[0].QuerySelector(".ann-badge-label").TextContent, Is.EqualTo("CR"));
                Assert.That(badges[0].QuerySelector(".ann-badge-count").TextContent, Is.EqualTo("2"));
                Assert.That(badges[0].GetAttribute("title"), Is.EqualTo("2 Change Requests, 1 needing attention"));
                Assert.That(badges[1].ClassList, Does.Contain("ann-kind-rid").And.Contain("is-resolved"));
                Assert.That(badges[1].ClassList, Does.Not.Contain("has-open"));
                Assert.That(badges[1].QuerySelector(".ann-badge-label").TextContent, Is.EqualTo("RID"));
                Assert.That(badges[1].QuerySelector(".ann-badge-count").TextContent, Is.EqualTo("1"));
            }
        }

        /// <summary>
        /// Verifies that the badge of a note shows an icon instead of the acronym and is never open.
        /// </summary>
        [Test]
        public void VerifyNoteBadgeRendersIconInsteadOfLabel()
        {
            this.annotationsViewModel.Setup(x => x.GetSummary(this.requirement)).Returns([new AnnotationKindSummary(AnnotationKind.Note, 3, 0)]);

            var component = this.Render();
            var badge = component.Find(".ann-badge");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(badge.ClassList, Does.Contain("ann-kind-note").And.Contain("is-resolved"));
                Assert.That(badge.QuerySelector(".ann-badge-label"), Is.Null);
                Assert.That(badge.QuerySelector(".ann-badge-count").TextContent, Is.EqualTo("3"));
                Assert.That(badge.GetAttribute("title"), Is.EqualTo("3 Notes"));
            }
        }

        /// <summary>
        /// Verifies that clicking a badge opens the panel narrowed to the kind of the badge.
        /// </summary>
        [Test]
        public void VerifyClickingBadgeOpensPanelOnKind()
        {
            this.annotationsViewModel.Setup(x => x.GetSummary(this.requirement)).Returns(
            [
                new AnnotationKindSummary(AnnotationKind.ChangeRequest, 2, 1),
                new AnnotationKindSummary(AnnotationKind.ReviewItemDiscrepancy, 1, 0)
            ]);

            var component = this.Render();

            component.Find(".ann-kind-cr").Click();

            this.annotationsViewModel.Verify(x => x.OpenPanel(this.requirement, AnnotationKind.ChangeRequest), Times.Once);
            this.annotationsViewModel.Verify(x => x.OpenPanel(It.IsAny<Thing>(), AnnotationKind.ReviewItemDiscrepancy), Times.Never);
        }

        /// <summary>
        /// Verifies that clicking the "annotate" button opens the panel on every kind.
        /// </summary>
        [Test]
        public void VerifyClickingAddButtonOpensPanelOnAllKinds()
        {
            this.annotationsViewModel.Setup(x => x.GetCreatableKinds(this.requirement)).Returns([AnnotationKind.Note]);

            var component = this.Render();

            component.Find(".ann-badge-add").Click();

            this.annotationsViewModel.Verify(x => x.OpenPanel(this.requirement, null), Times.Once);
        }
    }
}
