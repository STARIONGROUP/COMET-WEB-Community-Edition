// -------------------------------------------------------------------------------------------------------------------- 
// <copyright file="PropertiesViewModelTestFixture.cs" company="RHEA System S.A."> 
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

namespace COMETwebapp.Tests.ViewModels.Components.Viewer.PropertiesPanel
{
    using COMETwebapp.IterationServices;
    using COMETwebapp.Model;
    using COMETwebapp.Model.Primitives;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.JSInterop;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class PropertiesViewModelTestFixture
    {
        private TestContext context;
        private IPropertiesComponentViewModel viewModel;
        private Mock<IJSRuntime> jsRuntime;
        private Mock<IIterationService> iteratioService;
        private Mock<ISessionAnchor> sessionAnchor;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            var selectionMediator = new SelectionMediator();
            this.context.Services.AddSingleton<ISelectionMediator>(selectionMediator);

            this.jsRuntime = new Mock<IJSRuntime>();
            this.iteratioService = new Mock<IIterationService>();
            this.sessionAnchor = new Mock<ISessionAnchor>();

            this.viewModel = new PropertiesComponentViewModel(this.jsRuntime.Object, this.iteratioService.Object, this.sessionAnchor.Object, selectionMediator);
        }

        [Test]
        public void VerifyViewModel()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.JsInterop, Is.Not.Null);
                Assert.That(this.viewModel.IterationService, Is.Not.Null);
                Assert.That(this.viewModel.SessionAnchor, Is.Not.Null);
                Assert.That(this.viewModel.SelectionMediator, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyThatOnTreeSelectionWorks()
        {
            this.viewModel.IsVisible = true;
            Assert.That(this.viewModel.IsVisible, Is.True);
            this.viewModel.SelectionMediator.RaiseOnTreeSelectionChanged(new TreeNode(null));
            Assert.That(this.viewModel.IsVisible, Is.False);
            this.viewModel.SelectionMediator.RaiseOnTreeSelectionChanged(new TreeNode(new SceneObject(new Cube(1,1,1))));
            Assert.That(this.viewModel.IsVisible, Is.True);
        }

        [Test]
        public void VerifyThatOnModelSelectionWorks()
        {
            this.viewModel.IsVisible = true;
            Assert.That(this.viewModel.IsVisible, Is.True);
            this.viewModel.SelectionMediator.RaiseOnModelSelectionChanged(null);
            Assert.That(this.viewModel.IsVisible, Is.False);
            this.viewModel.SelectionMediator.RaiseOnModelSelectionChanged(new SceneObject(new Cube(1,1,1)));
            Assert.That(this.viewModel.IsVisible, Is.True);
        }
    }
}
