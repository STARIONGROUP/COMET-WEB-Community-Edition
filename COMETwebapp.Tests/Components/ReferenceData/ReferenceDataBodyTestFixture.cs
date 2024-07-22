// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ReferenceDataBodyTestFixture.cs" company="Starion Group S.A.">
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
    using Bunit;

    using CDP4Dal;

    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ReferenceData;
    using COMETwebapp.Components.ReferenceData.MeasurementScales;
    using COMETwebapp.Components.ReferenceData.ParameterTypes;
    using COMETwebapp.ViewModels.Components.ReferenceData;
    using COMETwebapp.ViewModels.Components.ReferenceData.MeasurementScales;
    using COMETwebapp.ViewModels.Components.ReferenceData.ParameterTypes;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DynamicData;

    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ReferenceDataBodyTestFixture
    {
        private TestContext context;
        private Mock<IParameterTypeTableViewModel> parameterTypesTableViewModel;
        private Mock<IMeasurementScalesTableViewModel> measurementScalesTableViewModel;
        private Mock<ISessionService> sessionService;
        private IRenderedComponent<ReferenceDataBody> renderer;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();

            this.parameterTypesTableViewModel = new Mock<IParameterTypeTableViewModel>();
            this.parameterTypesTableViewModel.Setup(x => x.Rows).Returns(new SourceList<ParameterTypeRowViewModel>());

            this.measurementScalesTableViewModel = new Mock<IMeasurementScalesTableViewModel>();
            this.measurementScalesTableViewModel.Setup(x => x.Rows).Returns(new SourceList<MeasurementScaleRowViewModel>());

            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.Session).Returns(new Mock<ISession>().Object);

            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());

            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddSingleton(this.sessionService.Object);
            this.context.Services.AddSingleton(this.parameterTypesTableViewModel.Object);
            this.context.Services.AddSingleton(this.measurementScalesTableViewModel.Object);
            this.context.Services.AddSingleton(configuration.Object);
            this.context.Services.AddSingleton(new Mock<IReferenceDataBodyViewModel>().Object);

            this.renderer = this.context.RenderComponent<ReferenceDataBody>();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyReferenceDataBody()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance, Is.Not.Null);
                Assert.That(this.renderer.Instance.SelectedComponent, Is.EqualTo(typeof(ParameterTypeTable)));
                this.parameterTypesTableViewModel.Verify(x => x.InitializeViewModel(), Times.Once);
            });

            var toolBarItem = this.renderer.FindAll("button").ElementAt(1);
            await this.renderer.InvokeAsync(() => toolBarItem.ClickAsync(new MouseEventArgs()));

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.SelectedComponent, Is.EqualTo(typeof(MeasurementScalesTable)));
                this.measurementScalesTableViewModel.Verify(x => x.InitializeViewModel(), Times.Once);
            });
        }
    }
}
