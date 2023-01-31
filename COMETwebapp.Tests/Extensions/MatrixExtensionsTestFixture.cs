// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixExtensionsTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
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

namespace COMETwebapp.Tests.Extensions
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;
    using COMETwebapp.Enumerations;
    using COMETwebapp.Utilities;
    using DevExpress.Data.Helpers;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;

    [TestFixture]
    public class MatrixExtensionsTestFixture
    {
        private double[] identity;
        private double[] matrix1;
        private double[] matrix2;

        private const double delta = 0.01;

        [SetUp]
        public void SetUp()
        {
            identity = new double[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            matrix1 = new double[] { 0.5, 0, 0.8660254, 0, 1, 0, -0.8660254, 0, 0.5 };
            matrix2 = new double[] { 0.6830127, 0.0540839, 0.7284014, 0.1830127, 0.9527706, -0.2423521, -0.7071068, 0.2988362, 0.6408564 };
        }

        [Test]
        public void VerifyThatIdentityDontChangeOrientation()
        {
            var angles = identity.ToEulerAngles();
            Assert.AreEqual(0.0, angles[0], delta);
            Assert.AreEqual(0.0, angles[1], delta);
            Assert.AreEqual(0.0, angles[2], delta);
        }

        [Test]
        public void VerifyThatAnglesCanBeExtractedFromMatrix1()
        {
            var angles = matrix1.ToEulerAngles();
            Assert.AreEqual(0.0, angles[0], delta);
            Assert.AreEqual(1.0471976, angles[1], delta);
            Assert.AreEqual(0.0, angles[2], delta);
        }

        [Test]
        public void VerifyThatAnglesCanBeExtractedFromMatrix2()
        {
            var angles = matrix2.ToEulerAngles();
            Assert.AreEqual(0.4363323, angles[0], delta);
            Assert.AreEqual(0.7853981, angles[1], delta);
            Assert.AreEqual(0.2617994, angles[2], delta);
        }

        [Test]
        public void VerifyThatAnglesCanBeExtractedFromMatrix3()
        {
            var angles = matrix1.ToEulerAngles(AngleFormat.Degrees);
            Assert.AreEqual(0.0, angles[0], delta);
            Assert.AreEqual(60.0, angles[1], delta);
            Assert.AreEqual(0.0, angles[2], delta);
        }

        [Test]
        public void VerifyThatAnglesCanBeExtractedFromMatrix4()
        {
            var angles = matrix2.ToEulerAngles(AngleFormat.Degrees);
            Assert.AreEqual(25.0, angles[0], delta);
            Assert.AreEqual(45.0, angles[1], delta);
            Assert.AreEqual(15.0, angles[2], delta);
        }

        [Test]
        public void VerifyThatToOrientationWorks()
        {
            var positions = new List<double>() { 1, 1, 1 };
            var invalidPositions = new List<double>() { 1, 1 };
            var matrix = new List<double>() { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            var invalidMatrix = new List<double> { 1, 1, 1 };

            var positionsResult = positions.ToOrientation(false, AngleFormat.Degrees);
            var matrixResult = matrix.ToOrientation(true, AngleFormat.Degrees);

            Assert.Multiple(() =>
            {
                Assert.That(positionsResult.X, Is.EqualTo(1));
                Assert.That(positionsResult.Y, Is.EqualTo(1));
                Assert.That(positionsResult.Z, Is.EqualTo(1));

                Assert.That(matrixResult.X, Is.EqualTo(0));
                Assert.That(matrixResult.Y, Is.EqualTo(0));
                Assert.That(matrixResult.Z, Is.EqualTo(0));

                Assert.Throws<ArgumentException>(() => invalidPositions.ToOrientation(false, AngleFormat.Degrees));
                Assert.Throws<ArgumentException>(() => invalidMatrix.ToOrientation(true, AngleFormat.Degrees));
            });
        }

        [Test]
        public void VerifyThatToEulerAnglesWorks()
        {
            var matrix = new List<double>() { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            var invalidMatrix = new List<double>() { 1, 0, 0, 0, 1, 0 };
            var matrix2 = new List<double>() { 1, 0, 0, 0, 1, 0, 1, 0, 1 };

            var eulerAngles = matrix.ToEulerAngles(AngleFormat.Degrees);
            var eulerAngles2 = matrix2.ToEulerAngles(AngleFormat.Degrees);

            Assert.Multiple(() =>
            {
                Assert.That(eulerAngles, Has.Length.EqualTo(3));
                Assert.That(eulerAngles[0], Is.EqualTo(0));
                Assert.That(eulerAngles[1], Is.EqualTo(0));
                Assert.That(eulerAngles[2], Is.EqualTo(0));
                Assert.That(eulerAngles, Has.Length.EqualTo(3));
                Assert.That(eulerAngles2[0], Is.EqualTo(-180));
                Assert.That(eulerAngles2[1], Is.EqualTo(-90));
                Assert.That(eulerAngles2[2], Is.EqualTo(0));

                Assert.Throws<ArgumentException>(() => invalidMatrix.ToEulerAngles(AngleFormat.Degrees));
            });
        }
    }
}
