// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsTreeTestFixture.cs" company="Starion Group S.A.">
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

    using FluentResults;

    using Microsoft.AspNetCore.Components.Web;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementsTreeTestFixture
    {
        private BunitContext context;
        private Mock<IRequirementsEditorBodyViewModel> viewModel;
        private RequirementsSpecification specification;
        private RequirementsGroup groupA;
        private RequirementsGroup groupB;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.specification = new RequirementsSpecification { Iid = Guid.NewGuid(), ShortName = "SPEC", Name = "Specification" };
            this.groupA = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "GA", Name = "Group A" };
            this.groupB = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "GB", Name = "Group B" };
            this.specification.Group.Add(this.groupA);
            this.groupA.Group.Add(this.groupB);

            this.viewModel = new Mock<IRequirementsEditorBodyViewModel>();
            this.viewModel.Setup(x => x.AvailableSpecifications).Returns([this.specification]);
            this.viewModel.Setup(x => x.SelectedSpecification).Returns(this.specification);
            this.viewModel.Setup(x => x.TreeUsesShortName).Returns(true);
            this.viewModel.Setup(x => x.GetGroups(this.specification)).Returns([this.groupA]);
            this.viewModel.Setup(x => x.GetGroups(this.groupA)).Returns([this.groupB]);
            this.viewModel.Setup(x => x.GetGroups(this.groupB)).Returns([]);
            this.viewModel.Setup(x => x.IsTreeNodeCollapsed(It.IsAny<Guid>())).Returns(false);
            this.viewModel.Setup(x => x.CanMoveGroup(It.IsAny<RequirementsGroup>(), It.IsAny<RequirementsContainer>())).Returns(false);
            this.viewModel.SetupProperty(x => x.DraggedGroup, null);
            this.viewModel.SetupProperty(x => x.DragOverContainer, null);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        private IRenderedComponent<RequirementsTree> Render()
        {
            return this.context.Render<RequirementsTree>(parameters => parameters
                .Add(p => p.ViewModel, this.viewModel.Object));
        }

        [Test]
        public void VerifyTreeRendersSpecificationAndGroupHierarchy()
        {
            var component = this.Render();

            Assert.Multiple(() =>
            {
                Assert.That(component.Markup, Does.Contain("SPEC"));
                Assert.That(component.Markup, Does.Contain("GA"));
                Assert.That(component.Markup, Does.Contain("GB"));
                Assert.That(component.FindAll(".req-tree-group-row"), Has.Count.EqualTo(2), "both the parent and nested group are rendered");
            });
        }

        [Test]
        public async Task VerifyOnGroupDragStart()
        {
            var component = this.Render();
            var groupRow = component.Find(".req-tree-group-row");

            await component.InvokeAsync(() => groupRow.DragStartAsync(new DragEventArgs()));

            Assert.That(this.viewModel.Object.DraggedGroup, Is.EqualTo(this.groupA));
        }

        [Test]
        public async Task VerifyOnDragEnter()
        {
            var component = this.Render();
            var groupRows = component.FindAll(".req-tree-group-row");

            await component.InvokeAsync(() => groupRows[1].DragEnterAsync(new DragEventArgs()));
            Assert.That(this.viewModel.Object.DragOverContainer, Is.EqualTo(this.groupB), "hovering the nested group row marks it as the drag-over container");

            var specRow = component.Find(".req-tree-spec");
            await component.InvokeAsync(() => specRow.DragEnterAsync(new DragEventArgs()));
            Assert.That(this.viewModel.Object.DragOverContainer, Is.EqualTo(this.specification), "hovering the specification row marks it as the drag-over container");
        }

        [Test]
        public async Task VerifyOnGroupDragEnd()
        {
            this.viewModel.Object.DraggedGroup = this.groupA;
            this.viewModel.Object.DragOverContainer = this.groupB;

            var component = this.Render();
            var groupRow = component.Find(".req-tree-group-row");

            await component.InvokeAsync(() => groupRow.DragEndAsync(new DragEventArgs()));

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Object.DraggedGroup, Is.Null, "the drag ending clears the dragged group");
                Assert.That(this.viewModel.Object.DragOverContainer, Is.Null, "the drag ending clears the drag-over container");
            });
        }

        [Test]
        public async Task VerifyOnDropOnContainer()
        {
            this.viewModel.Object.DraggedGroup = this.groupB;
            this.viewModel.Object.DragOverContainer = this.groupA;
            this.viewModel.Setup(x => x.CanMoveGroup(this.groupB, this.groupA)).Returns(true);
            this.viewModel.Setup(x => x.MoveGroupAsync(this.groupB, this.groupA)).Returns(Task.FromResult(Result.Ok()));

            var component = this.Render();
            var dropTargetRow = component.Find(".req-tree-group-row");

            Assert.That(dropTargetRow.ClassList, Does.Contain("req-tree-droptarget"), "the hovered, allowed drop target is highlighted");

            await component.InvokeAsync(() => dropTargetRow.DropAsync(new DragEventArgs()));

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.MoveGroupAsync(this.groupB, this.groupA), Times.Once, "an allowed drop moves the group");
                Assert.That(this.viewModel.Object.DraggedGroup, Is.Null, "the drag state is cleared after the drop");
            });

            this.viewModel.Invocations.Clear();
            this.viewModel.Object.DraggedGroup = this.groupB;
            this.viewModel.Setup(x => x.CanMoveGroup(this.groupB, this.groupA)).Returns(false);

            var secondComponent = this.Render();
            await secondComponent.InvokeAsync(() => secondComponent.Find(".req-tree-group-row").DropAsync(new DragEventArgs()));

            this.viewModel.Verify(x => x.MoveGroupAsync(It.IsAny<RequirementsGroup>(), It.IsAny<RequirementsContainer>()), Times.Never, "a disallowed drop must not move the group");
        }
    }
}
