// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelBodyTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.EngineeringModel
{
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.EngineeringModel;
    using COMETwebapp.ViewModels.Components.EngineeringModel;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Options;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class EngineeringModelBodyTestFixture
    {
        private TestContext context;
        private IRenderedComponent<EngineeringModelBody> renderer;
        private Mock<IEngineeringModelBodyViewModel> viewModel;
        private Mock<IOptionsTableViewModel> optionsTableViewModel;
        private Iteration iteration;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();

            this.viewModel = new Mock<IEngineeringModelBodyViewModel>();
            this.iteration = new Iteration();

            this.viewModel.Setup(x => x.CurrentThing).Returns(this.iteration);

            this.optionsTableViewModel = new Mock<IOptionsTableViewModel>();
            this.optionsTableViewModel.Setup(x => x.Rows).Returns(new SourceList<OptionRowViewModel>());
            this.viewModel.Setup(x => x.OptionsTableViewModel).Returns(this.optionsTableViewModel.Object);

            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());

            this.context.Services.AddSingleton(configuration.Object);
            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<EngineeringModelBody>();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ViewModel, Is.Not.Null);
                Assert.That(this.viewModel.Object.CurrentThing, Is.EqualTo(this.iteration));
            });
        }

        [Test]
        public async Task VerifySelectComponent()
        {
            var toolbarItem = this.renderer.FindComponent<DxToolbarItem>();
            await this.renderer.InvokeAsync(toolbarItem.Instance.Click.InvokeAsync);
            Assert.That(this.renderer.Instance.SelectedComponent, Is.Not.Null);
        }
    }
}
