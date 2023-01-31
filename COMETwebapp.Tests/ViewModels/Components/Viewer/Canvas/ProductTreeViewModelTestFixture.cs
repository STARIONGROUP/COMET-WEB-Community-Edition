// -------------------------------------------------------------------------------------------------------------------- 
// <copyright file="ProductTreeViewModelTestFixture.cs" company="RHEA System S.A."> 
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

namespace COMETwebapp.Tests.ViewModels.Components.Viewer.Canvas
{
    using COMETwebapp.Model;
    using COMETwebapp.Model.Primitives;
    using COMETwebapp.ViewModels.Components.Viewer.Canvas;
    using NUnit.Framework;
    using ReactiveUI;
    using System.Linq;

    using COMETwebapp.Utilities;

    using Moq;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ProductTreeViewModelTestFixture
    {
        private TestContext context;
        private IProductTreeViewModel viewModel;
        private Mock<ISelectionMediator> selectionMediator;
        private NodeComponentViewModel rootNodeVM;
        private NodeComponentViewModel node1VM;
        private NodeComponentViewModel node2VM;
        private NodeComponentViewModel node3VM;
        private NodeComponentViewModel node4VM;
        private NodeComponentViewModel node5VM;


        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.viewModel = new ProductTreeViewModel();
            this.selectionMediator = new Mock<ISelectionMediator>();

            var rootNode = new TreeNode(new SceneObject(null)) { Title = "Loft" };
            var node1 = new TreeNode(new SceneObject(new Cube(1, 1, 1))) { Title = "Bus" };
            var node2 = new TreeNode(new SceneObject(new Cube(1, 1, 1))){ Title = "LargeAreaDetector1" };
            var node3 = new TreeNode(new SceneObject(new Cube(1, 1, 1))){ Title = "OpticalBench" };
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

            this.viewModel.RootViewModel = this.rootNodeVM;
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
                Assert.That(this.rootNodeVM.IsDrawn, Is.False);
                Assert.That(this.node1VM.IsDrawn, Is.False);
                Assert.That(this.node2VM.IsDrawn, Is.True);
                Assert.That(this.node3VM.IsDrawn, Is.False);
                Assert.That(this.node4VM.IsDrawn, Is.False);
                Assert.That(this.node5VM.IsDrawn, Is.True);
            });
        }
    }
}
