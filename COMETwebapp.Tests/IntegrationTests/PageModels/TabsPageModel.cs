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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using COMETwebapp.Model;

    using Microsoft.Playwright;

    using static Microsoft.Playwright.Assertions;

    /// <summary>
    /// Page object for the Tabs application (the tabbed shell). Drives the "Open Tab" card: pick a View (the
    /// application), the model/domain/iteration the view requires, then open the tab so the application's real content
    /// renders.
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
        /// Gets the Blazor unhandled-error banner (hidden unless the open application crashed while rendering).
        /// </summary>
        public ILocator BlazorError => this.page.Locator("#blazor-error-ui");

        /// <summary>
        /// Gets the items of the currently open combo drop-down. Each item's content carries the application-owned
        /// <c>data-testid="combo-item"</c> attribute (set by every combo's <c>ItemTemplate</c>), so this does not depend
        /// on a DevExpress internal class. Only the open combo renders its items, and <see cref="OpenComboAsync" /> waits
        /// for the previous drop-down to close before opening the next, so this never matches the fading-out items of a
        /// just-closed one.
        /// </summary>
        private ILocator OpenDropdownItems => this.page.Locator("[data-testid=combo-item]");

        /// <summary>
        /// Opens the given application as a tab from the home "Open Tab" card. Selects it as the View, then the
        /// model/domain/iteration the view requires, then opens the tab and waits for the tab content to replace the card.
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

            // The View combo commits on lost focus, which is what makes the model/domain/iteration selectors this view
            // needs appear; blur it, then fill exactly those selectors.
            await this.page.Locator("input[name=view-selection]").BlurAsync();

            foreach (var selector in RequiredSelectorsFor(applicationName))
            {
                await this.SelectFirstComboItemAsync(selector);
            }

            await this.page.Locator("#opentab__button").ClickAsync();

            // Opening a tab loads the selected iteration from the COMET server, a network round-trip that can be slow on
            // CI. Once it completes the tab content replaces the "Open Tab" selection card.
            await Expect(this.page.Locator("#view-selection")).ToBeHiddenAsync(new LocatorAssertionsToBeHiddenOptions { Timeout = E2ETestBase.ServerRoundTripTimeoutMilliseconds });
        }

        /// <summary>
        /// Gets the ids of the model/domain/iteration selectors the given application's view requires, derived from the
        /// application's thing-of-interest (iteration views need all three, engineering-model views need the first two,
        /// and server/reference views need none).
        /// </summary>
        /// <param name="applicationName">The application display name.</param>
        /// <returns>The required selector ids, in the order they must be filled.</returns>
        private static IReadOnlyList<string> RequiredSelectorsFor(string applicationName)
        {
            var application = Applications.ExistingApplications
                .OfType<TabbedApplication>()
                .FirstOrDefault(app => app.Name == applicationName);

            return application?.ThingTypeOfInterest?.Name switch
            {
                "Iteration" => ["model-selection", "domain-selection", "iteration-selection"],
                "EngineeringModel" => ["model-selection", "domain-selection"],
                _ => []
            };
        }

        /// <summary>
        /// Ensures the "Open Tab" selection card is visible, reopening it from the tab bar's "+" entry when a tab is
        /// already open.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task EnsureOpenTabFormVisibleAsync()
        {
            var openNewTab = this.page.Locator("#open-new-tab");

            if (await openNewTab.IsVisibleAsync())
            {
                await openNewTab.ClickAsync();
            }

            await Expect(this.page.Locator("#view-selection")).ToBeVisibleAsync();
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

            await this.OpenDropdownItems
                .Filter(new LocatorFilterOptions { HasTextRegex = new Regex($"^\\s*{Regex.Escape(exactText)}\\s*$") })
                .First.ClickAsync();
        }

        /// <summary>
        /// Waits for a DevExpress combo box to be shown, opens it and selects its first item.
        /// </summary>
        /// <param name="comboId">The combo box component id.</param>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task SelectFirstComboItemAsync(string comboId)
        {
            await Expect(this.page.Locator($"#{comboId}")).ToBeVisibleAsync();
            await this.OpenComboAsync(comboId);
            await this.OpenDropdownItems.First.ClickAsync();
        }

        /// <summary>
        /// Clicks a combo box open and waits for its list to appear. It first waits for the open-tab form's
        /// application-owned <c>data-app-ready</c> marker and for any previous drop-down to have fully closed (so no stale
        /// item is matched). The application-owned marker signals the combos have rendered but cannot prove DevExpress has
        /// finished attaching their client-side scripts, so on a cold or heavily loaded circuit a first click can land
        /// before the handler is wired and silently fail to open the drop-down; the click is therefore retried until the
        /// items appear, re-clicking only while nothing is open so an already-open drop-down is never toggled shut.
        /// </summary>
        /// <param name="comboId">The combo box component id.</param>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task OpenComboAsync(string comboId)
        {
            await Expect(this.page.Locator("#open-tab-form[data-app-ready]")).ToBeAttachedAsync(new LocatorAssertionsToBeAttachedOptions { Timeout = E2ETestBase.ServerRoundTripTimeoutMilliseconds });
            await Expect(this.OpenDropdownItems).ToHaveCountAsync(0);

            var combo = this.page.Locator($"#{comboId}");

            for (var attempt = 0; attempt < ComboOpenAttempts; attempt++)
            {
                if (await this.OpenDropdownItems.CountAsync() == 0)
                {
                    await combo.ClickAsync();
                }

                try
                {
                    await Expect(this.OpenDropdownItems.First).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = ComboOpenAttemptTimeoutMilliseconds });
                    return;
                }
                catch (PlaywrightException) when (attempt < ComboOpenAttempts - 1)
                {
                    // The DevExpress client script had not attached when the click landed, so the drop-down did not open;
                    // retry on the next iteration.
                }
            }
        }

        /// <summary>
        /// The number of times <see cref="OpenComboAsync" /> retries opening a combo box before giving up, to absorb the
        /// gap between the form's readiness marker and DevExpress attaching the combo's client-side click handler.
        /// </summary>
        private const int ComboOpenAttempts = 5;

        /// <summary>
        /// The per-attempt timeout, in milliseconds, that <see cref="OpenComboAsync" /> waits for a combo's items to
        /// appear before re-clicking. Kept short so a click that missed the handler is retried quickly.
        /// </summary>
        private const int ComboOpenAttemptTimeoutMilliseconds = 3_000;
    }
}
