// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewerTestFixture.cs" company="Starion Group S.A.">
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

    using static Microsoft.Playwright.Assertions;

    /// <summary>
    /// End-to-end tests for the 3D Viewer application. Add Viewer-specific tests here.
    /// </summary>
    [TestFixture]
    public class ViewerTestFixture : ApplicationPageTestBase<ViewerPageModel>
    {
        /// <summary>
        /// The distance, in pixels, over which a resize handle is dragged.
        /// </summary>
        private const float DragDistance = 200;

        /// <summary>
        /// The smallest width increase, in pixels, that still proves a panel followed its handle once the row has
        /// little room left to give.
        /// </summary>
        private const float MinimumGrowth = 50;

        /// <summary>
        /// Gets the display name of the application under test, exactly as it appears in the "View" selector.
        /// </summary>
        protected override string ApplicationName => "3D Viewer";

        /// <summary>
        /// Builds the page object for the application under test.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        /// <returns>The page object.</returns>
        protected override ViewerPageModel CreatePageModel(IPage page) => new(page);

        /// <summary>
        /// Verifies GH937: editing a parameter value in the properties panel then clicking its revert affordance restores
        /// the original value in the editor and clears the changed state.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        public async Task VerifyParameterRevertRestoresOriginalValue()
        {
            await this.ExpandProductTreeAsync();

            var input = await this.SelectFirstElementWithQuantityEditorAsync();
            Assume.That(input, Is.Not.Null, "no element in the opened model exposes a numeric parameter editor");

            var originalValue = await input.InputValueAsync();

            await input.FillAsync("7");
            await input.DispatchEventAsync("input");

            await Expect(this.Page.Locator(".parameter-item-changed")).ToBeVisibleAsync();
            await Expect(this.Page.Locator("input.quantity-kind-parameter").First).ToHaveValueAsync("7");

            await this.Page.Locator(".parameter-item-revert").First.ClickAsync();

            await Expect(this.Page.Locator("input.quantity-kind-parameter").First).ToHaveValueAsync(originalValue);
            await Expect(this.Page.Locator(".parameter-item-changed")).ToBeHiddenAsync();
            await Expect(this.Tabs.BlazorError).ToBeHiddenAsync();
        }

        /// <summary>
        /// Verifies that the 3D Viewer product tree exposes the same controls as the System Representation tree
        /// the shared search bar and the "View" display-options cog.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        public async Task VerifyProductTreeControlsAreDisplayed()
        {
            await Expect(this.PageModel.SearchBar).ToBeVisibleAsync();
            await Expect(this.PageModel.ViewMenuButton).ToBeVisibleAsync();
            await Expect(this.Tabs.BlazorError).ToBeHiddenAsync();
        }

        /// <summary>
        /// Verifies that the product tree and the properties panel can each be minimized to a strip and restored,
        /// so the 3D view can take the whole width (issue GH936).
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        public async Task VerifyPanelsCanBeCollapsedAndRestored()
        {
            await this.PageModel.CollapseProductTreeButton.ClickAsync();
            await Expect(this.PageModel.ProductTree).ToBeHiddenAsync();
            await Expect(this.PageModel.ExpandProductTreeStrip).ToBeVisibleAsync();

            await this.PageModel.ExpandProductTreeStrip.ClickAsync();
            await Expect(this.PageModel.ProductTree).ToBeVisibleAsync();

            await this.PageModel.CollapsePropertiesButton.ClickAsync();
            await Expect(this.PageModel.PropertiesPanel).ToBeHiddenAsync();
            await Expect(this.PageModel.ExpandPropertiesStrip).ToBeVisibleAsync();

            await this.PageModel.ExpandPropertiesStrip.ClickAsync();
            await Expect(this.PageModel.PropertiesPanel).ToBeVisibleAsync();
            await Expect(this.Tabs.BlazorError).ToBeHiddenAsync();
        }

        /// <summary>
        /// Verifies that each drag handle widens the panel it belongs to: dragging the left handle to the right
        /// widens the product tree, dragging the right handle to the left widens the properties panel (issue GH936).
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        [Test]
        public async Task VerifyPanelsCanBeResized()
        {
            var treeBox = await this.PageModel.ProductTree.BoundingBoxAsync();
            var propertiesBox = await this.PageModel.PropertiesPanel.BoundingBoxAsync();

            Assume.That(treeBox, Is.Not.Null);
            Assume.That(propertiesBox, Is.Not.Null);

            // The right handle sits on the left of the panel it sizes, so it has to be dragged towards the start of
            // the row to widen it. Both drags pin their panel, which leaves the second one less room to grow.
            await this.DragResizerAsync(this.PageModel.RightResizer, -DragDistance);
            await this.DragResizerAsync(this.PageModel.LeftResizer, DragDistance);

            var widenedTreeBox = await this.PageModel.ProductTree.BoundingBoxAsync();
            var widenedPropertiesBox = await this.PageModel.PropertiesPanel.BoundingBoxAsync();

            Assume.That(widenedTreeBox, Is.Not.Null);
            Assume.That(widenedPropertiesBox, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(widenedPropertiesBox.Width, Is.GreaterThan(propertiesBox.Width + (DragDistance / 2)));
                Assert.That(widenedTreeBox.Width, Is.GreaterThan(treeBox.Width + MinimumGrowth));
            });

            await Expect(this.Tabs.BlazorError).ToBeHiddenAsync();
        }

        /// <summary>
        /// Drags a resize handle horizontally over the given distance.
        /// </summary>
        /// <param name="resizer">The handle to drag.</param>
        /// <param name="horizontalOffset">The distance in pixels, negative to drag towards the start of the row.</param>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task DragResizerAsync(ILocator resizer, float horizontalOffset)
        {
            var resizerBox = await resizer.BoundingBoxAsync();

            Assume.That(resizerBox, Is.Not.Null);

            var centerX = resizerBox.X + (resizerBox.Width / 2);
            var centerY = resizerBox.Y + (resizerBox.Height / 2);

            await this.Page.Mouse.MoveAsync(centerX, centerY);
            await this.Page.Mouse.DownAsync();
            await this.Page.Mouse.MoveAsync(centerX + horizontalOffset, centerY);
            await this.Page.Mouse.UpAsync();
        }

        /// <summary>
        /// Expands every collapsed node of the product tree so the leaf elements render.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task ExpandProductTreeAsync()
        {
            for (var pass = 0; pass < 6; pass++)
            {
                var expandIcons = this.Page.Locator(".expandIcon");
                var iconCount = await expandIcons.CountAsync();
                var expandedAny = false;

                for (var i = 0; i < iconCount; i++)
                {
                    var icon = expandIcons.Nth(i);

                    if (await icon.GetAttributeAsync("src") is { } src && src.Contains("Collapsed"))
                    {
                        await icon.ClickAsync();
                        await this.Page.WaitForTimeoutAsync(300);
                        expandedAny = true;
                    }
                }

                if (!expandedAny)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Clicks each tree node in turn until one selects an element whose properties panel exposes a quantity editor.
        /// </summary>
        /// <returns>The quantity editor input, or null when no element exposes one.</returns>
        private async Task<ILocator> SelectFirstElementWithQuantityEditorAsync()
        {
            var nodeCount = await this.Page.Locator(".treeNode").CountAsync();

            for (var i = 0; i < nodeCount; i++)
            {
                await this.Page.Locator(".treeNode").Nth(i).ClickAsync();
                await this.Page.WaitForTimeoutAsync(400);

                if (await this.Page.Locator("input.quantity-kind-parameter").CountAsync() > 0)
                {
                    return this.Page.Locator("input.quantity-kind-parameter").First;
                }
            }

            return null;
        }
    }
}
