// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewerBody.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.Viewer
{
    using COMET.Web.Common.Components.Applications;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Utilities;

    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="ViewerBody" /> component
    /// </summary>
    public partial class ViewerBody
    {
        /// <summary>
        /// The css class that hides a collapsed panel while keeping it in the render tree.
        /// </summary>
        private const string CollapsedPanelClass = "viewer-panel-collapsed";

        /// <summary>
        /// The minimum width, in pixels, that a drag-resized column may take.
        /// </summary>
        private const int MinimumPanelWidth = 260;

        /// <summary>
        /// Gets or sets a value indicating whether the component is rendered inside a split view pane.
        /// </summary>
        [CascadingParameter(Name = WebAppConstantValues.IsSplitViewCascadingValueName)]
        public bool IsSplitView { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IJSRuntime" /> used to initialise the column resizers.
        /// </summary>
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        /// <summary>
        /// Gets or sets the injected <see cref="ILogger{ViewerBody}" />.
        /// </summary>
        [Inject]
        public ILogger<ViewerBody> Logger { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product tree panel is minimized.
        /// </summary>
        public bool IsProductTreeCollapsed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the properties panel is minimized.
        /// </summary>
        public bool IsPropertiesPanelCollapsed { get; set; }

        /// <summary>
        /// Backing field for tracking the previous <see cref="IsSplitView" /> state
        /// </summary>
        private bool previousIsSplitView;

        /// <summary>
        /// Indicates whether the canvas needs to be re-initialized after layout rendering
        /// </summary>
        private bool needCanvasReinit;

        /// <summary>
        /// The reference to the <see cref="CanvasComponent" /> component
        /// </summary>
        public Canvas3D CanvasComponent { get; private set; }

        /// <summary>
        /// Method invoked after each time the component has been rendered. Note that the component does
        /// not automatically re-render after the completion of any returned <see cref="T:System.Threading.Tasks.Task" />, because
        /// that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await this.InitializeResizers();
                await this.CanvasComponent.ViewModel.InitCanvas(true);
            }
            else if (this.needCanvasReinit)
            {
                this.needCanvasReinit = false;

                if (this.CanvasComponent == null)
                {
                    return;
                }

                await this.CanvasComponent.ViewModel.InitCanvas(true);

                if (this.ViewModel.ProductTreeViewModel.RootViewModel != null)
                {
                    await this.RepopulateScene(this.ViewModel.ProductTreeViewModel.RootViewModel);
                }
            }
        }

        /// <summary>
        /// Initialises the drag-to-resize handles that size the product tree and the properties panel.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the async operation.</returns>
        private async Task InitializeResizers()
        {
            try
            {
                await this.JsRuntime.InvokeVoidAsync("cometResizer.init", "left-resizer", "leftColumn", MinimumPanelWidth);
                await this.JsRuntime.InvokeVoidAsync("cometResizer.init", "right-resizer", "rightColumn", MinimumPanelWidth, 0, true);
            }
            catch (Exception exception)
            {
                // JS interop failures during pre-rendering or test environments are non-fatal.
                this.Logger.LogWarning(exception, "Failed to initialise the column resizers for the 3D Viewer.");
            }
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in the render tree.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the async operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (this.previousIsSplitView && !this.IsSplitView)
            {
                this.needCanvasReinit = true;
            }

            this.previousIsSplitView = this.IsSplitView;
        }

        /// <summary>
        /// Handles the post-assignement flow of the <see cref="ApplicationBase{TViewModel}.ViewModel" /> property
        /// </summary>
        protected override void OnViewModelAssigned()
        {
            base.OnViewModelAssigned();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsLoading).Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            // The collapse chevron moves between the properties header and the finite state header depending on
            // which of the two is on top, so the body has to re-render when the properties panel appears.
            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.PropertiesViewModel.IsVisible)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.OptionSelector.SelectedOption)
                .Subscribe(_ => this.UpdateUrl()));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.ProductTreeViewModel.RootViewModel)
                .SubscribeAsync(_ => this.RepopulateScene(this.ViewModel.ProductTreeViewModel.RootViewModel)));
        }

        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue(QueryKeys.OptionKey, out var option))
            {
                this.ViewModel.OptionSelector.SelectedOption = this.ViewModel.OptionSelector.AvailableOptions.FirstOrDefault(x => x.Iid == option.FromShortGuid());
            }
        }

        /// <summary>
        /// Repopulates the scene starting from the passed <see cref="ViewerNode" />
        /// </summary>
        /// <param name="rootBaseNode">the root node of the tree</param>
        /// <returns>an asynchronous operation</returns>
        private async Task RepopulateScene(ViewerNodeViewModel rootBaseNode)
        {
            if (this.CanvasComponent == null)
            {
                return;
            }

            this.ViewModel.IsLoading = true;
            await this.CanvasComponent.ViewModel.ClearScene();

            var sceneObjects = rootBaseNode.GetFlatListOfDescendants()
                                .Where(x => x.SceneObject.Primitive is not null)
                                .Select(x => x.SceneObject).ToList();

            foreach (var sceneObject in sceneObjects)
            {
                await this.CanvasComponent.ViewModel.AddSceneObject(sceneObject);
            }

            this.ViewModel.IsLoading = false;
        }

        /// <summary>
        /// Sets the url of the <see cref="NavigationManager" /> based on the current values
        /// </summary>
        private void UpdateUrl()
        {
            var additionalParameters = new Dictionary<string, string>();

            if (this.ViewModel.OptionSelector.SelectedOption != null)
            {
                additionalParameters["option"] = this.ViewModel.OptionSelector.SelectedOption.Iid.ToShortGuid();
            }

            this.ViewModel.InitializeViewModel();
            this.UpdateUrlWithParameters(additionalParameters, WebAppConstantValues.ViewerPage);
        }
    }
}
