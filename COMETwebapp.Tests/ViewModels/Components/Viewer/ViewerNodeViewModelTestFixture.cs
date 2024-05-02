// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewerNodeViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.ViewModels.Components.Viewer
{
    using COMETwebapp.Model;
    using COMETwebapp.Model.Viewer.Primitives;
    using COMETwebapp.ViewModels.Components.Viewer;

    using NUnit.Framework;

    [TestFixture]
    public class ViewerNodeViewModelTestFixture
    {
        private ViewerNodeViewModel rootNode;
        private ViewerNodeViewModel node1;
        private ViewerNodeViewModel node2;
        private ViewerNodeViewModel node3;
        private ViewerNodeViewModel node4;
        private ViewerNodeViewModel node5;

        [SetUp]
        public void SetUp()
        {
            this.rootNode = new ViewerNodeViewModel(new SceneObject(null)) { Title = "Loft" };
            this.node1 = new ViewerNodeViewModel(new SceneObject(new Cube(1, 1, 1))) { Title = "Bus" };
            this.node2 = new ViewerNodeViewModel(new SceneObject(new Cube(1, 1, 1))) { Title = "LargeAreaDetector1" };
            this.node3 = new ViewerNodeViewModel(new SceneObject(new Cube(1, 1, 1))) { Title = "OpticalBench" };
            this.node4 = new ViewerNodeViewModel(new SceneObject(new Cube(1, 1, 1))) { Title = "StructuralTower" };
            this.node5 = new ViewerNodeViewModel(new SceneObject(null)) { Title = "WideFieldMonitor" };

            this.rootNode.AddChild(this.node1);
            this.node1.AddChild(this.node2);
            this.rootNode.AddChild(this.node3);
            this.node3.AddChild(this.node4);
            this.node4.AddChild(this.node5);
        }

        [Test]
        public void VerifyViewmodelData()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.rootNode.Parent, Is.Null);
                Assert.That(this.node1.Parent, Is.Not.Null);
                Assert.That(this.node2.Parent, Is.Not.Null);
                Assert.That(this.node3.Parent, Is.Not.Null);
                Assert.That(this.node4.Parent, Is.Not.Null);
                Assert.That(this.node5.Parent, Is.Not.Null);

                Assert.That(this.rootNode.GetChildren(), Has.Count.EqualTo(2));
                Assert.That(this.node1.GetChildren(), Has.Count.EqualTo(1));
                Assert.That(this.node2.GetChildren(), Has.Count.EqualTo(0));
                Assert.That(this.node3.GetChildren(), Has.Count.EqualTo(1));
                Assert.That(this.node4.GetChildren(), Has.Count.EqualTo(1));
                Assert.That(this.node5.GetChildren(), Has.Count.EqualTo(0));
            });
        }

        [Test]
        public void VerifyThatChildCanBeAdded()
        {
            var childVM = new ViewerNodeViewModel(null);

            this.rootNode.AddChild(childVM);

            Assert.Multiple(() =>
            {
                Assert.That(this.rootNode.Parent, Is.Null);
                Assert.That(this.rootNode.GetChildren(), Has.Count.EqualTo(3));
                Assert.That(childVM.Parent, Is.Not.Null);
                Assert.That(childVM.GetChildren(), Is.Empty);
                Assert.That(childVM.GetParentNode(), Is.EqualTo(this.rootNode));
            });
        }

        [Test]
        public void VerifyThatChildCanBeRemoved()
        {
            this.rootNode.RemoveChild(this.node3);

            Assert.Multiple(() =>
            {
                Assert.That(this.rootNode.Parent, Is.Null);
                Assert.That(this.rootNode.GetChildren(), Has.Count.EqualTo(1));
                Assert.That(this.node3.Parent, Is.Null);
                Assert.That(this.node3.GetChildren(), Is.Not.Empty);
                Assert.That(this.node3.GetParentNode(), Is.Null);
            });
        }

        [Test]
        public void VerifyThatRootNodeCanBeRetrieved()
        {
            var result1 = this.node3.GetRootNode();
            var result2 = this.node5.GetRootNode();
            var result3 = this.node2.GetRootNode();

            Assert.Multiple(() =>
            {
                Assert.That(result1, Is.Not.Null);
                Assert.That(result2, Is.Not.Null);
                Assert.That(result3, Is.Not.Null);
                Assert.That(result1, Is.EqualTo(this.rootNode));
                Assert.That(result2, Is.EqualTo(this.rootNode));
                Assert.That(result3, Is.EqualTo(this.rootNode));
            });
        }

        [Test]
        public void VerifyThatGetFlatListOfDescendantsWorks()
        {
            var descendants = this.rootNode.GetFlatListOfDescendants();

            Assert.Multiple(() =>
            {
                Assert.That(descendants, Has.Count.EqualTo(5));
                Assert.That(descendants.Contains(this.node1), Is.True);
                Assert.That(descendants.Contains(this.node2), Is.True);
                Assert.That(descendants.Contains(this.node3), Is.True);
                Assert.That(descendants.Contains(this.node4), Is.True);
                Assert.That(descendants.Contains(this.node5), Is.True);
            });
        }
    }
}
