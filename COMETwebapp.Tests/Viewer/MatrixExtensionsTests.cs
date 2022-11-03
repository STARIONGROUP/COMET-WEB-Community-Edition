// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixEstensionsTests.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
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

namespace COMETwebapp.Tests.Viewer
{
    using COMETwebapp.Utilities;

    using NUnit.Framework;

    /// <summary>
    /// Matrix tests that verifies the correct behavior of the extension methods
    /// </summary>
    [TestFixture]
    public class MatrixEstensionsTests
    {
        private double[] identity;
        private double[] matrix1;
        private double[] matrix2;

        private const double delta = 0.01;

        [SetUp]
        public void SetUp()
        {
            identity = new double[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            matrix1 = new double[]  { 0.5, 0, 0.8660254, 0, 1, 0, -0.8660254, 0, 0.5 };
            matrix2 = new double[]  { 0.3894127, -0.4119121, 0.8238241, 0.4119121,  0.8778825,  0.2442349, -0.8238241,  0.2442349,  0.5115302  };
        }

        [Test]
        public void VerifyThatIdentityDontChangeOrientation()
        {
            var angles = identity.ToEulerAngles();
            Assert.AreEqual(0, angles[0], delta);
            Assert.AreEqual(0, angles[1], delta);
            Assert.AreEqual(0, angles[2], delta);
        }

        [Test]
        public void VerifyThatAnglesCanBeExtractedFromMatrix1()
        {
            var angles = matrix1.ToEulerAngles();
            Assert.AreEqual(0, angles[0], delta);
            Assert.AreEqual(1.0471976, angles[1], delta);
            Assert.AreEqual(0, angles[2], delta);
        }

        [Test]
        public void VerifyThatAnglesCanBeExtractedFromMatrix2()
        {
            var angles = matrix2.ToEulerAngles();
            Assert.AreEqual(0.4454531, angles[0], delta);
            Assert.AreEqual(0.9681246, angles[1], delta);
            Assert.AreEqual(0.8134685, angles[2], delta);
        }
    }
}
