// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductTreeTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
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

namespace COMETwebapp.Tests.Components.Canvas
{
    using BlazorStrap;
    using Bunit;
    using Castle.Components.DictionaryAdapter.Xml;
    using COMETwebapp.Components.Canvas;
    using COMETwebapp.Model;
    using COMETwebapp.Primitives;
    using COMETwebapp.Utilities;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    using TestContext = Bunit.TestContext;


    [TestFixture]
    public class ProductTreeTestFixture
    {
        private TestContext context;
        private ProductTree productTree;
        private IRenderedComponent<ProductTree> tree;

        [SetUp]
        public void SetUp()
        {
            context = new TestContext();
            context.Services.AddBlazorStrap();
            context.Services.AddSingleton<ISelectionMediator, SelectionMediator>();

            var rootNode = new TreeNode(new SceneObject(null)) { Title = "rootNode" };

            var node1 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "first" };
            var node2 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "second" };
            var node3 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "third" };
            var node4 = new TreeNode(new SceneObject(null)) { Title = "fourth" };
            var node5 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "fifth" };

            node1.AddChild(node2);
            node1.AddChild(node3);
            node1.AddChild(node4);
            rootNode.AddChild(node1);
            rootNode.AddChild(node5);

            tree = context.RenderComponent<ProductTree>(parameters => parameters.Add(p => p.RootNode, rootNode));
            productTree = tree.Instance;
        }

        [Test]
        public void VerifyOnFilterChanged()
        {
            var nodesBeforeFiltering = tree.FindAll(".treeNode");
            productTree.OnFilterChanged(true);
            var nodesAfterFiltering1 = tree.FindAll(".treeNode");
            productTree.OnFilterChanged(false);
            var nodesAfterFiltering2 = tree.FindAll(".treeNode");
            Assert.Multiple(() =>
            {
                Assert.That(productTree.ShowNodesWithGeometry, Is.False);
                Assert.That(nodesBeforeFiltering, Has.Count.EqualTo(6));
                Assert.That(nodesAfterFiltering1, Has.Count.EqualTo(4));
                Assert.That(nodesAfterFiltering2, Has.Count.EqualTo(6));
            });
        }

        [Test]
        public void VerifyThatSearchWorks()
        {
            var nodesBeforeFiltering = tree.FindAll(".treeNode");
            productTree.OnSearchFilterChange(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "th" });
            var nodesAfterFiltering = tree.FindAll(".treeNode");
            
            Assert.That(productTree.SearchValue, Is.EqualTo("th"));

            productTree.OnSearchFilterChange(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = string.Empty });
            var nodesAfterFiltering2 = tree.FindAll(".treeNode");
            Assert.Multiple(() =>
            {
                Assert.That(productTree.SearchValue, Is.EqualTo(string.Empty));
                Assert.That(nodesBeforeFiltering, Has.Count.EqualTo(6));
                Assert.That(nodesAfterFiltering, Has.Count.EqualTo(3));
                Assert.That(nodesAfterFiltering2, Has.Count.EqualTo(6));
            });
        }
    }
}
