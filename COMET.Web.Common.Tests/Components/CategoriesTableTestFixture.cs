// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CategoriesTableTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Components
{
    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Test.Helpers;

    using DevExpress.Blazor;

    using NUnit.Framework;

    [TestFixture]
    public class CategoriesTableTestFixture
    {
        private BunitContext context;
        private DomainOfExpertise thing;
        private Category alpha;
        private Category bravo;
        private Category charlie;
        private List<Category> availableCategories;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.alpha = new Category { Iid = Guid.NewGuid(), ShortName = "alpha", Name = "Alpha" };
            this.bravo = new Category { Iid = Guid.NewGuid(), ShortName = "bravo", Name = "Bravo" };
            this.charlie = new Category { Iid = Guid.NewGuid(), ShortName = "charlie", Name = "Charlie" };

            this.availableCategories = new List<Category> { this.charlie, this.alpha, this.bravo };

            this.thing = new DomainOfExpertise
            {
                Iid = Guid.NewGuid(),
                ShortName = "doe",
                Name = "Domain"
            };
        }

        [TearDown]
        public void Teardown()
        {
            this.context.Dispose();
        }

        [Test]
        public void VerifyGridRendersOneRowPerAvailableCategory()
        {
            var renderer = this.context.Render<CategoriesTable>(parameter =>
            {
                parameter.Add(p => p.Thing, this.thing);
                parameter.Add(p => p.AvailableCategories, this.availableCategories);
            });

            var grid = renderer.FindComponent<DxGrid>();
            var data = ((IEnumerable<object>)grid.Instance.Data).Cast<Category>().ToList();

            Assert.That(data, Has.Count.EqualTo(this.availableCategories.Count));
            Assert.That(data, Is.EquivalentTo(this.availableCategories));
        }

        [Test]
        public void VerifySelectedCategoriesAreOrderedFirstThenAlphabetically()
        {
            this.thing.Category.Add(this.charlie);

            var renderer = this.context.Render<CategoriesTable>(parameter =>
            {
                parameter.Add(p => p.Thing, this.thing);
                parameter.Add(p => p.AvailableCategories, this.availableCategories);
            });

            var grid = renderer.FindComponent<DxGrid>();
            var data = ((IEnumerable<object>)grid.Instance.Data).Cast<Category>().ToList();

            Assert.Multiple(() =>
            {
                Assert.That(data[0], Is.EqualTo(this.charlie), "Selected category must appear first.");
                Assert.That(data[1], Is.EqualTo(this.alpha), "Unselected categories must follow alphabetically.");
                Assert.That(data[2], Is.EqualTo(this.bravo));
            });
        }

        [Test]
        public async Task VerifySelectionChangeUpdatesThingAndRaisesThingChanged()
        {
            ICategorizableThing capturedThing = null;

            var renderer = this.context.Render<CategoriesTable>(parameter =>
            {
                parameter.Add(p => p.Thing, this.thing);
                parameter.Add(p => p.AvailableCategories, this.availableCategories);
                parameter.Add(p => p.ThingChanged, t => capturedThing = t);
            });

            var grid = renderer.FindComponent<DxGrid>();

            await renderer.InvokeAsync(() => grid.Instance.SelectedDataItemsChanged.InvokeAsync(new object[] { this.alpha, this.bravo }));

            Assert.Multiple(() =>
            {
                Assert.That(this.thing.Category, Is.EquivalentTo(new[] { this.alpha, this.bravo }));
                Assert.That(capturedThing, Is.SameAs(this.thing));
            });
        }
    }
}
