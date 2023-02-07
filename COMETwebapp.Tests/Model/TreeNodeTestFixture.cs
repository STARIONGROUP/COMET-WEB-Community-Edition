// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TreeNodeTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Model
{
    using COMETwebapp.Model;
    using NUnit.Framework;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class TreeNodeTestFixture
    {
        private TreeNode rootNode;
        private TreeNode node1;
        private TreeNode node2;
        private TreeNode node3;
        private TreeNode node4;
        private TreeNode node5;
        private TreeNode node6;

        [SetUp]
        public void SetUp()
        {
            this.rootNode = new TreeNode(new SceneObject(null)) { Title = "RootViewModel" };
            node1 = new TreeNode(new SceneObject(null)) { Title = "first" };
            node2 = new TreeNode(new SceneObject(null)) { Title = "second" };
            node3 = new TreeNode(new SceneObject(null)) { Title = "third" };
            node4 = new TreeNode(new SceneObject(null)) { Title = "fourth" };
            node5 = new TreeNode(new SceneObject(null)) { Title = "fifth" };
            node6 = new TreeNode(new SceneObject(null)) { Title = "sixth" };

            this.rootNode.AddChild(node1);
            this.rootNode.AddChild(node2);
            node2.AddChild(node3);
            node3.AddChild(node4);
            this.rootNode.AddChild(node5);
            node5.AddChild(node6);
        }

        [Test]
        public void VerifyThatGetRootNodeWorks()
        {
            var result1 = this.rootNode.GetRootNode();
            var result2 = this.node1.GetRootNode();
            var result3 = this.node2.GetRootNode();
            var result4 = this.node3.GetRootNode();
            var result5 = this.node4.GetRootNode();
            var result6 = this.node5.GetRootNode();
            var result7 = this.node6.GetRootNode();

            Assert.Multiple(() =>
            {
                Assert.That(result1, Is.EqualTo(this.rootNode));
                Assert.That(result2, Is.EqualTo(this.rootNode));
                Assert.That(result3, Is.EqualTo(this.rootNode));
                Assert.That(result4, Is.EqualTo(this.rootNode));
                Assert.That(result5, Is.EqualTo(this.rootNode));
                Assert.That(result6, Is.EqualTo(this.rootNode));
                Assert.That(result7, Is.EqualTo(this.rootNode));
            });
        }

        [Test]
        public void VerifyGetFlatListOfDescendants()
        {
            var allNodes = this.rootNode.GetFlatListOfDescendants();
            Assert.Multiple(() =>
            {
                Assert.That(allNodes.Contains(this.rootNode), Is.True);
                Assert.That(allNodes.Contains(node1), Is.True);
                Assert.That(allNodes.Contains(node2), Is.True);
                Assert.That(allNodes.Contains(node3), Is.True);
                Assert.That(allNodes.Contains(node4), Is.True);
                Assert.That(allNodes.Contains(node5), Is.True);
                Assert.That(allNodes.Contains(node6), Is.True);
            });
        }

        [Test]
        public void VerifyThatGetParentWorks()
        {
            var parent1 = this.rootNode.GetParentNode();
            var parent2 = this.node3.GetParentNode();

            Assert.Multiple(() =>
            {
                Assert.That(parent1, Is.Null);
                Assert.That(parent2, Is.EqualTo(this.node2));
            });
        }

        [Test]
        public void VerifyThatGetChildrenWorks()
        {
            var children1 = this.rootNode.GetChildren();
            var children2 = this.node3.GetChildren();
            var children3 = this.node4.GetChildren();

            Assert.Multiple(() =>
            {
                Assert.That(children1, Has.Count.EqualTo(3));
                Assert.That(children2, Has.Count.EqualTo(1));
                Assert.That(children3, Has.Count.EqualTo(0));
            });
        }

        [Test]
        public void VerifyThatOverideEqualsWorks()
        {
            var newNode = new TreeNode(this.rootNode.SceneObject);
            Assert.Multiple(() =>
            {
                Assert.That(this.rootNode, Is.Not.EqualTo(this.node1));
                Assert.That(this.rootNode, Is.EqualTo(newNode));
            });
        }
    }
}
