// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ValueArrayExtensionsTestFixture.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Tests.Extensions
{
    using System.Collections.Generic;

    using CDP4Common.Types;

    using COMET.Web.Common.Extensions;

    using NUnit.Framework;

    [TestFixture]
    public class ValueArrayExtensionsTestFixture
    {
        [Test]
        public void VerifyThatContainsSameValuesWorks()
        {
            var valueArray1 = new ValueArray<string>(new List<string>() { "1", "ABC", "Test" });
            var valueArray2 = new ValueArray<string>(new List<string>() { "1", "ABC", "Test" });
            var valueArray3 = new ValueArray<string>(new List<string>() { "1", "ABC" });
            var valueArray4 = new ValueArray<string>(new List<string>() { "1", "ABCa", "Test" });
            var valueArray5 = new ValueArray<string>(new List<string>() { "1.0", "ABC", "Test" });

            Assert.Multiple(() =>
            {
                Assert.That(valueArray1.ContainsSameValues(valueArray2), Is.True);
                Assert.That(valueArray1.ContainsSameValues(valueArray3), Is.False);
                Assert.That(valueArray1.ContainsSameValues(valueArray4), Is.False);
                Assert.That(valueArray1.ContainsSameValues(valueArray5), Is.False);
            });
        }
    }
}
