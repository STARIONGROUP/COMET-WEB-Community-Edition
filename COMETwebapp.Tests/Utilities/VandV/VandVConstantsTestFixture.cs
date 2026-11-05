// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVConstantsTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
//
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Utilities.VandV
{
    using COMETwebapp.Utilities.VandV;

    using NUnit.Framework;

    [TestFixture]
    public class VandVConstantsTestFixture
    {
        [Test]
        public void VerifyNormalizeValue()
        {
            Assert.Multiple(() =>
            {
                Assert.That(VandVConstants.NormalizeValue(null), Is.Empty);
                Assert.That(VandVConstants.NormalizeValue("   "), Is.Empty);
                Assert.That(VandVConstants.NormalizeValue("Non-Compliant"), Is.EqualTo("noncompliant"));
                Assert.That(VandVConstants.NormalizeValue(" Non_Compliant "), Is.EqualTo("noncompliant"));
                Assert.That(VandVConstants.NormalizeValue("Further V&V Required"), Is.EqualTo("furthervvrequired"));
            });
        }

        [Test]
        public void VerifyValueEquals()
        {
            Assert.Multiple(() =>
            {
                Assert.That(VandVConstants.ValueEquals(VandVConstants.ComplianceNonCompliant, "Non_Compliant"), Is.True);
                Assert.That(VandVConstants.ValueEquals(VandVConstants.NotApplicable, "not_applicable"), Is.True);
                Assert.That(VandVConstants.ValueEquals(VandVConstants.ClosureFurtherRequired, "Further_VV_Required"), Is.True);
                Assert.That(VandVConstants.ValueEquals(VandVConstants.StatusInProgress, "In_Progress"), Is.True);
                Assert.That(VandVConstants.ValueEquals(null, string.Empty), Is.True);
                Assert.That(VandVConstants.ValueEquals(VandVConstants.ComplianceCompliant, VandVConstants.ComplianceNonCompliant), Is.False);
                Assert.That(VandVConstants.ValueEquals(VandVConstants.StatusPassed, null), Is.False);
            });
        }

        [Test]
        public void VerifyToShortName()
        {
            Assert.Multiple(() =>
            {
                Assert.That(VandVConstants.ToShortName(null), Is.Empty);
                Assert.That(VandVConstants.ToShortName(" "), Is.Empty);
                Assert.That(VandVConstants.ToShortName("In-Service"), Is.EqualTo("In_Service"));
                Assert.That(VandVConstants.ToShortName("Review of Design"), Is.EqualTo("Review_of_Design"));
                Assert.That(VandVConstants.ToShortName("Further V&V Required"), Is.EqualTo("Further_VV_Required"));
                Assert.That(VandVConstants.ToShortName("REQ.1/a"), Is.EqualTo("REQ1a"));
            });
        }
    }
}
