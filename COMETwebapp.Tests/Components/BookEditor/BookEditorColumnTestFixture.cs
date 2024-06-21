// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BookEditorColumnTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.BookEditor
{
    using Bunit;

    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.BookEditor;
    using COMETwebapp.Services.Interoperability;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;
    
    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class BookEditorColumnTestFixture
    {
        private TestContext context;
        private IRenderedComponent<BookEditorColumn<Book>> component;
        private Mock<IDomDataService> domDataService;
        private Mock<ILogger<BookEditorColumn<Book>>> loggerMock;
        private bool editIsClicked;
        private bool deleteIsClicked;
        private bool isCollapsed;
        private bool addNewItemIsClicked;
        private Book selectedBook;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.domDataService = new Mock<IDomDataService>();
            this.loggerMock = new Mock<ILogger<BookEditorColumn<Book>>>();

            this.context.Services.AddSingleton(this.domDataService.Object);
            this.context.Services.AddSingleton(this.loggerMock.Object);

            var book = new Book()
            {
                Name = "Book Example",
                ShortName = "BookExample",
                Owner = new DomainOfExpertise
                {
                    Name = "Sys",
                    ShortName = "sys"
                }
            };

            var onEditClicked = new EventCallbackFactory().Create<Book>(this, () => { this.editIsClicked = true;});
            var onDeleteClicked = new EventCallbackFactory().Create<Book>(this, () => { this.deleteIsClicked = true; });
            var onSelectedValueClicked = new EventCallbackFactory().Create<Book>(this, (b) => { this.selectedBook = b; });
            var onCollapseClicked = new EventCallbackFactory().Create(this, () => this.isCollapsed = true);
            var onAddNewItemClicked = new EventCallbackFactory().Create(this, () => this.addNewItemIsClicked = true);
            
            this.component = this.context.RenderComponent<BookEditorColumn<Book>>(parameters =>
            {
                parameters.Add(p => p.CollapseButtonIconClass, "icon-class");
                parameters.Add(p => p.HeaderTitle, "TestColumn");
                parameters.Add(p => p.HeaderHexColor, "#CCC");
                parameters.Add(p => p.Items, new List<Book>() { book });
                parameters.Add(p => p.OnEditClicked, onEditClicked);
                parameters.Add(p => p.OnDeleteClicked, onDeleteClicked);
                parameters.Add(p => p.SelectedValueChanged, onSelectedValueClicked);
                parameters.Add(p => p.CssClass, "node");
                parameters.Add(p => p.OnCollapseClicked, onCollapseClicked);
                parameters.Add(p => p.OnCreateNewItemClick, onAddNewItemClicked);
            });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyComponent()
        {
            var collapseButton = this.component.Find(".collapse-button");
            collapseButton.Click();

            Assert.That(this.isCollapsed, Is.True);

            var addItemButton = this.component.Find(".add-item-button");
            addItemButton.Click();

            Assert.That(this.addNewItemIsClicked, Is.True);

            var header = this.component.Find(".header-text");
            
            Assert.Multiple(() =>
            {
                Assert.That(header.Attributes["class"]?.Value, Does.Contain("header-text"));
                Assert.That(header.HasAttribute("style"), Is.True);
            });

            var nodeButton = this.component.Find(".node-button");
            nodeButton.Click();

            Assert.Multiple(() =>
            {
                Assert.That(this.selectedBook, Is.Not.Null);
                Assert.That(this.component.Instance.SelectedValue, Is.Not.Null);
            });

            var editButton = this.component.Find(".icon-edit");
            var deleteButton = this.component.Find(".icon-trash");

            editButton.Click();
            deleteButton.Click();

            Assert.Multiple(() =>
            {
                Assert.That(this.editIsClicked, Is.True);
                Assert.That(this.deleteIsClicked, Is.True);
            });
        }

        [Test]
        public async Task VerifyOnSizeChangedAndScroll()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.component.Instance.OnScroll, Throws.Nothing);
                Assert.That(this.component.Instance.OnSizeChanged, Throws.Nothing);
            });
            
            this.domDataService.Setup(x => x.GetElementSizeAndPosition(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).Throws(new Exception());
            await this.component.Instance.OnSizeChanged();
            this.loggerMock.Verify(LogLevel.Warning, x => !string.IsNullOrWhiteSpace(x.ToString()), Times.Once());
        }

        [Test]
        public async Task VerifyGeneratePathPoints()
        {
            this.domDataService.Setup(x => x.GetElementSizeAndPosition(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(new float[]{1,2,3,4,5}));
            await this.component.Instance.OnSizeChanged();
            this.component.Render();

            var polyline = this.component.Find("#pathPoints");
            Assert.That(polyline.GetAttribute("points"), Is.EqualTo("1,4,2,4,2,4,3,4"));
        }
    }
}

