// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementScalesTableTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.Components.ReferenceData
{
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ReferenceData;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.ReferenceData;
    using COMETwebapp.ViewModels.Components.ReferenceData.MeasurementScales;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class MeasurementScalesTableTestFixture
    {
        private TestContext context;
        private Mock<IMeasurementScalesTableViewModel> viewModel;
        private Mock<IShowHideDeprecatedThingsService> showHideService;
        private MeasurementScale measurementScale1;
        private MeasurementScale measurementScale2;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();

            this.viewModel = new Mock<IMeasurementScalesTableViewModel>();
            this.showHideService = new Mock<IShowHideDeprecatedThingsService>();
            this.showHideService.Setup(x => x.ShowDeprecatedThings).Returns(true);

            this.measurementScale1 = new OrdinalScale()
            {
                Name = "A name",
                ShortName = "AName",
                Container = new SiteReferenceDataLibrary(){ ShortName = "rdl" },
                Unit = new SimpleUnit() { ShortName = "unit1" },
                NumberSet = NumberSetKind.NATURAL_NUMBER_SET,
                IsDeprecated = false
            };

            this.measurementScale2 = new OrdinalScale()
            {
                Name = "B name",
                ShortName = "BName",
                Container = new SiteReferenceDataLibrary() { ShortName = "rdl" },
                Unit = new SimpleUnit(){ ShortName = "unit2" },
                NumberSet = NumberSetKind.INTEGER_NUMBER_SET,
                IsDeprecated = true
            };

            var rows = new SourceList<MeasurementScaleRowViewModel>();
            rows.Add(new MeasurementScaleRowViewModel(this.measurementScale1));
            rows.Add(new MeasurementScaleRowViewModel(this.measurementScale2));

            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.MeasurementUnits).Returns([]);
            this.viewModel.Setup(x => x.NumberSetKinds).Returns([]);
            this.viewModel.Setup(x => x.ReferenceDataLibraries).Returns([]);
            this.viewModel.Setup(x => x.ShowHideDeprecatedThingsService).Returns(this.showHideService.Object);
            this.viewModel.Setup(x => x.IsOnDeprecationMode).Returns(true);
            this.viewModel.Setup(x => x.Thing).Returns(new OrdinalScale());

            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.ConfigureDevExpressBlazor();
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
            var renderer = this.context.RenderComponent<MeasurementScalesTable>();

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.ShouldCreateThing, Is.EqualTo(false));
                Assert.That(renderer.Instance.ViewModel, Is.Not.Null);
                Assert.That(renderer.Markup, Does.Contain(this.measurementScale1.Name));
                Assert.That(renderer.Markup, Does.Contain(this.measurementScale2.Name));
                this.viewModel.Verify(x => x.InitializeViewModel(), Times.Once);
            });
        }

        [Test]
        public async Task VerifyDeprecatingAndUndeprecatingMeasurementScale()
        {
            var renderer = this.context.RenderComponent<MeasurementScalesTable>();

            var deprecateButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "deprecateButton");
            await renderer.InvokeAsync(deprecateButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OnDeprecateUnDeprecateButtonClick(It.IsAny<MeasurementScaleRowViewModel>()), Times.Once);

            var unDeprecateButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "undeprecateButton");
            await renderer.InvokeAsync(unDeprecateButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OnDeprecateUnDeprecateButtonClick(It.IsAny<MeasurementScaleRowViewModel>()), Times.Exactly(2));
        }

        [Test]
        public async Task VerifyAddingOrEditingMeasurementScale()
        {
            var renderer = this.context.RenderComponent<MeasurementScalesTable>();

            var addMeasurementScaleButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addMeasurementScaleButton");
            await renderer.InvokeAsync(addMeasurementScaleButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.ShouldCreateThing, Is.EqualTo(true));
                Assert.That(this.viewModel.Object.Thing, Is.InstanceOf(typeof(MeasurementScale)));
            });
            
            var editMeasurementScaleButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editScaleButton");
            await renderer.InvokeAsync(editMeasurementScaleButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.ShouldCreateThing, Is.EqualTo(false));
                Assert.That(this.viewModel.Object.Thing, Is.InstanceOf(typeof(MeasurementScale)));
            });
        }
    }
}
