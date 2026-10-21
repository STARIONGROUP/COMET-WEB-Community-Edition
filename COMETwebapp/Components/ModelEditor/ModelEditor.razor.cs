// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelEditor.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Components.ModelEditor
{
    using System.ComponentModel;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Components.Applications;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.ModelEditor.Rows;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.JSInterop;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="ModelEditor" /> component
    /// </summary>
    public partial class ModelEditor
    {
        /// <summary>
        /// The minimum width, in pixels, that a panel can be dragged down to
        /// </summary>
        private const int MinimumPanelWidth = 260;

        /// <summary>
        /// The injected <see cref="ISessionService" />, used to assert whether the open session allows writing
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets a value indicating whether the open session forbids any modification, which is the case for a
        /// session opened from an ECSS-E-TM-10-25 Annex C3 archive. Both trees become non-draggable and
        /// non-droppable when this is <see langword="true" />, so their content can still be inspected but never
        /// modified.
        /// </summary>
        public bool IsReadOnly => this.SessionService.IsReadOnly;

        /// <summary>
        /// Holds a reference to the data of the node where another node is dragged over
        /// </summary>
        private (ElementDefinitionTree, object) DragOverObject;

        /// <summary>
        /// Holds a reference to the data of the node that is currently dragged
        /// </summary>
        private (ElementDefinitionTree, ElementBaseTreeRowViewModel) DragObject;

        /// <summary>
        /// The validation messages to display
        /// </summary>
        private string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the source tree component
        /// </summary>
        public ElementDefinitionTree SourceTree { get; set; }

        /// <summary>
        /// Gets or sets the target tree component
        /// </summary>
        public ElementDefinitionTree TargetTree { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the source model panel is minimized, so that the target model tree gets more space
        /// </summary>
        public bool IsSourcePanelCollapsed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the details panel is minimized, so that the trees get more space
        /// </summary>
        public bool IsDetailsPanelCollapsed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the owning domain of expertise pill is shown on the nodes of both trees.
        /// The setting is held here, and not per tree, so that the source and the target panel always show the same thing.
        /// </summary>
        public bool ShowOwner { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the category pills are shown on the nodes of both trees
        /// </summary>
        public bool ShowCategories { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the tree nodes of both trees show the full element name (when
        /// <see langword="true" />) or the short name (when <see langword="false" />). Held here, not per tree, so
        /// the source and target panels always display the same way.
        /// </summary>
        public bool ShowName { get; set; } = true;

        /// <summary>
        /// Gets or sets the <see cref="IJSRuntime" /> used to initialise the column resizers
        /// </summary>
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        /// <summary>
        /// Subscribes to the iteration of both trees and initialises the drag-to-resize handles that sit between the source tree,
        /// the target tree and the details panel, on first render
        /// </summary>
        /// <param name="firstRender"><see langword="true" /> on the first render cycle.</param>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (!firstRender)
            {
                return;
            }

            this.SubscribeToTreeIterations();

            await this.InitialiseResizerAsync("source-resizer", "sourcePanel");
            await this.InitialiseResizerAsync("target-resizer", "targetPanel");
        }

        /// <summary>
        /// Subscribes to the <see cref="IElementDefinitionTreeViewModel.Iteration" /> of both trees, so that the ViewModel knows
        /// which iteration each panel shows and can tell whether a copy between the panels crosses iterations.
        /// This has to happen on the first render and not in <see cref="OnViewModelAssigned" />: the trees are captured with
        /// @ref, so they are still null while the parameters are set, and a component is not an
        /// <see cref="INotifyPropertyChanged" />, which means a WhenAnyValue over this.SourceTree would read that null once and
        /// never see the tree appear.
        /// </summary>
        private void SubscribeToTreeIterations()
        {
            this.Disposables.Add(this.SourceTree.ViewModel.WhenAnyValue(x => x.Iteration).SubscribeAsync(x =>
            {
                this.ViewModel.SourceIteration = x;
                return this.InvokeAsync(this.StateHasChanged);
            }));

            this.Disposables.Add(this.TargetTree.ViewModel.WhenAnyValue(x => x.Iteration).SubscribeAsync(x =>
            {
                this.ViewModel.TargetIteration = x;
                return this.InvokeAsync(this.StateHasChanged);
            }));
        }

        /// <summary>
        /// Initialises a single drag-to-resize handle, tolerating the JS interop being unavailable
        /// </summary>
        /// <param name="resizerId">The id of the resize handle</param>
        /// <param name="panelId">The id of the panel that the handle resizes</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task InitialiseResizerAsync(string resizerId, string panelId)
        {
            try
            {
                await this.JsRuntime.InvokeVoidAsync("cometResizer.init", resizerId, panelId, MinimumPanelWidth);
            }
            catch (Exception)
            {
                // JS interop failures during pre-rendering or test environments are non-fatal.
            }
        }

        /// <summary>
        /// Handles the post-assignement flow of the <see cref="ApplicationBase{TViewModel}.ViewModel" /> property
        /// </summary>
        protected override void OnViewModelAssigned()
        {
            base.OnViewModelAssigned();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnCopySettingsMode).SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsSourceModelSameAsTargetModel).SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {
        }

        /// <summary>
        /// Method executed when a <see cref="ElementBaseTreeRowViewModel" /> is selected
        /// </summary>
        /// <param name="elementRowViewModel">The <see cref="ElementBaseTreeRowViewModel" /></param>
        private void OnElementSelected(ElementBaseTreeRowViewModel elementRowViewModel)
        {
            this.ViewModel.DetailsPanelViewModel.SelectElement(elementRowViewModel?.ElementBase);
        }

        /// <summary>
        /// Is executed when dragging has been started for a specific node (<see cref="ElementBaseTreeRowViewModel"/>) in a specific <see cref="ElementDefinitionTree"/>
        /// </summary>
        /// <param name="nodeData">A <see cref="Tuple"/> that contains the specific <see cref="ElementDefinitionTree"/> and the specific node (<see cref="ElementBaseTreeRowViewModel"/>)</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private Task OnDragStartAsync((ElementDefinitionTree, ElementBaseTreeRowViewModel) nodeData)
        {
            this.DragObject = nodeData;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Is executed when dragging has been ended for a specific node (<see cref="ElementBaseTreeRowViewModel"/>) in a specific <see cref="ElementDefinitionTree"/>
        /// </summary>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private Task OnDragEndAsync()
        {
            this.DragObject = (null, null);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Is executed when a dragged node (<see cref="ElementBaseTreeRowViewModel"/>) has been dropped onto another element in a specific <see cref="ElementDefinitionTree"/>
        /// </summary>
        /// <param name="nodeData">
        /// A <see cref="Tuple"/> that contains the specific <see cref="ElementDefinitionTree"/>, the specific node
        /// (<see cref="ElementBaseTreeRowViewModel"/>) and the <see cref="DragEventArgs"/> of the drop
        /// </param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private async Task OnDropAsync((ElementDefinitionTree, ElementBaseTreeRowViewModel, DragEventArgs) nodeData)
        {
            this.ErrorMessage = string.Empty;

            if (this.IsReadOnly || this.DragObject.Item2 is null)
            {
                return;
            }

            try
            {
                switch (this.DragObject.Item2, nodeData.Item2)
                {
                    case (ElementDefinitionTreeRowViewModel draggedDefinition, null):
                        // Drop in the empty area of a tree: copy the ElementDefinition there.
                        await this.ViewModel.CopyAndAddNewElementAsync(nodeData.Item1, draggedDefinition.ElementBase, nodeData.Item3.GetCopyOperationKind());
                        break;
                    case (ElementUsageTreeRowViewModel draggedUsage, ElementDefinitionTreeRowViewModel targetDefinition):
                        // Move (re-parent) the existing usage under the target ElementDefinition.
                        await this.ViewModel.MoveElementUsageAsync((ElementUsage)draggedUsage.ElementBase, (ElementDefinition)targetDefinition.ElementBase);
                        break;
                    case (ElementDefinitionTreeRowViewModel draggedDefinition, ElementDefinitionTreeRowViewModel targetDefinition):
                        // Dragging a definition onto a node creates a fresh usage of it.
                        await this.ViewModel.AddNewElementUsageAsync(draggedDefinition.ElementBase, targetDefinition.ElementBase);
                        break;
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
            finally
            {
                this.DragOverObject = (null, null);
                this.DragObject = (null, null);

                this.StateHasChanged();
            }
        }

        /// <summary>
        /// Is executed when a dragged node (<see cref="ElementBaseTreeRowViewModel"/>) hovers over a specific element (<see cref="object"/>) in a specific <see cref="ElementDefinitionTree"/>
        /// </summary>
        /// <param name="elementData">A <see cref="Tuple"/> that contains the specific <see cref="ElementDefinitionTree"/> and the specific element (<see cref="object"/>)</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private Task OnDragEnterAsync((ElementDefinitionTree, object) elementData)
        {
            this.DragOverObject = elementData;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Is executed when a dragged node (<see cref="ElementBaseTreeRowViewModel"/>) leaves a previously hovered over specific element (<see cref="object"/>) in a specific <see cref="ElementDefinitionTree"/>
        /// </summary>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private Task OnDragLeaveAsync()
        {
            this.DragOverObject = (null, null);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the AllowNodeDrop property for a specific node in a <see cref="ElementDefinitionTree"/>, based on the calculated <see cref="ElementBase"/> data for <see cref="DragOverObject"/> and <see cref="DragObject"/>
        /// </summary>
        /// <param name="elementDefinitionTree">The <see cref="ElementDefinitionTree"/> to calculate this for</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private Task SetDropIsAllowedAsync(ElementDefinitionTree elementDefinitionTree)
        {
            elementDefinitionTree.AllowNodeDrop = this.CalculateDropIsAllowed(elementDefinitionTree);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Calculates is dropping of a dragged node (<see cref="DragObject"/>) is allowed onto the node where it hovers over (<see cref="DragOverObject"/>)
        /// </summary>
        /// <param name="elementDefinitionTree">The <see cref="ElementDefinitionTree"/> where to calculate for</param>
        /// <returns>A value indicating is dropping is actually allowed</returns>
        private bool CalculateDropIsAllowed(ElementDefinitionTree elementDefinitionTree)
        {
            var dragOverObject = this.DragOverObject;
            var dragObject = this.DragObject;

            if (!elementDefinitionTree.AllowDrop || dragObject == (null, null) || dragOverObject == dragObject || dragObject.Item2 is null)
            {
                return false;
            }

            // Hovering a node: only an ElementDefinition node is a valid drop target (a usage node is not).
            if (dragOverObject.Item2 is ElementBaseTreeRowViewModel)
            {
                if (dragOverObject.Item2 is not ElementDefinitionTreeRowViewModel dragOverVm)
                {
                    return false;
                }

                var targetDefinition = (ElementDefinition)dragOverVm.ElementBase;

                // Both a move and a create-usage are only allowed within a single iteration and with write permission.
                if (dragObject.Item2.ElementBase.GetContainerOfType<Iteration>() != targetDefinition.GetContainerOfType<Iteration>()
                    || !this.ViewModel.CanWriteElementUsage(targetDefinition))
                {
                    return false;
                }

                return dragObject.Item2 switch
                {
                    ElementUsageTreeRowViewModel usageRow => ((ElementUsage)usageRow.ElementBase).Container != targetDefinition
                                                             && !((ElementUsage)usageRow.ElementBase).ElementDefinition.HasUsageOf(targetDefinition),
                    ElementDefinitionTreeRowViewModel definitionRow => (ElementDefinition)definitionRow.ElementBase != targetDefinition
                                                                       && !((ElementDefinition)definitionRow.ElementBase).HasUsageOf(targetDefinition),
                    _ => false
                };
            }

            // Hovering the empty area of a tree: only an ElementDefinition can be copied there.
            return dragOverObject.Item1 == elementDefinitionTree && dragObject.Item2 is ElementDefinitionTreeRowViewModel;
        }
    }
}
