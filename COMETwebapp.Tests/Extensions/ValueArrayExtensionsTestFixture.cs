// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueArrayExtensionsFixture.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;

    using CDP4Common.Types;

    using COMETwebapp.Extensions;

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
