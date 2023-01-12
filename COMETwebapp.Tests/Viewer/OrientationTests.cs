// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrientationTests.cs" company="RHEA System S.A.">
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
    using COMETwebapp.Model;

    using NUnit.Framework;

    /// <summary>
    /// Matrix tests that verifies the correct behavior of the extension methods
    /// </summary>
    [TestFixture]
    public class OrientationTests
    {
        [Test]
        public void VerifyThatMatrixIsComputedFromOrientation()
        {
            var orientation = new Orientation(0, 0, 0);
            Assert.Multiple(() =>
            {
                Assert.That(1.0, Is.EqualTo(orientation.Matrix[0]));
                Assert.That(0.0, Is.EqualTo(orientation.Matrix[1]));
                Assert.That(0.0, Is.EqualTo(orientation.Matrix[2]));
                Assert.That(0.0, Is.EqualTo(orientation.Matrix[3]));
                Assert.That(1.0, Is.EqualTo(orientation.Matrix[4]));
                Assert.That(0.0, Is.EqualTo(orientation.Matrix[5]));
                Assert.That(0.0, Is.EqualTo(orientation.Matrix[6]));
                Assert.That(0.0, Is.EqualTo(orientation.Matrix[7]));
                Assert.That(1.0, Is.EqualTo(orientation.Matrix[8]));
            });
        }

        [Test]
        public void VerifyThatAnglesAreComputedFromOrientation()
        {
            var orientation = new Orientation(1, 2, 3);
            Assert.Multiple(() =>
            {
                Assert.That(1.0, Is.EqualTo(orientation.X));
                Assert.That(2.0, Is.EqualTo(orientation.Y));
                Assert.That(3.0, Is.EqualTo(orientation.Z));
                Assert.That(orientation.Angles.Length, Is.EqualTo(3));
                Assert.That(1.0, Is.EqualTo(orientation.Angles[0]));
                Assert.That(2.0, Is.EqualTo(orientation.Angles[1]));
                Assert.That(3.0, Is.EqualTo(orientation.Angles[2]));
            });
        }
    }
}