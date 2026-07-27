// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NodePillsTestFixture.cs" company="Starion Group S.A.">
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
    using System;
    using System.Collections.Concurrent;

    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Shared;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture for <see cref="NodePills" />.
    /// </summary>
    [TestFixture]
    public class NodePillsTestFixture
    {
        private BunitContext context;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Uri uri;
        private DomainOfExpertise owner;
        private Category category;
        private ElementDefinition elementDefinition;

        /// <summary>
        /// Set up the test run.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.uri = new Uri("http://test.com");

            this.owner = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { ShortName = "SYS", Name = "System" };
            this.category = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "KUR", Name = "Key User Requirement" };

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Owner = this.owner };
            this.elementDefinition.Category.Add(this.category);
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
        /// Renders the <see cref="NodePills" /> component with the given parameters.
        /// </summary>
        /// <param name="elementBase">the <see cref="ElementBase" /> to render pills for</param>
        /// <param name="showOwner">whether the owner pill should be shown</param>
        /// <param name="showCategories">whether the category pills should be shown</param>
        /// <returns>the rendered <see cref="NodePills" /> component</returns>
        private IRenderedComponent<NodePills> Render(ElementBase elementBase, bool showOwner, bool showCategories)
        {
            return this.context.Render<NodePills>(parameters => parameters
                .Add(p => p.ElementBase, elementBase)
                .Add(p => p.ShowOwner, showOwner)
                .Add(p => p.ShowCategories, showCategories));
        }

        /// <summary>
        /// Verifies that the owner and category pills are rendered when both toggles are enabled.
        /// </summary>
        [Test]
        public void VerifyOwnerAndCategoryPillsRenderWhenEnabled()
        {
            var component = this.Render(this.elementDefinition, true, true);
            var pills = component.FindAll("span.starion-pill");

            Assert.Multiple(() =>
            {
                Assert.That(pills, Has.Count.EqualTo(2));
                Assert.That(component.Markup, Does.Contain(this.owner.ShortName));
                Assert.That(component.Markup, Does.Contain(this.category.ShortName));
            });
        }

        /// <summary>
        /// Verifies that no pill is rendered when both toggles are disabled.
        /// </summary>
        [Test]
        public void VerifyPillsHiddenWhenTogglesOff()
        {
            var component = this.Render(this.elementDefinition, false, false);
            var pills = component.FindAll("span.starion-pill");

            Assert.That(pills, Is.Empty);
        }

        /// <summary>
        /// Verifies that the component renders nothing, and does not throw, when the <see cref="ElementBase" /> is null.
        /// </summary>
        [Test]
        public void VerifyNullElementBaseRendersNothing()
        {
            IRenderedComponent<NodePills> component = null;

            Assert.That(() => component = this.Render(null, true, true), Throws.Nothing);
            Assert.That(component.FindAll("span.starion-pill"), Is.Empty);
        }
    }
}
