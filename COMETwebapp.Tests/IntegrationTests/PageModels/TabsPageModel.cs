// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TabsPageModel.cs" company="Starion Group S.A.">
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
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Microsoft.Playwright;

    /// <summary>
    /// Page object for the Tabs application (the tabbed shell). Drives the "Open Tab" card: pick a View (the
    /// application), a model/domain/iteration, then open the tab so the application's real content renders.
    /// </summary>
    public class TabsPageModel
    {
        /// <summary>
        /// The <see cref="IPage" /> this page object drives.
        /// </summary>
        private readonly IPage page;

        /// <summary>
        /// Initializes a new instance of the <see cref="TabsPageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        public TabsPageModel(IPage page)
        {
            this.page = page;
        }

        /// <summary>
        /// Opens the given application as a tab from the home "Open Tab" card (the card is shown on the home page once
        /// authenticated). Selects it as the View, then the first available model/domain/iteration (only those the
        /// selected view actually requires), then opens the tab and waits for the tab content to replace the card.
        /// </summary>
        /// <remarks>
        /// The flow stays within the authenticated Blazor circuit — a full-page navigation to <c>/Tabs</c> would start
        /// a fresh, unauthenticated circuit and bounce back to the login page.
        /// </remarks>
        /// <param name="applicationName">The application display name, e.g. "Model Editor".</param>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task OpenApplicationTabAsync(string applicationName)
        {
            await this.EnsureOpenTabFormVisibleAsync();

            await this.SelectComboItemAsync("view-selection", applicationName);

            // The View combo commits its value on lost focus; blur it so the conditional model/domain/iteration
            // selectors settle into their final shape before we read them.
            await this.page.Locator("input[name=view-selection]").BlurAsync();
            await this.page.WaitForTimeoutAsync(800);

            // The model/domain/iteration selectors are rendered conditionally depending on the selected view, so only
            // fill the ones that appear.
            await this.SelectFirstItemIfPresentAsync("model-selection");
            await this.SelectFirstItemIfPresentAsync("domain-selection");
            await this.SelectFirstItemIfPresentAsync("iteration-selection");

            var openTabButton = this.page.Locator("#opentab__button");
            await openTabButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            // Continuous Blazor re-renders of this card defeat Playwright's click-stability check, so dispatch the
            // click directly instead of relying on actionability.
            await openTabButton.DispatchEventAsync("click");

            // Once a tab is open the tab content replaces the "Open Tab" selection card.
            await this.page.Locator("#view-selection").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached, Timeout = 30_000 });
        }

        /// <summary>
        /// Gets a value indicating whether the Blazor unhandled-error banner is currently visible, i.e. the opened
        /// application crashed while rendering.
        /// </summary>
        /// <returns><c>true</c> if the error banner is visible.</returns>
        public Task<bool> HasBlazorErrorAsync()
        {
            return this.page.Locator("#blazor-error-ui").IsVisibleAsync();
        }

        /// <summary>
        /// Ensures the "Open Tab" selection card is visible, opening it from the tab bar's "+" entry when a tab is
        /// already open.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task EnsureOpenTabFormVisibleAsync()
        {
            var viewSelection = this.page.Locator("#view-selection");

            if (await viewSelection.IsVisibleAsync())
            {
                return;
            }

            await this.page.Locator("#open-new-tab").ClickAsync();
            await viewSelection.WaitForAsync();
        }

        /// <summary>
        /// Opens a DevExpress combo box by id and selects the item whose text matches exactly.
        /// </summary>
        /// <param name="comboId">The combo box component id.</param>
        /// <param name="exactText">The exact item text to select.</param>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task SelectComboItemAsync(string comboId, string exactText)
        {
            await this.OpenComboAsync(comboId);

            var item = this.page.Locator(".dxbl-listbox-item")
                .Filter(new LocatorFilterOptions { HasTextRegex = new Regex($"^\\s*{Regex.Escape(exactText)}\\s*$") });

            await item.First.ClickAsync();
        }

        /// <summary>
        /// Opens a DevExpress combo box by id and selects its first item, but only if the combo box is present.
        /// </summary>
        /// <param name="comboId">The combo box component id.</param>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task SelectFirstItemIfPresentAsync(string comboId)
        {
            var combo = this.page.Locator($"#{comboId}");

            try
            {
                await combo.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 3000 });
            }
            catch (TimeoutException)
            {
                return;
            }

            if (!await this.OpenComboAsync(comboId))
            {
                return;
            }

            await this.page.Locator(".dxbl-listbox-item").First.ClickAsync();
        }

        /// <summary>
        /// Clicks a DevExpress combo box open and waits for its list to appear, retrying a few times because the first
        /// click occasionally does not open the drop-down.
        /// </summary>
        /// <param name="comboId">The combo box component id.</param>
        /// <returns><c>true</c> if the drop-down opened.</returns>
        private async Task<bool> OpenComboAsync(string comboId)
        {
            var items = this.page.Locator(".dxbl-listbox-item");

            for (var attempt = 0; attempt < 3; attempt++)
            {
                await this.page.Locator($"#{comboId}").ClickAsync();

                try
                {
                    await items.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 4000 });
                    return true;
                }
                catch (TimeoutException)
                {
                }
            }

            return false;
        }
    }
}
