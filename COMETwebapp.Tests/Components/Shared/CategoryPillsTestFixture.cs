// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CategoryPillsTestFixture.cs" company="Starion Group S.A.">
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

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Shared;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture for <see cref="CategoryPills" />.
    /// </summary>
    [TestFixture]
    public class CategoryPillsTestFixture
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
        /// Verifies that fewer categories than <see cref="CategoryPills.MaxVisible" /> render one pill per
        /// category and no overflow pill.
        /// </summary>
        [Test]
        public void VerifyFewCategoriesRenderNoOverflowPill()
        {
            var categories = new List<Category>
            {
                new() { Iid = Guid.NewGuid(), ShortName = "CAT1", Name = "Category One", PermissibleClass = { ClassKind.ElementDefinition } }
            };

            var rendered = this.context.Render<CategoryPills>(parameters => parameters.Add(p => p.Categories, categories));

            var pills = rendered.FindAll("span.starion-pill");

            Assert.Multiple(() =>
            {
                Assert.That(pills, Has.Count.EqualTo(1));
                Assert.That(pills[0].TextContent, Is.EqualTo("CAT1"));
                Assert.That(rendered.Markup, Does.Not.Contain("+1"));
            });
        }

        /// <summary>
        /// Verifies that more categories than <see cref="CategoryPills.MaxVisible" /> render exactly
        /// <see cref="CategoryPills.MaxVisible" /> individual pills plus a single "+N" overflow pill whose
        /// tooltip lists the hidden categories.
        /// </summary>
        [Test]
        public void VerifyExcessCategoriesCollapseIntoOverflowPill()
        {
            var categories = new List<Category>
            {
                new() { Iid = Guid.NewGuid(), ShortName = "CAT1", Name = "Category One", PermissibleClass = { ClassKind.ElementDefinition } },
                new() { Iid = Guid.NewGuid(), ShortName = "CAT2", Name = "Category Two", PermissibleClass = { ClassKind.ElementDefinition } },
                new() { Iid = Guid.NewGuid(), ShortName = "CAT3", Name = "Category Three", PermissibleClass = { ClassKind.ElementDefinition } }
            };

            var rendered = this.context.Render<CategoryPills>(parameters => parameters.Add(p => p.Categories, categories));

            var pills = rendered.FindAll("span.starion-pill");

            Assert.Multiple(() =>
            {
                Assert.That(pills, Has.Count.EqualTo(3), "Two visible pills plus one overflow pill.");
                Assert.That(pills[0].TextContent, Is.EqualTo("CAT1"));
                Assert.That(pills[1].TextContent, Is.EqualTo("CAT2"));
                Assert.That(pills[2].TextContent, Is.EqualTo("+1"));
                Assert.That(pills[2].GetAttribute("title"), Does.Contain("CAT3"));
            });
        }

        /// <summary>
        /// Verifies that <see cref="CategoryPills.UseShortName" /> set to <see langword="false" /> labels
        /// pills with the category's full name rather than its short name.
        /// </summary>
        [Test]
        public void VerifyUseShortNameFalseRendersFullName()
        {
            var categories = new List<Category>
            {
                new() { Iid = Guid.NewGuid(), ShortName = "CAT1", Name = "Category One", PermissibleClass = { ClassKind.ElementDefinition } }
            };

            var rendered = this.context.Render<CategoryPills>(parameters => parameters
                .Add(p => p.Categories, categories)
                .Add(p => p.UseShortName, false));

            var pill = rendered.Find("span.starion-pill");

            Assert.That(pill.TextContent, Is.EqualTo("Category One"));
        }

        /// <summary>
        /// Verifies that a <see langword="null" /> <see cref="CategoryPills.Categories" /> renders no pills
        /// instead of throwing.
        /// </summary>
        [Test]
        public void VerifyNullCategoriesRendersNoPills()
        {
            var rendered = this.context.Render<CategoryPills>(parameters => parameters.Add(p => p.Categories, null));

            Assert.That(rendered.FindAll("span.starion-pill"), Is.Empty);
        }
    }
}
