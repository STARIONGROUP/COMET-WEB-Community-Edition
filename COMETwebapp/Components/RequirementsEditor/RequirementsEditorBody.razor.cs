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
    using COMET.Web.Common.Services.SessionManagement;

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
        /// The injected <see cref="ISessionService" />, used to assert whether the open session allows writing
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets a value indicating whether the open session forbids any modification, which is the case for a session
        /// opened from an ECSS-E-TM-10-25 Annex C3 archive. Requirements can still be browsed and opened, but no
        /// specification, group or requirement can be created
        /// </summary>
        private bool IsReadOnly => this.SessionService.IsReadOnly;

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
                    x => x.ViewModel.ScrollTargetGroup,
                    x => x.ViewModel.ConfirmCancelPopupViewModel.IsVisible)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Scrolls the document to the requirement or group flagged as a scroll target once it has been rendered, after a
        /// traceability link or a table-of-contents entry navigated to it.
        /// </summary>
        /// <param name="firstRender">true on the first render of the component</param>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            var requirementTarget = this.ViewModel?.ScrollTarget;
            var groupTarget = this.ViewModel?.ScrollTargetGroup;

            if (requirementTarget != null)
            {
                this.ViewModel.ScrollTarget = null;
                await this.ScrollElementIntoView(RequirementsDocument.RequirementAnchorId(requirementTarget));
            }
            else if (groupTarget != null)
            {
                this.ViewModel.ScrollTargetGroup = null;
                await this.ScrollElementIntoView(RequirementsDocument.GroupAnchorId(groupTarget));
            }
        }

        /// <summary>
        /// Scrolls the element with the given <paramref name="anchorId" /> into view, swallowing the interop errors that
        /// are harmless for a purely cosmetic scroll.
        /// </summary>
        /// <param name="anchorId">The HTML id of the element to scroll to</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task ScrollElementIntoView(string anchorId)
        {
            try
            {
                await this.DomDataService.ScrollElementIntoView(anchorId);
            }
            catch (Exception exception) when (exception is JSException or JSDisconnectedException)
            {
                // The scroll is purely cosmetic; a stale cached DomData.js (missing ScrollElementIntoView) or a circuit
                // that disconnected mid-render must never kill the page. Navigation already switched to the target.
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
