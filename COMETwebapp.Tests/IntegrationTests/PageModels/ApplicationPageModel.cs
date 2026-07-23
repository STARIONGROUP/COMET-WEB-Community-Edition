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
    }
}
