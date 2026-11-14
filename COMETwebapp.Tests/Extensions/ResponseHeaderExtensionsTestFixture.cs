// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponseHeaderExtensionsTestFixture.cs" company="Starion Group S.A.">
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
    using COMETwebapp.Extensions;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Net.Http.Headers;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ResponseHeaderExtensionsTestFixture
    {
        [Test]
        public void VerifyAddSecurityHeadersSetsTheMissingHeaders()
        {
            var headers = new HeaderDictionary();

            ResponseHeaderExtensions.AddSecurityHeaders(headers);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(headers[HeaderNames.XFrameOptions].ToString(), Is.EqualTo("SAMEORIGIN"));
                Assert.That(headers[HeaderNames.ContentSecurityPolicy].ToString(), Is.EqualTo("frame-ancestors 'self'"));
                Assert.That(headers[HeaderNames.XContentTypeOptions].ToString(), Is.EqualTo("nosniff"));
            }
        }

        [Test]
        public void VerifyAddSecurityHeadersKeepsTheHeadersThatAreAlreadySet()
        {
            var headers = new HeaderDictionary
            {
                [HeaderNames.XFrameOptions] = "DENY",
                [HeaderNames.ContentSecurityPolicy] = "default-src 'self'"
            };

            ResponseHeaderExtensions.AddSecurityHeaders(headers);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(headers[HeaderNames.XFrameOptions].ToString(), Is.EqualTo("DENY"));
                Assert.That(headers[HeaderNames.ContentSecurityPolicy].ToString(), Is.EqualTo("default-src 'self'"));
                Assert.That(headers[HeaderNames.XContentTypeOptions].ToString(), Is.EqualTo("nosniff"));
            }
        }

        [Test]
        public async Task VerifyUseSecurityHeadersAddsTheHeadersBeforeTheNextMiddlewareRuns()
        {
            var app = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());
            IHeaderDictionary headersSeenByNext = null;

            app.UseSecurityHeaders();

            app.Run(context =>
            {
                headersSeenByNext = context.Response.Headers;
                return Task.CompletedTask;
            });

            var context = new DefaultHttpContext();
            await app.Build()(context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(headersSeenByNext, Is.Not.Null);
                Assert.That(context.Response.Headers[HeaderNames.XFrameOptions].ToString(), Is.EqualTo("SAMEORIGIN"));
                Assert.That(context.Response.Headers[HeaderNames.ContentSecurityPolicy].ToString(), Is.EqualTo("frame-ancestors 'self'"));
                Assert.That(context.Response.Headers[HeaderNames.XContentTypeOptions].ToString(), Is.EqualTo("nosniff"));
            }
        }

        [Test]
        public void VerifySetStaticFileCacheControlRequiresRevalidation()
        {
            var httpContext = new DefaultHttpContext();
            var responseContext = new StaticFileResponseContext(httpContext, new Mock<IFileInfo>().Object);

            ResponseHeaderExtensions.SetStaticFileCacheControl(responseContext);

            Assert.That(httpContext.Response.Headers.CacheControl.ToString(), Is.EqualTo("no-cache"));
        }
    }
}
