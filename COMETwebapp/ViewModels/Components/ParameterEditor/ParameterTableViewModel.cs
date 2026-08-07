// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTableViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.ParameterEditor
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Components.ParameterEditor.ParameterTable" />
    /// </summary>
    public class ParameterTableViewModel : DisposableObject, IParameterTableViewModel
    {
        /// <summary>
        /// The <see cref="IPermissionService" />
        /// </summary>
        private readonly IPermissionService permissionService;

        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// The set of <see cref="ElementBase" /> <c>Iid</c>s currently selected as a multi-select filter. Empty means
        /// "no filter".
        /// </summary>
        private HashSet<Guid> currentElementBaseIds = new();

        /// <summary>
        /// The currently effective <see cref="Option" />s. An empty selection resolves to the <see cref="Iteration" />'s
        /// default <see cref="Option" />, mirroring the previous single-select behavior.
        /// </summary>
        private List<Option> currentOptions = new();

        /// <summary>
        /// The set of <see cref="ParameterType" /> <c>Iid</c>s currently selected as a multi-select filter. Empty
        /// means "no filter".
        /// </summary>
        private HashSet<Guid> currentParameterTypeIds = new();

        /// <summary>
        /// The set of <see cref="Category" /> <c>Iid</c>s currently selected as a multi-select filter. Empty means
        /// "no filter"; otherwise rows whose owning <see cref="ElementBase" /> does not carry (directly or
        /// through its referenced <see cref="ElementDefinition" /> / super-categories) at least one of these
        /// categories are removed.
        /// </summary>
        private HashSet<Guid> currentCategoryIds = new();

        /// <summary>
        /// Gets or sets the <see cref="ParameterBaseRowViewModel" /> for this <see cref="ParameterTableViewModel" />
        /// </summary>
        private DomainOfExpertise domainOfExpertise;

        /// <summary>
        /// Backing field for <see cref="IsOnEditMode" />
        /// </summary>
        private bool isOnEditMode;

        /// <summary>
        /// Creates a new instance of <see cref="ParameterTableViewModel" />
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// Value asserting if only owned <see cref="ParameterOrOverrideBase" /> should be visible
        /// </summary>
        private bool ownedParameters;

        /// <summary>
        /// Gets the injected <see cref="ICDPMessageBus"/>
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// Creates a new instance of <see cref="ParameterTableViewModel" />
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        public ParameterTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus)
        {
            this.messageBus = messageBus;

            this.Disposables.Add(this.messageBus.Listen<HaveComponentParameterTypeSelectedEvent>()
                .Subscribe(x => this.HandleComponentSelected(x.HaveComponentParameter)));

            this.sessionService = sessionService;
            this.permissionService = this.sessionService.Session.PermissionService;
        }

        /// <summary>
        /// Indicates if compound parameter edit popup is visible
        /// </summary>
        public bool IsOnEditMode
        {
            get => this.isOnEditMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnEditMode, value);
        }

        /// <summary>
        /// The <see cref="IHaveComponentParameterTypeEditor" /> to show in the popup
        /// </summary>
        public IHaveComponentParameterTypeEditor HaveComponentParameterTypeEditorViewModel { get; set; }

        /// <summary>
        /// Gets the collection of the <see cref="ParameterBaseRowViewModel" />
        /// </summary>
        public SourceList<ParameterBaseRowViewModel> Rows { get; } = new();

        /// <summary>
        /// Gets the total count of parameters available before applying filters
        /// </summary>
        public int TotalParametersCount { get; private set; }

        /// <summary>
        /// Initializes this <see cref="IParameterTableViewModel" />
        /// </summary>
        /// <param name="currentIteration">The current <see cref="Iteration" /></param>
        /// <param name="currentDomain">The <see cref="DomainOfExpertise" /></param>
        /// <param name="selectedOptions">
        /// The collection of selected <see cref="Option" />s. <c>null</c> or an empty collection falls back to the
        /// <see cref="Iteration" />'s default <see cref="Option" />.
        /// </param>
        public void InitializeViewModel(Iteration currentIteration, DomainOfExpertise currentDomain, IEnumerable<Option> selectedOptions)
        {
            this.iteration = currentIteration;
            this.domainOfExpertise = currentDomain;
            this.currentOptions = this.ResolveSelectedOptions(selectedOptions);
            this.currentElementBaseIds = new HashSet<Guid>();
            this.currentParameterTypeIds = new HashSet<Guid>();
            this.Rows.Clear();

            if (this.iteration != null)
            {
                var ownedNestedParameters = this.currentOptions
                    .SelectMany(option => this.iteration.QueryParameterAndOverrideBases(option, this.domainOfExpertise))
                    .DistinctBy(x => x.Iid);

                this.TotalParametersCount = this.GetAllParameters().Count;
                this.Rows.AddRange(this.CreateParameterBaseRowViewModels(ownedNestedParameters, this.currentOptions.Select(x => x.Iid).ToHashSet()));
            }
        }

        /// <summary>
        /// Update the current <see cref="DomainOfExpertise" />
        /// </summary>
        /// <param name="currentDomain">The new <see cref="DomainOfExpertise" /></param>
        public void UpdateDomain(DomainOfExpertise currentDomain)
        {
            this.domainOfExpertise = currentDomain;
        }

        /// <summary>
        /// Apply filters based on a multi-select set of <see cref="Option" />s, a multi-select set of
        /// <see cref="ElementBase" />s, a multi-select set of <see cref="ParameterType" />s, a multi-select set of
        /// <see cref="Category" />s and <see cref="DomainOfExpertise" />.
        /// </summary>
        /// <param name="selectedOptions">
        /// The collection of selected <see cref="Option" />s. <c>null</c> or an empty collection falls back to the
        /// <see cref="Iteration" />'s default <see cref="Option" />.
        /// </param>
        /// <param name="selectedElementBases">
        /// The collection of <see cref="ElementBase" />s to filter on. <c>null</c> or an empty collection means no
        /// element filter is applied; otherwise rows whose owning <see cref="ElementBase" /> is not in the
        /// collection are removed.
        /// </param>
        /// <param name="selectedParameterTypes">
        /// The collection of <see cref="ParameterType" />s to filter on. <c>null</c> or an empty collection means
        /// no parameter-type filter is applied; otherwise rows whose <see cref="ParameterType" /> is not in the
        /// collection are removed.
        /// </param>
        /// <param name="selectedCategories">
        /// The collection of <see cref="Category" />s to filter on. <c>null</c> or an empty collection means no
        /// category filter is applied; otherwise rows whose owning <see cref="ElementBase" /> does not carry at
        /// least one of these categories (transitively, including the referenced
        /// <see cref="ElementDefinition" />'s categories for an <see cref="ElementUsage" />, and super-categories)
        /// are removed.
        /// </param>
        /// <param name="isOwnedParameters">
        /// Value asserting that only <see cref="Thing" />s owned by the current <see cref="DomainOfExpertise" />
        /// should be visible.
        /// </param>
        public void ApplyFilters(IEnumerable<Option> selectedOptions, IEnumerable<ElementBase> selectedElementBases, IEnumerable<ParameterType> selectedParameterTypes, IEnumerable<Category> selectedCategories, bool isOwnedParameters)
        {
            if (this.iteration == null)
            {
                return;
            }

            this.currentOptions = this.ResolveSelectedOptions(selectedOptions);

            this.currentElementBaseIds = selectedElementBases == null
                ? new HashSet<Guid>()
                : new HashSet<Guid>(selectedElementBases.Select(x => x.Iid));

            this.currentParameterTypeIds = selectedParameterTypes == null
                ? new HashSet<Guid>()
                : new HashSet<Guid>(selectedParameterTypes.Select(x => x.Iid));

            this.currentCategoryIds = selectedCategories == null
                ? new HashSet<Guid>()
                : new HashSet<Guid>(selectedCategories.Select(x => x.Iid));

            this.ownedParameters = isOwnedParameters;

            var parameters = this.GetAllParameters();
            this.TotalParametersCount = parameters.Count;

            var rows = this.CreateRowsBasedOnFilters(parameters);
            this.UpdateVisibleRows(rows);
        }

        /// <summary>
        /// Queries all <see cref="ParameterOrOverrideBase" /> from the current <see cref="Iteration" /> based on the current <see cref="Option" />s.
        /// </summary>
        /// <returns>A list of <see cref="ParameterOrOverrideBase" /> instances.</returns>
        private List<ParameterOrOverrideBase> GetAllParameters()
        {
            return this.currentOptions
                    .SelectMany(option => this.iteration.QueryParameterAndOverrideBases(option))
                    .DistinctBy(x => x.Iid)
                    .ToList();
        }

        /// <summary>
        /// Resolves the effective collection of <see cref="Option" />s to filter on. An empty or <c>null</c>
        /// <paramref name="selectedOptions" /> falls back to the current <see cref="Iteration" />'s default
        /// <see cref="Option" /> (or its first <see cref="Option" /> if none is set as default), mirroring the
        /// behavior of the former single-select <see cref="Option" /> filter.
        /// </summary>
        /// <param name="selectedOptions">The collection of <see cref="Option" />s selected by the user.</param>
        /// <returns>A non-null collection of <see cref="Option" />s to filter on.</returns>
        private List<Option> ResolveSelectedOptions(IEnumerable<Option> selectedOptions)
        {
            var options = selectedOptions?.ToList() ?? new List<Option>();

            if (options.Count > 0)
            {
                return options;
            }

            var defaultOption = this.iteration?.DefaultOption ?? this.iteration?.Option.FirstOrDefault();
            return defaultOption == null ? new List<Option>() : new List<Option> { defaultOption };
        }

        /// <summary>
        /// Remove rows related to a <see cref="Thing" /> that has been deleted
        /// </summary>
        /// <param name="deletedThings">A collection of deleted <see cref="Thing" /></param>
        public void RemoveRows(IEnumerable<Thing> deletedThings)
        {
            var rowsToRemove = new List<ParameterBaseRowViewModel>();

            foreach (var deletedThing in deletedThings)
            {
                switch (deletedThing)
                {
                    case ElementBase elementBase:
                        rowsToRemove.AddRange(this.Rows.Items.Where(x => x.Parameter.Container.Iid == elementBase.Iid));
                        break;
                    case ParameterOrOverrideBase parameter:
                        rowsToRemove.AddRange(this.Rows.Items.Where(x => x.Parameter.Iid == parameter.Iid));
                        break;
                    case ParameterValueSetBase parameterValueSetBase:
                        rowsToRemove.AddRange(this.Rows.Items.Where(x => x.ValueSetId == parameterValueSetBase.Iid));
                        break;
                }
            }

            this.Rows.RemoveMany(rowsToRemove);
        }

        /// <summary>
        /// Add rows related to <see cref="Thing" /> that has been added
        /// </summary>
        /// <param name="addedThings">A collection of added <see cref="Thing" /></param>
        public void AddRows(IEnumerable<Thing> addedThings)
        {
            var rows = this.CreateRowsBasedOnFilters(QueryParameterOrOverrides(addedThings).ToList());

            this.Rows.AddRange(rows.Where(x => this.Rows.Items.All(r => r.ValueSetId != x.ValueSetId)));
        }

        /// <summary>
        /// Updates rows related to <see cref="Thing" /> that have been updated
        /// </summary>
        /// <param name="updatedThings">A collection of updated <see cref="Thing" /></param>
        public void UpdateRows(IEnumerable<Thing> updatedThings)
        {
            var parameterOrOverrideBases = QueryParameterOrOverrides(updatedThings);

            foreach (var parameterValueSetBase in parameterOrOverrideBases.SelectMany(x => x.ValueSets).OfType<ParameterValueSetBase>())
            {
                var existingRow = this.Rows.Items.FirstOrDefault(x => x.ValueSetId == parameterValueSetBase.Iid);

                if (existingRow == null)
                {
                    continue;
                }

                var isReadOnly = !this.permissionService.CanWrite(parameterValueSetBase.Container);
                existingRow.UpdateProperties(isReadOnly);
            }
        }

        /// <summary>
        /// Set the <see cref="HaveComponentParameterTypeEditorViewModel" /> to show in the popup
        /// </summary>
        /// <param name="haveComponentParameterTypeEditorViewModel"></param>
        public void HandleComponentSelected(IHaveComponentParameterTypeEditor haveComponentParameterTypeEditorViewModel)
        {
            this.HaveComponentParameterTypeEditorViewModel = haveComponentParameterTypeEditorViewModel;
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Query all <see cref="ParameterOrOverrideBase" /> from the collection of <see cref="Thing" />s
        /// </summary>
        /// <param name="things">A collection of <see cref="Thing" /></param>
        /// <returns>All retrieved <see cref="ParameterOrOverrideBase" /></returns>
        private static IEnumerable<ParameterOrOverrideBase> QueryParameterOrOverrides(IEnumerable<Thing> things)
        {
            var parameters = new List<ParameterOrOverrideBase>();

            foreach (var addedThing in things)
            {
                switch (addedThing)
                {
                    case ElementBase elementBase:
                        parameters.AddRange(elementBase.QueryParameterAndOverrideBases());
                        break;
                    case ParameterOrOverrideBase parameter:
                        parameters.Add(parameter);
                        break;
                    case ParameterValueSetBase parameterValueSetBase:
                        parameters.Add(parameterValueSetBase.Container as ParameterOrOverrideBase);
                        break;
                }
            }

            return parameters.DistinctBy(x => x.Iid);
        }

        /// <summary>
        /// Create a collection of <see cref="ParameterBaseRowViewModel" /> based on selected filters
        /// </summary>
        /// <param name="parameters">
        /// The collection of <see cref="ParameterOrOverrideBase" /> for creating
        /// <see cref="ParameterBaseRowViewModel" />
        /// </param>
        /// <returns>A collection of <see cref="ParameterBaseRowViewModel" /></returns>
        private IEnumerable<ParameterBaseRowViewModel> CreateRowsBasedOnFilters(List<ParameterOrOverrideBase> parameters)
        {
            if (this.ownedParameters)
            {
                parameters.RemoveAll(x => x.Owner.Iid != this.domainOfExpertise.Iid);
            }

            if (this.currentElementBaseIds.Count > 0)
            {
                ApplyElementBaseFilter(parameters, this.currentElementBaseIds);
            }

            if (this.currentParameterTypeIds.Count > 0)
            {
                ApplyParameterTypeFilter(parameters, this.currentParameterTypeIds);
            }

            if (this.currentCategoryIds.Count > 0)
            {
                ApplyCategoryFilter(parameters, this.currentCategoryIds);
            }

            var optionIds = this.currentOptions.Select(x => x.Iid).ToHashSet();
            return this.CreateParameterBaseRowViewModels(parameters, optionIds).DistinctBy(x => x.ValueSetId);
        }

        /// <summary>
        /// Update the <see cref="Rows" /> collection based on the provided collection of <see cref="ParameterBaseRowViewModel" />
        /// </summary>
        /// <param name="rows">The collection of <see cref="ParameterBaseRowViewModel" /> that should be displayed</param>
        private void UpdateVisibleRows(IEnumerable<ParameterBaseRowViewModel> rows)
        {
            rows = rows.ToList();

            var rowsToRemove = this.Rows.Items
                .Where(x => rows.All(p => p.ValueSetId != x.ValueSetId));

            var rowsToAdd = rows.Where(x => this.Rows.Items.All(p => p.ValueSetId != x.ValueSetId));

            this.Rows.AddRange(rowsToAdd);
            this.Rows.RemoveMany(rowsToRemove);
        }

        /// <summary>
        /// Apply a filtering pass that retains only parameters whose <see cref="ParameterType" /> identifier is in
        /// <paramref name="parameterTypeIds" />. Mutates <paramref name="parameters" /> in place.
        /// </summary>
        /// <param name="parameters">A collection of <see cref="ParameterOrOverrideBase" /> to filter.</param>
        /// <param name="parameterTypeIds">The <see cref="HashSet{T}" /> of allowed <see cref="ParameterType" /> <c>Iid</c> values.</param>
        private static void ApplyParameterTypeFilter(List<ParameterOrOverrideBase> parameters, HashSet<Guid> parameterTypeIds)
        {
            parameters.RemoveAll(x => !parameterTypeIds.Contains(x.ParameterType.Iid));
        }

        /// <summary>
        /// Apply a filtering pass that retains only parameters whose owning <see cref="ElementBase" /> carries at
        /// least one of the categories whose identifier is in <paramref name="categoryIds" />. Resolution uses
        /// <see cref="CategorizableThingExtensions.GetAllCategories(ICategorizableThing, bool)" /> so that an
        /// <see cref="ElementUsage" /> inherits the categories of its referenced <see cref="ElementDefinition" />,
        /// and super-categories are honoured. Mutates <paramref name="parameters" /> in place.
        /// </summary>
        /// <param name="parameters">A collection of <see cref="ParameterOrOverrideBase" /> to filter.</param>
        /// <param name="categoryIds">The <see cref="HashSet{T}" /> of allowed <see cref="Category" /> <c>Iid</c> values.</param>
        private static void ApplyCategoryFilter(List<ParameterOrOverrideBase> parameters, HashSet<Guid> categoryIds)
        {
            parameters.RemoveAll(p =>
            {
                if (p.Container is not ElementBase elementBase)
                {
                    return true;
                }

                return !elementBase.GetAllCategories().Any(c => categoryIds.Contains(c.Iid));
            });
        }

        /// <summary>
        /// Apply a filtering pass that retains only parameters whose owning <see cref="ElementBase" /> identifier
        /// (or, for an <see cref="ElementUsage" />, its referenced <see cref="ElementDefinition" /> identifier) is
        /// in <paramref name="elementBaseIds" />. Mutates <paramref name="parameters" /> in place.
        /// </summary>
        /// <param name="parameters">A collection of <see cref="ParameterOrOverrideBase" /> to filter</param>
        /// <param name="elementBaseIds">The <see cref="HashSet{T}" /> of allowed <see cref="ElementBase" /> <c>Iid</c> values</param>
        private static void ApplyElementBaseFilter(List<ParameterOrOverrideBase> parameters, HashSet<Guid> elementBaseIds)
        {
            parameters.RemoveAll(x => x.Container switch
            {
                ElementDefinition elementDefinition => !elementBaseIds.Contains(elementDefinition.Iid),
                ElementUsage elementUsage => !elementBaseIds.Contains(elementUsage.Iid) && !elementBaseIds.Contains(elementUsage.ElementDefinition.Iid),
                _ => true
            });
        }

        /// <summary>
        /// Creates <see cref="ParameterBaseRowViewModel" /> based on a multi-select set of <see cref="Option" />s
        /// </summary>
        /// <param name="parameters">A collection of <see cref="ParameterOrOverrideBase" /></param>
        /// <param name="optionIds">The set of <see cref="Guid" /> of the selected <see cref="Option" />s</param>
        /// <returns>A collection of created <see cref="ParameterBaseRowViewModel" /></returns>
        private List<ParameterBaseRowViewModel> CreateParameterBaseRowViewModels(IEnumerable<ParameterOrOverrideBase> parameters, HashSet<Guid> optionIds)
        {
            var rows = new List<ParameterBaseRowViewModel>();

            foreach (var parameterOrOverrideBase in parameters.OrderBy(x => x.ParameterType.Name))
            {
                var isReadOnly = !this.permissionService.CanWrite(parameterOrOverrideBase);

                rows.AddRange(parameterOrOverrideBase.ValueSets.Where(x => x.ActualOption == null || optionIds.Contains(x.ActualOption.Iid))
                    .Where(x => rows.TrueForAll(r => ((ParameterValueSetBase)r.ValueSet).Iid != ((ParameterValueSetBase)x).Iid))
                    .Select(x => new ParameterBaseRowViewModel(this.sessionService, isReadOnly, parameterOrOverrideBase, x, this.messageBus)));
            }

            return rows;
        }
    }
}
