// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypesTableViewModelTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.ReferenceData.ParameterTypes;
    using COMETwebapp.Wrappers;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    using Result = FluentResults.Result;

    [TestFixture]
    public class ParameterTypesTableViewModelTestFixture
    {
        private ParameterTypeTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private Mock<ILogger<ParameterTypeTableViewModel>> loggerMock;
        private CDPMessageBus messageBus;
        private Mock<IShowHideDeprecatedThingsService> showHideService;
        private ParameterType parameterType;
        private SiteDirectory siteDirectory;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            this.showHideService = new Mock<IShowHideDeprecatedThingsService>();
            this.messageBus = new CDPMessageBus();
            this.loggerMock = new Mock<ILogger<ParameterTypeTableViewModel>>();

            this.parameterType = new BooleanParameterType
            {
                Iid = Guid.NewGuid(),
                ShortName = "parameterType",
                Name = "parameter type"
            };

            var siteReferenceDataLibrary = new SiteReferenceDataLibrary
            {
                ShortName = "rdl",
                Unit = { new SimpleUnit() },
                ParameterType =
                {
                    this.parameterType,
                    new SimpleQuantityKind
                    {
                        Name = "zname"
                    }
                },
                Scale =
                {
                    new OrdinalScale
                    {
                        Unit = new SimpleUnit()
                    }
                }
            };

            this.siteDirectory = new SiteDirectory
            {
                ShortName = "siteDirectory"
            };

            this.siteDirectory.SiteReferenceDataLibrary.Add(siteReferenceDataLibrary);

            this.permissionService.Setup(x => x.CanWrite(this.parameterType.ClassKind, this.parameterType.Container)).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(this.siteDirectory);
            this.sessionService.Setup(x => x.CreateOrUpdateThings(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>())).Returns(Task.FromResult(new Result()));

            this.viewModel = new ParameterTypeTableViewModel(this.sessionService.Object, this.showHideService.Object, this.messageBus, this.loggerMock.Object);
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
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(2));
                Assert.That(this.viewModel.Rows.Items.First().Thing, Is.EqualTo(this.parameterType));
                Assert.That(this.viewModel.ReferenceDataLibraries, Is.EqualTo(this.siteDirectory.SiteReferenceDataLibrary));
                Assert.That(this.viewModel.MeasurementScales.Count(), Is.EqualTo(1));
            });

            this.viewModel.CurrentThing = new SpecializedQuantityKind
            {
                General = new SimpleQuantityKind()
            };

            this.viewModel.InitializeViewModel();
            Assert.That(this.viewModel.MeasurementScales.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task VerifyMeasurementScaleAddOrEdit()
        {
            this.viewModel.InitializeViewModel();

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.SampledFunctionParameterType);
            await this.viewModel.CreateOrEditParameterType(true);
            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<ReferenceDataLibrary>(), It.IsAny<List<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.EnumerationParameterType);
            await this.viewModel.CreateOrEditParameterType(true);
            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<ReferenceDataLibrary>(), It.IsAny<List<Thing>>(), It.IsAny<NotificationDescription>()), Times.Exactly(2));

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.CompoundParameterType);
            this.viewModel.CurrentThing = this.viewModel.CurrentThing.Clone(false);
            await this.viewModel.CreateOrEditParameterType(true);
            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<ReferenceDataLibrary>(), It.IsAny<List<Thing>>(), It.IsAny<NotificationDescription>()), Times.Exactly(3));

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.DerivedQuantityKind);
            this.viewModel.CurrentThing = this.viewModel.CurrentThing.Clone(true);
            await this.viewModel.CreateOrEditParameterType(false);
            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<ReferenceDataLibrary>(), It.IsAny<List<Thing>>(), It.IsAny<NotificationDescription>()), Times.Exactly(4));

            this.sessionService.Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<ReferenceDataLibrary>(), It.IsAny<List<Thing>>(), It.IsAny<NotificationDescription>())).Throws(new Exception("Error"));
            await this.viewModel.CreateOrEditParameterType(false);
            this.loggerMock.Verify(LogLevel.Error, x => !string.IsNullOrWhiteSpace(x.ToString()), Times.Once());
        }

        [Test]
        public void VerifyParameterTypeRowProperties()
        {
            this.viewModel.InitializeViewModel();
            var parameterTypeRowViewModel = this.viewModel.Rows.Items.First();

            Assert.Multiple(() =>
            {
                Assert.That(parameterTypeRowViewModel.ContainerName, Is.EqualTo("rdl"));
                Assert.That(parameterTypeRowViewModel.Name, Is.EqualTo(this.parameterType.Name));
                Assert.That(parameterTypeRowViewModel.ShortName, Is.EqualTo(this.parameterType.ShortName));
                Assert.That(parameterTypeRowViewModel.Thing, Is.EqualTo(this.parameterType));
                Assert.That(parameterTypeRowViewModel.IsAllowedToWrite, Is.EqualTo(true));
                Assert.That(parameterTypeRowViewModel.Type, Is.EqualTo(nameof(BooleanParameterType)));
            });
        }

        [Test]
        public void VerifyParameterTypeSelection()
        {
            this.viewModel.InitializeViewModel();
            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.BooleanParameterType);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CurrentThing, Is.TypeOf<BooleanParameterType>());
                Assert.That(this.viewModel.SelectedParameterType.ClassKind, Is.EqualTo(ClassKind.BooleanParameterType));
            });

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.CompoundParameterType);
            Assert.That(this.viewModel.CurrentThing, Is.TypeOf<CompoundParameterType>());

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.DateParameterType);
            Assert.That(this.viewModel.CurrentThing, Is.TypeOf<DateParameterType>());

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.DateTimeParameterType);
            Assert.That(this.viewModel.CurrentThing, Is.TypeOf<DateTimeParameterType>());

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.DerivedQuantityKind);
            Assert.That(this.viewModel.CurrentThing, Is.TypeOf<DerivedQuantityKind>());

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.EnumerationParameterType);
            Assert.That(this.viewModel.CurrentThing, Is.TypeOf<EnumerationParameterType>());

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.SampledFunctionParameterType);
            Assert.That(this.viewModel.CurrentThing, Is.TypeOf<SampledFunctionParameterType>());

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.SimpleQuantityKind);
            Assert.That(this.viewModel.CurrentThing, Is.TypeOf<SimpleQuantityKind>());

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.SpecializedQuantityKind);
            Assert.That(this.viewModel.CurrentThing, Is.TypeOf<SpecializedQuantityKind>());

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.TextParameterType);
            Assert.That(this.viewModel.CurrentThing, Is.TypeOf<TextParameterType>());

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.TimeOfDayParameterType);
            Assert.That(this.viewModel.CurrentThing, Is.TypeOf<TimeOfDayParameterType>());

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.DomainOfExpertise);
            Assert.That(this.viewModel.CurrentThing, Is.TypeOf<TimeOfDayParameterType>());

            var parameterTypeToSet = new BooleanParameterType
            {
                Container = new SiteReferenceDataLibrary()
            };

            this.viewModel.CurrentThing = parameterTypeToSet;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CurrentThing, Is.EqualTo(parameterTypeToSet));
                Assert.That(this.viewModel.SelectedReferenceDataLibrary, Is.EqualTo(parameterTypeToSet.Container));
            });
        }
    }
}
