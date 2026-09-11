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
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="RequirementsChangelog" /> component: it renders the requirements changelog
    /// between the current iteration and a selected baseline iteration as a grid grouped by specification, with a
    /// specification filter; the grid's own column filters handle everything else.
    /// </summary>
    public partial class RequirementsChangelog
    {
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
        /// Gets or sets the callback invoked when the user asks to navigate to the element of a
        /// <see cref="RequirementChange" /> in the document.
        /// </summary>
        [Parameter]
        public EventCallback<RequirementChange> OnNavigateToElement { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IDomDataService" /> used to scroll a changelog row into view.
        /// </summary>
        [Inject]
        public IDomDataService DomDataService { get; set; }

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
        /// <see cref="selectedSpecifications" /> filter; the grid's own column filters refine the rest.
        /// </summary>
        private IEnumerable<RequirementChange> FilteredChanges => this.ViewModel.Changes
            .Where(x => this.selectedSpecifications.Count == 0 || this.selectedSpecifications.Contains(x.SpecificationLabel));

        /// <summary>
        /// Method invoked when the component is ready to start, having received its initial parameters.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.Changes)
                .Subscribe(_ =>
                {
                    this.selectedSpecifications = [];
                    this.InvokeAsync(this.StateHasChanged);
                }));

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.SelectedBaseline,
                    x => x.ViewModel.IsLoading,
                    x => x.ViewModel.HasCompared,
                    x => x.ViewModel.ScrollToElementId)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Scrolls the changelog row of <see cref="IRequirementsChangelogViewModel.ScrollToElementId" /> into view once
        /// it has been requested and rendered, after the user navigates back from the document.
        /// </summary>
        /// <param name="firstRender">true on the first render of the component</param>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (this.ViewModel.ScrollToElementId is { } id)
            {
                this.ViewModel.ClearScrollTarget();

                try
                {
                    await this.DomDataService.ScrollElementIntoView($"changelog-row-{id}");
                }
                catch (Exception exception) when (exception is JSException or JSDisconnectedException)
                {
                    // The scroll is purely cosmetic; a stale cached DomData.js or a circuit that disconnected
                    // mid-render must never kill the page.
                }
            }
        }

        /// <summary>
        /// Gets whether the given <paramref name="change" /> can be navigated to in the document: a deleted element is
        /// no longer part of the current iteration, and a relationship has no document location of its own.
        /// </summary>
        /// <param name="change">The <see cref="RequirementChange" /></param>
        /// <returns>true when a "go to element" button should be shown for the change</returns>
        private static bool IsNavigable(RequirementChange change)
        {
            return change.Kind != RequirementChangeKind.Deleted
                   && change.ElementKind is "Requirement" or "Requirements Group" or "Requirements Specification";
        }

        /// <summary>
        /// Exports the currently filtered <see cref="FilteredChanges" /> to an Excel workbook and offers it for
        /// download.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task ExportAsync()
        {
            await this.ViewModel.ExportAsync(this.FilteredChanges.ToList());
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
