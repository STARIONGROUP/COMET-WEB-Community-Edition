// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypesTableViewModelTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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
    using System.Collections.Concurrent;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Operations;
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
                },
                DefinedCategory =
                {
                    new Category { ShortName = "ptCat", Name = "PT category", PermissibleClass = { ClassKind.BooleanParameterType, ClassKind.TextParameterType } },
                    new Category { ShortName = "elCat", Name = "Element category", PermissibleClass = { ClassKind.ElementDefinition } },
                    new Category { ShortName = "anyPtCat", Name = "Any PT category", PermissibleClass = { ClassKind.ParameterType } }
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
                Assert.That(this.viewModel.AvailableLanguages, Is.Not.Empty);
            });

            this.viewModel.CurrentThing = new SpecializedQuantityKind
            {
                General = new SimpleQuantityKind()
            };

            this.viewModel.InitializeViewModel();
            Assert.That(this.viewModel.MeasurementScales.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task VerifyParameterTypeAddOrEdit()
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
        public async Task VerifyDefinitionsArePersistedAlongsideTheParameterType()
        {
            this.viewModel.InitializeViewModel();

            var existingDefinition = new Definition { Iid = Guid.NewGuid(), Content = "Existing", LanguageCode = "en-GB" };
            var newDefinition = new Definition { Iid = Guid.NewGuid(), Content = "New", LanguageCode = "fr-FR" };

            this.viewModel.CurrentThing = new TextParameterType
            {
                Iid = Guid.NewGuid(),
                ShortName = "withDefinitions",
                Name = "with definitions",
                Definition = { existingDefinition, newDefinition }
            };

            List<Thing> capturedThings = null;

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<ReferenceDataLibrary>(), It.IsAny<List<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((_, things, _) => capturedThings = things.ToList())
                .ReturnsAsync(new Result());

            await this.viewModel.CreateOrEditParameterType(true);

            Assert.That(capturedThings, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(capturedThings, Does.Contain(existingDefinition));
                Assert.That(capturedThings, Does.Contain(newDefinition));
                Assert.That(capturedThings, Does.Contain(this.viewModel.CurrentThing));
            });
        }

        [Test]
        public void VerifyCategoriesAreFilteredByPermissibleClass()
        {
            this.viewModel.InitializeViewModel();

            // CurrentThing defaults to BooleanParameterType.
            // ptCat (PermissibleClass includes BooleanParameterType, literal match) — eligible.
            // anyPtCat (PermissibleClass = ParameterType, superclass match via the inheritance chain) — eligible.
            // elCat (ElementDefinition only, unrelated branch) — excluded.
            Assert.That(this.viewModel.Categories.Select(c => c.ShortName), Is.EquivalentTo(new[] { "ptCat", "anyPtCat" }));
        }

        [Test]
        public void VerifyCategoriesIncludesPermissibleClassesAcrossInheritanceChain()
        {
            this.viewModel.InitializeViewModel();

            this.viewModel.CurrentThing = new SimpleQuantityKind
            {
                Iid = Guid.NewGuid(),
                ShortName = "sqk",
                Name = "simple quantity kind"
            };

            var eligibleShortNames = this.viewModel.Categories.Select(c => c.ShortName).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(eligibleShortNames, Does.Contain("anyPtCat"),
                    "A Category whose PermissibleClass is the ParameterType superclass must be eligible for SimpleQuantityKind via the inheritance chain.");
                Assert.That(eligibleShortNames, Does.Not.Contain("ptCat"),
                    "ptCat lists only Boolean/Text leaves and must not match SimpleQuantityKind.");
                Assert.That(eligibleShortNames, Does.Not.Contain("elCat"),
                    "elCat targets ElementDefinition and must remain excluded.");
            });
        }

        [Test]
        public async Task VerifyCategoriesArePersistedAlongsideTheParameterType()
        {
            this.viewModel.InitializeViewModel();

            var category = this.siteDirectory.SiteReferenceDataLibrary.First().DefinedCategory.First(c => c.ShortName == "ptCat");

            this.viewModel.CurrentThing = new TextParameterType
            {
                Iid = Guid.NewGuid(),
                ShortName = "withCategory",
                Name = "with category",
                Category = { category }
            };

            List<Thing> capturedThings = null;

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<ReferenceDataLibrary>(), It.IsAny<List<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((_, things, _) => capturedThings = things.ToList())
                .ReturnsAsync(new Result());

            await this.viewModel.CreateOrEditParameterType(true);

            Assert.Multiple(() =>
            {
                Assert.That(capturedThings, Does.Contain(this.viewModel.CurrentThing));
                Assert.That(((TextParameterType)this.viewModel.CurrentThing).Category, Does.Contain(category));
            });
        }

        [Test]
        public async Task VerifyEditingExistingParameterTypeWithNewDefinitionProducesCorrectOperations()
        {
            // Mirrors the production edit-existing flow: the cached parameter type is deep-cloned, a new
            // definition is appended in memory, then the resulting clones are fed through a real
            // ThingTransaction so we can assert the produced operation container matches what the server
            // expects (a Create for the new definition, an Update for the parameter type whose Definition
            // list now references the new Iid).
            this.viewModel.InitializeViewModel();

            var cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var siteDir = new SiteDirectory(Guid.NewGuid(), cache, null) { ShortName = "siteDir" };
            cache.TryAdd(siteDir.CacheKey, new Lazy<Thing>(() => siteDir));

            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), cache, null) { ShortName = "cached-rdl" };
            siteDir.SiteReferenceDataLibrary.Add(rdl);
            cache.TryAdd(rdl.CacheKey, new Lazy<Thing>(() => rdl));

            var cachedParameterType = new TextParameterType(Guid.NewGuid(), cache, null) { ShortName = "cached", Name = "cached" };
            rdl.ParameterType.Add(cachedParameterType);
            cache.TryAdd(cachedParameterType.CacheKey, new Lazy<Thing>(() => cachedParameterType));

            var cachedDefinition = new Definition(Guid.NewGuid(), cache, null) { Content = "cached", LanguageCode = "en-GB" };
            cachedParameterType.Definition.Add(cachedDefinition);
            cache.TryAdd(cachedDefinition.CacheKey, new Lazy<Thing>(() => cachedDefinition));

            var parameterTypeClone = cachedParameterType.Clone(true);
            var newDefinition = new Definition { Iid = Guid.NewGuid(), Content = "new", LanguageCode = "fr-FR" };
            parameterTypeClone.Definition.Add(newDefinition);

            this.viewModel.CurrentThing = parameterTypeClone;
            this.viewModel.SelectedReferenceDataLibrary = rdl;

            List<Thing> capturedThings = null;

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<ReferenceDataLibrary>(), It.IsAny<List<Thing>>(), It.IsAny<NotificationDescription>()))
                .Callback<Thing, IReadOnlyCollection<Thing>, NotificationDescription>((_, things, _) => capturedThings = things.ToList())
                .ReturnsAsync(new Result());

            await this.viewModel.CreateOrEditParameterType(false);

            Assert.That(capturedThings, Is.Not.Null);

            var rdlClone = rdl.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(rdlClone));

            foreach (var thing in capturedThings)
            {
                transaction.CreateOrUpdate(thing);
            }

            var operationContainer = transaction.FinalizeTransaction();
            var operations = operationContainer.Operations.ToList();

            var createOps = operations.Where(o => o.OperationKind == CDP4DalCommon.Protocol.Operations.OperationKind.Create).ToList();
            var updateOps = operations.Where(o => o.OperationKind == CDP4DalCommon.Protocol.Operations.OperationKind.Update).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(createOps, Has.Count.EqualTo(1), "exactly one Create op should be produced for the new Definition");
                Assert.That(createOps[0].ModifiedThing.Iid, Is.EqualTo(newDefinition.Iid));
                Assert.That(updateOps.Any(o => o.ModifiedThing.Iid == parameterTypeClone.Iid), Is.True, "the parameter type must be in an Update op");

                var ptDto = (CDP4Common.DTO.TextParameterType)updateOps.Single(o => o.ModifiedThing.Iid == parameterTypeClone.Iid).ModifiedThing;
                Assert.That(ptDto.Definition, Does.Contain(newDefinition.Iid), "the updated parameter type DTO must reference the new Definition Iid");
                Assert.That(ptDto.Definition, Does.Contain(cachedDefinition.Iid), "the updated parameter type DTO must keep the existing Definition Iid");
            });
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

            this.viewModel.SelectedParameterType = new ClassKindWrapper(ClassKind.ArrayParameterType);
            Assert.That(this.viewModel.CurrentThing, Is.TypeOf<ArrayParameterType>());

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
