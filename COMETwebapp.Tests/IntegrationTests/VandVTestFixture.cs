// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.IntegrationTests
{
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using COMETwebapp.Tests.IntegrationTests.PageModels;

    using Microsoft.Playwright;

    using NUnit.Framework;

    using static Microsoft.Playwright.Assertions;

    /// <summary>
    /// End-to-end tests of the V&amp;V tab of the Requirement Management application. Set-up writes reference data to the
    /// model, which cannot be undone, and only runs on a model that is not set up yet, which is what a fresh end-to-end
    /// database holds. The other tests read a specification of their own, retired again afterwards.
    /// </summary>
    [TestFixture]
    public class VandVTestFixture : ApplicationPageTestBase<VandVPageModel>
    {
        /// <summary>
        /// The name of the V&amp;V section tab.
        /// </summary>
        private const string VandVTab = "V&V";

        /// <summary>
        /// The name of the Requirements section tab.
        /// </summary>
        private const string RequirementsTab = "Requirements";

        /// <summary>
        /// The data the tests read, written once the model is set up.
        /// </summary>
        private VandVEndToEndData data;

        /// <summary>
        /// Gets the display name of the application under test, exactly as it appears in the "View" selector.
        /// </summary>
        protected override string ApplicationName => "Requirement Management";

        /// <summary>
        /// Builds the page object for the application under test.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        /// <returns>The page object.</returns>
        protected override VandVPageModel CreatePageModel(IPage page) => new(page);

        /// <summary>
        /// Retires the data the tests read.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [OneTimeTearDown]
        public async Task RetireDataAsync()
        {
            if (this.data != null)
            {
                await this.data.RetireAsync();
            }
        }

        /// <summary>
        /// Verifies that a model without V&amp;V reference data offers set-up, and shows the register once it is set up.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        [Order(1)]
        public async Task VerifySetUpOnAFreshModel()
        {
            await this.PageModel.OpenSectionTabAsync(VandVTab);
            await Expect(this.PageModel.EmptyState.Or(this.PageModel.Register)).ToBeVisibleAsync();

            Assume.That(await this.PageModel.EmptyState.IsVisibleAsync(), Is.True, "This model is set up already, and set-up only runs on one that is not.");

            await this.PageModel.SetUpAsync();
            await Expect(this.PageModel.EmptyState).ToBeHiddenAsync(new LocatorAssertionsToBeHiddenOptions { Timeout = ServerRoundTripTimeoutMilliseconds });
            await Expect(this.PageModel.Register).ToBeVisibleAsync();
            await Expect(this.Tabs.BlazorError).ToBeHiddenAsync();
        }

        /// <summary>
        /// Verifies that the register shows the requirements with what covers them, and those without.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        [Order(2)]
        public async Task VerifyRegisterShowsTheCoverage()
        {
            await this.OpenRegisterOnTheDataAsync();

            await Expect(this.PageModel.RequirementRows).ToHaveCountAsync(3);
            await Expect(this.PageModel.ItemRow(this.data.PassedItemShortName).Locator(".vandv-pill")).ToHaveTextAsync("Passed");
            await Expect(this.PageModel.RequirementRows.Filter(new LocatorFilterOptions { HasText = this.data.UncoveredRequirementShortName })).ToContainTextAsync("Not covered");
        }

        /// <summary>
        /// Verifies that a view preset shows its fields only, is marked as active, and that the register view shows them all.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        [Order(3)]
        public async Task VerifyViewPresets()
        {
            await this.OpenRegisterOnTheDataAsync();
            var labels = VandVPageModel.FieldLabels(this.PageModel.ItemRow(this.data.PassedItemShortName));

            await Expect(labels.Filter(new LocatorFilterOptions { HasText = "Method" })).ToHaveCountAsync(1);
            await Expect(labels.Filter(new LocatorFilterOptions { HasText = "Status" })).ToHaveCountAsync(1);

            await this.PageModel.ToggleViewMenuAsync();
            await this.PageModel.PresetButton("Planning").ClickAsync();

            await Expect(this.PageModel.PresetButton("Planning")).ToHaveAttributeAsync("aria-pressed", "true");
            await Expect(labels.Filter(new LocatorFilterOptions { HasText = "Method" })).ToHaveCountAsync(1);
            await Expect(labels.Filter(new LocatorFilterOptions { HasText = "Status" })).ToHaveCountAsync(0);

            await this.PageModel.PresetButton("Execution").ClickAsync();

            await Expect(this.PageModel.PresetButton("Planning")).ToHaveAttributeAsync("aria-pressed", "false");
            await Expect(labels.Filter(new LocatorFilterOptions { HasText = "Method" })).ToHaveCountAsync(0);
            await Expect(labels.Filter(new LocatorFilterOptions { HasText = "Status" })).ToHaveCountAsync(1);

            await this.PageModel.PresetButton("Register (VCD)").ClickAsync();

            await Expect(labels.Filter(new LocatorFilterOptions { HasText = "Method" })).ToHaveCountAsync(1);
            await Expect(labels.Filter(new LocatorFilterOptions { HasText = "Status" })).ToHaveCountAsync(1);

            await this.PageModel.ToggleViewMenuAsync();
        }

        /// <summary>
        /// Verifies that the items can be filtered on several stage gates, and that clearing the filter shows them all again.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        [Order(4)]
        public async Task VerifyStageGateFilter()
        {
            await this.OpenRegisterOnTheDataAsync();
            await Expect(this.PageModel.ItemRows).ToHaveCountAsync(2);

            await this.PageModel.AddStageGateFilterAsync("CDR");

            await Expect(this.PageModel.ItemRow(this.data.PassedItemShortName)).ToBeVisibleAsync();
            await Expect(this.PageModel.ItemRow(this.data.PlannedItemShortName)).ToBeHiddenAsync();

            await this.PageModel.AddStageGateFilterAsync("PDR");

            await Expect(this.PageModel.ItemRows).ToHaveCountAsync(2);

            await this.PageModel.ClearStageGateFilterAsync();

            await Expect(this.PageModel.RequirementRows).ToHaveCountAsync(3);
        }

        /// <summary>
        /// Verifies that the V&amp;V plan and the coverage badges only show on the Requirements tab while the switch is on.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        [Order(5)]
        public async Task VerifyShowVandVOnTheRequirementsTab()
        {
            await this.OpenRegisterOnTheDataAsync();
            await this.PageModel.OpenSectionTabAsync(RequirementsTab);

            var plan = this.PageModel.RequirementsTabSpecifications.Filter(new LocatorFilterOptions { HasTextRegex = new Regex("^\\s*VNV") });
            var specification = this.PageModel.RequirementsTabSpecifications.Filter(new LocatorFilterOptions { HasText = this.data.SpecificationShortName });

            await Expect(specification).ToBeVisibleAsync();
            await specification.ClickAsync();

            await Expect(plan).ToHaveCountAsync(0);
            await Expect(this.PageModel.RequirementsTabBadges).ToHaveCountAsync(0);

            await this.PageModel.ToggleViewMenuAsync();
            await this.PageModel.ShowVandVSwitch.ClickAsync();
            await this.PageModel.ToggleViewMenuAsync();

            await Expect(plan).ToHaveCountAsync(1);
            await Expect(this.PageModel.RequirementsTabBadges).Not.ToHaveCountAsync(0);

            await this.PageModel.ToggleViewMenuAsync();
            await this.PageModel.ShowVandVSwitch.ClickAsync();
            await this.PageModel.ToggleViewMenuAsync();

            await Expect(plan).ToHaveCountAsync(0);
            await Expect(this.PageModel.RequirementsTabBadges).ToHaveCountAsync(0);
            await Expect(this.Tabs.BlazorError).ToBeHiddenAsync();
        }

        /// <summary>
        /// Opens the register on the specification of the data, written and picked up by the app on the first call.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task OpenRegisterOnTheDataAsync()
        {
            await this.PageModel.OpenSectionTabAsync(VandVTab);
            await Expect(this.PageModel.Register).ToBeVisibleAsync();

            if (this.data == null)
            {
                this.data = await VandVEndToEndData.CreateAsync(ServerUrl, Username, Password);
                await this.Home.RefreshSessionAsync();
            }

            await this.PageModel.SelectSpecificationAsync(this.data.SpecificationShortName);
            await Expect(this.PageModel.ItemRow(this.data.PassedItemShortName)).ToBeVisibleAsync();
        }
    }
}
