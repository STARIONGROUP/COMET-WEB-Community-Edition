// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MultiComboBoxTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMET.Web.Common.Tests.Components
{
    using Bunit;

    using CDP4Common.DTO;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Test.Helpers;

    using DevExpress.Blazor;

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
                new() { Name = "Category" }
            };

            this.component = this.context.RenderComponent<MultiComboBox<Category>>(parameter =>
            {
                parameter.Add(p => p.Data, this.availableCategories);
                parameter.Add(p => p.Values, this.availableCategories);
                parameter.Add(p => p.ShowCheckBoxes, true);
                parameter.Add(p => p.MaxNumberOfChips, 2);
                parameter.Add(p => p.Enabled, true);

                parameter.Add(p => p.EditorTextTemplate, builder =>
                {
                    builder.OpenElement(0, "span");
                    builder.AddContent(1, ""); 
                    builder.CloseElement();
                });

                parameter.Add(p => p.RowTemplate, value => value.Name);
            });
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.IsTrue(this.component.Instance.Enabled);
                Assert.IsNotEmpty(this.component.Instance.Data);
                Assert.IsNotEmpty(this.component.Instance.Values);
                Assert.IsTrue(this.component.Instance.ShowCheckBoxes);
                Assert.IsNotNull(this.component.Instance.EditorTextTemplate);
            });
            
            this.component.Render();

            var comboBox = this.component.FindComponent<DxComboBox<Category, Category>>();
            Assert.IsNotNull(comboBox);
        }
    }
}