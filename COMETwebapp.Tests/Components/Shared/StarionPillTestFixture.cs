// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="StarionPillTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.Shared
{
    using Bunit;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Shared;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture for <see cref="StarionPill" />.
    /// </summary>
    [TestFixture]
    public class StarionPillTestFixture
    {
        private BunitContext context;

        /// <summary>
        /// Set up the test run.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
        }

        /// <summary>
        /// Tears down the test run.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        /// <summary>
        /// Renders the <see cref="StarionPill" /> component with the given parameters.
        /// </summary>
        /// <param name="colorSeed">the seed used to compute the pill's background colour</param>
        /// <param name="title">the pill's title attribute</param>
        /// <param name="style">an optional extra inline style</param>
        /// <returns>the rendered <see cref="StarionPill" /> component</returns>
        private IRenderedComponent<StarionPill> Render(string colorSeed, string title, string style = null)
        {
            return this.context.Render<StarionPill>(parameters => parameters
                .Add(p => p.ColorSeed, colorSeed)
                .Add(p => p.Title, title)
                .Add(p => p.Style, style)
                .AddChildContent("SYS"));
        }

        /// <summary>
        /// Verifies that a colored pill renders a <c>span.starion-pill</c> with a background-color style,
        /// the given title and the given child content.
        /// </summary>
        [Test]
        public void VerifyColoredPillRendersBackgroundColorAndTitle()
        {
            var component = this.Render("owner:SYS", "Owning Domain Of Expertise: SYS");
            var pill = component.Find("span.starion-pill");

            Assert.Multiple(() =>
            {
                Assert.That(pill.GetAttribute("style"), Does.Contain("background-color:"));
                Assert.That(pill.GetAttribute("title"), Is.EqualTo("Owning Domain Of Expertise: SYS"));
                Assert.That(pill.TextContent, Is.EqualTo("SYS"));
            });
        }

        /// <summary>
        /// Verifies that an empty <see cref="StarionPill.ColorSeed" /> renders a pill with no
        /// background-color style.
        /// </summary>
        [Test]
        public void VerifyEmptyColorSeedRendersNoBackgroundColor()
        {
            var component = this.Render(string.Empty, "states", "white-space:nowrap;");
            var pill = component.Find("span.starion-pill");

            Assert.That(pill.GetAttribute("style"), Does.Not.Contain("background-color:"));
        }

        /// <summary>
        /// Verifies that the <see cref="StarionPill.Style" /> parameter is appended to the rendered style.
        /// </summary>
        [Test]
        public void VerifyStyleParameterIsAppended()
        {
            var component = this.Render("category:KUR", "Category: KUR", "margin-right:4px;");
            var pill = component.Find("span.starion-pill");

            Assert.That(pill.GetAttribute("style"), Does.Contain("margin-right:4px;"));
        }
    }
}
