// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MeasurementScalesTableViewModelTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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

namespace COMETwebapp.Tests.ViewModels.Components.ReferenceData
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.ReferenceData.MeasurementScales;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class MeasurementScalesTableViewModelTestFixture
    {
        private MeasurementScalesTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private Assembler assembler;
        private Mock<ILogger<MeasurementScalesTableViewModel>> loggerMock;
        private CDPMessageBus messageBus;
        private Mock<IShowHideDeprecatedThingsService> showHideService;
        private MeasurementScale measurementScale;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            this.showHideService = new Mock<IShowHideDeprecatedThingsService>();
            this.messageBus = new CDPMessageBus();
            this.loggerMock = new Mock<ILogger<MeasurementScalesTableViewModel>>();

            this.measurementScale = new OrdinalScale()
            {
                ShortName = "scale",
                Name = "scale",
                Unit = new SimpleUnit(){ ShortName = "simpleUnit" },
                NumberSet = NumberSetKind.INTEGER_NUMBER_SET
            };

            var siteReferenceDataLibrary = new SiteReferenceDataLibrary()
            {
                ShortName = "rdl",
            };

            var siteDirectory = new SiteDirectory()
            {
                ShortName = "siteDirectory"
            };

            siteReferenceDataLibrary.Scale.Add(this.measurementScale);
            siteDirectory.SiteReferenceDataLibrary.Add(siteReferenceDataLibrary);

            this.assembler = new Assembler(new Uri("http://localhost:5000/"), this.messageBus);
            var lazyMeasurementScale = new Lazy<Thing>(this.measurementScale);
            this.assembler.Cache.TryAdd(new CacheKey(), lazyMeasurementScale);

            this.permissionService.Setup(x => x.CanWrite(this.measurementScale.ClassKind, this.measurementScale.Container)).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            session.Setup(x => x.Assembler).Returns(this.assembler);
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);

            this.viewModel = new MeasurementScalesTableViewModel(this.sessionService.Object, this.showHideService.Object, this.messageBus, this.loggerMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            this.viewModel.InitializeViewModel();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(1));
                Assert.That(this.viewModel.Rows.Items.First().Thing, Is.EqualTo(this.measurementScale));
            });
        }

        [Test]
        public void VerifyMeasurementScaleRowProperties()
        {
            this.viewModel.InitializeViewModel();
            var measurementScaleRow = this.viewModel.Rows.Items.First();

            Assert.Multiple(() =>
            {
                Assert.That(measurementScaleRow.ContainerName, Is.EqualTo("rdl"));
                Assert.That(measurementScaleRow.Name, Is.EqualTo(this.measurementScale.Name));
                Assert.That(measurementScaleRow.ShortName, Is.EqualTo(this.measurementScale.ShortName));
                Assert.That(measurementScaleRow.Thing, Is.EqualTo(this.measurementScale));
                Assert.That(measurementScaleRow.IsAllowedToWrite, Is.EqualTo(true));
                Assert.That(measurementScaleRow.Type, Is.EqualTo(nameof(OrdinalScale)));
            });
        }

        [Test]
        public void VerifySessionRefresh()
        {
            this.viewModel.InitializeViewModel();

            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            var siteReferenceDataLibrary = new SiteReferenceDataLibrary()
            {
                ShortName = "newShortname"
            };

            var scaleTest = new OrdinalScale()
            {
                Iid = Guid.NewGuid(),
                Container = siteReferenceDataLibrary,
                Unit = new SimpleUnit() { ShortName = "newSimpleUnit" },
                NumberSet = NumberSetKind.INTEGER_NUMBER_SET
            };

            this.messageBus.SendObjectChangeEvent(scaleTest, EventKind.Added);
            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items.First().Thing, EventKind.Removed);
            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items.First().Thing, EventKind.Updated);
            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);

            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            this.messageBus.SendObjectChangeEvent(siteReferenceDataLibrary, EventKind.Updated);
            this.messageBus.SendObjectChangeEvent(new PersonRole(), EventKind.Updated);
            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Items.First().ContainerName, Is.EqualTo(siteReferenceDataLibrary.ShortName));
                this.permissionService.Verify(x => x.CanWrite(scaleTest.ClassKind, It.IsAny<Thing>()), Times.AtLeast(this.viewModel.Rows.Count));
            });
        }
        
        [Test]
         public async Task VerifyRowOperations()
         {
             this.viewModel.InitializeViewModel();
             var measurementScaleRow = this.viewModel.Rows.Items.First();
             measurementScaleRow.IsDeprecated = false;

             Assert.Multiple(() =>
             {
                 Assert.That(measurementScaleRow, Is.Not.Null);
                 Assert.That(this.viewModel.IsOnDeprecationMode, Is.EqualTo(false));
             });

             this.viewModel.OnDeprecateUnDeprecateButtonClick(measurementScaleRow);

             Assert.Multiple(() =>
             {
                 Assert.That(this.viewModel.IsOnDeprecationMode, Is.EqualTo(true));
                 Assert.That(this.viewModel.Thing, Is.EqualTo(measurementScaleRow.Thing));
             });
             
             this.viewModel.OnCancelPopupButtonClick();
             Assert.That(this.viewModel.IsOnDeprecationMode, Is.EqualTo(false));

             await this.viewModel.OnConfirmPopupButtonClick();
             this.sessionService.Verify(x => x.UpdateThings(It.IsAny<SiteDirectory>(), It.Is<MeasurementScale>(c => c.IsDeprecated == true)));

             this.viewModel.Thing.IsDeprecated = true;
             await this.viewModel.OnConfirmPopupButtonClick();
             this.sessionService.Verify(x => x.UpdateThings(It.IsAny<SiteDirectory>(), It.Is<MeasurementScale>(c => c.IsDeprecated == false)));
        }
    }
}
