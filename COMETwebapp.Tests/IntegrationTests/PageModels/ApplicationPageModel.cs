// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationPageModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.IntegrationTests.PageModels
{
    using System.Threading.Tasks;

    using Microsoft.Playwright;

    /// <summary>
    /// Base class for the per-application page objects. A concrete page model supplies a <see cref="LandmarkSelector" />
    /// and exposes the page's main elements as <see cref="ILocator" /> members so feature tests do not have to
    /// rediscover selectors.
    /// </summary>
    public abstract class ApplicationPageModel : IApplicationPageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationPageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        protected ApplicationPageModel(IPage page)
        {
            this.Page = page;
        }

        /// <summary>
        /// Gets the <see cref="IPage" /> this page object drives.
        /// </summary>
        protected IPage Page { get; }

        /// <summary>
        /// Gets the selector for an element that is only present once this page has rendered.
        /// </summary>
        protected abstract string LandmarkSelector { get; }

        /// <summary>
        /// Gets a locator for an element that is only present once this page has rendered.
        /// </summary>
        public ILocator Landmark => this.Page.Locator(this.LandmarkSelector).First;

        /// <summary>
        /// Gets the main content area of the application shell, which is the element that scrolls when a page does
        /// not fit the viewport.
        /// </summary>
        public ILocator ShellContentArea => this.Page.Locator(".page-layout-item-content");

        /// <summary>
        /// Gets the tab content area that hosts the page introduction box and the application itself.
        /// </summary>
        public ILocator TabContentArea => this.Page.Locator("#tabs-page-content");

        /// <summary>
        /// Gets the number of pixels by which the content of the given element overflows it vertically.
        /// </summary>
        /// <param name="locator">The <see cref="ILocator" /> of the element to measure.</param>
        /// <returns>The vertical overflow, in pixels; zero when the content fits.</returns>
        public static Task<int> GetVerticalOverflowAsync(ILocator locator)
        {
            return locator.EvaluateAsync<int>("element => Math.max(0, element.scrollHeight - element.clientHeight)");
        }

        /// <summary>
        /// Gets the total height of the content of the given element, whether or not it fits.
        /// </summary>
        /// <param name="locator">The <see cref="ILocator" /> of the element to measure.</param>
        /// <returns>The content height, in pixels.</returns>
        public static Task<int> GetContentHeightAsync(ILocator locator)
        {
            return locator.EvaluateAsync<int>("element => element.scrollHeight");
        }
    }
}
