// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ApplicationsTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Model
{
    using COMETwebapp.Model;

    using NUnit.Framework;

    [TestFixture]
    public class ApplicationsTestFixture
    {
        [Test]
        public void VerifyApplications()
        {
            var applications = Applications.ExistingApplications;

            Assert.That(applications, Has.Count.EqualTo(13));

            foreach (var application in applications)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(application.Name, Is.Not.Null.Or.Empty);
                    Assert.That(application.Color, Is.Not.Null.Or.Empty);
                    Assert.That(application.Icon, Is.Not.Null.Or.Empty);
                    Assert.That(application.Description, Is.Not.Null.Or.Empty);
                    Assert.That(application.Url, Is.Not.Null.Or.Empty);
                });
            }
        }
    }
}

