// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensionsTest.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
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
    using NUnit.Framework;
    using System.Drawing;
    using COMETwebapp.Extensions;

    [TestFixture]
    public class StringExtensionsTest
    {
        private const string decodedText = "COMET-WEB-Community-Edition";

        private const string encodedText = "Q09NRVQtV0VCLUNvbW11bml0eS1FZGl0aW9u";

        [Test]
        public void Verify_that_Base64_encode_and_decode_returns_expected_result()
        {
            Assert.That(decodedText.Base64Encode(), Is.EqualTo(encodedText));
            Assert.That(encodedText.Base64Decode(), Is.EqualTo(decodedText));
        }

        [Test]
        public void VerifyThatTextCanBeParsedIntoColor()
        {
            string hexColor = "#444333";
            string rgbColor = "255:15:120";
            string nameColor = "darkred";

            Color hexParsedColor = hexColor.ParseToColor();
            Color rgbParsedColor = rgbColor.ParseToColor();
            Color nameParsedColor = nameColor.ParseToColor();

            Assert.AreEqual(ColorTranslator.FromHtml(hexColor).ToArgb(), hexParsedColor.ToArgb());
            Assert.AreEqual(Color.FromArgb(255, 15, 120).ToArgb(), rgbParsedColor.ToArgb());
            Assert.AreEqual(Color.FromName(nameColor).ToArgb(), nameParsedColor.ToArgb());
        }

        [Test]
        public void VerifyThatTextCanBeParsedIntoHexColor()
        {
            string hexColor = "#ff0000";
            string rgbColor = "255:0:0";
            string nameColor = "Red";

            string hexParsedColor = hexColor.ParseToHexColor();
            string rgbParsedColor = rgbColor.ParseToHexColor();
            string nameParsedColor = nameColor.ParseToHexColor();

            StringAssert.AreEqualIgnoringCase(hexColor, hexParsedColor);
            StringAssert.AreEqualIgnoringCase(hexColor, rgbParsedColor);
            StringAssert.AreEqualIgnoringCase(hexColor, nameParsedColor);
        }

        [Test]
        public void VerifyThatTextRefersToSameColor()
        {
            string hexColor = "#ff0000";
            string rgbColor = "255:0:0";
            string nameColor = "Red";

            Color hexParsedColor = hexColor.ParseToColor();
            Color rgbParsedColor = rgbColor.ParseToColor();
            Color nameParsedColor = nameColor.ParseToColor();

            Assert.AreEqual(hexParsedColor.ToArgb(), rgbParsedColor.ToArgb());
            Assert.AreEqual(hexParsedColor.ToArgb(), nameParsedColor.ToArgb());
            Assert.AreEqual(nameParsedColor.ToArgb(), rgbParsedColor.ToArgb());
        }

    }
}
