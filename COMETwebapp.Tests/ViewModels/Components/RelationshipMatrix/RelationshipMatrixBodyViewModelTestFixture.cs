// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RelationshipMatrixBodyViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.RelationshipMatrix
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using DynamicData;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Model.RelationshipMatrix;
    using COMETwebapp.Services.Export;
    using COMETwebapp.Services.FileStore;
    using COMETwebapp.Services.RelationshipMatrix;
    using COMETwebapp.ViewModels.Components.RelationshipMatrix;

    using FluentResults;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RelationshipMatrixBodyViewModelTestFixture
    {
        private RelationshipMatrixBodyViewModel viewModel;
        private CDPMessageBus messageBus;
        private Mock<ISessionService> sessionService;
        private Mock<IExportService> exportService;
        private Mock<IFileStoreService> fileStoreService;
        private Iteration iteration;
        private ElementDefinition rowEd;
        private ElementDefinition rowEd2;
        private ElementDefinition colEd;
        private ElementDefinition colEd2;
        private BinaryRelationshipRule rule;
        private Category rowCategory;
        private Category colCategory;
        private Category relationshipCategory;
        private DomainOfExpertise domain;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();
            var session = new Mock<ISession>();
            var permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            session.Setup(x => x.PermissionService).Returns(permissionService.Object);

            this.domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };

            var categoryCache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var iDalUri = new Uri("https://test.example");

            var rowCategoryIid = Guid.NewGuid();
            var colCategoryIid = Guid.NewGuid();
            var relationshipCategoryIid = Guid.NewGuid();

            this.rowCategory = new Category(rowCategoryIid, categoryCache, iDalUri) { ShortName = "ROW", Name = "row category", PermissibleClass = { ClassKind.ElementDefinition } };
            this.colCategory = new Category(colCategoryIid, categoryCache, iDalUri) { ShortName = "COL", Name = "column category", PermissibleClass = { ClassKind.ElementDefinition } };
            this.relationshipCategory = new Category(relationshipCategoryIid, categoryCache, iDalUri) { ShortName = "REL", Name = "relationship category", PermissibleClass = { ClassKind.BinaryRelationship } };

            categoryCache.TryAdd(new CacheKey(rowCategoryIid, null), new Lazy<Thing>(() => this.rowCategory));
            categoryCache.TryAdd(new CacheKey(colCategoryIid, null), new Lazy<Thing>(() => this.colCategory));
            categoryCache.TryAdd(new CacheKey(relationshipCategoryIid, null), new Lazy<Thing>(() => this.relationshipCategory));

            this.rule = new BinaryRelationshipRule
            {
                Iid = Guid.NewGuid(), ShortName = "rule1", Name = "rule one", ForwardRelationshipName = "relates to",
                RelationshipCategory = this.relationshipCategory, SourceCategory = this.rowCategory, TargetCategory = this.colCategory
            };

            var rdl = new SiteReferenceDataLibrary
            {
                ShortName = "siteRdl", Name = "Site RDL",
                DefinedCategory = { this.rowCategory, this.colCategory, this.relationshipCategory },
                Rule = { this.rule }
            };

            var siteDirectory = new SiteDirectory { Domain = { this.domain }, SiteReferenceDataLibrary = { rdl } };

            var modelSetup = new EngineeringModelSetup
            {
                Name = "Model", ShortName = "MOD", ActiveDomain = { this.domain },
                RequiredRdl = { new ModelReferenceDataLibrary { RequiredRdl = rdl } }
            };

            siteDirectory.Model.Add(modelSetup);

            var iterationSetup = new IterationSetup { IterationNumber = 1 };
            modelSetup.IterationSetup.Add(iterationSetup);

            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(this.domain);

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.sessionService
                .Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            this.rowEd = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "rowEd", Name = "row element", Owner = this.domain, Category = { this.rowCategory } };
            this.rowEd2 = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "rowEd2", Name = "row element 2", Owner = this.domain, Category = { this.rowCategory } };
            this.colEd = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "colEd", Name = "column element", Owner = this.domain, Category = { this.colCategory } };
            this.colEd2 = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "colEd2", Name = "column element 2", Owner = this.domain, Category = { this.colCategory } };

            this.iteration = new Iteration { Iid = Guid.NewGuid(), IterationSetup = iterationSetup, Container = new EngineeringModel { EngineeringModelSetup = modelSetup } };
            this.iteration.Element.Add(this.rowEd);
            this.iteration.Element.Add(this.rowEd2);
            this.iteration.Element.Add(this.colEd);
            this.iteration.Element.Add(this.colEd2);

            var openIterations = new SourceList<Iteration>();
            openIterations.Add(this.iteration);
            this.sessionService.Setup(x => x.OpenIterations).Returns(openIterations);

            this.exportService = new Mock<IExportService>();
            this.exportService.Setup(x => x.ExportAndDownloadAsync(It.IsAny<IExporter>())).Returns(Task.CompletedTask);

            this.fileStoreService = new Mock<IFileStoreService>();

            this.viewModel = new RelationshipMatrixBodyViewModel(this.sessionService.Object, this.messageBus, new Mock<ILogger<RelationshipMatrixBodyViewModel>>().Object,
                this.exportService.Object, this.fileStoreService.Object)
            {
                CurrentThing = this.iteration
            };
        }

        [TearDown]
        public void TearDown()
        {
            this.viewModel.Dispose();
            this.messageBus.ClearSubscriptions();
            this.messageBus.Dispose();
        }

        /// <summary>
        /// Creates and adds to <see cref="iteration" /> an existing <see cref="BinaryRelationship" /> from
        /// <see cref="rowEd" /> to <see cref="colEd" />, categorized with <see cref="relationshipCategory" />.
        /// </summary>
        private void AddExistingRelationship()
        {
            var relationship = new BinaryRelationship { Iid = Guid.NewGuid(), Source = this.rowEd, Target = this.colEd, Owner = this.domain, Category = { this.relationshipCategory } };
            this.iteration.Relationship.Add(relationship);
        }

        /// <summary>
        /// Scopes <see cref="RelationshipMatrixBodyViewModel.RowConfiguration" /> to <see cref="rowCategory" /> and
        /// <see cref="RelationshipMatrixBodyViewModel.ColumnConfiguration" /> to <see cref="colCategory" />.
        /// </summary>
        private void ConfigureRowsAndColumns()
        {
            this.viewModel.RowConfiguration.SelectedClassKind = ClassKind.ElementDefinition;
            this.viewModel.RowConfiguration.CategorySelector.SelectedCategories = [this.rowCategory];

            this.viewModel.ColumnConfiguration.SelectedClassKind = ClassKind.ElementDefinition;
            this.viewModel.ColumnConfiguration.CategorySelector.SelectedCategories = [this.colCategory];
        }

        [Test]
        public async Task VerifyOnThingChanged()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableRules, Does.Contain(this.rule));
                Assert.That(this.viewModel.SelectedRule, Is.Null);
            });
        }

        [Test]
        public async Task VerifyRebuildMatrix()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.AddExistingRelationship();
            this.ConfigureRowsAndColumns();
            this.viewModel.SelectedRule = this.rule;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.RowThings, Does.Contain(this.rowEd));
                Assert.That(this.viewModel.ColumnThings, Does.Contain(this.colEd));
                Assert.That(this.viewModel.GetCell(this.rowEd, this.colEd).Direction, Is.EqualTo(RelationshipDirectionKind.RowToColumn));
            });
        }

        [Test]
        public async Task VerifyUnrelatedRowsAndColumns()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.AddExistingRelationship();
            this.ConfigureRowsAndColumns();
            this.viewModel.SelectedRule = this.rule;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsRowUnrelated(this.rowEd), Is.False);
                Assert.That(this.viewModel.IsRowUnrelated(this.rowEd2), Is.True);
                Assert.That(this.viewModel.IsColumnUnrelated(this.colEd), Is.False);
                Assert.That(this.viewModel.IsColumnUnrelated(this.colEd2), Is.True);
            });
        }

        [Test]
        public async Task VerifySelectCellPermissions()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.AddExistingRelationship();
            this.viewModel.SelectedRule = this.rule;

            this.viewModel.SelectCell(this.rowEd, this.colEd);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CanCreateRowToColumn, Is.False);
                Assert.That(this.viewModel.CanCreateColumnToRow, Is.True);
                Assert.That(this.viewModel.CanDelete, Is.True);
            });

            this.viewModel.SelectCell(this.rowEd2, this.colEd2);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CanCreateRowToColumn, Is.True);
                Assert.That(this.viewModel.CanCreateColumnToRow, Is.True);
                Assert.That(this.viewModel.CanDelete, Is.False);
            });

            this.viewModel.SelectCell(this.rowEd, this.rowEd);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CanCreateRowToColumn, Is.False);
                Assert.That(this.viewModel.CanCreateColumnToRow, Is.False);
            });
        }

        [Test]
        public async Task VerifyCreateRelationship()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.SelectedRule = this.rule;
            this.viewModel.SelectCell(this.rowEd2, this.colEd2);

            await this.viewModel.CreateRelationshipAsync(RelationshipDirectionKind.RowToColumn);

            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(
                It.IsAny<Thing>(),
                It.Is<IReadOnlyCollection<Thing>>(things => things.OfType<BinaryRelationship>()
                    .Any(r => r.Source.Iid == this.rowEd2.Iid && r.Target.Iid == this.colEd2.Iid && r.Category.Contains(this.relationshipCategory))),
                It.IsAny<NotificationDescription>()), Times.Once);
        }

        [Test]
        public async Task VerifyDeleteRelationship()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.AddExistingRelationship();
            this.viewModel.SelectedRule = this.rule;
            this.viewModel.SelectCell(this.rowEd, this.colEd);

            await this.viewModel.DeleteRelationshipAsync();

            this.sessionService.Verify(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Once);
        }

        [Test]
        public async Task VerifySwapAxes()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.ConfigureRowsAndColumns();

            this.viewModel.SwapAxes();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.RowConfiguration.CategorySelector.SelectedCategories, Is.EquivalentTo(new[] { this.colCategory }));
                Assert.That(this.viewModel.ColumnConfiguration.CategorySelector.SelectedCategories, Is.EquivalentTo(new[] { this.rowCategory }));
            });
        }

        [Test]
        public async Task VerifyShowRelatedOnly()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.AddExistingRelationship();
            this.ConfigureRowsAndColumns();
            this.viewModel.SelectedRule = this.rule;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.RowThings, Does.Contain(this.rowEd2));
                Assert.That(this.viewModel.ColumnThings, Does.Contain(this.colEd2));
            });

            this.viewModel.ShowRelatedOnly = true;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.RowThings, Does.Contain(this.rowEd));
                Assert.That(this.viewModel.RowThings, Does.Not.Contain(this.rowEd2));
                Assert.That(this.viewModel.ColumnThings, Does.Contain(this.colEd));
                Assert.That(this.viewModel.ColumnThings, Does.Not.Contain(this.colEd2));
            });
        }

        [Test]
        public async Task VerifyExport()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.ConfigureRowsAndColumns();
            this.viewModel.SelectedRule = this.rule;

            await this.viewModel.ExportAsync();

            this.exportService.Verify(x => x.ExportAndDownloadAsync(It.Is<IExporter>(exporter => exporter.FileName == "RelationshipMatrix.xlsx")), Times.Once);
        }

        [Test]
        public async Task VerifyExportConfiguration()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            await this.viewModel.ExportConfigurationAsync();

            this.exportService.Verify(x => x.ExportAndDownloadAsync(It.Is<IExporter>(exporter => exporter.FileName == "RelationshipMatrix.json")), Times.Once);
        }

        [Test]
        public async Task VerifyImportConfigurationRoundTrips()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.ConfigureRowsAndColumns();
            this.viewModel.SelectedRule = this.rule;
            this.viewModel.ShowRelatedOnly = true;

            using var stream = new RelationshipMatrixConfigurationExporter(
                this.viewModel.RowConfiguration, this.viewModel.ColumnConfiguration, this.rule, true, true, false).Export();

            this.viewModel.SelectedRule = null;
            this.viewModel.RowConfiguration.SelectedClassKind = null;
            this.viewModel.ColumnConfiguration.SelectedClassKind = null;
            this.viewModel.ShowRelatedOnly = false;

            await this.viewModel.ImportConfigurationAsync(stream);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedRule, Is.EqualTo(this.rule));
                Assert.That(this.viewModel.RowConfiguration.SelectedClassKind, Is.EqualTo(ClassKind.ElementDefinition));
                Assert.That(this.viewModel.RowConfiguration.CategorySelector.SelectedCategories, Does.Contain(this.rowCategory));
                Assert.That(this.viewModel.ColumnConfiguration.CategorySelector.SelectedCategories, Does.Contain(this.colCategory));
                Assert.That(this.viewModel.ShowRelatedOnly, Is.True);
            });
        }

        [Test]
        public async Task VerifyCanUseModelFileStore()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.fileStoreService.Setup(x => x.GetFileType(this.iteration, "json")).Returns((FileType)null);
            Assert.That(this.viewModel.CanUseModelFileStore, Is.False);

            this.fileStoreService.Setup(x => x.GetFileType(this.iteration, "json")).Returns(new FileType());
            Assert.That(this.viewModel.CanUseModelFileStore, Is.True);
        }

        [Test]
        public async Task VerifySaveConfigurationToStore()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.ConfigureRowsAndColumns();
            this.viewModel.SelectedRule = this.rule;

            var jsonFileType = new FileType();
            this.fileStoreService.Setup(x => x.GetFileType(this.iteration, "json")).Returns(jsonFileType);

            var folder = new Folder();

            this.fileStoreService
                .Setup(x => x.SaveFileAsync(this.iteration, FileStoreType.Domain, "MyConfig.json", jsonFileType, It.IsAny<byte[]>(), folder))
                .ReturnsAsync(Result.Ok());

            await this.viewModel.SaveConfigurationToStoreAsync(FileStoreType.Domain, "MyConfig", folder);

            this.fileStoreService.Verify(x => x.SaveFileAsync(this.iteration, FileStoreType.Domain, "MyConfig.json", jsonFileType, It.IsAny<byte[]>(), folder), Times.Once);
            Assert.That(this.viewModel.FileStoreMessage, Does.Contain("Saved"));

            this.fileStoreService
                .Setup(x => x.SaveFileAsync(this.iteration, FileStoreType.Common, "RelationshipMatrix.json", jsonFileType, It.IsAny<byte[]>(), null))
                .ReturnsAsync(Result.Fail("No Common file store available"));

            await this.viewModel.SaveConfigurationToStoreAsync(FileStoreType.Common);

            Assert.That(this.viewModel.FileStoreMessage, Does.Contain("No Common file store available"));
        }

        [Test]
        public async Task VerifyCreateFileStore()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.fileStoreService.Setup(x => x.StoreExists(this.iteration, FileStoreType.Common)).Returns(false);
            this.fileStoreService.Setup(x => x.CreateStoreAsync(this.iteration, FileStoreType.Common)).ReturnsAsync(Result.Ok());

            await this.viewModel.CreateFileStoreAsync(FileStoreType.Common);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.StoreExists(FileStoreType.Common), Is.False);
                Assert.That(this.viewModel.FileStoreMessage, Does.Contain("Created"));
            });

            this.fileStoreService.Verify(x => x.CreateStoreAsync(this.iteration, FileStoreType.Common), Times.Once);
        }

        [Test]
        public async Task VerifyLoadConfigurationFromStore()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.ConfigureRowsAndColumns();
            this.viewModel.SelectedRule = this.rule;

            using var stream = new RelationshipMatrixConfigurationExporter(
                this.viewModel.RowConfiguration, this.viewModel.ColumnConfiguration, this.rule, true, false, false).Export();
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer);

            var file = new CDP4Common.EngineeringModelData.File();
            this.fileStoreService.Setup(x => x.ReadFileAsync(file)).ReturnsAsync(buffer.ToArray());

            this.viewModel.SelectedRule = null;
            this.viewModel.RowConfiguration.SelectedClassKind = null;

            await this.viewModel.LoadConfigurationFromStoreAsync(file);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedRule, Is.EqualTo(this.rule));
                Assert.That(this.viewModel.RowConfiguration.SelectedClassKind, Is.EqualTo(ClassKind.ElementDefinition));
            });
        }

        [Test]
        public async Task VerifyOnSessionRefreshedRebuildsMatrix()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.ConfigureRowsAndColumns();
            this.viewModel.SelectedRule = this.rule;

            var relationship = new BinaryRelationship { Iid = Guid.NewGuid(), Source = this.rowEd, Target = this.colEd, Owner = this.domain, Category = { this.relationshipCategory } };
            this.iteration.Relationship.Add(relationship);

            this.messageBus.SendMessage(new ObjectChangedEvent(relationship, EventKind.Added), typeof(BinaryRelationship));
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);

            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GetCell(this.rowEd, this.colEd).Direction, Is.EqualTo(RelationshipDirectionKind.RowToColumn));
                Assert.That(this.viewModel.IsLoading, Is.False);
            });
        }

        [Test]
        public async Task VerifyCreateRelationshipGuardsAndHandlesError()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            // No selected cell / rule short-circuits without a write
            await this.viewModel.CreateRelationshipAsync(RelationshipDirectionKind.RowToColumn);
            this.sessionService.Verify(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Never);

            this.viewModel.SelectedRule = this.rule;
            this.viewModel.SelectCell(this.rowEd2, this.colEd2);

            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await this.viewModel.CreateRelationshipAsync(RelationshipDirectionKind.ColumnToRow);

            Assert.That(this.viewModel.IsLoading, Is.False);
        }

        [Test]
        public async Task VerifyDeleteRelationshipGuardsAndHandlesError()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            // No selected cell short-circuits without a delete
            await this.viewModel.DeleteRelationshipAsync();
            this.sessionService.Verify(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()), Times.Never);

            this.AddExistingRelationship();
            this.viewModel.SelectedRule = this.rule;
            this.viewModel.SelectCell(this.rowEd, this.colEd);

            this.sessionService
                .Setup(x => x.DeleteThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await this.viewModel.DeleteRelationshipAsync();

            Assert.That(this.viewModel.IsLoading, Is.False);
        }

        [Test]
        public async Task VerifyImportHandlesEmptyAndInvalidConfiguration()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.SelectedRule = this.rule;

            using var empty = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{}"));
            await this.viewModel.ImportConfigurationAsync(empty);

            using var invalid = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("not valid json"));
            await this.viewModel.ImportConfigurationAsync(invalid);

            // Neither a usable-but-empty nor an unparsable file changes the current selection
            Assert.That(this.viewModel.SelectedRule, Is.EqualTo(this.rule));
        }

        [Test]
        public async Task VerifySaveConfigurationWithoutJsonFileType()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.fileStoreService.Setup(x => x.GetFileType(this.iteration, "json")).Returns((FileType)null);

            var saved = await this.viewModel.SaveConfigurationToStoreAsync(FileStoreType.Domain);

            Assert.Multiple(() =>
            {
                Assert.That(saved, Is.False);
                Assert.That(this.viewModel.FileStoreMessage, Does.Contain("JSON file type"));
            });
        }

        [Test]
        public async Task VerifyCreateFileStoreReportsFailureAndError()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.fileStoreService.Setup(x => x.CreateStoreAsync(this.iteration, FileStoreType.Common)).ReturnsAsync(Result.Fail("boom"));
            var failed = await this.viewModel.CreateFileStoreAsync(FileStoreType.Common);

            Assert.Multiple(() =>
            {
                Assert.That(failed, Is.False);
                Assert.That(this.viewModel.FileStoreMessage, Does.Contain("Could not create"));
            });

            this.fileStoreService.Setup(x => x.CreateStoreAsync(this.iteration, FileStoreType.Domain)).ThrowsAsync(new InvalidOperationException("boom"));
            var errored = await this.viewModel.CreateFileStoreAsync(FileStoreType.Domain);

            Assert.That(errored, Is.False);
        }

        [Test]
        public async Task VerifyExportAndConfigurationExportHandleError()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.ConfigureRowsAndColumns();
            this.viewModel.SelectedRule = this.rule;

            this.exportService.Setup(x => x.ExportAndDownloadAsync(It.IsAny<IExporter>())).ThrowsAsync(new InvalidOperationException("boom"));

            await this.viewModel.ExportAsync();
            await this.viewModel.ExportConfigurationAsync("cfg");

            Assert.That(this.viewModel.IsLoading, Is.False);
        }

        [Test]
        public async Task VerifyLoadConfigurationHandlesError()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.fileStoreService.Setup(x => x.ReadFileAsync(It.IsAny<CDP4Common.EngineeringModelData.File>())).ThrowsAsync(new InvalidOperationException("boom"));

            await this.viewModel.LoadConfigurationFromStoreAsync(new CDP4Common.EngineeringModelData.File());

            Assert.That(this.viewModel.IsLoading, Is.False);
        }

        [Test]
        public async Task VerifyConfigurationDialogVisibleToggles()
        {
            await TaskHelper.WaitWhileAsync(() => this.viewModel.IsLoading);

            this.viewModel.IsConfigurationDialogVisible = true;

            Assert.That(this.viewModel.IsConfigurationDialogVisible, Is.True);
        }
    }
}
