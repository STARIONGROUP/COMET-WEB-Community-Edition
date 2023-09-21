// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="InputEditorTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMET.Web.Common.Tests.Components.BookEditor
{
    using Bunit;

    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components.BookEditor;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components.BookEditor;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class InputEditorTestFixture
    {
        private TestContext context;
        private IRenderedComponent<InputEditor<Book>> component;
        private Book book;
        private List<DomainOfExpertise> activeDomains;
        private List<Category> availableCategories;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.activeDomains = new List<DomainOfExpertise>
            {
                new() { Name = "Sys" }
            };

            this.availableCategories = new List<Category>
            {
                new() { Name = "Category" }
            };

            this.book = new Book()
            {
                Name = "Book Example",
                ShortName = "bookExample",
                Owner = this.activeDomains.First(),
                Category = this.availableCategories
            };

            this.component = this.context.RenderComponent<InputEditor<Book>>(parameters =>
            {
                parameters.Add(p => p.Item, this.book);
                parameters.Add(p => p.ActiveDomains, this.activeDomains);
                parameters.Add(p => p.AvailableCategories, this.availableCategories);
            });
        }

        [Test]
        public void VerifyComponent()
        {
            var dxtabs = this.component.FindComponent<DxTabs>();

            dxtabs.Instance.ActiveTabIndex = 0;

            var textboxes = this.component.FindComponents<DxTextBox>();
            var combobox = this.component.FindComponent<DxComboBox<DomainOfExpertise, DomainOfExpertise>>();

            var nameTextbox = textboxes[0];
            var shortNameTextbox = textboxes[1];

            Assert.Multiple(() =>
            {
                Assert.That(nameTextbox.Instance.Text, Is.EqualTo("Book Example"));
                Assert.That(shortNameTextbox.Instance.Text, Is.EqualTo("bookExample"));
                Assert.That(combobox.Instance.Value, Is.EqualTo(this.activeDomains.First()));
            });

            dxtabs.Instance.ActiveTabIndex = 1;
            this.component.Render();

            var listbox = this.component.FindComponent<DxListBox<Category, Category>>();

            Assert.Multiple(() =>
            {
                Assert.That(listbox.Instance.Data, Is.EquivalentTo(this.availableCategories));
                Assert.That(listbox.Instance.Values, Is.EquivalentTo(this.availableCategories));
            });
        }
    }
}

