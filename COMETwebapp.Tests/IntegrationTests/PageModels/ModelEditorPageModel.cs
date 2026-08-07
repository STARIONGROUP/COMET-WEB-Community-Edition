// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelEditorPageModel.cs" company="Starion Group S.A.">
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
    /// Page object for the Model Editor application (source model panel, target model panel and element details panel).
    /// </summary>
    public class ModelEditorPageModel : ApplicationPageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelEditorPageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        public ModelEditorPageModel(IPage page)
            : base(page)
        {
        }

        /// <summary>
        /// Gets a selector for an element that is only present once this page has rendered.
        /// </summary>
        protected override string LandmarkSelector => "#sourcePanel";

        /// <summary>
        /// Gets the source model panel.
        /// </summary>
        public ILocator SourcePanel => this.Page.Locator("#sourcePanel");

        /// <summary>
        /// Gets the target model panel.
        /// </summary>
        public ILocator TargetPanel => this.Page.Locator("#targetPanel");

        /// <summary>
        /// Gets the element details panel.
        /// </summary>
        public ILocator DetailsPanel => this.Page.Locator("#detailsPanel");

        /// <summary>
        /// Gets the "View" display-options cog in the upper-right of the toolbar (holds the Owner / Categories toggles).
        /// </summary>
        public ILocator ViewMenuButton => this.Page.Locator("[id^='modelEditorViewMenuButton']");

        /// <summary>
        /// Gets the element search text box.
        /// </summary>
        public ILocator SearchBox => this.Page.Locator("#search-textbox");

        /// <summary>
        /// Gets the element rows of the source model tree, targeted by the application-owned <c>data-testid</c> attribute
        /// set on each tree item rather than a DevExpress internal class.
        /// </summary>
        public ILocator SourceElements => this.Page.Locator("#sourcePanel [data-testid=element-node]");

        /// <summary>
        /// Selects the first element in the source model tree, which opens it in the details panel.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public Task SelectFirstSourceElementAsync()
        {
            return this.SourceElements.First.ClickAsync();
        }
    }
}
