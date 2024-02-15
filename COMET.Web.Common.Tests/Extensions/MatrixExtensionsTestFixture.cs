// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MatrixExtensionsTestFixture.cs" company="RHEA System S.A.">
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
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Extensions;

    using NUnit.Framework;

    [TestFixture]
    public class MatrixExtensionsTestFixture
    {
        private double[] identity;
        private double[] matrix1;
        private double[] matrix2;

        private const double Delta = 0.01;

        [SetUp]
        public void SetUp()
        {
            this.identity = new double[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            this.matrix1 = new[] { 0.5, 0, 0.8660254, 0, 1, 0, -0.8660254, 0, 0.5 };
            this.matrix2 = new[] { 0.6830127, 0.0540839, 0.7284014, 0.1830127, 0.9527706, -0.2423521, -0.7071068, 0.2988362, 0.6408564 };
        }

        [Test]
        public void VerifyThatAnglesCanBeExtractedFromMatrix1()
        {
            var angles = this.matrix1.ToEulerAngles();
            Assert.That(angles[0], Is.EqualTo(0.0).Within(Delta));
            Assert.That(angles[1], Is.EqualTo(1.0471976).Within(Delta));
            Assert.That(angles[2], Is.EqualTo(0.0).Within(Delta));
        }

        [Test]
        public void VerifyThatAnglesCanBeExtractedFromMatrix2()
        {
            var angles = this.matrix2.ToEulerAngles();
            Assert.That(angles[0], Is.EqualTo(0.4363323).Within(Delta));
            Assert.That(angles[1], Is.EqualTo(0.7853981).Within(Delta));
            Assert.That(angles[2], Is.EqualTo(0.2617994).Within(Delta));
        }

        [Test]
        public void VerifyThatAnglesCanBeExtractedFromMatrix3()
        {
            var angles = this.matrix1.ToEulerAngles(AngleFormat.Degrees);
            Assert.That(angles[0], Is.EqualTo(0.0).Within(Delta));
            Assert.That(angles[1], Is.EqualTo(60.0).Within(Delta));
            Assert.That(angles[2], Is.EqualTo(0.0).Within(Delta));
        }

        [Test]
        public void VerifyThatAnglesCanBeExtractedFromMatrix4()
        {
            var angles = this.matrix2.ToEulerAngles(AngleFormat.Degrees);
            Assert.That(angles[0], Is.EqualTo(25.0).Within(Delta));
            Assert.That(angles[1], Is.EqualTo(45.0).Within(Delta));
            Assert.That(angles[2], Is.EqualTo(15.0).Within(Delta));
        }

        [Test]
        public void VerifyThatIdentityDontChangeOrientation()
        {
            var angles = this.identity.ToEulerAngles();
            Assert.That(angles[0], Is.EqualTo(0.0).Within(Delta));
            Assert.That(angles[1], Is.EqualTo(0.0).Within(Delta));
            Assert.That(angles[2], Is.EqualTo(0.0).Within(Delta));
        }

        [Test]
        public void VerifyThatToEulerAnglesWorks()
        {
            var matrix = new List<double> { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            var invalidMatrix = new List<double> { 1, 0, 0, 0, 1, 0 };
            var matrixBis = new List<double> { 1, 0, 0, 0, 1, 0, 1, 0, 1 };

            var eulerAngles = matrix.ToEulerAngles(AngleFormat.Degrees);
            var eulerAngles2 = matrixBis.ToEulerAngles(AngleFormat.Degrees);

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

        [Test]
        public void VerifyThatToOrientationWorks()
        {
            var positions = new List<double> { 1, 1, 1 };
            var invalidPositions = new List<double> { 1, 1 };
            var matrix = new List<double> { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
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
    }
}
