// -------------------------------------------------------------------------------------------------------------------- 
// <copyright file="MultipleActualFiniteStateSelectorViewModelTestFixture.cs" company="RHEA System S.A."> 
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
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using COMETwebapp.ViewModels.Components.Viewer.Canvas;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class MultipleActualFiniteStateSelectorViewModelTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private readonly Uri uri = new("http://www.rheagroup.com");
        private IMultipleActualFiniteStateSelectorViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            
            var actualFiniteStateList1 = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteStateList2 = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            
            var actualFiniteState1 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteState2 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteState3 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteState4 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);

            actualFiniteStateList1.ActualState.Add(actualFiniteState1);
            actualFiniteStateList1.ActualState.Add(actualFiniteState2);
            actualFiniteStateList2.ActualState.Add(actualFiniteState3);
            actualFiniteStateList2.ActualState.Add(actualFiniteState4);

            this.viewModel = new MultipleActualFiniteStateSelectorViewModel()
            {
                CurrentIteration = new Iteration
                {
                    Iid = Guid.NewGuid(),
                    ActualFiniteStateList = { actualFiniteStateList1, actualFiniteStateList2 }
                }
            };
         }

        [Test]
        public void VerifyViewModel()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel, Is.Not.Null);
                Assert.That(this.viewModel.SelectedFiniteStates, Is.Not.Null);
                Assert.That(this.viewModel.ActualFiniteStateSelectorViewModels.ToList(), Has.Count.EqualTo(2));
                Assert.That(this.viewModel.CurrentIteration, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyThatUpdatePropertiesIsCalled()
        {
            var oldFiniteStates = this.viewModel.ActualFiniteStateListsCollection.Items;
            this.viewModel.CurrentIteration = new Iteration();
            
            Assert.Multiple(() =>
            {
                Assert.That(oldFiniteStates, Is.Not.EqualTo(this.viewModel.ActualFiniteStateListsCollection.Items));
                Assert.That(this.viewModel.ActualFiniteStateListsCollection.Items, Is.Empty);
            });
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            this.viewModel.ActualFiniteStateSelectorViewModels = Enumerable.Empty<IActualFiniteStateSelectorViewModel>();
            this.viewModel.InitializeViewModel();
            
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ActualFiniteStateSelectorViewModels, Is.Not.Empty);
                Assert.That(this.viewModel.SelectedFiniteStates, Is.Not.Empty);
            });
        }

        [Test]
        public void VerifyOnActualFiniteStateSelectionChanged()
        {
            var stateSelector = this.viewModel.ActualFiniteStateSelectorViewModels.Last();
            stateSelector.SelectedFiniteState = stateSelector.ActualFiniteStates.Last();

            this.viewModel.OnActualFiniteStateSelectionChanged(stateSelector);

            var selectedStatesChanged = false;

            this.WhenAnyValue(x => x.viewModel.SelectedFiniteStates).Subscribe(_ =>
            {
                selectedStatesChanged = true;
            });

            Assert.Multiple(() =>
            {
                Assert.That(selectedStatesChanged, Is.True);
            });
        }
    }
}
