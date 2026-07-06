// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResultExtensionsTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
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
    using System;

    using COMETwebapp.Extensions;

    using FluentResults;

    using NUnit.Framework;

    [TestFixture]
    public class ResultExtensionsTestFixture
    {
        [Test]
        public void VerifyGetHtmlErrorsDescriptionToleratesBackslashPaths()
        {
            // An exception whose text contains a Windows path (e.g. "\Users") must not be treated as a regex
            // pattern — that raised a RegexParseException ("Unrecognized escape sequence \U") and crashed the
            // error toast, hiding the real error.
            var exception = new InvalidOperationException(@"Failure at C:\Users\someone\Unrecognized");
            var result = Result.Fail(new ExceptionalError("boom", exception));

            string html = null;

            Assert.Multiple(() =>
            {
                Assert.That(() => html = result.GetHtmlErrorsDescription(), Throws.Nothing);
                Assert.That(html, Does.Contain("boom"));
            });
        }
    }
}
