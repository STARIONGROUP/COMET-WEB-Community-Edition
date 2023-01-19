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

namespace COMETwebapp.Tests.Viewer
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
            this.context = new TestContext();
            this.context.Services.AddBlazorStrap();
            this.context.Services.AddSingleton<ISelectionMediator, SelectionMediator>();

            var rootNode = new TreeNode(new SceneObject(null)) { Title = "rootNode" };
            rootNode.Parent = null;

            var node1 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "first" };
            var node2 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "second" };
            var node3 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "third" };
            var node4 = new TreeNode(new SceneObject(null)) { Title = "fourth" };
            var node5 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "fifth" };

            node1.Children.Add(node2);
            node1.Children.Add(node3);
            node1.Children.Add(node4);
            rootNode.Children.Add(node1);
            rootNode.Children.Add(node5);

            this.tree = this.context.RenderComponent<ProductTree>(parameters => parameters.Add(p => p.RootNode, rootNode));
            this.productTree = this.tree.Instance;
        }

        [Test]
        public void VerifyOnFilterChanged()
        {
            var nodesBeforeFiltering = this.tree.FindAll(".treeNode");
            this.productTree.OnFilterChanged(true);
            var nodesAfterFiltering = this.tree.FindAll(".treeNode");
            Assert.Multiple(() =>
            {
                Assert.That(this.productTree, Is.True);
                Assert.That(nodesBeforeFiltering, Has.Count.EqualTo(6));
                Assert.That(nodesAfterFiltering, Has.Count.EqualTo(4));
            });
        }

        [Test]
        public void VerifyThatSearchWorks()
        {
            var nodesBeforeFiltering = this.tree.FindAll(".treeNode");
            this.productTree.OnSearchFilterChange(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "th" });
            var nodesAfterFiltering = this.tree.FindAll(".treeNode");
            Assert.Multiple(() =>
            {
                Assert.That(this.productTree.SearchValue, Is.EqualTo("th"));
                Assert.That(nodesBeforeFiltering, Has.Count.EqualTo(6));
                Assert.That(nodesAfterFiltering, Has.Count.EqualTo(3));
            });
        }
    }
}
