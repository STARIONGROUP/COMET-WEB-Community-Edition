// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeComponentViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.Viewer.Canvas
{
    using BlazorStrap;

    using Bunit;

    using COMETwebapp.Components.Viewer.Canvas;
    using COMETwebapp.Model;
    using COMETwebapp.Model.Primitives;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Shared;
    using COMETwebapp.ViewModels.Components.Viewer.Canvas;

    using DevExpress.Data.Mask.Internal;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class NodeComponentViewModelTestFixture
    {
        private TestContext context;
        private INodeComponentViewModel rootNodeVM;
        private INodeComponentViewModel node1VM;
        private INodeComponentViewModel node2VM;
        private INodeComponentViewModel node3VM;
        private INodeComponentViewModel node4VM;
        private INodeComponentViewModel node5VM;
        private Mock<ISelectionMediator> selectionMediator;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.selectionMediator = new Mock<ISelectionMediator>();

            var treeNode = new TreeNode(new SceneObject(new Sphere(1)));

            var rootNode = new TreeNode(new SceneObject(null)) { Title = "Loft" };
            var node1 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "Bus" };
            var node2 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "LargeAreaDetector1" };
            var node3 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "OpticalBench" };
            var node4 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "StructuralTower" };
            var node5 = new TreeNode(new SceneObject(null)) { Title = "WideFieldMonitor" };

            this.rootNodeVM = new NodeComponentViewModel(rootNode, this.selectionMediator.Object);
            this.node1VM = new NodeComponentViewModel(node1, this.selectionMediator.Object);
            this.node2VM = new NodeComponentViewModel(node2, this.selectionMediator.Object);
            this.node3VM = new NodeComponentViewModel(node3, this.selectionMediator.Object);
            this.node4VM = new NodeComponentViewModel(node4, this.selectionMediator.Object);
            this.node5VM = new NodeComponentViewModel(node5, this.selectionMediator.Object);

            this.rootNodeVM.AddChild(this.node1VM);
            this.node1VM.AddChild(this.node2VM);
            this.rootNodeVM.AddChild(this.node3VM);
            this.node3VM.AddChild(this.node4VM);
            this.node4VM.AddChild(this.node5VM);
        }

        [Test]
        public void VerifyViewmodelData()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.rootNodeVM.Parent, Is.Null);
                Assert.That(this.node1VM.Parent, Is.Not.Null);
                Assert.That(this.node2VM.Parent, Is.Not.Null);
                Assert.That(this.node3VM.Parent, Is.Not.Null);
                Assert.That(this.node4VM.Parent, Is.Not.Null);
                Assert.That(this.node5VM.Parent, Is.Not.Null);

                Assert.That(this.rootNodeVM.Children, Has.Count.EqualTo(2));
                Assert.That(this.rootNodeVM.Node.GetChildren(), Has.Count.EqualTo(2));

                Assert.That(this.node1VM.Children, Has.Count.EqualTo(1));
                Assert.That(this.node1VM.Node.GetChildren(), Has.Count.EqualTo(1));

                Assert.That(this.node2VM.Children, Has.Count.EqualTo(0));
                Assert.That(this.node2VM.Node.GetChildren(), Has.Count.EqualTo(0));

                Assert.That(this.node3VM.Children, Has.Count.EqualTo(1));
                Assert.That(this.node3VM.Node.GetChildren(), Has.Count.EqualTo(1));

                Assert.That(this.node4VM.Children, Has.Count.EqualTo(1));
                Assert.That(this.node4VM.Node.GetChildren(), Has.Count.EqualTo(1));

                Assert.That(this.node5VM.Children, Has.Count.EqualTo(0));
                Assert.That(this.node5VM.Node.GetChildren(), Has.Count.EqualTo(0));
            });
        }

        [Test]
        public void VerifyThatChildCanBeAdded()
        {
            var treeNode = new TreeNode(new SceneObject(new Cone(1, 1)));
            var childVM = new NodeComponentViewModel(treeNode, this.selectionMediator.Object);

            this.rootNodeVM.AddChild(childVM);

            Assert.Multiple(() =>
            {
                Assert.That(this.rootNodeVM.Parent, Is.Null);
                Assert.That(this.rootNodeVM.Children, Has.Count.EqualTo(3));
                Assert.That(this.rootNodeVM.Node.GetChildren(), Has.Count.EqualTo(3));
                Assert.That(this.rootNodeVM.Node.GetParentNode(), Is.Null);
                Assert.That(childVM.Parent, Is.Not.Null);
                Assert.That(childVM.Children, Is.Empty);
                Assert.That(childVM.Node.GetChildren(), Is.Empty);
                Assert.That(childVM.GetParentNode(), Is.EqualTo(this.rootNodeVM));
                Assert.That(childVM.Node.GetParentNode(), Is.EqualTo(this.rootNodeVM.Node));
            });
        }

        [Test]
        public void VerifyThatChildCanBeRemoved()
        {
            this.rootNodeVM.RemoveChild(this.node3VM);

            Assert.Multiple(() =>
            {
                Assert.That(this.rootNodeVM.Parent, Is.Null);
                Assert.That(this.rootNodeVM.Children, Has.Count.EqualTo(1));
                Assert.That(this.rootNodeVM.Node.GetChildren(), Has.Count.EqualTo(1));
                Assert.That(this.rootNodeVM.Node.GetParentNode(), Is.Null);
                Assert.That(this.node3VM.Parent, Is.Null);
                Assert.That(this.node3VM.Children, Is.Not.Empty);
                Assert.That(this.node3VM.Node.GetChildren(), Is.Not.Empty);
                Assert.That(this.node3VM.GetParentNode(), Is.Null);
                Assert.That(this.node3VM.Node.GetParentNode(), Is.Null);
            });
        }

        [Test]
        public void VerifyThatRootNodeCanBeRetrieved()
        {
            var result1 = this.node3VM.GetRootNode();
            var result2 = this.node5VM.GetRootNode();
            var result3 = this.node2VM.GetRootNode();

            Assert.Multiple(() =>
            {
                Assert.That(result1, Is.Not.Null);
                Assert.That(result2, Is.Not.Null);
                Assert.That(result3, Is.Not.Null);
                Assert.That(result1, Is.EqualTo(this.rootNodeVM));
                Assert.That(result2, Is.EqualTo(this.rootNodeVM));
                Assert.That(result3, Is.EqualTo(this.rootNodeVM));
            });
        }

        [Test]
        public void VerifyThatGetFlatListOfDescendantsWorks()
        {
            var descendants = this.rootNodeVM.GetFlatListOfDescendants(false);

            Assert.Multiple(() =>
            {
                Assert.That(descendants, Has.Count.EqualTo(5));
                Assert.That(descendants.Contains(this.node1VM), Is.True);
                Assert.That(descendants.Contains(this.node2VM), Is.True);
                Assert.That(descendants.Contains(this.node3VM), Is.True);
                Assert.That(descendants.Contains(this.node4VM), Is.True);
                Assert.That(descendants.Contains(this.node5VM), Is.True);
            });
        }
    }
}
