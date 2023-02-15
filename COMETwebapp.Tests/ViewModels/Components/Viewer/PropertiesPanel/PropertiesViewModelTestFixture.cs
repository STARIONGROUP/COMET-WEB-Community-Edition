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
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class PropertiesViewModelTestFixture
    {
        private TestContext context;
        private IPropertiesComponentViewModel viewModel;
        private Mock<IBabylonInterop> babylonInterop;
        private Mock<ISessionService> sessionService;
        private Mock<ISelectionMediator> selectionMediator;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.selectionMediator = new Mock<ISelectionMediator>();

            this.selectionMediator.Setup(x => x.RaiseOnModelSelectionChanged(null)).Callback(() => this.viewModel.IsVisible = false);
            this.selectionMediator.Setup(x => x.RaiseOnTreeSelectionChanged(null)).Callback(() => this.viewModel.IsVisible = false);

            this.context.Services.AddSingleton(this.selectionMediator.Object);

            this.babylonInterop = new Mock<IBabylonInterop>();
            this.sessionService = new Mock<ISessionService>();

            this.viewModel = new PropertiesComponentViewModel(this.babylonInterop.Object, this.sessionService.Object, this.selectionMediator.Object);
        }

        [Test]
        public void VerifyThatOnModelSelectionWorks()
        {
            this.viewModel.IsVisible = true;
            Assert.That(this.viewModel.IsVisible, Is.True);
            this.viewModel.SelectionMediator.RaiseOnModelSelectionChanged(null);
            Assert.That(this.viewModel.IsVisible, Is.False);
        }

        [Test]
        public void VerifyThatOnTreeSelectionWorks()
        {
            this.viewModel.IsVisible = true;
            Assert.That(this.viewModel.IsVisible, Is.True);
            this.viewModel.SelectionMediator.RaiseOnTreeSelectionChanged(null);
            Assert.That(this.viewModel.IsVisible, Is.False);
        }

        [Test]
        public void VerifyViewModel()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.BabylonInterop, Is.Not.Null);
                Assert.That(this.viewModel.SessionService, Is.Not.Null);
                Assert.That(this.viewModel.SelectionMediator, Is.Not.Null);
                Assert.That(this.viewModel.SelectedParameter, Is.Null);
                Assert.That(this.viewModel.ParametersInUse, Is.Not.Null);
                Assert.That(this.viewModel.ParametersInUse, Has.Count.EqualTo(0));
                Assert.That(this.viewModel.ParameterHaveChanges, Is.False);
            });
        }
    }
}
