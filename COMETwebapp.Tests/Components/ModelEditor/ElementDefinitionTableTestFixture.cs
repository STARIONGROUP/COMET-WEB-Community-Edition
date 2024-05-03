// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTableTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.ModelEditor
{
    using Bunit;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.ViewModels.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.ModelEditor.AddParameterViewModel;
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
        private Mock<IElementDefinitionTableViewModel> elementDefinitionTableViewModel;
        private Mock<IAddParameterViewModel> addParameterViewModel;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddSingleton<ISessionService, SessionService>();
            this.context.Services.AddSingleton<IDraggableElementService, DraggableElementService>();
            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());
            this.context.Services.AddSingleton(configuration.Object);

            this.elementDefinitionDetailsViewModel = new ElementDefinitionDetailsViewModel();

            this.addParameterViewModel = new Mock<IAddParameterViewModel>();
            this.addParameterViewModel.Setup(x => x.ParameterTypeSelectorViewModel).Returns(new ParameterTypeSelectorViewModel());

            this.elementDefinitionTableViewModel = new Mock<IElementDefinitionTableViewModel>();
            this.elementDefinitionTableViewModel.Setup(x => x.RowsTarget).Returns([new ElementDefinitionRowViewModel { ElementDefinitionName = "Test" }]);
            this.elementDefinitionTableViewModel.Setup(x => x.RowsSource).Returns([new ElementDefinitionRowViewModel { ElementDefinitionName = "Test1" }]);
            this.elementDefinitionTableViewModel.Setup(x => x.ElementDefinitionDetailsViewModel).Returns(this.elementDefinitionDetailsViewModel);
            this.elementDefinitionTableViewModel.Setup(x => x.AddParameterViewModel).Returns(this.addParameterViewModel.Object);
            this.elementDefinitionTableViewModel.Setup(x => x.SelectedElementDefinition).Returns(new ElementDefinition());

            this.context.Services.AddSingleton(this.elementDefinitionTableViewModel.Object);

            this.renderedComponent = this.context.RenderComponent<ElementDefinitionTable>();

            this.table = this.renderedComponent.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyAddingElementDefinition()
        {
            var addButton = this.renderedComponent.FindComponents<DxButton>().First(x => x.Instance.Id == "addElementDefinition");
            Assert.That(addButton.Instance, Is.Not.Null);
            await this.renderedComponent.InvokeAsync(addButton.Instance.Click.InvokeAsync);
        }

        [Test]
        public async Task VerifyAddParameter()
        {
            this.elementDefinitionTableViewModel.Setup(x => x.IsOnAddingParameterMode).Returns(true);

            var addButton = this.renderedComponent.FindComponents<DxButton>().First(x => x.Instance.Id == "addParameter");
            Assert.That(addButton.Instance, Is.Not.Null);

            await this.renderedComponent.InvokeAsync(addButton.Instance.Click.InvokeAsync);
            this.elementDefinitionTableViewModel.Verify(x => x.OpenAddParameterPopup(), Times.Once);

            var addParameterComponent = this.renderedComponent.FindComponent<AddParameter>();
            Assert.That(addParameterComponent.Instance, Is.Not.Null);
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
    }
}
