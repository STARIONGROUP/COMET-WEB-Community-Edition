// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateSelectorComponentTestFixture.cs" company="RHEA System S.A.">
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

using COMETwebapp.Components.Viewer.Canvas;

namespace COMETwebapp.Tests.Components.Viewer.Canvas
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using BlazorStrap;

    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.ViewModels.Components.Viewer.Canvas;

    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ActualFiniteStateSelectorComponentTestFixture
    {
        private TestContext context;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Uri uri = new Uri("http://www.rheagroup.com");
        private IRenderedComponent<ActualFiniteStateSelectorComponent> rendererComponent;
        private IActualFiniteStateSelectorViewModel viewModel;
        private ActualFiniteState actualFiniteState1;
        private ActualFiniteState actualFiniteState2;
        private ActualFiniteState actualFiniteState3;
        private ActualFiniteState actualFiniteState4;
        private List<ActualFiniteStateList> collectionOfActualFiniteStateLists;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();

            this.viewModel = new ActualFiniteStateSelectorViewModel();

            this.context.Services.AddSingleton<IActualFiniteStateSelectorViewModel>(this.viewModel);
            this.context.Services.AddBlazorStrap();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            var possibleFiniteState1 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteState1 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteState1.PossibleState.Add(possibleFiniteState1);

            var possibleFiniteState2 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteState2 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteState2.PossibleState.Add(possibleFiniteState2);

            var possibleFiniteStateList1 = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            possibleFiniteStateList1.PossibleState.Add(possibleFiniteState1);
            possibleFiniteStateList1.PossibleState.Add(possibleFiniteState2);
            possibleFiniteStateList1.DefaultState = possibleFiniteState1;

            var actualFiniteStateList1 = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            actualFiniteStateList1.ActualState.Add(this.actualFiniteState1);
            actualFiniteStateList1.ActualState.Add(this.actualFiniteState2);
            actualFiniteStateList1.PossibleFiniteStateList.Add(possibleFiniteStateList1);

            var possibleFiniteState3 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteState3 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteState3.PossibleState.Add(possibleFiniteState3);

            var possibleFiniteState4 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteState4 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteState4.PossibleState.Add(possibleFiniteState4);

            var possibleFiniteStateList2 = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            possibleFiniteStateList2.PossibleState.Add(possibleFiniteState3);
            possibleFiniteStateList2.PossibleState.Add(possibleFiniteState4);
            possibleFiniteStateList2.DefaultState = possibleFiniteState3;

            var actualFiniteStateList2 = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            actualFiniteStateList2.ActualState.Add(this.actualFiniteState3);
            actualFiniteStateList2.ActualState.Add(this.actualFiniteState4);
            actualFiniteStateList2.PossibleFiniteStateList.Add(possibleFiniteStateList2);

            this.collectionOfActualFiniteStateLists = new List<ActualFiniteStateList>() { actualFiniteStateList1, actualFiniteStateList2 };
            
            this.rendererComponent = this.context.RenderComponent<ActualFiniteStateSelectorComponent>(parameters =>
            {
                parameters.Add(p => p.ActualFiniteStateListsCollection, this.collectionOfActualFiniteStateLists);
            });
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyThatActualStateCanBeClicked()
        {
            var actualFiniteState = this.rendererComponent.Find(".actual-finite-state");
            Assert.That(actualFiniteState, Is.Not.Null);
            actualFiniteState.Click();
        }

        [Test]
        public void VerifyOnActualFiniteStateSelected()
        {
            var previousStates = this.viewModel.SelectedStates;
            this.viewModel.OnActualFiniteStateSelected(this.actualFiniteState2);
            Assert.That(previousStates, Is.Not.EquivalentTo(this.viewModel.SelectedStates));
        }

        [Test]
        public void VerifyThatViewModelCanBeInitialized()
        {
            var previousStates = this.viewModel.SelectedStates;
            this.viewModel.ActualFiniteStateListsCollection.Clear();
            Assert.DoesNotThrow(() => this.viewModel.InitializeViewModel());
            this.viewModel.ActualFiniteStateListsCollection = this.collectionOfActualFiniteStateLists;
            Assert.DoesNotThrow(() => this.viewModel.InitializeViewModel());
            Assert.That(previousStates, Is.Not.EquivalentTo(this.viewModel.SelectedStates));
        }
    }
}
