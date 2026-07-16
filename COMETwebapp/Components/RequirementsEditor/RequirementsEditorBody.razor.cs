// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsEditorBody.razor.cs" company="Starion Group S.A.">
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

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="RequirementsEditorBody" /> component.
    /// </summary>
    public partial class RequirementsEditorBody
    {
        /// <summary>
        /// The available <see cref="RequirementRowDisplayMode" />s offered as a layout toggle.
        /// </summary>
        private static readonly RequirementRowDisplayMode[] DisplayModes =
        [
            RequirementRowDisplayMode.ShortNameAndDefinition,
            RequirementRowDisplayMode.NameAndDefinition,
            RequirementRowDisplayMode.ShortNameNameAndDefinition
        ];

        /// <summary>
        /// Whether the "View" layout-and-display-options dropdown is open.
        /// </summary>
        private bool viewMenuOpen;

        /// <summary>
        /// Gets or sets the <see cref="IDomDataService" /> used to scroll a navigated-to requirement into view.
        /// </summary>
        [Inject]
        public IDomDataService DomDataService { get; set; }

        /// <summary>
        /// Handles the post-assignment flow of the <see cref="COMET.Web.Common.Components.Applications.ApplicationBase{TViewModel}.ViewModel" /> property.
        /// A single subscription re-renders the whole body (tree + document) whenever the selection, search or filters change.
        /// </summary>
        protected override void OnViewModelAssigned()
        {
            base.OnViewModelAssigned();

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.SelectedSpecification,
                    x => x.ViewModel.SearchText,
                    x => x.ViewModel.IsTocCollapsed,
                    x => x.ViewModel.DisplayMode,
                    x => x.ViewModel.SelectedOwners,
                    x => x.ViewModel.SelectedCategories,
                    x => x.ViewModel.ScrollTarget)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.ShowHideDeprecatedThingsService.ShowDeprecatedThings)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.DraggedGroup, x => x.ViewModel.DragOverContainer)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.IsOnEditMode,
                    x => x.ViewModel.IsLoading,
                    x => x.ViewModel.ConfirmCancelPopupViewModel.IsVisible)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Scrolls the document to the <see cref="IRequirementsEditorBodyViewModel.ScrollTarget" /> once it has been
        /// rendered, after a traceability link navigated to it.
        /// </summary>
        /// <param name="firstRender">true on the first render of the component</param>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            var target = this.ViewModel?.ScrollTarget;

            if (target != null)
            {
                this.ViewModel.ScrollTarget = null;

                try
                {
                    await this.DomDataService.ScrollElementIntoView(RequirementsDocument.RequirementAnchorId(target));
                }
                catch (Exception exception) when (exception is JSException or JSDisconnectedException)
                {
                    // The scroll is purely cosmetic; a stale cached DomData.js (missing ScrollElementIntoView) or a
                    // circuit that disconnected mid-render must never kill the page. Navigation already switched the
                    // specification and expanded the target's groups.
                }
            }
        }

        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the URL.
        /// The Requirements Editor does not yet take any URL parameters.
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {
        }

        /// <summary>
        /// Gets the short toolbar label for the given <paramref name="mode" />.
        /// </summary>
        /// <param name="mode">The <see cref="RequirementRowDisplayMode" /></param>
        /// <returns>The label</returns>
        private static string GetDisplayModeLabel(RequirementRowDisplayMode mode)
        {
            return mode switch
            {
                RequirementRowDisplayMode.ShortNameAndDefinition => "ID",
                RequirementRowDisplayMode.NameAndDefinition => "Name",
                _ => "ID + Name"
            };
        }

        /// <summary>
        /// Gets the tooltip describing the given <paramref name="mode" />.
        /// </summary>
        /// <param name="mode">The <see cref="RequirementRowDisplayMode" /></param>
        /// <returns>The tooltip text</returns>
        private static string GetDisplayModeTitle(RequirementRowDisplayMode mode)
        {
            return mode switch
            {
                RequirementRowDisplayMode.ShortNameAndDefinition => "Short name – definition on one line",
                RequirementRowDisplayMode.NameAndDefinition => "Name – definition on one line",
                _ => "Short name and name, definition on the next line"
            };
        }
    }
}
