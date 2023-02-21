// -------------------------------------------------------------------------------------------------------------------- 
// <copyright file="ActualFiniteStateSelectorViewModelTestFixture.cs" company="RHEA System S.A."> 
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
    using COMETwebapp.ViewModels.Components.Viewer.Canvas;

    using NUnit.Framework;
    using System;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using CDP4Common.CommonData;

    [TestFixture]
    public class ActualFiniteStateSelectorViewModelTestFixture
    {
        private IActualFiniteStateSelectorViewModel viewModel;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private readonly Uri uri = new("http://www.rheagroup.com");
        private ActualFiniteState actualFiniteState;

        [SetUp]
        public void SetUp()
        {
            //this.viewModel = new ActualFiniteStateSelectorViewModel();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            
            var possibleFiniteStateList1 = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            var possibleFiniteStateList2 = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);

            var actualFiniteStateList1 = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteStateList2 = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);

            var possibleFiniteState1 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var possibleFiniteState2 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var possibleFiniteState3 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var possibleFiniteState4 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);

            possibleFiniteStateList1.DefaultState = possibleFiniteState1;
            possibleFiniteStateList2.DefaultState = possibleFiniteState3;

            actualFiniteStateList1.PossibleFiniteStateList.Add(possibleFiniteStateList1);
            actualFiniteStateList2.PossibleFiniteStateList.Add(possibleFiniteStateList1);

            var actualFiniteState1 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteState = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteState3 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            var actualFiniteState4 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);

            actualFiniteState1.Container = possibleFiniteState1;
            this.actualFiniteState.Container = possibleFiniteState2;
            actualFiniteState3.Container = possibleFiniteState3;
            actualFiniteState4.Container = possibleFiniteState4;

            actualFiniteStateList1.ActualState.Add(actualFiniteState1);
            actualFiniteStateList1.ActualState.Add(this.actualFiniteState);
            actualFiniteStateList2.ActualState.Add(actualFiniteState3);
            actualFiniteStateList2.ActualState.Add(actualFiniteState4);


        }

    }
}
