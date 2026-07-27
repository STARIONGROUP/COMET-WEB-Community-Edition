// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SourceConfigurationViewModelTestFixture.cs" company="Starion Group S.A.">
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
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Model.RelationshipMatrix;
    using COMETwebapp.ViewModels.Components.RelationshipMatrix;

    using NUnit.Framework;

    [TestFixture]
    public class SourceConfigurationViewModelTestFixture
    {
        private SourceConfigurationViewModel viewModel;
        private int onUpdateCallCount;
        private Iteration iteration;
        private Category categoryA;
        private Category categoryB;
        private DomainOfExpertise domainX;
        private DomainOfExpertise domainY;
        private ElementDefinition edInCategoryA;
        private ElementDefinition edInCategoryB;
        private List<DefinedThing> candidates;

        [SetUp]
        public void SetUp()
        {
            // Categories need a shared, self-registered Cache: the SDK's IsMemberOfCategory walks
            // Category.AllDerivedCategories(), which throws on the empty per-instance cache a plain
            // "new Category { ... }" gets (real, session-backed Category instances share an Assembler cache).
            var categoryCache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var iDalUri = new Uri("https://test.example");

            var categoryAIid = Guid.NewGuid();
            var categoryBIid = Guid.NewGuid();

            this.categoryA = new Category(categoryAIid, categoryCache, iDalUri) { ShortName = "CATA", Name = "category A", PermissibleClass = { ClassKind.ElementDefinition } };
            this.categoryB = new Category(categoryBIid, categoryCache, iDalUri) { ShortName = "CATB", Name = "category B", PermissibleClass = { ClassKind.ElementDefinition } };

            categoryCache.TryAdd(new CacheKey(categoryAIid, null), new Lazy<Thing>(() => this.categoryA));
            categoryCache.TryAdd(new CacheKey(categoryBIid, null), new Lazy<Thing>(() => this.categoryB));

            this.domainX = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "DX", Name = "Domain X" };
            this.domainY = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "DY", Name = "Domain Y" };

            var rdl = new SiteReferenceDataLibrary { ShortName = "siteRdl", Name = "Site RDL", DefinedCategory = { this.categoryA, this.categoryB } };
            var siteDirectory = new SiteDirectory { Domain = { this.domainX, this.domainY }, SiteReferenceDataLibrary = { rdl } };

            var modelSetup = new EngineeringModelSetup
            {
                Name = "Model", ShortName = "MOD", ActiveDomain = { this.domainX, this.domainY },
                RequiredRdl = { new ModelReferenceDataLibrary { RequiredRdl = rdl } }
            };

            siteDirectory.Model.Add(modelSetup);

            var iterationSetup = new IterationSetup { IterationNumber = 1 };
            modelSetup.IterationSetup.Add(iterationSetup);

            this.iteration = new Iteration { Iid = Guid.NewGuid(), IterationSetup = iterationSetup, Container = new EngineeringModel { EngineeringModelSetup = modelSetup } };

            this.edInCategoryA = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "EDA", Name = "Alpha element", Owner = this.domainX, Category = { this.categoryA } };
            this.edInCategoryB = new ElementDefinition { Iid = Guid.NewGuid(), ShortName = "EDB", Name = "Beta element", Owner = this.domainY, Category = { this.categoryB } };

            this.candidates = [this.edInCategoryA, this.edInCategoryB];

            this.viewModel = new SourceConfigurationViewModel(() => this.onUpdateCallCount++);
        }

        [TearDown]
        public void TearDown()
        {
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyDefaults()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedBooleanOperatorKind, Is.EqualTo(CategoryBooleanOperatorKind.Or));
                Assert.That(this.viewModel.IncludeSubcategories, Is.True);
                Assert.That(this.viewModel.SelectedDisplayKind, Is.EqualTo(MatrixDisplayKind.Name));
                Assert.That(this.viewModel.SelectedSortOrder, Is.EqualTo(MatrixSortOrder.Ascending));
                Assert.That(this.viewModel.PossibleClassKinds, Does.Contain(ClassKind.ElementDefinition));
                Assert.That(this.viewModel.PossibleClassKinds, Does.Contain(ClassKind.Requirement));
            });
        }

        [Test]
        public void VerifyCurrentIterationPopulatesOwners()
        {
            this.viewModel.CurrentIteration = this.iteration;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableOwners, Is.EquivalentTo(new[] { this.domainX, this.domainY }));

                // An empty owner selection means "all owners" (no filter), which keeps the owner box compact.
                Assert.That(this.viewModel.SelectedOwners, Is.Empty);
            });
        }

        [Test]
        public void VerifyOnUpdateFires()
        {
            this.onUpdateCallCount = 0;
            this.viewModel.SelectedClassKind = ClassKind.ElementDefinition;
            Assert.That(this.onUpdateCallCount, Is.GreaterThan(0));

            this.onUpdateCallCount = 0;
            this.viewModel.SelectedBooleanOperatorKind = CategoryBooleanOperatorKind.And;
            Assert.That(this.onUpdateCallCount, Is.GreaterThan(0));

            this.onUpdateCallCount = 0;
            this.viewModel.IncludeSubcategories = false;
            Assert.That(this.onUpdateCallCount, Is.GreaterThan(0));

            this.onUpdateCallCount = 0;
            this.viewModel.SelectedOwners = [this.domainX];
            Assert.That(this.onUpdateCallCount, Is.GreaterThan(0));

            this.onUpdateCallCount = 0;
            this.viewModel.SelectedSortOrder = MatrixSortOrder.Descending;
            Assert.That(this.onUpdateCallCount, Is.GreaterThan(0));
        }

        [Test]
        public void VerifyQuerySourceThings()
        {
            this.viewModel.SelectedClassKind = ClassKind.ElementDefinition;

            this.viewModel.CategorySelector.SelectedCategories = [this.categoryA];
            Assert.That(this.viewModel.QuerySourceThings(this.candidates), Is.EqualTo(new[] { this.edInCategoryA }));

            this.viewModel.SelectedBooleanOperatorKind = CategoryBooleanOperatorKind.Or;
            this.viewModel.CategorySelector.SelectedCategories = [this.categoryA, this.categoryB];
            Assert.That(this.viewModel.QuerySourceThings(this.candidates), Is.EquivalentTo(new[] { this.edInCategoryA, this.edInCategoryB }));

            this.viewModel.SelectedBooleanOperatorKind = CategoryBooleanOperatorKind.And;
            Assert.That(this.viewModel.QuerySourceThings(this.candidates), Is.Empty);

            this.viewModel.SelectedBooleanOperatorKind = CategoryBooleanOperatorKind.Or;
            this.viewModel.SelectedOwners = [this.domainX];
            Assert.That(this.viewModel.QuerySourceThings(this.candidates), Is.EqualTo(new[] { this.edInCategoryA }));

            this.viewModel.SelectedOwners = [];
            this.viewModel.CategorySelector.SelectedCategories = [];
            Assert.That(this.viewModel.QuerySourceThings(this.candidates), Is.Empty);

            this.viewModel.CategorySelector.SelectedCategories = [this.categoryA, this.categoryB];
            this.viewModel.SelectedSortOrder = MatrixSortOrder.Ascending;
            Assert.That(this.viewModel.QuerySourceThings(this.candidates), Is.EqualTo(new[] { this.edInCategoryA, this.edInCategoryB }));

            this.viewModel.SelectedSortOrder = MatrixSortOrder.Descending;
            Assert.That(this.viewModel.QuerySourceThings(this.candidates), Is.EqualTo(new[] { this.edInCategoryB, this.edInCategoryA }));
        }

        [Test]
        public void VerifyCaptureAndRestoreSnapshot()
        {
            this.viewModel.CurrentIteration = this.iteration;
            this.viewModel.SelectedClassKind = ClassKind.ElementDefinition;
            this.viewModel.CategorySelector.SelectedCategories = [this.categoryA];
            this.viewModel.SelectedOwners = [this.domainX];
            this.viewModel.SelectedBooleanOperatorKind = CategoryBooleanOperatorKind.And;
            this.viewModel.IncludeSubcategories = false;
            this.viewModel.SelectedDisplayKind = MatrixDisplayKind.ShortName;
            this.viewModel.SelectedSortKind = MatrixDisplayKind.ShortName;
            this.viewModel.SelectedSortOrder = MatrixSortOrder.Descending;

            var snapshot = this.viewModel.CaptureSnapshot();

            using var restored = new SourceConfigurationViewModel(() => { }) { CurrentIteration = this.iteration };
            restored.RestoreSnapshot(snapshot);

            Assert.Multiple(() =>
            {
                Assert.That(restored.SelectedClassKind, Is.EqualTo(ClassKind.ElementDefinition));
                Assert.That(restored.SelectedBooleanOperatorKind, Is.EqualTo(CategoryBooleanOperatorKind.And));
                Assert.That(restored.IncludeSubcategories, Is.False);
                Assert.That(restored.SelectedDisplayKind, Is.EqualTo(MatrixDisplayKind.ShortName));
                Assert.That(restored.SelectedSortKind, Is.EqualTo(MatrixDisplayKind.ShortName));
                Assert.That(restored.SelectedSortOrder, Is.EqualTo(MatrixSortOrder.Descending));
                Assert.That(restored.CategorySelector.SelectedCategories, Is.EquivalentTo(new[] { this.categoryA }));
                Assert.That(restored.SelectedOwners, Is.EquivalentTo(new[] { this.domainX }));
            });
        }
    }
}
