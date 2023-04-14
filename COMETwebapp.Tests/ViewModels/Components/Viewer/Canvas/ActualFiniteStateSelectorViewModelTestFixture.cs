// -------------------------------------------------------------------------------------------------------------------- 
// <copyright file="ActualFiniteStateSelectorViewModelTestFixture.cs" company="RHEA System S.A."> 
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
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;
    
    using COMETwebapp.ViewModels.Components.Viewer.Canvas;

    using Microsoft.AspNetCore.Components;

    using NUnit.Framework;
    
    [TestFixture]
    public class ActualFiniteStateSelectorViewModelTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private readonly Uri uri = new("http://www.rheagroup.com");
        private IActualFiniteStateSelectorViewModel viewModel;
        private EventCallback<ActualFiniteStateSelectorViewModel> eventCallback;
        private bool eventCallbackCalled;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            var possibleFiniteStateList1 = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);

            var actualFiniteStateList1 = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);

            var possibleFiniteState1 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            possibleFiniteStateList1.DefaultState = possibleFiniteState1;

            actualFiniteStateList1.PossibleFiniteStateList.Add(possibleFiniteStateList1);

            var actualFiniteState1 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteState2 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);

            actualFiniteStateList1.ActualState.Add(actualFiniteState1);
            actualFiniteStateList1.ActualState.Add(actualFiniteState2);

            this.eventCallback = new EventCallbackFactory().Create(this, (ActualFiniteStateSelectorViewModel vm) =>
            {
                this.eventCallbackCalled = true;
            });

            this.eventCallbackCalled = false;

            this.viewModel = new ActualFiniteStateSelectorViewModel(actualFiniteStateList1, this.eventCallback)
            {
                CurrentIteration = new Iteration
                {
                    Iid = Guid.NewGuid(),
                    ActualFiniteStateList = { actualFiniteStateList1 }
                }
            };
        }

        [Test]
        public void VerifyViewModel()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel, Is.Not.Null);
                Assert.That(this.viewModel.ActualFiniteStates, Is.Not.Null);
                Assert.That(this.viewModel.ActualFiniteStates.ToList(), Has.Count.EqualTo(2));
                Assert.That(this.viewModel.SelectedFiniteState, Is.Not.Null);
                Assert.That(this.viewModel.CurrentIteration, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyThatUpdatePropertiesIsCalled()
        {
            var oldFiniteStates = this.viewModel.ActualFiniteStates;
            this.viewModel.CurrentIteration = new Iteration();
            Assert.Multiple(() =>
            {
                Assert.That(oldFiniteStates, Is.Not.EqualTo(this.viewModel.ActualFiniteStates));
                Assert.That(this.viewModel.ActualFiniteStates.ToList(), Is.Empty);
            });
        }

        [Test]
        public void VerifySelectActualFiniteState()
        {
            var oldSelectedFiniteState = this.viewModel.SelectedFiniteState;
            this.viewModel.SelectActualFiniteState(new ActualFiniteState());

            Assert.Multiple(() =>
            {
                Assert.That(oldSelectedFiniteState, Is.EqualTo(this.viewModel.SelectedFiniteState));
                Assert.That(this.eventCallbackCalled, Is.True);
            });

            this.eventCallbackCalled = false;
            oldSelectedFiniteState = this.viewModel.SelectedFiniteState;
            this.viewModel.SelectActualFiniteState(this.viewModel.ActualFiniteStates.Last());

            Assert.Multiple(() =>
            {
                Assert.That(oldSelectedFiniteState, Is.Not.EqualTo(this.viewModel.SelectedFiniteState));
                Assert.That(this.eventCallbackCalled, Is.True);
            });
        }
    }
}
