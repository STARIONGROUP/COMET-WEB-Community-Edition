// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DefaultNaturalLanguagesTestFixture.cs" company="Starion Group S.A.">
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
    using System.Linq;

    using COMETwebapp.Utilities;

    using NUnit.Framework;

    [TestFixture]
    public class DefaultNaturalLanguagesTestFixture
    {
        [Test]
        public void VerifyDefaultLanguagesAreLoadedFromTheEmbeddedResource()
        {
            var languages = DefaultNaturalLanguages.All;

            Assert.Multiple(() =>
            {
                Assert.That(languages, Has.Count.GreaterThan(100), "The bundled IME language set must be available even under InvariantGlobalization.");
                Assert.That(languages.Select(x => x.LanguageCode), Does.Contain("en"));
                Assert.That(languages.Single(x => x.LanguageCode == "en").Name, Is.EqualTo("English"));
                Assert.That(languages.Single(x => x.LanguageCode == "fr").NativeName, Is.EqualTo("français"), "The native name (correct alphabet) must be loaded.");
                Assert.That(languages.Select(x => x.LanguageCode), Is.Unique);
            });
        }
    }
}
