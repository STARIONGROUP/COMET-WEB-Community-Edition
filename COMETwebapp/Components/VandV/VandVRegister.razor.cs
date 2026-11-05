// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVRegister.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.VandV
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;
    using COMETwebapp.Utilities.VandV;
    using COMETwebapp.ViewModels.Components.VandV;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    using ReactiveUI;

    /// <summary>
    /// The V&amp;V register (Verification Control Document): the shared table of contents beside document-style
    /// rows of requirements, their V&amp;V items with aligned field cells, and the items' procedure steps.
    /// </summary>
    public partial class VandVRegister
    {
        /// <summary>
        /// What an unset field shows: a muted "-", the placeholder COMET uses for an unset value, so a blank never reads as
        /// "checked and empty".
        /// </summary>
        private const string Unset = "-";

        /// <summary>
        /// The CSS class of a passed pill.
        /// </summary>
        private const string PassedPill = "vandv-pill-passed";

        /// <summary>
        /// The CSS class of a failed pill.
        /// </summary>
        private const string FailedPill = "vandv-pill-failed";

        /// <summary>
        /// The CSS class of an open pill.
        /// </summary>
        private const string OpenPill = "vandv-pill-open";

        /// <summary>
        /// The presets offered in the view menu, in display order.
        /// </summary>
        private static readonly VandVViewPreset[] Presets =
        [
            VandVViewPreset.Register, VandVViewPreset.Planning, VandVViewPreset.Procedure, VandVViewPreset.Execution, VandVViewPreset.Compliance
        ];

        /// <summary>
        /// Makes the ids of this instance unique, as a split view can show the tab twice.
        /// </summary>
        private readonly Guid instanceId = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the <see cref="IVandVRegisterViewModel" />.
        /// </summary>
        [Parameter]
        public IVandVRegisterViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IDomDataService" /> used to scroll a focused requirement into view.
        /// </summary>
        [Inject]
        public IDomDataService DomDataService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IJSRuntime" /> used to initialise the table-of-contents drag-to-resize handle.
        /// </summary>
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ILogger{TCategoryName}" /> used to record a swallowed resizer or scroll interop failure.
        /// </summary>
        [Inject]
        public ILogger<VandVRegister> Logger { get; set; }

        /// <summary>
        /// Gets the HTML id of the row of the given thing.
        /// </summary>
        /// <param name="iid">The iid of the row's thing.</param>
        /// <returns>The id.</returns>
        public string RowId(Guid iid)
        {
            return $"vandv-row-{this.instanceId:N}-{iid}";
        }

        /// <summary>
        /// Gets the id of the table of contents.
        /// </summary>
        private string TocId => $"vandv-toc-{this.instanceId:N}";

        /// <summary>
        /// Gets the id of the drag-to-resize handle of the table of contents.
        /// </summary>
        private string TocResizerId => $"vandv-toc-resizer-{this.instanceId:N}";

        /// <summary>
        /// Method invoked when the component is ready to start, having received its initial parameters.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.Rows, x => x.ViewModel.ScrollTarget)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.IsTocCollapsed,
                    x => x.ViewModel.DisplayMode,
                    x => x.ViewModel.TreeUsesShortName,
                    x => x.ViewModel.IndentGroups,
                    x => x.ViewModel.ShowOwner,
                    x => x.ViewModel.ShowCategory)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Scrolls to the focused requirement once it has been rendered.
        /// </summary>
        /// <param name="firstRender">true on the first render.</param>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await RequirementsEditorInterop.InitialiseTocResizerAsync(this.JsRuntime, this.Logger, this.TocResizerId, this.TocId);
            }

            if (this.ViewModel?.ScrollTarget is not { } target)
            {
                return;
            }

            this.ViewModel.ClearScrollTarget();
            await RequirementsEditorInterop.ScrollIntoViewAsync(this.DomDataService, this.Logger, this.RowId(target));
        }

        /// <summary>
        /// Gets the categories shown on a row. An item's category only says it is a V&amp;V item, which its place already does.
        /// </summary>
        /// <param name="row">The <see cref="VandVRegisterRow" />.</param>
        /// <returns>The categories.</returns>
        private static IEnumerable<Category> GetCategories(VandVRegisterRow row)
        {
            return row.Kind != VandVRegisterRowKind.Item && row.Thing is ICategorizableThing thing ? thing.Category : [];
        }

        /// <summary>
        /// Gets the indentation level of a row: its depth, or with "Indent groups" off only what sets an item and a step
        /// apart from their requirement.
        /// </summary>
        /// <param name="row">The <see cref="VandVRegisterRow" />.</param>
        /// <returns>The level.</returns>
        private int GetIndent(VandVRegisterRow row)
        {
            if (this.ViewModel.IndentGroups)
            {
                return row.Depth;
            }

            return row.Kind switch
            {
                VandVRegisterRowKind.Item => 1,
                VandVRegisterRowKind.Step => 2,
                _ => 0
            };
        }

        /// <summary>
        /// Gets the CSS class colouring a requirement's coverage: failed, verified, open, or not covered.
        /// </summary>
        /// <param name="rollUp">The roll-up of the requirement's items.</param>
        /// <returns>The CSS class.</returns>
        private static string GetCoverageClass(VandVRollUp? rollUp)
        {
            return rollUp switch
            {
                null or { Total: 0 } => "vandv-pill-uncovered",
                { Failed: > 0 } => FailedPill,
                { IsVerified: true } => PassedPill,
                _ => OpenPill
            };
        }

        /// <summary>
        /// Gets the CSS class colouring an item's outcome.
        /// </summary>
        /// <param name="outcome">The <see cref="VandVItemOutcome" />.</param>
        /// <returns>The CSS class.</returns>
        private static string GetOutcomeClass(VandVItemOutcome? outcome)
        {
            return outcome switch
            {
                VandVItemOutcome.Passed => PassedPill,
                VandVItemOutcome.Failed => FailedPill,
                _ => OpenPill
            };
        }

        /// <summary>
        /// Gets the CSS class colouring a step's result.
        /// </summary>
        /// <param name="step">The procedure step.</param>
        /// <returns>The CSS class.</returns>
        private static string GetStepResultClass(Requirement step)
        {
            var result = VandVQueries.GetValue(step, VandVConstants.StepResult);

            if (VandVConstants.ValueEquals(result, VandVConstants.StepPass))
            {
                return PassedPill;
            }

            return VandVConstants.ValueEquals(result, VandVConstants.StepFail) || VandVConstants.ValueEquals(result, VandVConstants.StepBlocked)
                ? FailedPill
                : OpenPill;
        }
    }
}
