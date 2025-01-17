﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CardViewTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Components.CardView
{
    using Bunit;

    using COMET.Web.Common.Components.CardView;

    using DevExpress.Blazor;
    using DevExpress.Blazor.Internal;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    public class CardViewTestFixture
    {
        private TestContext context;
        private TestClass testClass1 = new ();
        private TestClass testClass2 = new ();
        private TestClass testClass3 = new ();
        private TestClass[] testClasses;
        private readonly string[] searchFields = ["Id", "Name"];
        private readonly string[] sortFields = [string.Empty, "Id", "Name"];

        private static RenderFragment<TestClass> NormalTemplate()
        {
            RenderFragment Template(TestClass child) =>
                builder =>
                {
                    builder.OpenComponent<CardField<TestClass>>(0);
                    builder.AddAttribute(1, "Context", child);
                    builder.AddAttribute(2, "FieldName", "Id");
                    builder.CloseComponent();
                    builder.OpenComponent<CardField<TestClass>>(3);
                    builder.AddAttribute(4, "Context", child);
                    builder.AddAttribute(5, "FieldName", "Name");
                    builder.CloseComponent();
                };

            return Template;
        }

        private static RenderFragment<TestClass> NoSearchAndSortTemplate()
        {
            RenderFragment Template(TestClass child) =>
                builder =>
                {
                    builder.OpenComponent<CardField<TestClass>>(0);
                    builder.AddAttribute(1, "Context", child);
                    builder.AddAttribute(2, "FieldName", "Id");
                    builder.AddAttribute(3, "AllowSearch", false);
                    builder.AddAttribute(4, "AllowSort", false);
                    builder.CloseComponent();
                    builder.OpenComponent<CardField<TestClass>>(5);
                    builder.AddAttribute(6, "Context", child);
                    builder.AddAttribute(7, "FieldName", "Name");
                    builder.AddAttribute(8, "AllowSearch", false);
                    builder.AddAttribute(9, "AllowSort", false);
                    builder.CloseComponent();
                };

            return Template;
        }

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.testClasses = [this.testClass1, this.testClass2, this.testClass3];

            this.context.Services.AddDevExpressBlazor(_ => ConfigureJsInterop(this.context.JSInterop));

            this.context.JSInterop.SetupVoid("DxBlazor.AdaptiveDropDown.init");
            this.context.JSInterop.SetupVoid("DxBlazor.DropDown.getReference");
            this.context.JSInterop.SetupVoid("DxBlazor.ComboBox.loadModule");
            this.context.JSInterop.SetupVoid("DxBlazor.Input.loadModule");
        }

        [Test]
        public void VerifyComponent()
        {
            var component = this.context.RenderComponent<CardView<TestClass>>(parameters =>
            {
                parameters
                    .Add(p => p.Items, this.testClasses)
                    .Add(p => p.ItemSize, 150)
                    .Add(p => p.ItemTemplate, NormalTemplate())
                    .Add(p => p.ScrollableAreaCssClass, "scrollable-area");
            });

            var cardView = component;

            Assert.Multiple(() =>
                {
                    Assert.That(cardView.Instance.AllowSort, Is.True);
                    Assert.That(cardView.Instance.AllowSearch, Is.True);
                    Assert.That(cardView.Instance.ItemSize, Is.EqualTo(150));
                    Assert.That(cardView.Instance.SearchFields, Is.EquivalentTo(this.searchFields));
                    Assert.That(cardView.Instance.SortFields, Is.EquivalentTo(this.sortFields));
                    Assert.That(cardView.Instance.ScrollableAreaCssClass, Is.EqualTo("scrollable-area"));
                });

            var textBoxParentComponent = component.Find("#search-textbox");

            Assert.Multiple(() =>
            {
                Assert.That(textBoxParentComponent, Is.Not.Null);
                Assert.That(textBoxParentComponent.Attributes.Single(x => x.Name == "style").Value.Contains("visibility:block"), Is.True);
            });

            var comboBoxParentComponent = component.Find("#sort-dropdown");

            Assert.Multiple(() =>
            {
                Assert.That(comboBoxParentComponent, Is.Not.Null);
                Assert.That(comboBoxParentComponent.Attributes.Single(x => x.Name == "style").Value.Contains("visibility:block"), Is.True);
            });

            var scrollableAreaComponent = component.Find(".scrollable-area");

            Assert.That(scrollableAreaComponent, Is.Not.Null);

            var cardFields = component.FindComponents<CardField<TestClass>>();

            Assert.Multiple(() =>
            {
                Assert.That(cardFields.Count, Is.EqualTo(6));

                foreach (var cardField in cardFields)
                {
                    Assert.That(cardField.Instance.AllowSearch, Is.True);
                    Assert.That(cardField.Instance.AllowSort, Is.True);
                }

                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass1.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass2.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass3.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass1.Id.ToString())).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass2.Id.ToString())).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass3.Id.ToString())).Count, Is.EqualTo(1));
            });
        }

        [Test]
        public void VerifyComponentWithoutSortAndSearch()
        {
            var component = this.context.RenderComponent<CardView<TestClass>>(parameters =>
            {
                parameters
                    .Add(p => p.Items, this.testClasses)
                    .Add(p => p.ItemSize, 150)
                    .Add(p => p.ItemTemplate, NoSearchAndSortTemplate());
            });

            var cardView = component;

            Assert.Multiple(() =>
            {
                Assert.That(cardView.Instance.AllowSort, Is.False);
                Assert.That(cardView.Instance.AllowSearch, Is.False);
                Assert.That(cardView.Instance.ItemSize, Is.EqualTo(150));
                Assert.That(cardView.Instance.SearchFields.Count, Is.EqualTo(0));
                Assert.That(cardView.Instance.SortFields.Count, Is.EqualTo(1));
            });

            var textBoxParentComponent = component.Find("#search-textbox");

            Assert.Multiple(() =>
            {
                Assert.That(textBoxParentComponent, Is.Not.Null);
                Assert.That(textBoxParentComponent.Attributes.Single(x => x.Name == "style").Value.Contains("visibility:hidden"), Is.True);
            });

            var comboBoxParentComponent = component.Find("#sort-dropdown");

            Assert.Multiple(() =>
            {
                Assert.That(comboBoxParentComponent, Is.Not.Null);
                Assert.That(comboBoxParentComponent.Attributes.Single(x => x.Name == "style").Value.Contains("visibility:hidden"), Is.True);
            });

            var cardFields = component.FindComponents<CardField<TestClass>>();

            Assert.Multiple(() =>
            {
                Assert.That(cardFields.Count, Is.EqualTo(6));

                foreach (var cardField in cardFields)
                {
                    Assert.That(cardField.Instance.AllowSearch, Is.False);
                    Assert.That(cardField.Instance.AllowSort, Is.False);
                }

                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass1.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass2.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass3.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass1.Id.ToString())).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass2.Id.ToString())).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass3.Id.ToString())).Count, Is.EqualTo(1));
            });
        }

        [Test]
        public void VerifyComponentRerenders()
        {
            var component = this.context.RenderComponent<CardView<TestClass>>(parameters =>
            {
                parameters
                    .Add(p => p.Items, this.testClasses)
                    .Add(p => p.ItemSize, 150)
                    .Add(p => p.ItemTemplate, NoSearchAndSortTemplate());
            });
           
            var cardFields = component.FindComponents<CardField<TestClass>>();

            Assert.Multiple(() =>
            {
                Assert.That(cardFields.Count, Is.EqualTo(6));

                foreach (var cardField in cardFields)
                {
                    Assert.That(cardField.Instance.AllowSearch, Is.False);
                    Assert.That(cardField.Instance.AllowSort, Is.False);
                }

                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass1.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass2.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass3.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass1.Id.ToString())).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass2.Id.ToString())).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass3.Id.ToString())).Count, Is.EqualTo(1));
            });

            // create new collection of items
            this.testClass1 = new TestClass();
            this.testClass2 = new TestClass();
            this.testClass3 = new TestClass();
            this.testClasses = [this.testClass1, this.testClass2, this.testClass3];

            component.SetParametersAndRender(parameters => parameters
                .Add(p => p.Items, this.testClasses));

            cardFields = component.FindComponents<CardField<TestClass>>();

            Assert.Multiple(() =>
            {
                Assert.That(cardFields.Count, Is.EqualTo(6));

                foreach (var cardField in cardFields)
                {
                    Assert.That(cardField.Instance.AllowSearch, Is.False);
                    Assert.That(cardField.Instance.AllowSort, Is.False);
                }

                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass1.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass2.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass3.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass1.Id.ToString())).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass2.Id.ToString())).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass3.Id.ToString())).Count, Is.EqualTo(1));
            });
        }

        [Test]
        public void VerifySelectComponent()
        {
            var component = this.context.RenderComponent<CardView<TestClass>>(parameters =>
            {
                parameters
                    .Add(p => p.Items, this.testClasses)
                    .Add(p => p.ItemSize, 150)
                    .Add(p => p.ItemTemplate, NormalTemplate());
            });

            var cardView = component;

            Assert.Multiple(() =>
            {
                Assert.That(cardView.Instance.AllowSort, Is.True);
                Assert.That(cardView.Instance.AllowSearch, Is.True);
                Assert.That(cardView.Instance.ItemSize, Is.EqualTo(150));
                Assert.That(cardView.Instance.SearchFields, Is.EquivalentTo(this.searchFields));
                Assert.That(cardView.Instance.SortFields, Is.EquivalentTo(this.sortFields));
            });

            var firstCardField = component.Find(".card");
            firstCardField.Click();

            var selectedCardField = component.Find(".selected");

            Assert.Multiple(() =>
            {
                Assert.That(firstCardField.InnerHtml, Is.EqualTo(selectedCardField.InnerHtml));
            });
        }

        [Test]
        public async Task VerifySearchComponent()
        {
            var component = this.context.RenderComponent<CardView<TestClass>>(parameters =>
            {
                parameters
                    .Add(p => p.Items, this.testClasses)
                    .Add(p => p.ItemSize, 150)
                    .Add(p => p.ItemTemplate, NormalTemplate());
            });

            var cardView = component;

            Assert.Multiple(() =>
            {
                Assert.That(cardView.Instance.AllowSort, Is.True);
                Assert.That(cardView.Instance.AllowSearch, Is.True);
                Assert.That(cardView.Instance.ItemSize, Is.EqualTo(150));
                Assert.That(cardView.Instance.SearchFields, Is.EquivalentTo(this.searchFields));
                Assert.That(cardView.Instance.SortFields, Is.EquivalentTo(this.sortFields));
            });

            var textBoxParentComponent = component.Find("#search-textbox");

            Assert.Multiple(() =>
            {
                Assert.That(textBoxParentComponent, Is.Not.Null);
                Assert.That(textBoxParentComponent.Attributes.Single(x => x.Name == "style").Value.Contains("visibility:block"), Is.True);
            });

            var textBoxComponent = component.FindComponent<DxTextBox>();
            await component.InvokeAsync(() => textBoxComponent.Instance.TextChanged.InvokeAsync(this.testClass1.Id.ToString()));

            var cardFields = component.FindComponents<CardField<TestClass>>();

            Assert.Multiple(() =>
            {
                Assert.That(cardFields.Count, Is.EqualTo(2));

                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass1.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass2.Name)).ToList().Count, Is.EqualTo(0));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass3.Name)).ToList().Count, Is.EqualTo(0));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass1.Id.ToString()) && x.Markup.Contains("search-mark")).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass2.Id.ToString())).ToList().Count, Is.EqualTo(0));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass3.Id.ToString())).Count, Is.EqualTo(0));
            });

            await component.InvokeAsync(async () => await textBoxComponent.Instance.TextChanged.InvokeAsync(string.Empty));

            cardFields = component.FindComponents<CardField<TestClass>>();

            Assert.Multiple(() =>
            {
                Assert.That(cardFields.Count, Is.EqualTo(6));

                Assert.That(cardFields.Where(x => x.Markup.Contains("search-mark")).ToList().Count, Is.EqualTo(0));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass1.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass2.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass3.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass1.Id.ToString())).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass2.Id.ToString())).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass3.Id.ToString())).Count, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task VerifySortComponent()
        {
            var component = this.context.RenderComponent<CardView<TestClass>>(parameters =>
            {
                parameters
                    .Add(p => p.Items, this.testClasses)
                    .Add(p => p.ItemSize, 150)
                    .Add(p => p.ItemTemplate, NormalTemplate());
            });

            var cardView = component;

            Assert.Multiple(() =>
            {
                Assert.That(cardView.Instance.AllowSort, Is.True);
                Assert.That(cardView.Instance.AllowSearch, Is.True);
                Assert.That(cardView.Instance.ItemSize, Is.EqualTo(150));
                Assert.That(cardView.Instance.SearchFields, Is.EquivalentTo(this.searchFields));
                Assert.That(cardView.Instance.SortFields, Is.EquivalentTo(this.sortFields));
                Assert.That(cardView.Instance.SelectedSortField == string.Empty);
            });

            var sortParentComponent = component.Find("#sort-dropdown");

            Assert.Multiple(() =>
            {
                Assert.That(sortParentComponent, Is.Not.Null);
                Assert.That(sortParentComponent.Attributes.Single(x => x.Name == "style").Value.Contains("visibility:block"), Is.True);
            });

            var cardFields = component.FindComponents<CardField<TestClass>>();

            Assert.Multiple(() =>
            {
                Assert.That(cardFields.Count, Is.EqualTo(6));

                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass1.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass2.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass3.Name)).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass1.Id.ToString())).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass2.Id.ToString())).ToList().Count, Is.EqualTo(1));
                Assert.That(cardFields.Where(x => x.Markup.Contains(this.testClass3.Id.ToString())).Count, Is.EqualTo(1));
            });

            await component.InvokeAsync(() => component.Instance.OnSelectedSortItemChanged("Id"));

            component.Render();

            cardFields = component.FindComponents<CardField<TestClass>>();

            var sortedTestClasses = this.testClasses.OrderBy(x => x.Id).Select(x => x.Name).ToList();
            var sortedCarFields = cardFields.Where(x => x.Markup.StartsWith("Name-")).Select(x => x.Markup).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(component.Instance.SelectedSortField == "Id");
                Assert.That(sortedTestClasses[0], Is.EqualTo(sortedCarFields[0]));
                Assert.That(sortedTestClasses[1], Is.EqualTo(sortedCarFields[1]));
                Assert.That(sortedTestClasses[2], Is.EqualTo(sortedCarFields[2]));
            });

            await component.InvokeAsync(() => component.Instance.OnSelectedSortItemChanged(string.Empty));

            component.Render();

            cardFields = component.FindComponents<CardField<TestClass>>();

            sortedCarFields = cardFields.Where(x => x.Markup.StartsWith("Name-")).Select(x => x.Markup).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(component.Instance.SelectedSortField == string.Empty);
                Assert.That(this.testClasses[0].Name, Is.EqualTo(sortedCarFields[0]));
                Assert.That(this.testClasses[1].Name, Is.EqualTo(sortedCarFields[1]));
                Assert.That(this.testClasses[2].Name, Is.EqualTo(sortedCarFields[2]));
            });
        }

        /// <summary>
        /// Configure the <see cref="BunitJSInterop" /> for DevExpress
        /// </summary>
        /// <param name="interop">The <see cref="BunitJSInterop" /> to configure</param>
        private static void ConfigureJsInterop(BunitJSInterop interop)
        {
            interop.Mode = JSRuntimeMode.Loose;

            var rootModule = interop.SetupModule("./_content/DevExpress.Blazor/dx-blazor.js");
            rootModule.Mode = JSRuntimeMode.Strict;

            rootModule.Setup<DeviceInfo>("getDeviceInfo", _ => true)
                .SetResult(new DeviceInfo(false));
        }
    }

    public class TestClass
    {
        public Guid Id { get; } = Guid.NewGuid();

        public string Name { get; } = $"Name-{DateTime.Now.Ticks}";
    }
}
