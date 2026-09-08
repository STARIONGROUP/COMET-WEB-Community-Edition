// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsChangelog.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.RequirementsEditor
{
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="RequirementsChangelog" /> component: it renders the requirements changelog
    /// between the current iteration and a selected baseline iteration as a grid grouped by specification, with a
    /// change-kind filter and a specification filter.
    /// </summary>
    public partial class RequirementsChangelog
    {
        /// <summary>
        /// The <see cref="RequirementChangeKind" />s currently shown; defaults to every kind present in the last
        /// comparison.
        /// </summary>
        private HashSet<RequirementChangeKind> activeKinds = [];

        /// <summary>
        /// The <see cref="RequirementChange.SpecificationLabel" />s currently selected in the specification filter; an
        /// empty selection shows every specification.
        /// </summary>
        private List<string> selectedSpecifications = [];

        /// <summary>
        /// Gets or sets the <see cref="IRequirementsChangelogViewModel" />.
        /// </summary>
        [Parameter]
        public IRequirementsChangelogViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Gets the distinct <see cref="RequirementChange.SpecificationLabel" />s present in the
        /// <see cref="ViewModel" />'s <see cref="IRequirementsChangelogViewModel.Changes" />, offered by the
        /// specification filter.
        /// </summary>
        private IEnumerable<string> AvailableSpecifications => this.ViewModel.Changes
            .Select(x => x.SpecificationLabel)
            .Distinct()
            .OrderBy(x => x);

        /// <summary>
        /// Gets the <see cref="ViewModel" />'s <see cref="IRequirementsChangelogViewModel.Changes" /> that pass the
        /// <see cref="activeKinds" /> and <see cref="selectedSpecifications" /> filters.
        /// </summary>
        private IEnumerable<RequirementChange> FilteredChanges => this.ViewModel.Changes
            .Where(x => this.activeKinds.Contains(x.Kind))
            .Where(x => this.selectedSpecifications.Count == 0 || this.selectedSpecifications.Contains(x.SpecificationLabel));

        /// <summary>
        /// Method invoked when the component is ready to start, having received its initial parameters.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.Changes)
                .Subscribe(changes =>
                {
                    this.activeKinds = changes.Select(x => x.Kind).Distinct().ToHashSet();
                    this.selectedSpecifications = [];
                    this.InvokeAsync(this.StateHasChanged);
                }));

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.SelectedBaseline,
                    x => x.ViewModel.IsLoading,
                    x => x.ViewModel.HasCompared)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Gets whether the given <paramref name="kind" /> is currently shown.
        /// </summary>
        /// <param name="kind">The <see cref="RequirementChangeKind" /></param>
        /// <returns>true when rows of this kind are shown</returns>
        private bool IsKindActive(RequirementChangeKind kind)
        {
            return this.activeKinds.Contains(kind);
        }

        /// <summary>
        /// Shows or hides the rows of the given <paramref name="kind" />.
        /// </summary>
        /// <param name="kind">The <see cref="RequirementChangeKind" /></param>
        private void ToggleKind(RequirementChangeKind kind)
        {
            if (!this.activeKinds.Remove(kind))
            {
                this.activeKinds.Add(kind);
            }
        }

        /// <summary>
        /// Colours a data row according to its <see cref="RequirementChange.Kind" />: green for a created or restored
        /// change, blue for a modified change, and red for a deprecated or deleted change.
        /// </summary>
        /// <param name="e">The <see cref="GridCustomizeElementEventArgs" /></param>
        private void OnCustomizeElement(GridCustomizeElementEventArgs e)
        {
            if (e.ElementType != GridElementType.DataRow)
            {
                return;
            }

            var change = (RequirementChange)this.Grid.GetDataItem(e.VisibleIndex);

            e.CssClass = change.Kind switch
            {
                RequirementChangeKind.Created or RequirementChangeKind.Restored => "changelog-row-created",
                RequirementChangeKind.Modified => "changelog-row-modified",
                RequirementChangeKind.Deprecated or RequirementChangeKind.Deleted => "changelog-row-deleted",
                _ => e.CssClass
            };
        }
    }
}
