// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CategoryHierarchyDiagramTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Tests.Components.ReferenceData
{
    using Blazor.Diagrams;
    using Blazor.Diagrams.Components;
    using Blazor.Diagrams.Core.Geometry;

    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ReferenceData.Categories;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.ViewModels.Components.ReferenceData.Categories;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class CategoryHierarchyDiagramTestFixture
    {
        private TestContext context;
        private IRenderedComponent<CategoryHierarchyDiagram> renderer;
        private Mock<IJsUtilitiesService> jsUtilities;
        private Category category;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.category = new Category()
            {
                Name = "name",
                ShortName = "shortname",
                SuperCategory = [new Category()]
            };

            this.jsUtilities = new Mock<IJsUtilitiesService>();
            this.jsUtilities.Setup(x => x.GetItemDimensions(It.IsAny<string>())).ReturnsAsync([1, 2]);

            this.context.JSInterop.Setup<DiagramCanvas>("ZBlazorDiagrams.observe", _ => true).SetResult(new DiagramCanvas());
            this.context.JSInterop.Setup<Rectangle>("ZBlazorDiagrams.getBoundingClientRect", _ => true).SetResult(new Rectangle(1.0, 1.1, 1.1, 1.1));
            this.context.Services.AddSingleton(this.jsUtilities.Object);

            this.renderer = this.context.RenderComponent<CategoryHierarchyDiagram>(parameters =>
            {
                parameters.Add(p => p.Category, this.category);
            });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance, Is.Not.Null);
                Assert.That(this.renderer.Instance.IsOnDetailsMode, Is.EqualTo(false));
                Assert.That(this.category.SuperCategory, Has.Count.EqualTo(1));
                Assert.That(this.renderer.Instance.Diagram.Nodes, Has.Count.EqualTo(2));
                this.jsUtilities.Verify(x => x.GetItemDimensions(It.IsAny<string>()), Times.Once);
            });

            var detailsButton = this.renderer.FindComponent<DxButton>();
            await this.renderer.InvokeAsync(detailsButton.Instance.Click.InvokeAsync);
            Assert.That(this.renderer.Instance.IsOnDetailsMode, Is.EqualTo(true));
        }
    }
}
