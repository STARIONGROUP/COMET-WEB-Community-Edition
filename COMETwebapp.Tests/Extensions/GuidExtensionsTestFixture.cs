// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GuidExtensionsTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
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
    using System.Collections.Generic;
    using COMETwebapp.Extensions;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="GuidExtensions"/> class.
    /// </summary>
    [TestFixture]
    public class GuidExtensionsTestFixture
    {
        private const string guidString = "c9a646d3-9c61-4cb7-bfcd-ee2522c8f633";

        private const string shortGuid = "00amyWGct0y_ze4lIsj2Mw";

        [Test]
        public void Verify_that_ToShortGuid_from_string_returns_expected_result()
        {
            Assert.That(guidString.ToShortGuid(), Is.EqualTo(shortGuid));
        }

        [Test]
        public void Verify_that_ToShortGuid_from_guid_returns_expected_result()
        {
            var guid = Guid.Parse(guidString);

            Assert.That(guid.ToShortGuid(), Is.EqualTo(shortGuid));
        }

        [Test]
        public void Verify_that_FromShortGuid_from_string_returns_expected_result()
        {
            var guid = Guid.Parse(guidString);

            Assert.That(shortGuid.FromShortGuid(), Is.EqualTo(guid));
        }

        [Test]
        public void Verify_ToShortGuids_from_string()
        {
            var guids = new List<string>() { guidString, guidString };
            var result = new List<string>() { shortGuid, shortGuid };

            Assert.That(guids.ToShortGuids(), Is.EquivalentTo(result));
        }

        [Test]
        public void Verify_ToShortGuids_from_guid()
        {
            var guids = new List<Guid>() { Guid.Parse(guidString), Guid.Parse(guidString) };
            var result = new List<string>() { shortGuid, shortGuid };

            Assert.That(guids.ToShortGuids(), Is.EquivalentTo(result));
        }

        [Test]
        public void Verify_Short_Guid_Array()
        {
            var guids = new List<Guid>() { Guid.Parse(guidString), Guid.Parse(guidString) };
            var shortGuidArray = "[" + shortGuid + ";" + shortGuid + "]";
            Assert.That(guids.ToShortGuidArray(), Is.EqualTo(shortGuidArray));
            Assert.That(shortGuidArray.FromShortGuidArray(), Is.EqualTo(guids));
        }
    }
}
