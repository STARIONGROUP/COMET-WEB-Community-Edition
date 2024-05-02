// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="StringExtensionsTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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
    using System.Drawing;

    using COMET.Web.Common.Extensions;

    using NUnit.Framework;

    [TestFixture]
    public class StringExtensionsTestFixture
    {
        [Test]
        public void VerifyThatTextCanBeParsedIntoColor()
        {
            const string hexColor = "#444333";
            const string rgbColor = "255:15:120";
            const string nameColor = "darkred";

            var hexParsedColor = hexColor.ParseToColor();
            var rgbParsedColor = rgbColor.ParseToColor();
            var nameParsedColor = nameColor.ParseToColor();

            Assert.Multiple(() =>
            {
                Assert.That(hexParsedColor.ToArgb(), Is.EqualTo(ColorTranslator.FromHtml(hexColor).ToArgb()));
                Assert.That(rgbParsedColor.ToArgb(), Is.EqualTo(Color.FromArgb(255, 15, 120).ToArgb()));
                Assert.That(nameParsedColor.ToArgb(), Is.EqualTo(Color.FromName(nameColor).ToArgb()));
            });
        }

        [Test]
        public void VerifyThatTextCanBeParsedIntoHexColor()
        {
            const string hexColor = "#ff0000";
            const string rgbColor = "255:0:0";
            const string nameColor = "Red";

            var hexParsedColor = hexColor.ParseToHexColor();
            var rgbParsedColor = rgbColor.ParseToHexColor();
            var nameParsedColor = nameColor.ParseToHexColor();

            Assert.Multiple(() =>
            {
               Assert.That(string.Equals(hexParsedColor,hexColor, StringComparison.InvariantCultureIgnoreCase));
               Assert.That(string.Equals(rgbParsedColor, hexColor, StringComparison.InvariantCultureIgnoreCase));
               Assert.That(string.Equals(nameParsedColor,hexColor, StringComparison.InvariantCultureIgnoreCase));
            });
        }

        [Test]
        public void VerifyThatTextRefersToSameColor()
        {
            const string hexColor = "#ff0000";
            const string rgbColor = "255:0:0";
            const string nameColor = "Red";

            var hexParsedColor = hexColor.ParseToColor();
            var rgbParsedColor = rgbColor.ParseToColor();
            var nameParsedColor = nameColor.ParseToColor();

            Assert.Multiple(() =>
            {
                Assert.That(rgbParsedColor.ToArgb(), Is.EqualTo(hexParsedColor.ToArgb()));
                Assert.That(nameParsedColor.ToArgb(), Is.EqualTo(hexParsedColor.ToArgb()));
                Assert.That(rgbParsedColor.ToArgb(), Is.EqualTo(nameParsedColor.ToArgb()));
            });
        }
    }
}
