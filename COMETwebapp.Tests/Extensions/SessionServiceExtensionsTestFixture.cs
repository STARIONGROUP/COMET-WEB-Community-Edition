// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionServiceExtensionsTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Extensions
{
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Extensions;
    using COMETwebapp.Utilities;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture for <see cref="SessionServiceExtensions" />.
    /// </summary>
    [TestFixture]
    public class SessionServiceExtensionsTestFixture
    {
        private Mock<ISessionService> sessionService;

        [SetUp]
        public void SetUp()
        {
            this.sessionService = new Mock<ISessionService>();
        }

        [Test]
        public void VerifyGetAvailableNaturalLanguages()
        {
            var siteDirectory = new SiteDirectory();
            siteDirectory.NaturalLanguage.Add(new NaturalLanguage { LanguageCode = "custom", Name = "Custom Language" });

            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            var languages = this.sessionService.Object.GetAvailableNaturalLanguages();

            Assert.Multiple(() =>
            {
                Assert.That(languages, Is.Not.Empty);
                Assert.That(languages.Any(x => x.LanguageCode == "custom"), Is.True);
            });
        }

        [Test]
        public void VerifyGetAvailableNaturalLanguagesWithNullSiteDirectory()
        {
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns((SiteDirectory)null);
            var languages = this.sessionService.Object.GetAvailableNaturalLanguages();

            Assert.Multiple(() =>
            {
                Assert.That(languages, Is.Not.Empty);
                Assert.That(languages, Has.Count.EqualTo(DefaultNaturalLanguages.All.Count));
            });
        }
    }
}
