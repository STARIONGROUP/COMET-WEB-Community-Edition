// -------------------------------------------------------------------------------------------------------------------- 
// <copyright file="ParameterValueSetRelationExtensionsTestFixture.cs" company="RHEA System S.A."> 
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

namespace COMETwebapp.Tests.Extensions
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using COMETwebapp.Extensions;
    
    using NUnit.Framework;

    [TestFixture]
    public class ParameterValueSetRelationExtensionsTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Uri uri = new ("http://www.rheagroup.com");
        private Dictionary<ParameterBase, IValueSet> relations1;
        private Dictionary<ParameterBase, IValueSet> relations2;
        private Dictionary<ParameterBase, IValueSet> relations3;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.relations1 = new Dictionary<ParameterBase, IValueSet>();
            this.relations2 = new Dictionary<ParameterBase, IValueSet>();
            this.relations3 = new Dictionary<ParameterBase, IValueSet>();

            var valueArray1 = new ValueArray<string>(new List<string>() { "1", "2.23", "0.12", "-0.1", ".01" }) ;
            var valueArray2 = new ValueArray<string>(new List<string>() { "1.0", "2.23", "0.12", "-0.1", ".01" });
            var valueArray3 = new ValueArray<string>(new List<string>() { "1", "2.23", "0.12", "-0.11" });

            var parameterValueSet1 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            parameterValueSet1.Manual = valueArray1;
            parameterValueSet1.ValueSwitch = ParameterSwitchKind.MANUAL;

            var parameterValueSet2 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            parameterValueSet2.Manual = valueArray2;
            parameterValueSet2.ValueSwitch = ParameterSwitchKind.MANUAL;

            var parameterValueSet3 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            parameterValueSet3.Manual = valueArray3;
            parameterValueSet3.ValueSwitch = ParameterSwitchKind.MANUAL;

            var parameter1 = new Parameter(Guid.NewGuid(), this.cache, this.uri);
            parameter1.ValueSet.Add(parameterValueSet1);

            var parameter2 = new Parameter(Guid.NewGuid(), this.cache, this.uri);
            parameter2.ValueSet.Add(parameterValueSet2);

            var parameter3 = new Parameter(Guid.NewGuid(), this.cache, this.uri);
            parameter3.ValueSet.Add(parameterValueSet3);

            this.relations1.Add(parameter1, parameterValueSet1);
            this.relations1.Add(parameter2, parameterValueSet2);
            this.relations1.Add(parameter3, parameterValueSet3);

            this.relations2.Add(parameter1, parameterValueSet1);
            this.relations2.Add(parameter2, parameterValueSet2);
            this.relations2.Add(parameter3, parameterValueSet3);

            this.relations3.Add(parameter1, parameterValueSet3);
            this.relations3.Add(parameter2, parameterValueSet2);
            this.relations3.Add(parameter3, parameterValueSet1);
        }

        [Test]
        public void VerifyThatGetChangesOnParametersWorks()
        {
            var changes1 = this.relations1.GetChangesOnParameters(this.relations2);
            var changes2 = this.relations1.GetChangesOnParameters(this.relations3);
            var changes3 = this.relations3.GetChangesOnParameters(this.relations2);

            Assert.Multiple(() =>
            {
                Assert.That(changes1, Has.Count.EqualTo(0));
                Assert.That(changes2, Has.Count.EqualTo(2));
                Assert.That(changes3, Has.Count.EqualTo(2));
            });
        }
    }
}
