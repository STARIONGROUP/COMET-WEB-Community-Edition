// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MeasurementUnitsTableTestFixture.cs" company="Starion Group S.A.">
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

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ReferenceData.MeasurementUnits;
    using COMETwebapp.Components.SiteDirectory;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.ReferenceData.MeasurementUnits;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;
    using COMETwebapp.Wrappers;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class MeasurementUnitsTableTestFixture
    {
        private TestContext context;
        private Mock<IMeasurementUnitsTableViewModel> viewModel;
        private Mock<IShowHideDeprecatedThingsService> showHideService;
        private MeasurementUnit measurementUnit1;
        private MeasurementUnit measurementUnit2;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();

            this.viewModel = new Mock<IMeasurementUnitsTableViewModel>();
            this.showHideService = new Mock<IShowHideDeprecatedThingsService>();
            this.showHideService.Setup(x => x.ShowDeprecatedThings).Returns(true);

            this.measurementUnit1 = new SimpleUnit
            {
                Name = "A name",
                ShortName = "AName",
                Container = new SiteReferenceDataLibrary { ShortName = "rdl" },
                IsDeprecated = false
            };

            this.measurementUnit2 = new SimpleUnit
            {
                Name = "B name",
                ShortName = "BName",
                Container = new SiteReferenceDataLibrary { ShortName = "rdl" },
                IsDeprecated = true
            };

            var rows = new SourceList<MeasurementUnitRowViewModel>();
            rows.Add(new MeasurementUnitRowViewModel(this.measurementUnit1));
            rows.Add(new MeasurementUnitRowViewModel(this.measurementUnit2));

            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.ShowHideDeprecatedThingsService).Returns(this.showHideService.Object);
            this.viewModel.Setup(x => x.CurrentThing).Returns(new SimpleUnit());
            this.viewModel.Setup(x => x.MeasurementUnitTypes).Returns([new ClassKindWrapper(ClassKind.SimpleUnit)]);

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
        public async Task VerifyAddingOrEditingMeasurementUnit()
        {
            var renderer = this.context.RenderComponent<MeasurementUnitsTable>();

            var addMeasurementUnitButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "dataItemDetailsButton");
            await renderer.InvokeAsync(addMeasurementUnitButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.ShouldCreateThing, Is.EqualTo(true));
                Assert.That(this.viewModel.Object.CurrentThing, Is.InstanceOf(typeof(SimpleUnit)));
            });

            var unitsGrid = renderer.FindComponent<DxGrid>();
            await renderer.InvokeAsync(() => unitsGrid.Instance.SelectedDataItemChanged.InvokeAsync(new MeasurementUnitRowViewModel(this.measurementUnit1)));
            Assert.That(renderer.Instance.IsOnEditMode, Is.EqualTo(true));

            var unitsForm = renderer.FindComponent<MeasurementUnitsForm>();
            var unitsEditForm = unitsForm.FindComponent<EditForm>();
            await unitsForm.InvokeAsync(unitsEditForm.Instance.OnValidSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.CreateOrEditMeasurementUnit(false), Times.Once);
                Assert.That(this.viewModel.Object.CurrentThing, Is.InstanceOf(typeof(SimpleUnit)));
            });

            var form = renderer.FindComponent<DxGrid>();
            await renderer.InvokeAsync(form.Instance.EditModelSaving.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditMeasurementUnit(false), Times.Once);
        }

        [Test]
        public void VerifyOnInitialized()
        {
            var renderer = this.context.RenderComponent<MeasurementUnitsTable>();

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.ShouldCreateThing, Is.EqualTo(false));
                Assert.That(renderer.Instance.ViewModel, Is.Not.Null);
                Assert.That(renderer.Markup, Does.Contain(this.measurementUnit1.Name));
                Assert.That(renderer.Markup, Does.Contain(this.measurementUnit2.Name));
                this.viewModel.Verify(x => x.InitializeViewModel(), Times.Once);
            });
        }
    }
}
