// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ResponseHeaderExtensions.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Extensions
{
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Net.Http.Headers;

    /// <summary>
    /// Extension methods for the <see cref="IApplicationBuilder" /> that set the security and caching headers of the
    /// responses of the application
    /// </summary>
    public static class ResponseHeaderExtensions
    {
        /// <summary>
        /// The value of the X-Frame-Options header: the application may only be framed by pages of its own origin
        /// </summary>
        public const string FrameOptions = "SAMEORIGIN";

        /// <summary>
        /// The value of the Content-Security-Policy header: the application may only be framed by pages of its own origin.
        /// Browsers that support it use this instead of the X-Frame-Options header.
        /// </summary>
        public const string ContentSecurityPolicy = "frame-ancestors 'self'";

        /// <summary>
        /// The value of the X-Content-Type-Options header: browsers use the declared content type instead of guessing it
        /// </summary>
        public const string ContentTypeOptions = "nosniff";

        /// <summary>
        /// The value of the Cache-Control header of static files: browsers and proxies may store them, but must
        /// revalidate them with the server, through their ETag, before using them. A new version of the application is
        /// therefore picked up straight away, while an unchanged file costs a 304 response only.
        /// </summary>
        public const string StaticFileCacheControl = "no-cache";

        /// <summary>
        /// Adds the security headers to every response of the application, static files included
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder" /></param>
        /// <returns>The <see cref="IApplicationBuilder" />, for chaining</returns>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                AddSecurityHeaders(context.Response.Headers);
                await next(context);
            });
        }

        /// <summary>
        /// Serves the static files of the application, which browsers and proxies must revalidate before using them
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder" /></param>
        /// <returns>The <see cref="IApplicationBuilder" />, for chaining</returns>
        public static IApplicationBuilder UseRevalidatedStaticFiles(this IApplicationBuilder app)
        {
            return app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context => SetStaticFileCacheControl(context)
            });
        }

        /// <summary>
        /// Adds the security headers that are not set yet
        /// </summary>
        /// <param name="headers">The <see cref="IHeaderDictionary" /> of the response</param>
        public static void AddSecurityHeaders(IHeaderDictionary headers)
        {
            headers.TryAdd(HeaderNames.XFrameOptions, FrameOptions);
            headers.TryAdd(HeaderNames.ContentSecurityPolicy, ContentSecurityPolicy);
            headers.TryAdd(HeaderNames.XContentTypeOptions, ContentTypeOptions);
        }

        /// <summary>
        /// Sets the Cache-Control header of a static file response
        /// </summary>
        /// <param name="context">The <see cref="StaticFileResponseContext" /></param>
        public static void SetStaticFileCacheControl(StaticFileResponseContext context)
        {
            context.Context.Response.Headers.CacheControl = StaticFileCacheControl;
        }
    }
}
