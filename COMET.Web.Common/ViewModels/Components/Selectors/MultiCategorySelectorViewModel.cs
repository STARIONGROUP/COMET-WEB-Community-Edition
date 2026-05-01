// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MultiCategorySelectorViewModel.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.ViewModels.Components.Selectors
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using ReactiveUI;

    /// <summary>
    /// Default <see cref="IMultiCategorySelectorViewModel" /> implementation. Mirrors the shape of
    /// <see cref="MultiParameterTypeSelectorViewModel" /> but selects <see cref="Category" />s applicable to
    /// <see cref="ElementBase" /> and its concrete subclasses.
    /// </summary>
    public class MultiCategorySelectorViewModel : BelongsToIterationSelectorViewModel, IMultiCategorySelectorViewModel
    {
        /// <summary>
        /// The <see cref="ClassKind" />s for which a <see cref="Category" /> must be permissible to appear in
        /// <see cref="AvailableCategories" />. Covers the abstract base and both concrete element types so that
        /// categories defined for any of them are surfaced.
        /// </summary>
        private static readonly HashSet<ClassKind> ApplicableClassKinds =
        [
            ClassKind.ElementBase,
            ClassKind.ElementDefinition,
            ClassKind.ElementUsage
        ];

        /// <summary>
        /// Backing field for <see cref="SelectedCategories" />.
        /// </summary>
        private IEnumerable<Category> selectedCategories = new List<Category>();

        /// <summary>
        /// Gets or sets the currently selected <see cref="Category" />s. An empty collection means "no filter" —
        /// every <see cref="Category" /> in <see cref="AvailableCategories" /> matches.
        /// </summary>
        public IEnumerable<Category> SelectedCategories
        {
            get => this.selectedCategories;
            set => this.RaiseAndSetIfChanged(ref this.selectedCategories, value ?? new List<Category>());
        }

        /// <summary>
        /// Gets the collection of <see cref="Category" />s that the user can pick from. Populated from the
        /// current <see cref="Iteration" />'s accessible reference data libraries and filtered to categories whose
        /// <see cref="Category.PermissibleClass" /> covers <see cref="ElementBase" />,
        /// <see cref="ElementDefinition" /> or <see cref="ElementUsage" />.
        /// </summary>
        public IEnumerable<Category> AvailableCategories { get; private set; } = Enumerable.Empty<Category>();

        /// <summary>
        /// Recomputes <see cref="AvailableCategories" /> from the <see cref="SiteDirectory" /> containing the
        /// current <see cref="Iteration" /> and resets <see cref="SelectedCategories" /> to the empty selection.
        /// </summary>
        protected override void UpdateProperties()
        {
            this.SelectedCategories = new List<Category>();

            if (this.CurrentIteration is null)
            {
                this.AvailableCategories = Enumerable.Empty<Category>();
                return;
            }

            var siteDirectory = this.CurrentIteration.IterationSetup.GetContainerOfType<SiteDirectory>();

            this.AvailableCategories = siteDirectory
                .AvailableReferenceDataLibraries()
                .SelectMany(rdl => rdl.QueryCategoriesFromChainOfRdls())
                .Distinct()
                .Where(c => c.PermissibleClass.Any(ApplicableClassKinds.Contains))
                .OrderBy(c => c.Name, StringComparer.InvariantCultureIgnoreCase)
                .ToList();
        }
    }
}
