// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MeasurementScalesTableViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.ReferenceData
{
    using AntDesign;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.ReferenceData.MeasurementScales;
    using COMETwebapp.Wrappers;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    using Result = FluentResults.Result;

    [TestFixture]
    public class MeasurementScalesTableViewModelTestFixture
    {
        private MeasurementScalesTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private Mock<ILogger<MeasurementScalesTableViewModel>> loggerMock;
        private CDPMessageBus messageBus;
        private Mock<IShowHideDeprecatedThingsService> showHideService;
        private Mock<INotificationService> notificationService;
        private MeasurementScale measurementScale;
        private SiteDirectory siteDirectory;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            this.showHideService = new Mock<IShowHideDeprecatedThingsService>();
            this.notificationService = new Mock<INotificationService>();
            this.messageBus = new CDPMessageBus();
            this.loggerMock = new Mock<ILogger<MeasurementScalesTableViewModel>>();

            this.measurementScale = new OrdinalScale
            {
                ShortName = "scale",
                Name = "scale",
                Unit = new SimpleUnit { ShortName = "simpleUnit" },
                NumberSet = NumberSetKind.INTEGER_NUMBER_SET
            };

            var siteReferenceDataLibrary = new SiteReferenceDataLibrary
            {
                ShortName = "rdl",
                Unit = { new SimpleUnit() },
                ParameterType = { new SimpleQuantityKind() },
                Scale =
                {
                    this.measurementScale,
                    new OrdinalScale
                    {
                        ValueDefinition = { new ScaleValueDefinition { Iid = Guid.NewGuid() } },
                        Name = "zname"
                    }
                }
            };

            this.siteDirectory = new SiteDirectory
            {
                ShortName = "siteDirectory"
            };

            this.siteDirectory.SiteReferenceDataLibrary.Add(siteReferenceDataLibrary);
            this.permissionService.Setup(x => x.CanWrite(this.measurementScale.ClassKind, this.measurementScale.Container)).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(this.siteDirectory);
            this.sessionService.Setup(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>())).Returns(Task.FromResult(new Result()));

            this.viewModel = new MeasurementScalesTableViewModel(this.sessionService.Object, this.showHideService.Object, this.messageBus, this.loggerMock.Object, this.notificationService.Object);
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
            this.viewModel.Thing.ValueDefinition.Add(new ScaleValueDefinition());

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(2));
                Assert.That(this.viewModel.Rows.Items.First().Thing.Iid, Is.EqualTo(this.measurementScale.Iid));
                Assert.That(this.viewModel.ReferenceQuantityKinds.Count(), Is.EqualTo(1));
                Assert.That(this.viewModel.ReferenceScaleValueDefinitions.Count(), Is.EqualTo(1));
                Assert.That(this.viewModel.MeasurementScales.Count(), Is.EqualTo(2));
                Assert.That(this.viewModel.MeasurementUnits.Count(), Is.EqualTo(1));
                Assert.That(this.viewModel.NumberSetKinds.Count(), Is.EqualTo(Enum.GetValues<NumberSetKind>().Length));
                Assert.That(this.viewModel.LogarithmBaseKinds.Count(), Is.EqualTo(Enum.GetValues<LogarithmBaseKind>().Length));
            });
        }

        [Test]
        public async Task VerifyMeasurementScaleAddOrEdit()
        {
            this.viewModel.InitializeViewModel();
            this.viewModel.SelectedMeasurementScaleType = new ClassKindWrapper(ClassKind.LogarithmicScale);

            var scaleValueDefinition = new ScaleValueDefinition
            {
                ShortName = "valueDefinition",
                Value = "val"
            };

            var mappingToReferenceScale = new MappingToReferenceScale
            {
                DependentScaleValue = scaleValueDefinition,
                ReferenceScaleValue = new ScaleValueDefinition()
            };

            this.viewModel.Thing.ValueDefinition.Add(scaleValueDefinition);
            this.viewModel.Thing.MappingToReferenceScale.Add(mappingToReferenceScale);
            this.viewModel.SelectedReferenceQuantityValue.Value = "value";
            this.viewModel.SelectedReferenceQuantityValue.Scale = new OrdinalScale();
            Assert.That(((LogarithmicScale)this.viewModel.Thing).ReferenceQuantityValue, Has.Count.EqualTo(0));

            await this.viewModel.CreateOrEditMeasurementScale(true);

            Assert.Multiple(() =>
            {
                this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<ReferenceDataLibrary>(), It.IsAny<List<Thing>>()), Times.Once);
                Assert.That(((LogarithmicScale)this.viewModel.Thing).ReferenceQuantityValue, Has.Count.EqualTo(1));
            });

            var exceptionalError = new ExceptionalError(new InvalidDataException("Invalid data"));
            var regularError = new Error("Failure");
            this.viewModel.SelectedReferenceQuantityValue.Value = string.Empty;

            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>()))
                .Returns(Task.FromResult(new Result { Reasons = { regularError, exceptionalError } }));

            await this.viewModel.CreateOrEditMeasurementScale(true);

            Assert.Multiple(() =>
            {
                this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<ReferenceDataLibrary>(), It.IsAny<List<Thing>>()), Times.Exactly(2));
                Assert.That(((LogarithmicScale)this.viewModel.Thing).ReferenceQuantityValue, Has.Count.EqualTo(0));
            });

            ((LogarithmicScale)this.viewModel.Thing).ReferenceQuantityValue.Add(new ScaleReferenceQuantityValue { Value = "val" });
            this.viewModel.SelectedReferenceQuantityValue.Value = "value";
            this.viewModel.SelectedReferenceQuantityValue.Scale = new OrdinalScale();
            await this.viewModel.CreateOrEditMeasurementScale(true);

            Assert.Multiple(() =>
            {
                this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<ReferenceDataLibrary>(), It.IsAny<List<Thing>>()), Times.Exactly(3));
                Assert.That(((LogarithmicScale)this.viewModel.Thing).ReferenceQuantityValue, Has.Count.EqualTo(1));
            });

            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>())).Throws(new Exception());
            await this.viewModel.CreateOrEditMeasurementScale(false);
            this.loggerMock.Verify(LogLevel.Error, x => !string.IsNullOrWhiteSpace(x.ToString()), Times.Once());
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
        public void VerifyMeasurementScaleSelection()
        {
            this.viewModel.InitializeViewModel();
            this.viewModel.SelectedMeasurementScaleType = new ClassKindWrapper(ClassKind.OrdinalScale);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Thing, Is.TypeOf<OrdinalScale>());
                Assert.That(this.viewModel.SelectedMeasurementScaleType.ClassKind, Is.EqualTo(ClassKind.OrdinalScale));
            });

            this.viewModel.SelectedMeasurementScaleType = new ClassKindWrapper(ClassKind.CyclicRatioScale);
            Assert.That(this.viewModel.Thing, Is.TypeOf<CyclicRatioScale>());

            this.viewModel.SelectedMeasurementScaleType = new ClassKindWrapper(ClassKind.IntervalScale);
            Assert.That(this.viewModel.Thing, Is.TypeOf<IntervalScale>());

            this.viewModel.SelectedMeasurementScaleType = new ClassKindWrapper(ClassKind.RatioScale);
            Assert.That(this.viewModel.Thing, Is.TypeOf<RatioScale>());

            this.viewModel.SelectedMeasurementScaleType = new ClassKindWrapper(ClassKind.LogarithmicScale);
            Assert.That(this.viewModel.Thing, Is.TypeOf<LogarithmicScale>());

            this.viewModel.SelectedMeasurementScaleType = new ClassKindWrapper(ClassKind.SimpleUnit);
            Assert.That(this.viewModel.Thing, Is.TypeOf<LogarithmicScale>());

            var measurementScaleToSet = new LogarithmicScale();
            this.viewModel.SelectMeasurementScale(measurementScaleToSet);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Thing, Is.EqualTo(measurementScaleToSet));
                Assert.That(this.viewModel.Thing.ValueDefinition, Is.EqualTo(measurementScaleToSet.ValueDefinition));
                Assert.That(this.viewModel.Thing.MappingToReferenceScale, Is.EqualTo(measurementScaleToSet.MappingToReferenceScale));
            });
        }

        [Test]
        public void VerifySessionRefresh()
        {
            this.viewModel.InitializeViewModel();

            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);
            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(2));

            var siteReferenceDataLibrary = new SiteReferenceDataLibrary
            {
                ShortName = "newShortname"
            };

            var scaleTest = new OrdinalScale
            {
                Iid = Guid.NewGuid(),
                Container = siteReferenceDataLibrary,
                Unit = new SimpleUnit { ShortName = "newSimpleUnit" },
                NumberSet = NumberSetKind.INTEGER_NUMBER_SET
            };

            this.messageBus.SendObjectChangeEvent(scaleTest, EventKind.Added);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items.First().Thing, EventKind.Removed);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            this.messageBus.SendObjectChangeEvent(this.viewModel.Rows.Items.First().Thing, EventKind.Updated);
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            Assert.That(this.viewModel.Rows, Has.Count.EqualTo(1));

            this.messageBus.SendObjectChangeEvent(siteReferenceDataLibrary, EventKind.Updated);
            this.messageBus.SendObjectChangeEvent(new PersonRole(), EventKind.Updated);
            this.messageBus.SendMessage(new SessionEvent(null, SessionStatus.EndUpdate));

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Items.First().ContainerName, Is.EqualTo(siteReferenceDataLibrary.ShortName));
                this.permissionService.Verify(x => x.CanWrite(scaleTest.ClassKind, It.IsAny<Thing>()), Times.AtLeast(this.viewModel.Rows.Count));
            });
        }
    }
}
