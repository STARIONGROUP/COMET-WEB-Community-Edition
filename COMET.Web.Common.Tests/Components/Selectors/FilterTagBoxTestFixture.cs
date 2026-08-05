// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FilterTagBoxTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.Components.Selectors
{
    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components.Selectors;
    using COMET.Web.Common.Test.Helpers;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using NUnit.Framework;

    [TestFixture]
    public class FilterTagBoxTestFixture
    {
        private BunitContext context;
        private List<Category> availableCategories;
        private List<Category> selectedCategories;
        private IEnumerable<Category> valuesChangedResult;

        [SetUp]
        public void Setup()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.availableCategories = new List<Category>
            {
                new() { Iid = Guid.NewGuid(), Name = "Category A" },
                new() { Iid = Guid.NewGuid(), Name = "Category B" }
            };

            this.selectedCategories = new List<Category> { this.availableCategories[0] };
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyFilterTagBoxComponent()
        {
            var valuesChanged = new EventCallbackFactory().Create<IEnumerable<Category>>(this, values => this.valuesChangedResult = values);

            var renderer = this.context.Render<FilterTagBox<Category>>(parameters =>
            {
                parameters.Add(p => p.Data, this.availableCategories);
                parameters.Add(p => p.Values, this.selectedCategories);
                parameters.Add(p => p.ValuesChanged, valuesChanged);
                parameters.Add(p => p.TextFieldName, nameof(Category.Name));
                parameters.Add(p => p.NullText, "Filter by category...");
            });

            var tagBox = renderer.FindComponent<DxTagBox<Category, Category>>();

            Assert.Multiple(() =>
            {
                Assert.That(tagBox.Instance.Data, Is.EqualTo(this.availableCategories));
                Assert.That(tagBox.Instance.Values, Is.EqualTo(this.selectedCategories));
                Assert.That(tagBox.Instance.TextFieldName, Is.EqualTo(nameof(Category.Name)));
                Assert.That(tagBox.Instance.NullText, Is.EqualTo("Filter by category..."));
                Assert.That(renderer.Instance.AdditionalAttributes, Is.Empty, "With no unmatched attributes, nothing extra is splatted onto the tag box.");
            });

            // Invoked on FilterTagBox's own ValuesChanged rather than the nested DxTagBox: DevExpress does not
            // echo the bound EventCallback back onto its own instance under bunit.
            var newValues = new List<Category> { this.availableCategories[1] };
            await renderer.InvokeAsync(() => renderer.Instance.ValuesChanged.InvokeAsync(newValues));

            Assert.That(this.valuesChangedResult, Is.EqualTo(newValues));

            renderer.Render(parameters => parameters.AddUnmatched("id", "categoryFilter"));

            Assert.That(renderer.Instance.AdditionalAttributes["id"], Is.EqualTo("categoryFilter"));
        }
    }
}
