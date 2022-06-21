// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensionsTest.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
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

namespace COMETwebapp.Tests.Utilities
{
    using NUnit.Framework;
    using COMETwebapp.Utilities;


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
    }
}
