// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MultiCategorySelectorViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.ViewModels.Components.Selectors
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using NUnit.Framework;

    [TestFixture]
    public class MultiCategorySelectorViewModelTestFixture
    {
        private MultiCategorySelectorViewModel viewModel;
        private Category elementDefinitionCategory;
        private Category elementUsageCategory;
        private Category parameterTypeCategory;
        private Iteration iteration;

        [SetUp]
        public void Setup()
        {
            this.viewModel = new MultiCategorySelectorViewModel();

            this.elementDefinitionCategory = new Category
            {
                Iid = Guid.NewGuid(),
                ShortName = "edCat",
                Name = "ED category",
                PermissibleClass = { ClassKind.ElementDefinition }
            };

            this.elementUsageCategory = new Category
            {
                Iid = Guid.NewGuid(),
                ShortName = "euCat",
                Name = "EU category",
                PermissibleClass = { ClassKind.ElementUsage }
            };

            this.parameterTypeCategory = new Category
            {
                Iid = Guid.NewGuid(),
                ShortName = "ptCat",
                Name = "PT category",
                PermissibleClass = { ClassKind.ParameterType }
            };

            var rdl = new SiteReferenceDataLibrary
            {
                ShortName = "siteRdl",
                Name = "Site RDL",
                DefinedCategory =
                {
                    this.elementDefinitionCategory,
                    this.elementUsageCategory,
                    this.parameterTypeCategory
                }
            };

            var siteDirectory = new SiteDirectory
            {
                ShortName = "siteDir",
                SiteReferenceDataLibrary = { rdl }
            };

            var modelSetup = new EngineeringModelSetup
            {
                ShortName = "model",
                RequiredRdl = { new ModelReferenceDataLibrary { RequiredRdl = rdl } }
            };

            siteDirectory.Model.Add(modelSetup);

            var iterationSetup = new IterationSetup();
            modelSetup.IterationSetup.Add(iterationSetup);

            this.iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                IterationSetup = iterationSetup
            };
        }

        [TearDown]
        public void TearDown()
        {
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyInitialState()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableCategories, Is.Empty);
                Assert.That(this.viewModel.SelectedCategories, Is.Empty);
                Assert.That(this.viewModel.CurrentIteration, Is.Null);
            });
        }

        [Test]
        public void VerifyAvailableCategoriesAreScopedToElementBaseAndOrdered()
        {
            this.viewModel.CurrentIteration = this.iteration;

            var available = this.viewModel.AvailableCategories.ToList();

            Assert.Multiple(() =>
            {
                Assert.That(available.Select(c => c.ShortName), Is.EquivalentTo(new[] { "edCat", "euCat" }),
                    "Only categories whose PermissibleClass covers ElementBase / ElementDefinition / ElementUsage should appear.");
                Assert.That(available.Select(c => c.Name).ToList(),
                    Is.EqualTo(available.Select(c => c.Name).OrderBy(n => n, StringComparer.InvariantCultureIgnoreCase).ToList()),
                    "AvailableCategories must be sorted alphabetically by Name.");
                Assert.That(this.viewModel.SelectedCategories, Is.Empty);
            });
        }

        [Test]
        public void VerifySettingNullSelectionFallsBackToEmptyCollection()
        {
            this.viewModel.SelectedCategories = null;
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedCategories, Is.Not.Null);
                Assert.That(this.viewModel.SelectedCategories, Is.Empty);
            });
        }

        [Test]
        public void VerifyChangingIterationResetsSelection()
        {
            this.viewModel.CurrentIteration = this.iteration;
            this.viewModel.SelectedCategories = new List<Category> { this.elementDefinitionCategory };

            Assert.That(this.viewModel.SelectedCategories.Count(), Is.EqualTo(1));

            this.viewModel.CurrentIteration = null;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableCategories, Is.Empty);
                Assert.That(this.viewModel.SelectedCategories, Is.Empty);
            });
        }

        [Test]
        public void VerifyOverridingApplicableClassKindsExpandsScope()
        {
            this.viewModel.ApplicableClassKinds = new[] { ClassKind.ParameterType };
            this.viewModel.CurrentIteration = this.iteration;

            Assert.That(this.viewModel.AvailableCategories.Select(c => c.ShortName),
                Is.EquivalentTo(new[] { "ptCat" }),
                "Overriding ApplicableClassKinds to ParameterType must surface only ParameterType-permissible categories.");
        }

        [Test]
        public void VerifyEmptyApplicableClassKindsDisablesPermissibleClassFilter()
        {
            this.viewModel.ApplicableClassKinds = Array.Empty<ClassKind>();
            this.viewModel.CurrentIteration = this.iteration;

            var available = this.viewModel.AvailableCategories.ToList();

            Assert.Multiple(() =>
            {
                Assert.That(available.Select(c => c.ShortName), Is.EquivalentTo(new[] { "edCat", "euCat", "ptCat" }),
                    "An empty ApplicableClassKinds must disable the PermissibleClass filter and surface every Category.");
                Assert.That(available.Select(c => c.Name).ToList(),
                    Is.EqualTo(available.Select(c => c.Name).OrderBy(n => n, StringComparer.InvariantCultureIgnoreCase).ToList()),
                    "AvailableCategories must remain alphabetically sorted regardless of the configured scope.");
            });
        }
    }
}
