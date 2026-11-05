// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVPageModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
//
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.IntegrationTests.PageModels
{
    using System.Threading.Tasks;

    using Microsoft.Playwright;

    using static Microsoft.Playwright.Assertions;

    /// <summary>
    /// Page object for the V&amp;V tab of the Requirement Management application, and for what the tab changes on the
    /// Requirements tab.
    /// </summary>
    public class VandVPageModel : ApplicationPageModel
    {
        /// <summary>
        /// The text of the button that opens the set-up dialog.
        /// </summary>
        private const string SetupButtonText = "Set up V&V...";

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVPageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        public VandVPageModel(IPage page)
            : base(page)
        {
        }

        /// <summary>
        /// Gets a selector for an element that is only present once this page has rendered.
        /// </summary>
        protected override string LandmarkSelector => "#requirementmanagement-body";

        /// <summary>
        /// Gets the empty state shown on a model that is not set up for V&amp;V.
        /// </summary>
        public ILocator EmptyState => this.Page.Locator(".vandv-empty");

        /// <summary>
        /// Gets the button of the empty state that opens the set-up dialog.
        /// </summary>
        public ILocator SetupButton => this.EmptyState.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = SetupButtonText });

        /// <summary>
        /// Gets the confirm button of the set-up dialog.
        /// </summary>
        public ILocator SetupConfirmButton => this.Page.Locator("#vandv-setup-confirm");

        /// <summary>
        /// Gets the list of what the set-up dialog is going to create.
        /// </summary>
        public ILocator SetupMissingList => this.Page.Locator("#vandv-setup-missing");

        /// <summary>
        /// Gets the register.
        /// </summary>
        public ILocator Register => this.Page.Locator(".vandv-register");

        /// <summary>
        /// Gets the item rows of the register.
        /// </summary>
        public ILocator ItemRows => this.Register.Locator(".vandv-row-item");

        /// <summary>
        /// Gets the requirement rows of the register.
        /// </summary>
        public ILocator RequirementRows => this.Register.Locator(".vandv-row-requirement");

        /// <summary>
        /// Gets the specifications in the table of contents of the Requirements tab.
        /// </summary>
        public ILocator RequirementsTabSpecifications => this.Page.Locator(".req-view-pane:not(.req-view-hidden) .req-toc .req-tree-spec");

        /// <summary>
        /// Gets the V&amp;V badges on the requirement rows of the Requirements tab.
        /// </summary>
        public ILocator RequirementsTabBadges => this.Page.Locator(".req-view-pane:not(.req-view-hidden) .req-vandv-badge");

        /// <summary>
        /// Gets the switch of the Requirements tab that shows the V&amp;V data.
        /// </summary>
        public ILocator ShowVandVSwitch => this.Page.Locator("#requirement-show-vandv");

        /// <summary>
        /// Gets the item row of the given short name.
        /// </summary>
        /// <param name="shortName">The short name of the item.</param>
        /// <returns>The row.</returns>
        public ILocator ItemRow(string shortName)
        {
            return this.ItemRows.Filter(new LocatorFilterOptions { HasText = shortName });
        }

        /// <summary>
        /// Gets the field labels of the given row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>The labels.</returns>
        public static ILocator FieldLabels(ILocator row)
        {
            return row.Locator(".vandv-field-label");
        }

        /// <summary>
        /// Gets the rendered button of a view preset in the open view menu.
        /// </summary>
        /// <param name="presetName">The name of the preset.</param>
        /// <returns>The button.</returns>
        public ILocator PresetButton(string presetName)
        {
            return this.Page.Locator(".view-options-menu").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = presetName, Exact = true });
        }

        /// <summary>
        /// Opens a section tab.
        /// </summary>
        /// <param name="tabName">The text of the tab.</param>
        /// <returns>A <see cref="Task" />.</returns>
        public Task OpenSectionTabAsync(string tabName)
        {
            // DevExpress keeps a second, hidden copy of every toolbar item, so only the first one can be clicked
            return this.Page.Locator(".req-view-switch").GetByText(tabName, new LocatorGetByTextOptions { Exact = true }).First.ClickAsync();
        }

        /// <summary>
        /// Opens the set-up dialog from the empty state and confirms it with the default stage gates.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task SetUpAsync()
        {
            await this.SetupButton.ClickAsync();
            await Expect(this.SetupMissingList).ToBeVisibleAsync();
            await this.SetupConfirmButton.ClickAsync();
        }

        /// <summary>
        /// Selects a specification in the table of contents of the register, unless it is selected already: selecting it
        /// again would show all specifications.
        /// </summary>
        /// <param name="shortName">The short name of the specification.</param>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task SelectSpecificationAsync(string shortName)
        {
            var specification = this.Register.Locator(".req-toc .req-tree-spec").Filter(new LocatorFilterOptions { HasText = shortName });
            await Expect(specification).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = E2ETestBase.ServerRoundTripTimeoutMilliseconds });

            if (await this.Register.Locator(".req-toc .req-tree-spec.selected").Filter(new LocatorFilterOptions { HasText = shortName }).CountAsync() == 0)
            {
                await specification.ClickAsync();
            }
        }

        /// <summary>
        /// Opens or closes the view menu of the visible tab.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public Task ToggleViewMenuAsync()
        {
            return this.Page.Locator(".req-view-pane:not(.req-view-hidden) .view-options-button").ClickAsync();
        }

        /// <summary>
        /// Adds a value to the stage gate filter of the register.
        /// </summary>
        /// <param name="stageGate">The stage gate.</param>
        /// <returns>A <see cref="Task" />.</returns>
        public Task AddStageGateFilterAsync(string stageGate)
        {
            return this.AddFilterValueAsync(0, stageGate);
        }

        /// <summary>
        /// Clears the stage gate filter of the register with its clear button, which only shows while the filter is hovered.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task ClearStageGateFilterAsync()
        {
            var filter = this.Register.Locator(".filter-bar-item").First;
            await filter.HoverAsync();

            // the accessible name DevExpress gives its clear button, which the app has no hook on
            await filter.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Clear value" }).ClickAsync();
        }

        /// <summary>
        /// Gets the text input of a filter of the register.
        /// </summary>
        /// <param name="position">The position of the filter in the toolbar.</param>
        /// <returns>The input.</returns>
        private ILocator FilterInput(int position)
        {
            return this.Register.Locator(".filter-bar-item").Nth(position).Locator("input").First;
        }

        /// <summary>
        /// Types a value into a filter of the register and takes it from the list.
        /// </summary>
        /// <param name="position">The position of the filter in the toolbar.</param>
        /// <param name="value">The value to pick.</param>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task AddFilterValueAsync(int position, string value)
        {
            var input = this.FilterInput(position);

            // typing narrows the list down to the value, whose text also appears on the rows, so Enter takes the right one
            await input.ClickAsync();
            await input.PressSequentiallyAsync(value, new LocatorPressSequentiallyOptions { Delay = 30 });
            await input.PressAsync("Enter");
            await input.PressAsync("Escape");
        }
    }
}
