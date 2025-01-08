// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTable.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
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

namespace COMETwebapp.Components.MultiModelEditor
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.MultiModelEditor;
    using COMETwebapp.ViewModels.Components.MultiModelEditor.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="ElementDefinitionTree" /> component
    /// </summary>
    public partial class ElementDefinitionTree
    {
        /// <summary>
        /// The injected <see cref="IElementDefinitionTreeViewModel"/>
        /// </summary>
        [Inject]
        public IElementDefinitionTreeViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the injected <see cref="ILogger{ElementDefinitionTree}"/>
        /// </summary>
        [Inject]
        public ILogger<ElementDefinitionTree> Logger { get; set; }

        /// <summary>
        /// The Iteration
        /// </summary>
        [Parameter]
        public Iteration InitialIteration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that another model can be selected for this TreeView or not
        /// </summary>
        [Parameter]
        public bool IsModelSelectionEnabled { get; set; }

        /// <summary>
        /// Fires after node selection has been changed for a specific item.
        /// </summary>
        [Parameter]
        public EventCallback<ElementBaseTreeRowViewModel> SelectionChanged { get; set; }

        /// <summary>
        /// Fires after node dragging has been started for a specific item.
        /// </summary>
        [Parameter]
        public EventCallback<(ElementDefinitionTree, ElementBaseTreeRowViewModel)> OnDragStart { get; set; } = new();

        /// <summary>
        /// Fires after node dragging has been ended for a specific item.
        /// </summary>
        [Parameter]
        public EventCallback<(ElementDefinitionTree, ElementBaseTreeRowViewModel)> OnDragEnd { get; set; } = new();

        /// <summary>
        /// Fires after node drop has been executed on a specific item.
        /// </summary>
        [Parameter]
        public EventCallback<(ElementDefinitionTree, ElementBaseTreeRowViewModel)> OnDrop { get; set; } = new();

        /// <summary>
        /// Fires after node drag-over has been started for a specific item.
        /// </summary>
        [Parameter]
        public EventCallback<(ElementDefinitionTree, object)> OnDragEnter { get; set; } = new();

        /// <summary>
        /// Fires after node drag-over has been ended for a specific item.
        /// </summary>
        [Parameter]
        public EventCallback<(ElementDefinitionTree, object)> OnDragLeave { get; set; } = new();

        /// <summary>
        /// Fires when calculation of <see cref="AllowNodeDrop"/> is necessary
        /// </summary>
        [Parameter]
        public EventCallback<ElementDefinitionTree> OnCalculateDropIsAllowed { get; set; } = new();

        /// <summary>
        /// Is evaluated when calculation if a node is draggable is necessary
        /// </summary>
        [Parameter]
        public Func<ElementDefinitionTree, ElementBaseTreeRowViewModel, bool> AllowNodeDrag { get; set; } = (_, _) => true;

        /// <summary>
        /// Gets or sets a value indicating that dragging a node is allowed for this <see cref="ElementDefinitionTree"/>
        /// </summary>
        [Parameter]
        public bool AllowDrag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that dropping a node is allowed for this <see cref="ElementDefinitionTree"/>
        /// </summary>
        [Parameter]
        public bool AllowDrop { get; set; }

        /// <summary>
        /// Holds a reference to the object where the dragged node is dragged over
        /// </summary>
        private object dragOverNode;

        /// <summary>
        /// Holds a reference to dragover count due to nested child elements of the dragOverNode
        /// </summary>
        private int dragOverCounter;

        /// <summary>
        /// Gets or sets a value indicating that dropping a node is allowed for this <see cref="ElementDefinitionTree"/>
        /// </summary>
        public bool AllowNodeDrop { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TreeView"/>
        /// </summary>
        public DxTreeView TreeView { get; set; }

        /// <summary>
        /// Method invoked after each time the component has been rendered. Note that the component does
        /// not automatically re-render after the completion of any returned <see cref="Task" />, because
        /// that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="ComponentBase.OnAfterRender" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="ComponentBase.OnAfterRender(bool)" /> and <see cref="OnAfterRenderAsync(bool)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (this.ViewModel.Iteration != this.InitialIteration)
            {
                this.InitialIteration = this.ViewModel.Iteration;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            this.ViewModel.Iteration ??= this.InitialIteration;
        }

        /// <summary>
        /// Clears the selected node(s) in this <see cref="ElementDefinitionTree"/>
        /// </summary>
        public void ClearSelection()
        {
            this.TreeView.ClearSelection();
        }

        /// <summary>
        /// Is executed when dragging of a node has started
        /// </summary>
        /// <param name="node">The node where dragging has been started for</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private async Task DragStartAsync(ElementBaseTreeRowViewModel node)
        {
            await this.OnDragStart.InvokeAsync((this, node));
            await this.OnCalculateDropIsAllowed.InvokeAsync(this);
            this.Logger.LogDebug("DragStart");
        }

        /// <summary>
        /// Is executed when dragging of a node has ended
        /// </summary>
        /// <param name="node">The node where dragging has been ended for</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private async Task DragEndAsync(ElementBaseTreeRowViewModel node)
        {
            await this.OnDragEnd.InvokeAsync((this, node));
            await this.OnCalculateDropIsAllowed.InvokeAsync(this);
            this.Logger.LogDebug("DragEnd");
        }

        /// <summary>
        /// Is executed when a node has been dropped onto another node
        /// </summary>
        /// <param name="node">The node where the dragged node has been dropped onto</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private async Task DropAsync(ElementBaseTreeRowViewModel node)
        {
            this.dragOverNode = null;

            if (this.AllowDrop)
            {
                await this.OnDrop.InvokeAsync((this, node));
                await this.OnCalculateDropIsAllowed.InvokeAsync(this);
                this.Logger.LogDebug("Drop");
            }
        }

        /// <summary>
        /// Is executed when a dragged node hover over another droppable node
        /// </summary>
        /// <param name="node">The node where the dragged node has been hovered over</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private async Task DragEnterAsync(object node)
        {
            if (this.AllowDrop)
            {
                if (this.dragOverNode != node)
                {
                    this.dragOverCounter = 0;
                }

                this.dragOverNode = node;
                this.dragOverCounter++;

                await this.OnDragEnter.InvokeAsync((this, node));
                await this.OnCalculateDropIsAllowed.InvokeAsync(this);

                this.Logger.LogDebug("DragEnter");
            }
        }

        /// <summary>
        /// Is executed when a dragged node is not hovered over another droppable node anymore
        /// </summary>
        /// <param name="node">The node where the dragged node had been hovered over</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private async Task DragLeaveAsync(object node)
        {
            if (this.AllowDrop)
            {
                this.dragOverCounter--;

                if (this.dragOverCounter <= 0)
                {
                    this.dragOverNode = null;
                    await this.OnDragLeave.InvokeAsync((this, node));
                    await this.OnCalculateDropIsAllowed.InvokeAsync(this);
                    this.Logger.LogDebug("DragLeave");
                }
            }
        }
    }
}
