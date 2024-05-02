// -------------------------------------------------------------------------------------------------------------------- 
// <copyright file="ViewerProductTreeViewModelTestFixture.cs" company="Starion Group S.A."> 
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

    using NUnit.Framework;

    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer;

    using Moq;

    [TestFixture]
    public class ViewerProductTreeViewModelTestFixture
    {
        private ViewerProductTreeViewModel viewModel;
        private Mock<ISelectionMediator> selectionMediator;
        private ViewerNodeViewModel rootNode;
        private ViewerNodeViewModel node1;
        private ViewerNodeViewModel node2;
        private ViewerNodeViewModel node3;
        private ViewerNodeViewModel node4;
        private ViewerNodeViewModel node5;

        [SetUp]
        public void SetUp()
        {
            this.selectionMediator = new Mock<ISelectionMediator>();
            this.viewModel = new ViewerProductTreeViewModel(this.selectionMediator.Object);

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
            this.node4.AddChild(this.node1);
            this.rootNode.AddChild(this.node5);

            this.viewModel.RootViewModel = this.rootNode;
        }

        [Test]
        public void VerifyInitialization()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.TreeFilters, Has.Count.EqualTo(2));
                Assert.That(this.viewModel.SelectedFilter, Is.EqualTo(Enumerations.TreeFilter.ShowFullTree));
                Assert.That(this.viewModel.SearchText, Is.Empty);
                Assert.That(this.viewModel.RootViewModel, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyOnFilterChanged()
        {
            this.viewModel.SelectedFilter = Enumerations.TreeFilter.ShowNodesWithGeometry;
            this.viewModel.OnFilterChanged();
            var fullTree = this.viewModel.RootViewModel.GetFlatListOfDescendants(true);

            var nodesDrawn = fullTree.Where(x => x.IsDrawn).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(fullTree, Is.Not.Null);
                Assert.That(fullTree, Has.Count.EqualTo(6));
                Assert.That(nodesDrawn, Has.Count.EqualTo(4));
            });

            this.viewModel.SelectedFilter = Enumerations.TreeFilter.ShowFullTree;
            this.viewModel.OnFilterChanged();
            fullTree = this.viewModel.RootViewModel.GetFlatListOfDescendants(true);
            nodesDrawn = fullTree.Where(x => x.IsDrawn).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(fullTree, Is.Not.Null);
                Assert.That(fullTree, Has.Count.EqualTo(6));
                Assert.That(nodesDrawn, Has.Count.EqualTo(6));
            });
        }

        [Test]
        public void VerifyOnSearchFilterChange()
        {
            this.viewModel.SearchText = "de";
            this.viewModel.OnSearchFilterChange();

            Assert.Multiple(() =>
            {
                Assert.That(this.rootNode.IsDrawn, Is.False);
                Assert.That(this.node1.IsDrawn, Is.False);
                Assert.That(this.node2.IsDrawn, Is.True);
                Assert.That(this.node3.IsDrawn, Is.False);
                Assert.That(this.node4.IsDrawn, Is.False);
                Assert.That(this.node5.IsDrawn, Is.True);
            });
        }
    }
}
