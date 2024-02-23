// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateSelectorTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.Components.Viewer.Canvas
{
    using System.Collections.Concurrent;

    using Bunit;
    
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Viewer;
    using COMETwebapp.ViewModels.Components.Viewer;

    using Moq;
    
    using NUnit.Framework;
    
    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ActualFiniteStateSelectorTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private readonly Uri uri = new("http://www.rheagroup.com");
        private TestContext context;
        private IRenderedComponent<ActualFiniteStateSelector> renderedComponent;
        private ActualFiniteStateSelector actualStateSelector;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();

            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            var possibleFiniteStateList1 = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            var possibleFiniteStateList2 = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);

            var actualFiniteStateList1 = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteStateList2 = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);

            var possibleFiniteState1 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var possibleFiniteState3 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);

            possibleFiniteStateList1.DefaultState = possibleFiniteState1;
            possibleFiniteStateList2.DefaultState = possibleFiniteState3;

            actualFiniteStateList1.PossibleFiniteStateList.Add(possibleFiniteStateList1);
            actualFiniteStateList2.PossibleFiniteStateList.Add(possibleFiniteStateList2);

            var actualFiniteState1 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteState2 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteState3 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteState4 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            
            actualFiniteStateList1.ActualState.Add(actualFiniteState1);
            actualFiniteStateList1.ActualState.Add(actualFiniteState2);
            actualFiniteStateList2.ActualState.Add(actualFiniteState3);
            actualFiniteStateList2.ActualState.Add(actualFiniteState4);

            var viewModel = new Mock<IActualFiniteStateSelectorViewModel>();

            var actualFiniteStates = new List<ActualFiniteState>()
            {
                actualFiniteState1,
                actualFiniteState2,
                actualFiniteState3,
                actualFiniteState4
            };

            viewModel.Setup(x => x.ActualFiniteStates).Returns(actualFiniteStates);

            this.renderedComponent = this.context.RenderComponent<ActualFiniteStateSelector>(parameters =>
            {
                parameters.Add(p => p.ViewModel, viewModel.Object);
            });

            this.actualStateSelector = this.renderedComponent.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderedComponent, Is.Not.Null);
                Assert.That(this.actualStateSelector, Is.Not.Null);
                Assert.That(this.actualStateSelector.ViewModel, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyMarkUp()
        {
            var header = this.renderedComponent.Find(".actual-finite-state-list");
            var state = this.renderedComponent.Find(".actual-finite-state");

            Assert.Multiple(() =>
            {
                Assert.That(header, Is.Not.Null);
                Assert.That(state, Is.Not.Null);
            });
        }
    }
}
