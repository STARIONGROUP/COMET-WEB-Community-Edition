// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.IntegrationTests
{
    using System.Threading.Tasks;

    using COMETwebapp.Tests.IntegrationTests.PageModels;

    using Microsoft.Playwright;

    using NUnit.Framework;

    /// <summary>
    /// End-to-end tests for the Engineering Model application. Add Engineering-Model-specific tests here.
    /// </summary>
    [TestFixture]
    public class EngineeringModelTestFixture : ToolbarApplicationTestFixture<EngineeringModelPageModel>
    {
        /// <summary>
        /// Gets the display name of the application under test, exactly as it appears in the "View" selector.
        /// </summary>
        protected override string ApplicationName => "Engineering Model";

        /// <summary>
        /// Builds the page object for the application under test.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        /// <returns>The page object.</returns>
        protected override EngineeringModelPageModel CreatePageModel(IPage page) => new(page);

        /// <summary>
        /// Verifies that the view/edit details panel next to the section's table is outlined (issue #930): without a
        /// border it reads as floating on the page rather than as a panel of its own. The panel is the shared
        /// <c>DataItemDetailsComponent</c>, so this covers every table-plus-details application page.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        public async Task VerifyDetailsPanelIsBordered()
        {
            await this.PageModel.OpenSectionAsync("Domain File Store");

            var style = await this.PageModel.DetailsPanel.EvaluateAsync<string[]>(
                "panel => { const s = getComputedStyle(panel); return [s.borderTopStyle, s.borderTopWidth, s.borderTopColor, s.borderRadius]; }");

            Assert.Multiple(() =>
            {
                Assert.That(style[0], Is.EqualTo("solid"), "the details panel must be outlined so it reads as a distinct panel");
                Assert.That(style[1], Is.Not.EqualTo("0px"));
                Assert.That(style[2], Is.Not.EqualTo("rgba(0, 0, 0, 0)"));
                Assert.That(style[3], Is.Not.EqualTo("0px"));
            });
        }

        /// <summary>
        /// Verifies that the table and the section buttons above it are rounded to the same corner radius as the
        /// details panel beside them (issue #932): the DevExpress default is squarer than the radius the application
        /// draws its own panels with, which made the three sit visibly out of step.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        public async Task VerifyTableAndSectionButtonsShareThePanelCornerRadius()
        {
            await this.PageModel.OpenSectionAsync("Domain File Store");

            var panelRadius = await GetTopLeftRadiusAsync(this.PageModel.DetailsPanel);
            var tableRadius = await GetTopLeftRadiusAsync(this.PageModel.SectionTable);
            var buttonRadius = await GetTopLeftRadiusAsync(this.PageModel.SectionTabs.First);

            Assert.Multiple(() =>
            {
                Assert.That(tableRadius, Is.EqualTo(panelRadius), "the table must share the details panel's corner radius");
                Assert.That(buttonRadius, Is.EqualTo(panelRadius), "the section buttons must share the details panel's corner radius");
            });
        }

        /// <summary>
        /// Verifies that the page introduction box is outlined like the table and the details panel below it, and
        /// that its information glyph is drawn large enough to read as a full "i" rather than a smudged circle.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        public async Task VerifyIntroductionBoxMatchesTheSurfacesBelowIt()
        {
            await this.PageModel.OpenSectionAsync("Options");

            var introduction = await GetOutlineAsync(this.PageModel.IntroductionBox);
            var panel = await GetOutlineAsync(this.PageModel.DetailsPanel);
            var iconBox = await this.PageModel.IntroductionIcon.BoundingBoxAsync();

            Assert.Multiple(() =>
            {
                Assert.That(introduction[0], Is.EqualTo(panel[0]), "the introduction box must carry the same border as the details panel");
                Assert.That(introduction[1], Is.EqualTo(panel[1]), "the introduction box must carry the same border colour as the details panel");
                Assert.That(introduction[2], Is.EqualTo(panel[2]), "the introduction box must carry the same corner radius as the details panel");
                Assert.That(iconBox.Width, Is.GreaterThan(16), "the information glyph is unreadable at the inherited font size");
            });
        }

        /// <summary>
        /// Verifies that the section tabs read like the application tabs above the page (issue #932): the current
        /// section is a raised white card and the others are flat, rather than every section being an identical
        /// DevExpress button.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        public async Task VerifyCurrentSectionTabIsRaised()
        {
            await this.PageModel.OpenSectionAsync("Options");

            var current = await GetTabAppearanceAsync(this.PageModel.CurrentSectionTab);
            var other = await GetTabAppearanceAsync(this.PageModel.SectionTabs.Last);

            Assert.Multiple(() =>
            {
                Assert.That(current[0], Is.EqualTo("rgb(255, 255, 255)"), "the current section tab must be a white card");
                Assert.That(current[1], Is.Not.EqualTo("none"), "the current section tab must be raised");
                Assert.That(current[2], Is.EqualTo("700"), "the current section tab must be bold");
                Assert.That(other[0], Is.EqualTo("rgba(0, 0, 0, 0)"), "the other section tabs must stay flat");
                Assert.That(other[1], Is.EqualTo("none"), "the other section tabs must not be raised");
            });
        }

        /// <summary>
        /// Reads the computed border width, border colour and corner radius that make a surface read as a card.
        /// </summary>
        /// <param name="locator">The surface to measure.</param>
        /// <returns>The border width, the border colour and the corner radius, in that order.</returns>
        private static Task<string[]> GetOutlineAsync(ILocator locator)
        {
            return locator.EvaluateAsync<string[]>(
                "surface => { const s = getComputedStyle(surface); return [s.borderTopWidth, s.borderTopColor, s.borderTopLeftRadius]; }");
        }

        /// <summary>
        /// Reads the computed background, shadow and weight that tell a raised section tab from a flat one.
        /// </summary>
        /// <param name="locator">The section tab to measure.</param>
        /// <returns>The background colour, the box shadow and the font weight, in that order.</returns>
        private static Task<string[]> GetTabAppearanceAsync(ILocator locator)
        {
            return locator.EvaluateAsync<string[]>(
                "tab => { const s = getComputedStyle(tab); return [s.backgroundColor, s.boxShadow, s.fontWeight]; }");
        }

        /// <summary>
        /// Reads the computed top-left corner radius of an element.
        /// </summary>
        /// <param name="locator">The element to measure.</param>
        /// <returns>The radius, as the browser reports it (for example <c>8px</c>).</returns>
        private static Task<string> GetTopLeftRadiusAsync(ILocator locator)
        {
            return locator.EvaluateAsync<string>("element => getComputedStyle(element).borderTopLeftRadius");
        }
    }
}
