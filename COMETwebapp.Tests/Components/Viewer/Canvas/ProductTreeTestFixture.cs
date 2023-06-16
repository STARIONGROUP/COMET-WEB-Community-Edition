// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewerProductTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Viewer;
    using COMETwebapp.Enumerations;
    using COMETwebapp.Model;
    using COMETwebapp.Model.Viewer.Primitives;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ViewerProductTreeTestFixture
    {
        private TestContext context;
        private ViewerProductTree productTree;
        private IRenderedComponent<ViewerProductTree> renderedComponent;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            var productTreeVM = new Mock<ViewerProductTreeViewModel>();
            productTreeVM.Setup(x => x.TreeFilters).Returns(new List<TreeFilter>() { TreeFilter.ShowFullTree, TreeFilter.ShowNodesWithGeometry });
            productTreeVM.Setup(x => x.SelectedFilter).Returns(TreeFilter.ShowFullTree);
            this.context.Services.AddSingleton(productTreeVM.Object);

            var selectionMediator = new SelectionMediator();
            
            var rootNode = new ViewerNodeViewModel(new SceneObject(null)) { Title = "rootBaseNode" };

            var node1 = new ViewerNodeViewModel(new SceneObject(new Cube(1, 1, 1))) { Title = "first" };
            var node2 = new ViewerNodeViewModel(new SceneObject(new Cube(1, 1, 1))) { Title = "second" };
            var node3 = new ViewerNodeViewModel(new SceneObject(new Cube(1, 1, 1))) { Title = "third" };
            var node4 = new ViewerNodeViewModel(new SceneObject(new Cube(1, 1, 1))) { Title = "fourth" };
            var node5 = new ViewerNodeViewModel(new SceneObject(new Cube(1, 1, 1))) { Title = "fifth" };
            
            node1.AddChild(node2);
            node1.AddChild(node3);
            node1.AddChild(node4);
            rootNode.AddChild(node1);
            rootNode.AddChild(node5);

            this.renderedComponent = this.context.RenderComponent<ViewerProductTree>(parameters =>
            {
                parameters.Add(p => p.ViewModel, productTreeVM.Object);
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
            });
        }
    }
}
