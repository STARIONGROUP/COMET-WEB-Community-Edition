// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductTreeTestFixture.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;

    using Bunit;

    using COMETwebapp.Components.Viewer.Canvas;
    using COMETwebapp.Enumerations;
    using COMETwebapp.Model;
    using COMETwebapp.Model.Primitives;
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer.Canvas;
   
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;
    
    [TestFixture]
    public class ProductTreeTestFixture
    {
        private TestContext context;
        private ProductTree productTree;
        private IRenderedComponent<ProductTree> renderedComponent;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            var productTreeVM = new Mock<IProductTreeViewModel>();
            productTreeVM.Setup(x => x.TreeFilters).Returns(new List<TreeFilter>() { TreeFilter.ShowFullTree, TreeFilter.ShowNodesWithGeometry });
            productTreeVM.Setup(x => x.SelectedFilter).Returns(TreeFilter.ShowFullTree);
            this.context.Services.AddSingleton(productTreeVM.Object);

            var selectionMediator = new SelectionMediator();
            
            var rootNode = new TreeNode(new SceneObject(null)) { Title = "rootNode" };
            var rootNodeVM = new NodeComponentViewModel(rootNode, selectionMediator);

            var node1 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "first" };
            var node2 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "second" };
            var node3 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "third" };
            var node4 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "fourth" };
            var node5 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "fifth" };
            
            var nodeVM1 = new NodeComponentViewModel(node1, selectionMediator);
            var nodeVM2 = new NodeComponentViewModel(node2, selectionMediator);
            var nodeVM3 = new NodeComponentViewModel(node3, selectionMediator);
            var nodeVM4 = new NodeComponentViewModel(node4, selectionMediator);
            var nodeVM5 = new NodeComponentViewModel(node5, selectionMediator);

            nodeVM1.AddChild(nodeVM2);
            nodeVM1.AddChild(nodeVM3);
            nodeVM1.AddChild(nodeVM4);
            rootNodeVM.AddChild(nodeVM1);
            rootNodeVM.AddChild(nodeVM5);
            
            this.renderedComponent = this.context.RenderComponent<ProductTree>(parameter =>
            {
                parameter.Add(p => p.RootViewModel, rootNodeVM);
            });
 
            this.productTree = this.renderedComponent.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.productTree, Is.Not.Null);
                Assert.That(this.productTree.ViewModel, Is.Not.Null);
                Assert.That(this.productTree.RootViewModel, Is.Not.Null);
            });
        }
    }
}
