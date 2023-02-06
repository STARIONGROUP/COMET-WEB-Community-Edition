// -------------------------------------------------------------------------------------------------------------------- 
// <copyright file="ViewerPageTestFixture.cs" company="RHEA System S.A."> 
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

namespace COMETwebapp.Tests.Pages
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    using BlazorStrap;
    
    using Bunit;
    
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;
    
    using COMETwebapp.IterationServices;
    using COMETwebapp.Pages.Viewer;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Utilities;
    
    using Microsoft.Extensions.DependencyInjection;
    
    using Moq;
    
    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ViewerPageTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ViewerPage> renderedComponent;
        private Mock<ISessionService> sessionAnchorMock;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Uri uri = new Uri("http://www.rheagroup.com");
        private ViewerPage viewerPage;
        private Option option1, option2;
        private ActualFiniteStateList actualFiniteStateList1, actualFiniteStateList2;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;
            this.context.JSInterop.SetupVoid("InitCanvas");
            this.context.Services.AddBlazorStrap();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.sessionAnchorMock = new Mock<ISessionService>();
            var iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.option1 = new Option(Guid.NewGuid(), this.cache, this.uri) { Name = "Option1" };
            this.option2 = new Option(Guid.NewGuid(), this.cache, this.uri) { Name = "Option2" };
            iteration.Option.Add(option1);
            iteration.Option.Add(option2);
            iteration.DefaultOption = option1;

            var topElement = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            iteration.TopElement = topElement;

            var elementUsage1 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri);
            var elementUsage2 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri);

            var elementDefinition1 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            elementDefinition1.ContainedElement.Add(elementUsage1);

            var elementDefinition2 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            elementDefinition2.ContainedElement.Add(elementUsage2);

            iteration.Element.Add(elementDefinition1);
            iteration.Element.Add(elementDefinition2);

            var possibleFiniteStateList1 = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            var possibleFiniteStateList2 = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);

            this.actualFiniteStateList1 = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteStateList2 = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);

            var possibleFiniteState1 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var possibleFiniteState2 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var possibleFiniteState3 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var possibleFiniteState4 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);

            possibleFiniteStateList1.DefaultState = possibleFiniteState1;
            possibleFiniteStateList2.DefaultState = possibleFiniteState3;

            this.actualFiniteStateList1.PossibleFiniteStateList.Add(possibleFiniteStateList1);
            this.actualFiniteStateList2.PossibleFiniteStateList.Add(possibleFiniteStateList1);

            var actualFiniteState1 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteState2 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteState3 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteState4 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);

            actualFiniteState1.Container = possibleFiniteState1;
            actualFiniteState2.Container = possibleFiniteState2;
            actualFiniteState3.Container = possibleFiniteState3;
            actualFiniteState4.Container = possibleFiniteState4;

            this.actualFiniteStateList1.ActualState.Add(actualFiniteState1);
            this.actualFiniteStateList1.ActualState.Add(actualFiniteState2);
            this.actualFiniteStateList2.ActualState.Add(actualFiniteState3);
            this.actualFiniteStateList2.ActualState.Add(actualFiniteState4);


            iteration.ActualFiniteStateList.Add(this.actualFiniteStateList1);
            iteration.ActualFiniteStateList.Add(this.actualFiniteStateList2);

            this.sessionAnchorMock.Setup(x => x.DefaultIteration).Returns(iteration);

            this.context.Services.AddSingleton<ISessionService>(this.sessionAnchorMock.Object);
            this.context.Services.AddSingleton<IIterationService, IterationService>();
            this.context.Services.AddSingleton<ISelectionMediator, SelectionMediator>();

            this.renderedComponent = this.context.RenderComponent<ViewerPage>();
            this.viewerPage = this.renderedComponent.Instance;
        }

        [Test]
        public void VerifyThatComponentIsCorrectlyInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewerPage.Elements, Is.Not.Null);
                Assert.That(this.viewerPage.Elements, Has.Count.EqualTo(3));
                Assert.That(this.viewerPage.TotalOptions, Is.Not.Null);
                Assert.That(this.viewerPage.TotalOptions, Has.Count.EqualTo(2));
                Assert.That(this.viewerPage.SelectedOption, Is.Not.Null);
                Assert.That(this.viewerPage.SelectedOption, Is.EqualTo(this.viewerPage.TotalOptions.First()));
                Assert.That(this.viewerPage.ListActualFiniteStateLists, Is.Not.Null);
                Assert.That(this.viewerPage.SelectedActualFiniteStates, Is.Not.Null);
                Assert.That(this.viewerPage.RootNode, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyOnOptionFilterChangeWorks()
        {
            Assert.That(this.viewerPage.SelectedOption, Is.EqualTo(this.option1));
            this.viewerPage.OnOptionFilterChange("Option2");
            Assert.That(this.viewerPage.SelectedOption, Is.EqualTo(this.option2));
        }

        [Test]
        public void VerifyThatActualFiniteStateChangedWorks()
        {
            var selectedActualStatesBefore = this.viewerPage.SelectedActualFiniteStates;
            var actualFiniteState1 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteState2 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);

            this.viewerPage.ActualFiniteStateChanged(new System.Collections.Generic.List<ActualFiniteState>() { actualFiniteState1, actualFiniteState2 });
            CollectionAssert.AreNotEqual(selectedActualStatesBefore, this.viewerPage.SelectedActualFiniteStates);
        }
    }
}
