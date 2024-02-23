// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MultiComboBoxTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMET.Web.Common.Tests.Components
{
    using Bunit;

    using CDP4Common.DTO;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Test.Helpers;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using TestContext = Bunit.TestContext;

    using NUnit.Framework;

    [TestFixture]
    public class MultiComboBoxTestFixture
    {
        private TestContext context;
        private IRenderedComponent<MultiComboBox<Category>> component;
        private List<Category> availableCategories;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.availableCategories = new List<Category>
            {
                new() { Name = "Category" },
                new() { Name = "Category2" },
                new() { Name = "Category3" },
                new() { Name = "Category4" },
                new() { Name = "Category5" },
            };
        }
        
        [TestCase(true, false)]
        [TestCase(false, false)]
        [TestCase(false, true)]
        public async Task VerifyComponent(bool isComponentEnabled, bool isComponentReadOnly)
        {
            this.component = this.context.RenderComponent<MultiComboBox<Category>>(parameter =>
            {
                parameter.Add(p => p.Data, this.availableCategories);
                parameter.Add(p => p.Values, this.availableCategories);
                parameter.Add(p => p.ShowCheckBoxes, true);
                parameter.Add(p => p.MaxNumberOfChips, 2);
                parameter.Add(p => p.Enabled, isComponentEnabled);
                parameter.Add(p => p.IsReadOnly, isComponentReadOnly);

                parameter.Add(p => p.EditorTextTemplate, builder =>
                {
                    builder.OpenElement(0, "span");
                    builder.AddContent(1, ""); 
                    builder.CloseElement();
                });

                parameter.Add(p => p.RowTemplate, value => value.Name);
            });
            
            Assert.Multiple(() =>
            {
                Assert.That(this.component.Instance.Data, Is.Not.Empty);
                Assert.That(this.component.Instance.Values, Is.Not.Empty);
                Assert.That(this.component.Instance.ShowCheckBoxes, Is.True);
                Assert.That(this.component.Instance.EditorTextTemplate, Is.Not.Null);
                Assert.That(this.component.Instance.MaxNumberOfChips, Is.EqualTo(2));
                Assert.That(this.component.Instance.RowTemplate, Is.Not.Null);
            });
            
            var comboBox = this.component.FindComponent<DxComboBox<Category, Category>>();
            
            Assert.Multiple(() =>
            {   
                Assert.That(comboBox, Is.Not.Null);
                Assert.That(comboBox.Instance.Value, Is.Null);
                Assert.That(comboBox.Instance.ReadOnly, Is.EqualTo(isComponentReadOnly));
                Assert.That(comboBox.Instance.Enabled, Is.EqualTo(isComponentEnabled));
            });
            
            await this.component.InvokeAsync(() => comboBox.Instance.ShowDropDown());
            
            var dropdownItems = this.component.FindAll(".item-template-checkbox");
            
            Assert.Multiple(() =>
            {
                Assert.That(dropdownItems, Is.Not.Null);
                Assert.That(dropdownItems, Is.Not.Empty);
                Assert.That(dropdownItems.Count, Is.EqualTo(this.availableCategories.Count)); 
            });
        }
    }
}