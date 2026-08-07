// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterEditorPageModel.cs" company="Starion Group S.A.">
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
    /// Page object for the Parameter Editor application (element/parameter-type/category/option selectors and the
    /// parameters grid).
    /// </summary>
    public class ParameterEditorPageModel : ApplicationPageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterEditorPageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        public ParameterEditorPageModel(IPage page)
            : base(page)
        {
        }

        /// <summary>
        /// Gets a selector for an element that is only present once this page has rendered.
        /// </summary>
        protected override string LandmarkSelector => "#parametereditor-body";

        /// <summary>
        /// Gets the parameter editor body container.
        /// </summary>
        public ILocator Body => this.Page.Locator("#parametereditor-body");

        /// <summary>
        /// Gets the parameters table.
        /// </summary>
        public ILocator ParameterTable => this.Page.Locator("#parameter-table");

        /// <summary>
        /// Gets the "View" display-and-filter options cog in the upper-right of the toolbar.
        /// </summary>
        public ILocator ViewMenuButton => this.Body.Locator("[id^='parameterEditorViewMenuButton']");

        /// <summary>
        /// Gets the "Only Parameters owned by … domain" toggle, which lives inside the <see cref="ViewMenuButton" />
        /// dropdown (open the cog before interacting with it).
        /// </summary>
        public ILocator OwnedParametersToggle => this.Page.Locator("#only-owned-parameters-toggle");

        /// <summary>
        /// Gets the element group rows. Each group row is located through the application-owned
        /// <c>data-testid="parameter-group-row"</c> attribute set on its group cell (rather than a DevExpress internal
        /// class), then widened to the whole table row so its expand/collapse button - which lives in a sibling cell -
        /// can be reached.
        /// </summary>
        public ILocator GroupRows => this.Page.Locator("#parameter-table tr:has(td[data-testid=parameter-group-row])");

        /// <summary>
        /// Gets the rows currently rendered in the parameter table (collapsing a group hides its rows).
        /// </summary>
        public ILocator Rows => this.Page.Locator("#parameter-table tr");

        /// <summary>
        /// Clicks the first element group's expand/collapse button (the single button in the group row), which expands or
        /// collapses that group.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public Task ToggleFirstGroupAsync()
        {
            return this.GroupRows.First.Locator("button").First.ClickAsync();
        }
    }
}
