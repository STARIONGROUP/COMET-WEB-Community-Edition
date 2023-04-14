// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ValueSetExtensionsTestFixture.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Extensions;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ValueSetExtensionsTestFixture
    {
        [Test]
        public void VerifyThatParseToOrientationWorks()
        {
            var numericValueSet = new Mock<IValueSet>();
            var newValueArray1 = new ValueArray<string>(new List<string> { "1", "2", "3" });
            numericValueSet.Setup(x => x.ActualValue).Returns(newValueArray1);
            var orientation = numericValueSet.Object.ParseIValueToOrientation(AngleFormat.Degrees);

            Assert.Multiple(() =>
            {
                Assert.That(orientation.AngleFormat, Is.EqualTo(AngleFormat.Degrees));
                Assert.That(orientation.X, Is.EqualTo(1));
                Assert.That(orientation.Y, Is.EqualTo(2));
                Assert.That(orientation.Z, Is.EqualTo(3));
            });
        }

        [Test]
        public void VerifyThatParseToOrientationWorksWithWrongData()
        {
            var numericValueSet = new Mock<IValueSet>();
            var newValueArray1 = new ValueArray<string>(new List<string> { "1", "A", "3" });
            numericValueSet.Setup(x => x.ActualValue).Returns(newValueArray1);

            var orientation = numericValueSet.Object.ParseIValueToOrientation(AngleFormat.Degrees);

            Assert.Multiple(() =>
            {
                Assert.That(orientation.AngleFormat, Is.EqualTo(AngleFormat.Degrees));
                Assert.That(orientation.X, Is.EqualTo(0));
                Assert.That(orientation.Y, Is.EqualTo(0));
                Assert.That(orientation.Z, Is.EqualTo(0));
            });
        }

        [Test]
        public void VerifyThatParseToPositionWorks()
        {
            var numericValueSet = new Mock<IValueSet>();
            var newValueArray1 = new ValueArray<string>(new List<string> { "1", "2", "3" });
            numericValueSet.Setup(x => x.ActualValue).Returns(newValueArray1);

            var letterValueSet = new Mock<IValueSet>();
            var newValueArray2 = new ValueArray<string>(new List<string> { "a", "1", "3" });
            letterValueSet.Setup(x => x.ActualValue).Returns(newValueArray2);

            var emptyValueSet = new Mock<IValueSet>();
            var newValueArray3 = new ValueArray<string>(new List<string>());
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
        public void VerifyThatToDoublesWorks()
        {
            var numericValueSet = new Mock<IValueSet>();
            var newValueArray1 = new ValueArray<string>(new List<string> { "1", "2", "3" });
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
