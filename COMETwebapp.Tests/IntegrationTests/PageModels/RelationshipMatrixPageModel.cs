// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixPageModel.cs" company="Starion Group S.A.">
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
    /// Page object for the Relationship Matrix application (row/column source configuration panel and the matrix grid).
    /// </summary>
    public class RelationshipMatrixPageModel : ApplicationPageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipMatrixPageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        public RelationshipMatrixPageModel(IPage page)
            : base(page)
        {
        }

        /// <summary>
        /// Gets a selector for an element that is only present once this page has rendered.
        /// </summary>
        protected override string LandmarkSelector => "#matrixConfigPanel";

        /// <summary>
        /// Gets the row/column source configuration panel.
        /// </summary>
        public ILocator ConfigurationPanel => this.Page.Locator("#matrixConfigPanel");

        /// <summary>
        /// Gets the strip that is shown in place of the configuration panel once it is minimized, and that restores it.
        /// </summary>
        public ILocator CollapsedConfigurationStrip => this.Page.Locator("#expandMatrixConfigPanel");

        /// <summary>
        /// Gets the content area that holds the matrix grid.
        /// </summary>
        public ILocator MatrixContent => this.Page.Locator(".matrix-content");

        /// <summary>
        /// Minimizes the configuration panel, after which the <see cref="CollapsedConfigurationStrip" /> is shown.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public Task CollapseConfigurationPanelAsync()
        {
            return this.Page.Locator("#matrix-config-minimize").ClickAsync();
        }

        /// <summary>
        /// Restores the configuration panel from its minimized <see cref="CollapsedConfigurationStrip" />.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public Task ExpandConfigurationPanelAsync()
        {
            return this.CollapsedConfigurationStrip.ClickAsync();
        }
    }
}
