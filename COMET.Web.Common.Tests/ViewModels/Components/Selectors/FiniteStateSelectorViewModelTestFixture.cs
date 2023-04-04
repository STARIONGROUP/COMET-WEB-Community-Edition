// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FiniteStateSelectorViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.ViewModels.Components.Selectors
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using NUnit.Framework;

    [TestFixture]
    public class FiniteStateSelectorViewModelTestFixture
    {
        private FiniteStateSelectorViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            this.viewModel = new FiniteStateSelectorViewModel();
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableFiniteStates, Is.Empty);
                Assert.That(this.viewModel.CurrentIteration, Is.Null);
                Assert.That(this.viewModel.SelectedActualFiniteState, Is.Null);
            });
        }

        [Test]
        public void VerifyUpdateProperties()
        {
            var possibleFiniteStateList = new PossibleFiniteStateList() { Iid = Guid.NewGuid(), Name = "possibleList"};
            possibleFiniteStateList.PossibleState.Add(new PossibleFiniteState(){Iid = Guid.NewGuid(), Name = "c"});
            possibleFiniteStateList.PossibleState.Add(new PossibleFiniteState(){Iid = Guid.NewGuid(), Name = "b" });

            var possibleFiniteStateList2 = new PossibleFiniteStateList() { Iid = Guid.NewGuid(), Name = "anotherlist"};
            possibleFiniteStateList2.PossibleState.Add(new PossibleFiniteState() {Iid = Guid.NewGuid(), Name = "a" });

            var actualFinititeStateList = InitializeActualFiniteStateList(possibleFiniteStateList);
            var actualFinititeStateList2 = InitializeActualFiniteStateList(possibleFiniteStateList2);

            var iteration = new Iteration()
            {
                Iid = Guid.NewGuid(),
                ActualFiniteStateList = { actualFinititeStateList, actualFinititeStateList2 }
            };

            this.viewModel.CurrentIteration = iteration;
            Assert.That(this.viewModel.AvailableFiniteStates.Count(), Is.EqualTo(3));

            this.viewModel.CurrentIteration = null;
            Assert.That(this.viewModel.AvailableFiniteStates, Is.Empty);
        }

        private static ActualFiniteStateList InitializeActualFiniteStateList(PossibleFiniteStateList possibleFiniteStateList)
        {
            var actualFiniteStateList = new ActualFiniteStateList() { Iid = Guid.NewGuid() };
            actualFiniteStateList.PossibleFiniteStateList.Add(possibleFiniteStateList);

            foreach (var possibleFiniteState in possibleFiniteStateList.PossibleState.ToList())
            {
                actualFiniteStateList.ActualState.Add(new ActualFiniteState() { Iid = Guid.NewGuid(), PossibleState = new List<PossibleFiniteState> { possibleFiniteState } });
            }

            return actualFiniteStateList;
        }
    }
}
