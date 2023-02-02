// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeComponentTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.Viewer.Canvas
{
    using BlazorStrap;

    using Bunit;

    using COMETwebapp.Components.Viewer.Canvas;
    using COMETwebapp.Model;
    using COMETwebapp.Model.Primitives;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer.Canvas;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class NodeComponentTestFixture
    {
        private TestContext context;
        private NodeComponent nodeComponent;
        private Mock<ISelectionMediator> selectionMediator;
        private IRenderedComponent<NodeComponent> renderedComponent;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.Services.AddBlazorStrap();

            this.selectionMediator = new Mock<ISelectionMediator>();

            this.context.Services.AddSingleton(this.selectionMediator.Object);

            var treeNode = new TreeNode(new SceneObject(null));

            this.renderedComponent = this.context.RenderComponent<NodeComponent>(parameters => parameters.Add(p => p.ViewModel.Node, treeNode));
            this.nodeComponent = this.renderedComponent.Instance;
        }

        [Test]
        public void VerifyTreeNodeSelectionChanged()
        {
            var treeNode = this.renderedComponent.Find(".treeNode");
            treeNode.Click();
            this.selectionMediator.Verify(x => x.RaiseOnTreeSelectionChanged(this.nodeComponent.ViewModel), Times.Once);
        }

        [Test]
        public void VerifyTreeNodeVisibilityChanged()
        {
            var treeNode = this.renderedComponent.Find(".treeIcon");
            treeNode.Click();
            this.selectionMediator.Verify(x => x.RaiseOnTreeVisibilityChanged(this.nodeComponent.ViewModel), Times.Once);
        }

        [Test]
        public void VerifyThatSelectionOfOtherNodeDontWorks()
        {
            var treeNodeComponent = this.renderedComponent.Find(".treeNode");
            treeNodeComponent.Click();

            var treeNode = new NodeComponentViewModel(new TreeNode(new SceneObject(null)), this.selectionMediator.Object);
            this.selectionMediator.Verify(x => x.RaiseOnTreeSelectionChanged(treeNode), Times.Never);
            Assert.That(treeNode.IsSelected, Is.False);
        }
    }
}
