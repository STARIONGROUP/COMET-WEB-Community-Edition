// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NotificationDescriptionBuilderTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Utilities
{
    using COMETwebapp.Utilities;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="NotificationDescriptionBuilder" />
    /// </summary>
    [TestFixture]
    public class NotificationDescriptionBuilderTestFixture
    {
        [Test]
        public void VerifyBuild()
        {
            var deleted = NotificationDescriptionBuilder.Build("Change Request", "CR-1", "deleted", "delete");
            var annotated = NotificationDescriptionBuilder.Build("Requirement", "R1", "annotated with a Change Request", "add the Change Request to");
            var unnamed = NotificationDescriptionBuilder.Build("Note", string.Empty, "deleted", "delete");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(deleted.OnSuccess, Is.EqualTo("Change Request 'CR-1' deleted"));
                Assert.That(deleted.OnError, Is.EqualTo("Could not delete the Change Request 'CR-1'"));
                Assert.That(annotated.OnSuccess, Is.EqualTo("Requirement 'R1' annotated with a Change Request"));
                Assert.That(annotated.OnError, Is.EqualTo("Could not add the Change Request to the Requirement 'R1'"));
                Assert.That(unnamed.OnSuccess, Is.EqualTo("Note '' deleted"), "an empty label is quoted as is");
            }
        }
    }
}
