// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterBaseRowViewModelComparerTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMETwebapp.Tests.Comparer
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMETwebapp.Comparer;
    using COMETwebapp.ViewModels.Components.ParameterEditor;

    using NUnit.Framework;

    [TestFixture]
    public class ParameterBaseRowViewModelComparerTestFixture
    {
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyComparer()
        {
            var firstParameterType = new BooleanParameterType()
            {
                Name = "bool"
            };

            var secondParameterType = new BooleanParameterType()
            {
                Name = "the bool"
            };

            var elementDefinition = new ElementDefinition();
            
            var firstParameter = new  Parameter()
            {
                ParameterType = firstParameterType,
                ValueSet = 
                { 
                    new ParameterValueSet()
                    {
                        Published = new ValueArray<string>(new []{"-"}),
                        Computed = new ValueArray<string>(new []{"-"})
                    }
                }
            };

            var secondParameter = new Parameter()
            {
                ParameterType = secondParameterType,
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Published = new ValueArray<string>(new []{"-"}),
                        Computed = new ValueArray<string>(new []{"-"})
                    }
                }
            };

            elementDefinition.Parameter.AddRange(new List<Parameter>{secondParameter, firstParameter});

            var firstRow = new ParameterBaseRowViewModel(null, true, firstParameter, firstParameter.ValueSet[0], this.messageBus);
            var secondRow = new ParameterBaseRowViewModel(null, true, secondParameter, secondParameter.ValueSet[0], this.messageBus);
            var comparer = new ParameterBaseRowViewModelComparer();

            Assert.Multiple(() =>
            {
                Assert.That(comparer.Compare(null, null), Is.EqualTo(0));
                Assert.That(comparer.Compare(null, firstRow), Is.LessThan(0));
                Assert.That(comparer.Compare(firstRow, null), Is.GreaterThan(0));
                Assert.That(comparer.Compare(firstRow, secondRow), Is.LessThan(0));
                Assert.That(comparer.Compare(secondRow, firstRow), Is.GreaterThan(0));
            });
        }
    }
}
