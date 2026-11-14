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
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
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
            string frameOptionsSeenByNext = null;
            string contentSecurityPolicySeenByNext = null;
            string contentTypeOptionsSeenByNext = null;

            app.UseSecurityHeaders();

            // The values are copied here: the next middleware may start the response, after which headers can no
            // longer be added, so the headers must already be set when it runs
            app.Run(context =>
            {
                frameOptionsSeenByNext = context.Response.Headers[HeaderNames.XFrameOptions].ToString();
                contentSecurityPolicySeenByNext = context.Response.Headers[HeaderNames.ContentSecurityPolicy].ToString();
                contentTypeOptionsSeenByNext = context.Response.Headers[HeaderNames.XContentTypeOptions].ToString();
                return Task.CompletedTask;
            });

            await app.Build()(new DefaultHttpContext());

            using (Assert.EnterMultipleScope())
            {
                Assert.That(frameOptionsSeenByNext, Is.EqualTo("SAMEORIGIN"));
                Assert.That(contentSecurityPolicySeenByNext, Is.EqualTo("frame-ancestors 'self'"));
                Assert.That(contentTypeOptionsSeenByNext, Is.EqualTo("nosniff"));
            }
        }

        [Test]
        public async Task VerifyStaticFilesAreServedWithRevalidationAndSecurityHeaders()
        {
            var webRoot = Directory.CreateTempSubdirectory("comet-web-static-");

            try
            {
                await System.IO.File.WriteAllTextAsync(Path.Combine(webRoot.FullName, "app.css"), "body { }");

                using var fileProvider = new PhysicalFileProvider(webRoot.FullName);
                var environment = new Mock<IWebHostEnvironment>();
                environment.Setup(x => x.WebRootFileProvider).Returns(fileProvider);

                var services = new ServiceCollection()
                    .AddSingleton(environment.Object)
                    .AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance)
                    .BuildServiceProvider();

                // The same order as in Program
                var app = new ApplicationBuilder(services);
                app.UseSecurityHeaders();
                app.UseRevalidatedStaticFiles();
                app.Run(context =>
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return Task.CompletedTask;
                });

                var pipeline = app.Build();

                var staticFile = CreateGetRequest("/app.css");
                await pipeline(staticFile);

                var missingFile = CreateGetRequest("/missing.css");
                await pipeline(missingFile);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(staticFile.Response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
                    Assert.That(staticFile.Response.Headers.CacheControl.ToString(), Is.EqualTo("no-cache"));
                    Assert.That(staticFile.Response.Headers.ETag.ToString(), Is.Not.Empty, "the ETag makes the revalidation cheap");
                    Assert.That(staticFile.Response.Headers[HeaderNames.XFrameOptions].ToString(), Is.EqualTo("SAMEORIGIN"));
                    Assert.That(staticFile.Response.Headers[HeaderNames.ContentSecurityPolicy].ToString(), Is.EqualTo("frame-ancestors 'self'"));
                    Assert.That(staticFile.Response.Headers[HeaderNames.XContentTypeOptions].ToString(), Is.EqualTo("nosniff"));

                    Assert.That(missingFile.Response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
                    Assert.That(missingFile.Response.Headers.CacheControl.ToString(), Is.Empty, "only static file responses are given the Cache-Control header");
                }
            }
            finally
            {
                webRoot.Delete(true);
            }
        }

        /// <summary>
        /// Creates the <see cref="HttpContext" /> of a GET request for the given path
        /// </summary>
        /// <param name="path">The requested path</param>
        /// <returns>The <see cref="HttpContext" /></returns>
        private static DefaultHttpContext CreateGetRequest(string path)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethods.Get;
            context.Request.Path = path;
            context.Response.Body = new MemoryStream();
            return context;
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
