// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTableTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Components.ModelEditor
{
    using System.Collections.ObjectModel;

    using Bunit;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.ViewModels.Components.ModelEditor;
	using COMETwebapp.ViewModels.Components.SystemRepresentation;
	using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;
	using DevExpress.Blazor;
	using Microsoft.Extensions.DependencyInjection;
	using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ElementDefinitionTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ElementDefinitionTable> renderedComponent;
        private ElementDefinitionTable table;
        private ElementDefinitionDetailsViewModel elementDefinitionDetailsViewModel;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddSingleton<ISessionService, SessionService>();
            this.context.Services.AddSingleton<IDraggableElementService, DraggableElementService>();

            this.elementDefinitionDetailsViewModel = new ElementDefinitionDetailsViewModel();

            var elementDefinitionTableViewModel = new Mock<IElementDefinitionTableViewModel>();
            elementDefinitionTableViewModel.Setup(x => x.RowsTarget).Returns(new ObservableCollection<ElementDefinitionRowViewModel>() { new ElementDefinitionRowViewModel() { ElementDefinitionName = "Test" } });
            elementDefinitionTableViewModel.Setup(x => x.RowsSource).Returns(new ObservableCollection<ElementDefinitionRowViewModel>() { new ElementDefinitionRowViewModel() { ElementDefinitionName = "Test1" } });
            elementDefinitionTableViewModel.Setup(x => x.ElementDefinitionDetailsViewModel).Returns(this.elementDefinitionDetailsViewModel);

            this.context.Services.AddSingleton(elementDefinitionTableViewModel.Object);

            this.renderedComponent = this.context.RenderComponent<ElementDefinitionTable>();

            this.table = this.renderedComponent.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderedComponent, Is.Not.Null);
                Assert.That(this.table, Is.Not.Null);
                Assert.That(this.table.ViewModel, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyMoveGridRow()
        {
            Assert.That(() => this.table.MoveGridRow(1, 1, It.IsAny<bool>()), Throws.Nothing);
        }

		[Test]
		public async Task VerifyAddingElementDefinition()
		{
			var addButton = this.renderedComponent.FindComponents<DxButton>().First(x => x.Instance.Id == "addElementDefinition");

			Assert.That(addButton.Instance, Is.Not.Null);

			await this.renderedComponent.InvokeAsync(addButton.Instance.Click.InvokeAsync);
		}
	}
}
