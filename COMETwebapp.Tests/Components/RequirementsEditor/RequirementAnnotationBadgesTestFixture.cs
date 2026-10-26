// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementAnnotationBadgesTestFixture.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;
    using COMETwebapp.ViewModels.Components.RequirementsEditor.Annotations;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="RequirementAnnotationBadges" /> component.
    /// </summary>
    [TestFixture]
    public class RequirementAnnotationBadgesTestFixture
    {
        /// <summary>
        /// The <see cref="BunitContext" /> the component is rendered in.
        /// </summary>
        private BunitContext context;

        /// <summary>
        /// The mocked <see cref="IRequirementsEditorBodyViewModel" /> handed to the component.
        /// </summary>
        private Mock<IRequirementsEditorBodyViewModel> viewModel;

        /// <summary>
        /// The mocked <see cref="IRequirementAnnotationsViewModel" /> the component reads its summary from.
        /// </summary>
        private Mock<IRequirementAnnotationsViewModel> annotationsViewModel;

        /// <summary>
        /// The <see cref="Requirement" /> the badges are rendered for.
        /// </summary>
        private Requirement requirement;

        /// <summary>
        /// Creates the context, the requirement and the mocked view models, with no annotation and no creatable kind.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.requirement = new Requirement { Iid = Guid.NewGuid(), ShortName = "R1", Name = "First requirement" };

            this.annotationsViewModel = new Mock<IRequirementAnnotationsViewModel>();
            this.annotationsViewModel.Setup(x => x.GetSummary(this.requirement)).Returns([]);
            this.annotationsViewModel.Setup(x => x.CreatableKinds).Returns([]);

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
        /// Renders the component for the <see cref="requirement" />.
        /// </summary>
        /// <returns>The <see cref="IRenderedComponent{TComponent}" /></returns>
        private IRenderedComponent<RequirementAnnotationBadges> Render()
        {
            return this.context.Render<RequirementAnnotationBadges>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object)
                .Add(p => p.Requirement, this.requirement));
        }

        /// <summary>
        /// Verifies that a requirement without annotation offers the discreet "annotate" button when the user can create one.
        /// </summary>
        [Test]
        public void VerifyAddButtonRendersWhenNoAnnotationAndKindsCreatable()
        {
            this.annotationsViewModel.Setup(x => x.CreatableKinds).Returns([RequirementAnnotationKind.Note, RequirementAnnotationKind.ChangeRequest]);

            var component = this.Render();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(component.FindAll(".req-annotation-add"), Has.Count.EqualTo(1));
                Assert.That(component.Find(".req-annotation-add").GetAttribute("aria-label"), Is.EqualTo("Annotate R1"));
                Assert.That(component.FindAll(".req-annotation-badge"), Is.Empty);
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
                Assert.That(component.FindAll(".req-annotation-badges"), Has.Count.EqualTo(1));
            }
        }

        /// <summary>
        /// Verifies that one badge is rendered per kind, solid while an annotation of the kind is open and muted otherwise.
        /// </summary>
        [Test]
        public void VerifyBadgesRenderPerKindWithOpenState()
        {
            this.annotationsViewModel.Setup(x => x.CreatableKinds).Returns([RequirementAnnotationKind.Note]);

            this.annotationsViewModel.Setup(x => x.GetSummary(this.requirement)).Returns(
            [
                new AnnotationKindSummary(RequirementAnnotationKind.ChangeRequest, 2, 1),
                new AnnotationKindSummary(RequirementAnnotationKind.ReviewItemDiscrepancy, 1, 0)
            ]);

            var component = this.Render();
            var badges = component.FindAll(".req-annotation-badge");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(badges, Has.Count.EqualTo(2));
                Assert.That(component.FindAll(".req-annotation-add"), Is.Empty, "the add button is replaced by the badges");
                Assert.That(badges[0].ClassList, Does.Contain("ann-kind-cr").And.Contain("has-open"));
                Assert.That(badges[0].ClassList, Does.Not.Contain("is-resolved"));
                Assert.That(badges[0].QuerySelector(".req-annotation-badge-label").TextContent, Is.EqualTo("CR"));
                Assert.That(badges[0].QuerySelector(".req-annotation-badge-count").TextContent, Is.EqualTo("2"));
                Assert.That(badges[0].GetAttribute("title"), Is.EqualTo("1 of 2 Change Requests open"));
                Assert.That(badges[1].ClassList, Does.Contain("ann-kind-rid").And.Contain("is-resolved"));
                Assert.That(badges[1].ClassList, Does.Not.Contain("has-open"));
                Assert.That(badges[1].QuerySelector(".req-annotation-badge-label").TextContent, Is.EqualTo("RID"));
                Assert.That(badges[1].QuerySelector(".req-annotation-badge-count").TextContent, Is.EqualTo("1"));
            }
        }

        /// <summary>
        /// Verifies that the badge of a note shows an icon instead of the acronym and is never open.
        /// </summary>
        [Test]
        public void VerifyNoteBadgeRendersIconInsteadOfLabel()
        {
            this.annotationsViewModel.Setup(x => x.GetSummary(this.requirement)).Returns([new AnnotationKindSummary(RequirementAnnotationKind.Note, 3, 0)]);

            var component = this.Render();
            var badge = component.Find(".req-annotation-badge");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(badge.ClassList, Does.Contain("ann-kind-note").And.Contain("is-resolved"));
                Assert.That(badge.QuerySelector(".req-annotation-badge-label"), Is.Null);
                Assert.That(badge.QuerySelector(".req-annotation-badge-count").TextContent, Is.EqualTo("3"));
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
                new AnnotationKindSummary(RequirementAnnotationKind.ChangeRequest, 2, 1),
                new AnnotationKindSummary(RequirementAnnotationKind.ReviewItemDiscrepancy, 1, 0)
            ]);

            var component = this.Render();

            component.Find(".ann-kind-cr").Click();

            this.annotationsViewModel.Verify(x => x.OpenPanel(this.requirement, RequirementAnnotationKind.ChangeRequest), Times.Once);
            this.annotationsViewModel.Verify(x => x.OpenPanel(It.IsAny<Requirement>(), RequirementAnnotationKind.ReviewItemDiscrepancy), Times.Never);
        }

        /// <summary>
        /// Verifies that clicking the "annotate" button opens the panel on every kind.
        /// </summary>
        [Test]
        public void VerifyClickingAddButtonOpensPanelOnAllKinds()
        {
            this.annotationsViewModel.Setup(x => x.CreatableKinds).Returns([RequirementAnnotationKind.Note]);

            var component = this.Render();

            component.Find(".req-annotation-add").Click();

            this.annotationsViewModel.Verify(x => x.OpenPanel(this.requirement, null), Times.Once);
        }
    }
}
