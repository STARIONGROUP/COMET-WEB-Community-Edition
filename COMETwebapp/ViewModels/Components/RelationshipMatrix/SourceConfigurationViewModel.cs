// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SourceConfigurationViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.RelationshipMatrix
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Model.RelationshipMatrix;
    using COMETwebapp.Model.RelationshipMatrix.Configuration;

    using ReactiveUI;

    /// <summary>
    /// Default <see cref="ISourceConfigurationViewModel" /> implementation.
    /// </summary>
    public class SourceConfigurationViewModel : DisposableObject, ISourceConfigurationViewModel
    {
        /// <summary>
        /// The action invoked whenever a filter or option of this axis changes, so that the owning
        /// Relationship Matrix body view model can rebuild the matrix.
        /// </summary>
        private readonly Action onUpdate;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration" />.
        /// </summary>
        private Iteration currentIteration;

        /// <summary>
        /// Backing field for <see cref="SelectedClassKind" />.
        /// </summary>
        private ClassKind? selectedClassKind;

        /// <summary>
        /// Backing field for <see cref="SelectedBooleanOperatorKind" />.
        /// </summary>
        private CategoryBooleanOperatorKind selectedBooleanOperatorKind = CategoryBooleanOperatorKind.Or;

        /// <summary>
        /// Backing field for <see cref="IncludeSubcategories" />.
        /// </summary>
        private bool includeSubcategories = true;

        /// <summary>
        /// Backing field for <see cref="AvailableOwners" />.
        /// </summary>
        private IEnumerable<DomainOfExpertise> availableOwners = [];

        /// <summary>
        /// Backing field for <see cref="SelectedOwners" />.
        /// </summary>
        private IEnumerable<DomainOfExpertise> selectedOwners = [];

        /// <summary>
        /// Backing field for <see cref="SelectedDisplayKind" />.
        /// </summary>
        private MatrixDisplayKind selectedDisplayKind = MatrixDisplayKind.Name;

        /// <summary>
        /// Backing field for <see cref="SelectedSortKind" />.
        /// </summary>
        private MatrixDisplayKind selectedSortKind = MatrixDisplayKind.Name;

        /// <summary>
        /// Backing field for <see cref="SelectedSortOrder" />.
        /// </summary>
        private MatrixSortOrder selectedSortOrder = MatrixSortOrder.Ascending;

        /// <summary>
        /// Creates a new instance of <see cref="SourceConfigurationViewModel" />
        /// </summary>
        /// <param name="onUpdate">The action invoked whenever a filter or option of this axis changes</param>
        public SourceConfigurationViewModel(Action onUpdate)
        {
            this.onUpdate = onUpdate;

            this.Disposables.Add((IDisposable)this.CategorySelector);

            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedClassKind).Subscribe(_ =>
            {
                this.RepopulateCategorySelector();
                this.onUpdate();
            }));

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.CategorySelector.SelectedCategories,
                    x => x.SelectedBooleanOperatorKind,
                    x => x.IncludeSubcategories,
                    x => x.SelectedOwners,
                    x => x.SelectedDisplayKind,
                    x => x.SelectedSortKind,
                    x => x.SelectedSortOrder)
                .Subscribe(_ => this.onUpdate()));
        }

        /// <summary>
        /// Gets or sets the current <see cref="Iteration" />.
        /// </summary>
        public Iteration CurrentIteration
        {
            get => this.currentIteration;
            set
            {
                this.RaiseAndSetIfChanged(ref this.currentIteration, value);
                this.CategorySelector.CurrentIteration = value;
                this.PopulateAvailableOwners();
            }
        }

        /// <summary>
        /// Gets the <see cref="ClassKind" />s that can be picked as this axis' <see cref="SelectedClassKind" />.
        /// </summary>
        public IEnumerable<ClassKind> PossibleClassKinds { get; } =
        [
            ClassKind.ElementDefinition,
            ClassKind.ElementUsage,
            ClassKind.Option,
            ClassKind.RequirementsSpecification,
            ClassKind.RequirementsGroup,
            ClassKind.Requirement
        ];

        /// <summary>
        /// Gets or sets the <see cref="ClassKind" /> of the things shown on this axis.
        /// </summary>
        public ClassKind? SelectedClassKind
        {
            get => this.selectedClassKind;
            set => this.RaiseAndSetIfChanged(ref this.selectedClassKind, value);
        }

        /// <summary>
        /// Gets the <see cref="IMultiCategorySelectorViewModel" /> used to select the <see cref="Category" />s that scope this axis.
        /// </summary>
        public IMultiCategorySelectorViewModel CategorySelector { get; } = new MultiCategorySelectorViewModel { ApplicableClassKinds = [] };

        /// <summary>
        /// Gets the <see cref="CategoryBooleanOperatorKind" />s that can be picked as this axis' <see cref="SelectedBooleanOperatorKind" />.
        /// </summary>
        public IEnumerable<CategoryBooleanOperatorKind> PossibleBooleanOperatorKinds { get; } = Enum.GetValues<CategoryBooleanOperatorKind>();

        /// <summary>
        /// Gets or sets the <see cref="CategoryBooleanOperatorKind" /> used to combine the selected <see cref="Category" />s.
        /// </summary>
        public CategoryBooleanOperatorKind SelectedBooleanOperatorKind
        {
            get => this.selectedBooleanOperatorKind;
            set => this.RaiseAndSetIfChanged(ref this.selectedBooleanOperatorKind, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether membership of a sub-category of a selected <see cref="Category" /> also matches.
        /// </summary>
        public bool IncludeSubcategories
        {
            get => this.includeSubcategories;
            set => this.RaiseAndSetIfChanged(ref this.includeSubcategories, value);
        }

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise" />s that can be picked as owner filters.
        /// </summary>
        public IEnumerable<DomainOfExpertise> AvailableOwners
        {
            get => this.availableOwners;
            private set => this.RaiseAndSetIfChanged(ref this.availableOwners, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="DomainOfExpertise" />s that a thing's <see cref="IOwnedThing.Owner" /> must be one of to be shown on this axis.
        /// </summary>
        public IEnumerable<DomainOfExpertise> SelectedOwners
        {
            get => this.selectedOwners;
            set => this.RaiseAndSetIfChanged(ref this.selectedOwners, value);
        }

        /// <summary>
        /// Gets the <see cref="MatrixDisplayKind" />s that can be picked as this axis' <see cref="SelectedDisplayKind" />.
        /// </summary>
        public IEnumerable<MatrixDisplayKind> PossibleDisplayKinds { get; } = Enum.GetValues<MatrixDisplayKind>();

        /// <summary>
        /// Gets or sets the <see cref="MatrixDisplayKind" /> used to label this axis' things.
        /// </summary>
        public MatrixDisplayKind SelectedDisplayKind
        {
            get => this.selectedDisplayKind;
            set => this.RaiseAndSetIfChanged(ref this.selectedDisplayKind, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="MatrixDisplayKind" /> used to sort this axis' things.
        /// </summary>
        public MatrixDisplayKind SelectedSortKind
        {
            get => this.selectedSortKind;
            set => this.RaiseAndSetIfChanged(ref this.selectedSortKind, value);
        }

        /// <summary>
        /// Gets the <see cref="MatrixSortOrder" />s that can be picked as this axis' <see cref="SelectedSortOrder" />.
        /// </summary>
        public IEnumerable<MatrixSortOrder> PossibleSortOrders { get; } = Enum.GetValues<MatrixSortOrder>();

        /// <summary>
        /// Gets or sets the <see cref="MatrixSortOrder" /> used to sort this axis' things.
        /// </summary>
        public MatrixSortOrder SelectedSortOrder
        {
            get => this.selectedSortOrder;
            set => this.RaiseAndSetIfChanged(ref this.selectedSortOrder, value);
        }

        /// <summary>
        /// Filters and sorts the given <paramref name="candidates" /> according to this axis' configuration.
        /// </summary>
        /// <param name="candidates">The candidate <see cref="DefinedThing" />s</param>
        /// <returns>The matching, ordered things shown on this axis</returns>
        public IReadOnlyList<DefinedThing> QuerySourceThings(IEnumerable<DefinedThing> candidates)
        {
            var selectedCategories = this.CategorySelector.SelectedCategories.ToList();
            var selectedOwnerList = this.SelectedOwners.ToList();

            var things = candidates
                .Where(x => this.SelectedClassKind.HasValue && x.ClassKind == this.SelectedClassKind)
                .Where(x => MatchesCategories((ICategorizableThing)x, selectedCategories, this.SelectedBooleanOperatorKind, this.IncludeSubcategories))
                .Where(x => x is not IOwnedThing ownedThing || selectedOwnerList.Count == 0 || selectedOwnerList.Contains(ownedThing.Owner))
                .Where(x => x is not IDeprecatableThing { IsDeprecated: true });

            return this.SelectedSortOrder == MatrixSortOrder.Ascending
                ? things.OrderBy(this.GetSortKey, StringComparer.InvariantCultureIgnoreCase).ToList()
                : things.OrderByDescending(this.GetSortKey, StringComparer.InvariantCultureIgnoreCase).ToList();
        }

        /// <summary>
        /// Captures this axis' configuration into a <see cref="MatrixSourceConfiguration" /> (used to swap axes and to export).
        /// </summary>
        /// <returns>The captured <see cref="MatrixSourceConfiguration" /></returns>
        public MatrixSourceConfiguration CaptureSnapshot()
        {
            return new MatrixSourceConfiguration
            {
                SelectedClassKind = this.SelectedClassKind,
                SelectedCategories = this.CategorySelector.SelectedCategories.Select(x => x.Iid).ToList(),
                SelectedOwners = this.SelectedOwners.Select(x => x.Iid).ToList(),
                SelectedBooleanOperatorKind = this.SelectedBooleanOperatorKind,
                IncludeSubcategories = this.IncludeSubcategories,
                SelectedDisplayKind = this.SelectedDisplayKind,
                SelectedSortKind = this.SelectedSortKind,
                SortOrder = this.SelectedSortOrder
            };
        }

        /// <summary>
        /// Restores this axis' configuration from a <see cref="MatrixSourceConfiguration" />, silently dropping any
        /// <see cref="Category" /> or <see cref="DomainOfExpertise" /> that no longer resolves against <see cref="CurrentIteration" />.
        /// </summary>
        /// <param name="snapshot">The <see cref="MatrixSourceConfiguration" /> to restore</param>
        public void RestoreSnapshot(MatrixSourceConfiguration snapshot)
        {
            this.SelectedClassKind = snapshot.SelectedClassKind;
            this.SelectedBooleanOperatorKind = snapshot.SelectedBooleanOperatorKind;
            this.IncludeSubcategories = snapshot.IncludeSubcategories;
            this.SelectedDisplayKind = snapshot.SelectedDisplayKind;
            this.SelectedSortKind = snapshot.SelectedSortKind;
            this.SelectedSortOrder = snapshot.SortOrder;

            this.CategorySelector.SelectedCategories = this.CategorySelector.AvailableCategories
                .Where(x => (snapshot.SelectedCategories ?? []).Contains(x.Iid))
                .ToList();

            this.SelectedOwners = this.AvailableOwners
                .Where(x => (snapshot.SelectedOwners ?? []).Contains(x.Iid))
                .ToList();
        }

        /// <summary>
        /// Determines whether the given <paramref name="thing" /> matches the given <paramref name="categories" />,
        /// combined through the given <paramref name="booleanOperatorKind" />. An empty <paramref name="categories" />
        /// selection never matches.
        /// </summary>
        /// <param name="thing">The <see cref="ICategorizableThing" /> to test</param>
        /// <param name="categories">The selected <see cref="Category" />s</param>
        /// <param name="booleanOperatorKind">The <see cref="CategoryBooleanOperatorKind" /> used to combine the categories</param>
        /// <param name="includeSubcategories">Whether sub-category membership also matches</param>
        /// <returns>true if <paramref name="thing" /> matches</returns>
        private static bool MatchesCategories(ICategorizableThing thing, IReadOnlyCollection<Category> categories, CategoryBooleanOperatorKind booleanOperatorKind, bool includeSubcategories)
        {
            if (categories.Count == 0)
            {
                return false;
            }

            bool Matches(Category category)
            {
                return includeSubcategories ? thing.IsMemberOfCategory(category) : thing.Category.Contains(category);
            }

            return booleanOperatorKind == CategoryBooleanOperatorKind.And ? categories.All(Matches) : categories.Any(Matches);
        }

        /// <summary>
        /// Gets the label of the given <paramref name="thing" />, used both to sort and to display it, based on <see cref="SelectedSortKind" />.
        /// </summary>
        /// <param name="thing">The <see cref="DefinedThing" /></param>
        /// <returns>The <see cref="DefinedThing.Name" /> or <see cref="DefinedThing.ShortName" /></returns>
        private string GetSortKey(DefinedThing thing)
        {
            return this.SelectedSortKind == MatrixDisplayKind.ShortName ? thing.ShortName : thing.Name;
        }

        /// <summary>
        /// Repopulates <see cref="AvailableOwners" /> and resets <see cref="SelectedOwners" /> to all available owners.
        /// </summary>
        private void PopulateAvailableOwners()
        {
            this.AvailableOwners = this.CurrentIteration == null
                ? []
                : ((EngineeringModel)this.CurrentIteration.Container).EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name).ToList();

            this.SelectedOwners = [];
        }

        /// <summary>
        /// Scopes <see cref="CategorySelector" /> to <see cref="SelectedClassKind" /> and recomputes its
        /// <see cref="IMultiCategorySelectorViewModel.AvailableCategories" />. Toggling <see cref="IBelongsToIterationSelectorViewModel.CurrentIteration" />
        /// through <see langword="null" /> is required: re-assigning the same <see cref="Iteration" /> reference is a
        /// no-op for a reactive property, so it would not otherwise pick up the new <see cref="IMultiCategorySelectorViewModel.ApplicableClassKinds" />.
        /// </summary>
        private void RepopulateCategorySelector()
        {
            this.CategorySelector.ApplicableClassKinds = this.SelectedClassKind is { } classKind ? [classKind] : this.PossibleClassKinds.ToList();

            var iteration = this.CurrentIteration;
            this.CategorySelector.CurrentIteration = null;
            this.CategorySelector.CurrentIteration = iteration;
        }
    }
}
