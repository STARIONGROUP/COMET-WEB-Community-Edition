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

namespace COMETwebapp.Components.ModelEditor
{
    using COMET.Web.Common.Components.Applications;
    using COMET.Web.Common.Extensions;

    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.ViewModels.Components.ModelEditor.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="ElementDefinitionTable" /> component
    /// </summary>
    public partial class ElementDefinitionTable
    {
        /// <summary>
        /// Gets or sets the <see cref="IDraggableElementService" />
        /// </summary>
        [Inject]
        public IDraggableElementService DraggableElementService { get; set; }

        /// <summary>
        /// Value indicating whether the dragging should be reinitialized.
        /// </summary>
        private bool ReInitializeDragging { get; set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid FirstGrid { get; set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid SecondGrid { get; set; }

        /// <summary>
        /// Helper reference to the DotNet object
        /// </summary>
        private DotNetObjectReference<ElementDefinitionTable> DotNetHelper { get; set; }

        /// <summary>
        /// The validation messages to display
        /// </summary>
        private string ErrorMessage { get; set; }

        /// <summary>
        /// Method invoked when dropping a row in the grid
        /// </summary>
        [JSInvokable]
        public void MoveGridRow(int droppableIndex, int draggableRowVisibleIndex, bool fromFirstGrid)
        {
            var sourceGrid = fromFirstGrid ? this.FirstGrid : this.SecondGrid;
            var targetGrid = fromFirstGrid ? this.SecondGrid : this.FirstGrid;

            var targetItems = fromFirstGrid ? this.ViewModel.RowsSource : this.ViewModel.RowsTarget;

            var sourceItem = (ElementDefinitionRowViewModel)sourceGrid.GetDataItem(draggableRowVisibleIndex - 1);
            var targetItem = (ElementDefinitionRowViewModel)targetGrid.GetDataItem(droppableIndex - 1);

            if (sourceItem.ElementDefinitionName == targetItem.ElementDefinitionName)
            {
                this.ErrorMessage = "Cannot move an element definition to itself";
            }
            else
            {
                this.ErrorMessage = string.Empty;

                var copiedItem = new ElementDefinitionRowViewModel
                {
                    ElementDefinitionName = targetItem.ElementDefinitionName,
                    ElementUsageName = sourceItem.ElementDefinitionName
                };

                targetItems.Add(copiedItem);

                this.ReInitializeDragging = true;
            }

            this.InvokeAsync(this.StateHasChanged);
        }

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
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                this.DotNetHelper = DotNetObjectReference.Create(this);
                await this.DraggableElementService.LoadDotNetHelper(this.DotNetHelper);
                await this.DraggableElementService.InitDraggableGrids(GetGridSelector(this.FirstGrid), GetGridSelector(this.SecondGrid));
            }
            else
            {
                if (this.ReInitializeDragging)
                {
                    this.ReInitializeDragging = false;
                    await this.DraggableElementService.InitDraggableGrids(GetGridSelector(this.FirstGrid), GetGridSelector(this.SecondGrid));
                }
            }
        }

        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {
        }

        /// <summary>
        /// Handles the post-assignement flow of the <see cref="ApplicationBase{TViewModel}.ViewModel" /> property
        /// </summary>
        protected override void OnViewModelAssigned()
        {
            base.OnViewModelAssigned();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnCreationMode).SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnAddingParameterMode).SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Method used to define the grid selector
        /// </summary>
        private static string GetGridSelector(IGrid grid)
        {
            return string.Join(
                string.Empty,
                grid.CssClass
                    .Split(" ")
                    .Where(i => !string.IsNullOrWhiteSpace(i))
                    .Select(i => "." + i.Trim())
            );
        }

        /// <summary>
        /// Method executed when a <see cref="ElementDefinitionRowViewModel" /> is selected
        /// </summary>
        /// <param name="args">The <see cref="GridRowClickEventArgs" /></param>
        private void OnElementSelected(GridRowClickEventArgs args)
        {
            var selectedNode = (ElementDefinitionRowViewModel)args.Grid.GetDataItem(args.VisibleIndex);
            SetGridSelectedDataItem(args.Grid, selectedNode);

            if (args.Grid == this.FirstGrid)
            {
                SetGridSelectedDataItem(this.SecondGrid, null);
            }

            if (args.Grid == this.SecondGrid)
            {
                SetGridSelectedDataItem(this.FirstGrid, null);
            }

            this.ViewModel.SelectElement(selectedNode.ElementBase);
        }

        /// <summary>
        /// Method invoked to highlight the top element and the selected groups
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeElementEventArgs" /> </param>
        private static void OnCustomizeElement(GridCustomizeElementEventArgs e)
        {
            if (e.ElementType != GridElementType.GroupCell)
            {
                return;
            }

            var isTopElement = (bool)e.Grid.GetRowValue(e.VisibleIndex, nameof(ElementDefinitionRowViewModel.IsTopElement));

            if (isTopElement)
            {
                e.CssClass = "font-weight-bold";
            }

            var elementDefinitionName = (string)e.Grid.GetRowValue(e.VisibleIndex, nameof(ElementDefinitionRowViewModel.ElementDefinitionName));
            var selectedElementDefinitionName = (ElementDefinitionRowViewModel)e.Grid.SelectedDataItem;

            var isSubItemSelected = ((IEnumerable<ElementDefinitionRowViewModel>)e.Grid.Data)
                .Any(x => x.ElementDefinitionName == elementDefinitionName && x.ElementDefinitionName == selectedElementDefinitionName?.ElementDefinitionName);

            if (isSubItemSelected)
            {
                e.CssClass += " highlighted-item";
            }
        }

        /// <summary>
        /// Sets the selected data item from the given grid
        /// </summary>
        /// <param name="grid">The grid to be updated</param>
        /// <param name="dataItem">The new data item</param>
        private static void SetGridSelectedDataItem(IGrid grid, object dataItem)
        {
            grid.BeginUpdate();
            grid.SelectedDataItem = dataItem;
            grid.EndUpdate();
        }
    }
}
