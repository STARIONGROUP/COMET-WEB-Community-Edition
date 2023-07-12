// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="GuidExtensionsTestFixture.cs" company="RHEA System S.A.">
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
    using COMET.Web.Common.Extensions;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="GuidExtensions" /> class.
    /// </summary>
    [TestFixture]
    public class GuidExtensionsTestFixture
    {
        private const string GuidString = "c9a646d3-9c61-4cb7-bfcd-ee2522c8f633";
        private const string ShortGuid = "00amyWGct0y_ze4lIsj2Mw";

        [Test]
        public void Verify_Short_Guid_Array()
        {
            var guids = new List<Guid> { Guid.Parse(GuidString), Guid.Parse(GuidString) };
            const string shortGuidArray = "[" + ShortGuid + ";" + ShortGuid + "]";
            
            Assert.Multiple(() =>
            {
                Assert.That(guids.ToShortGuidArray(), Is.EqualTo(shortGuidArray));
                Assert.That(shortGuidArray.FromShortGuidArray(), Is.EqualTo(guids));
            });
        }

        [Test]
        public void Verify_that_FromShortGuid_from_string_returns_expected_result()
        {
            var guid = Guid.Parse(GuidString);

            Assert.That(ShortGuid.FromShortGuid(), Is.EqualTo(guid));
        }

        [Test]
        public void Verify_that_ToShortGuid_from_guid_returns_expected_result()
        {
            var guid = Guid.Parse(GuidString);

            Assert.That(guid.ToShortGuid(), Is.EqualTo(ShortGuid));
        }

        [Test]
        public void Verify_that_ToShortGuid_from_string_returns_expected_result()
        {
            Assert.That(GuidString.ToShortGuid(), Is.EqualTo(ShortGuid));
        }

        [Test]
        public void Verify_ToShortGuids_from_guid()
        {
            var guids = new List<Guid> { Guid.Parse(GuidString), Guid.Parse(GuidString) };
            var result = new List<string> { ShortGuid, ShortGuid };

            Assert.That(guids.ToShortGuids(), Is.EquivalentTo(result));
        }

        [Test]
        public void Verify_ToShortGuids_from_string()
        {
            var guids = new List<string> { GuidString, GuidString };
            var result = new List<string> { ShortGuid, ShortGuid };

            Assert.That(guids.ToShortGuids(), Is.EquivalentTo(result));
        }
    }
}
