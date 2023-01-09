// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixExtensionsTestFixture.cs" company="RHEA System S.A.">
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
    using DevExpress.Data.Helpers;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;

    [TestFixture]
    public class MatrixExtensionsTestFixture
    {
        [Test]
        public void VerifyThatToOrientationWorks()
        {
            var positions = new List<double>() { 1, 1, 1 };
            var invalidPositions = new List<double>() { 1, 1 };
            var matrix = new List<double>() { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            var invalidMatrix = new List<double> { 1, 1, 1 };

            var positionsResult = positions.ToOrientation(false, Enumerations.AngleFormat.Degrees);            
            var matrixResult = matrix.ToOrientation(true, Enumerations.AngleFormat.Degrees);

            Assert.Multiple(() =>
            {
                Assert.That(positionsResult.X, Is.EqualTo(1));
                Assert.That(positionsResult.Y, Is.EqualTo(1));
                Assert.That(positionsResult.Z, Is.EqualTo(1));

                Assert.That(matrixResult.X, Is.EqualTo(0));
                Assert.That(matrixResult.Y, Is.EqualTo(0));
                Assert.That(matrixResult.Z, Is.EqualTo(0));

                Assert.Throws<ArgumentException>(() => invalidPositions.ToOrientation(true, Enumerations.AngleFormat.Degrees));
                Assert.Throws<ArgumentException>(() => invalidMatrix.ToOrientation(true, Enumerations.AngleFormat.Degrees));
            });
        }

        [Test]
        public void VerifyThatToEulerAnglesWorks()
        {
            var matrix = new List<double>() { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
            var invalidMatrix = new List<double>() { 1, 0, 0, 0, 1, 0 };

            var eulerAngles = matrix.ToEulerAngles(Enumerations.AngleFormat.Degrees);

            Assert.Multiple(() =>
            {
                Assert.That(eulerAngles[0], Is.EqualTo(0));
                Assert.That(eulerAngles[1], Is.EqualTo(0));
                Assert.That(eulerAngles[2], Is.EqualTo(0));
                Assert.Throws<ArgumentException>(() => invalidMatrix.ToEulerAngles(Enumerations.AngleFormat.Degrees));
            });
        }
    }
}
