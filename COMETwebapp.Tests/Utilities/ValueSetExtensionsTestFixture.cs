// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueSetExtensionsTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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
namespace COMETwebapp.Tests.Utilities
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;
    using COMETwebapp.Utilities;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class ValueSetExtensionsTestFixture
    {
        [Test]
        public void VerifyThatParseToPositionWorks()
        {
            Mock<IValueSet> numericValueSet = new Mock<IValueSet>();
            ValueArray<string> newValueArray1 = new ValueArray<string>(new List<string>() { "1", "2", "3" });
            numericValueSet.Setup(x => x.ActualValue).Returns(newValueArray1);

            Mock<IValueSet> letterValueSet = new Mock<IValueSet>();
            ValueArray<string> newValueArray2 = new ValueArray<string>(new List<string>() { "a", "1", "3" });
            letterValueSet.Setup(x => x.ActualValue).Returns(newValueArray2);

            Mock<IValueSet> emptyValueSet = new Mock<IValueSet>();
            ValueArray<string> newValueArray3 = new ValueArray<string>(new List<string>() { });
            emptyValueSet.Setup(x => x.ActualValue).Returns(newValueArray3);

            var numericResult = numericValueSet.Object.ParseIValueToPosition();            
            var letterResult = letterValueSet.Object.ParseIValueToPosition();

            Assert.Multiple(() =>
            {
                Assert.That(numericResult.Length, Is.EqualTo(3));
                Assert.That(numericResult[0], Is.EqualTo(1));
                Assert.That(numericResult[1], Is.EqualTo(2));
                Assert.That(numericResult[2], Is.EqualTo(3));

                Assert.That(letterResult.Length, Is.EqualTo(3));
                Assert.That(letterResult[0], Is.EqualTo(0));
                Assert.That(letterResult[1], Is.EqualTo(0));
                Assert.That(letterResult[2], Is.EqualTo(0));

                Assert.Throws<ArgumentException>(() => emptyValueSet.Object.ParseIValueToPosition());
            });
        }

        [Test]
        public void VerifyThatParseToOrientationWorks()
        {
            Mock<IValueSet> numericValueSet = new Mock<IValueSet>();
            ValueArray<string> newValueArray1 = new ValueArray<string>(new List<string>() { "1", "2", "3" });
            numericValueSet.Setup(x => x.ActualValue).Returns(newValueArray1);

            var orientation = numericValueSet.Object.ParseIValueToOrientation(Enumerations.AngleFormat.Degrees);

            Assert.Multiple(() =>
            {
                Assert.That(orientation.AngleFormat, Is.EqualTo(Enumerations.AngleFormat.Degrees));
                Assert.That(orientation.X, Is.EqualTo(1));
                Assert.That(orientation.Y, Is.EqualTo(2));
                Assert.That(orientation.Z, Is.EqualTo(3));
            });          
        }

        [Test]
        public void VerifyThatToDoublesWorks()
        {
            Mock<IValueSet> numericValueSet = new Mock<IValueSet>();
            ValueArray<string> newValueArray1 = new ValueArray<string>(new List<string>() { "1", "2", "3" });
            numericValueSet.Setup(x => x.ActualValue).Returns(newValueArray1);

            var result = numericValueSet.Object.ToDoubles(out var list);
            var doubles = list.ToArray();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(result);
                Assert.That(doubles[0], Is.EqualTo(1));
                Assert.That(doubles[1], Is.EqualTo(2));
                Assert.That(doubles[2], Is.EqualTo(3));
            });
        }
    }
}
