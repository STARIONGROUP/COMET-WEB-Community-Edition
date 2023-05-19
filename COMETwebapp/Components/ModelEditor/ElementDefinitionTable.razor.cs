// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElemenDefinitionTable.razor.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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
    using COMETwebapp.ViewModels.Components.ModelEditor;
    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    /// <summary>
    /// Support class for the <see cref="ElementDefinitionTable" /> component
    /// </summary>
    public partial class ElementDefinitionTable
    {
        /// <summary>
        /// Gets or sets the <see cref="IElementDefinitionTableViewModel" />
        /// </summary>
        [Inject]
        public IElementDefinitionTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Property used to invoke the JavaScript module
        /// </summary>
        [Inject]
        public IJSRuntime JS { get; set; }

        /// <summary>
        /// Value indicating whether the dragging should be reinitialized.
        /// </summary>
        bool ReInitializeDragging { get; set; }

        /// <summary>
        ///     Gets or sets the grid control that is being customized.
        /// </summary>
        IGrid FirstGrid { get; set; }

        /// <summary>
        ///     Gets or sets the grid control that is being customized.
        /// </summary>
        IGrid SecondGrid { get; set; }

        /// <summary>
        /// Helper reference to the DotNet object
        /// </summary>
        DotNetObjectReference<ElementDefinitionTable> DotNetHelper { get; set; }

        /// <summary>
        /// Property used to reference the JavaScript module
        /// </summary>
        IJSObjectReference JsModule { get; set; }

        /// <summary>
        /// Method invoked after each time the component has been rendered. Note that the component does
        /// not automatically re-render after the completion of any returned <see cref="Task"/>, because
        /// that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="OnAfterRender(bool)"/> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="OnAfterRender(bool)"/> and <see cref="OnAfterRenderAsync(bool)"/> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender"/> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                JsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./Components/ModelEditor/ElementDefinitionTable.razor.js");

                DotNetHelper = DotNetObjectReference.Create(this);
                await JsModule.InvokeVoidAsync("setDotNetHelper", DotNetHelper);
                await JsModule.InvokeVoidAsync("initialize", GetGridSelector(FirstGrid), GetGridSelector(SecondGrid));
            }
            else
            {
                if (ReInitializeDragging)
                {
                    ReInitializeDragging = false;
                    await JsModule.InvokeVoidAsync("initialize", GetGridSelector(FirstGrid), GetGridSelector(SecondGrid));
                }
            }
        }

        /// <summary>
        ///     Method invoked to add index to the data row element
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs"/>
        void Grid_CustomizeElement(GridCustomizeElementEventArgs e)
        {
            if (e.ElementType == GridElementType.DataRow)
                e.Attributes["data-visible-index"] = e.VisibleIndex;
        }

        /// <summary>
        ///  Method invoked when dropping a row in the grid
        /// </summary>
        [JSInvokable]
        public void MoveGridRow(int droppableIndex, int draggableRowVisibleIndex, bool fromFirstGrid)
        {
            var sourceGrid = fromFirstGrid ? FirstGrid : SecondGrid;
            var targetGrid = fromFirstGrid ? SecondGrid : FirstGrid;

            var targetItems = fromFirstGrid ? this.ViewModel.RowsSource : this.ViewModel.RowsTarget;

            var sourceItem = (ElementDefinitionRowViewModel)sourceGrid.GetDataItem(draggableRowVisibleIndex - 1);
            var targetItem = (ElementDefinitionRowViewModel)targetGrid.GetDataItem(droppableIndex - 1);
            sourceItem.ElementUsageName = sourceItem.ElementDefinitionName;
            sourceItem.ElementDefinitionName = targetItem.ElementDefinitionName;
            targetItems.Add(sourceItem);

            ReInitializeDragging = true;
            StateHasChanged();
        }

        /// <summary>
        ///   Method used to define the grid selector
        /// </summary>
        static string GetGridSelector(IGrid grid)
        {
            return string.Join(
                string.Empty,
                grid.CssClass
                    .Split(" ")
                    .Where(i => !string.IsNullOrWhiteSpace(i))
                    .Select(i => "." + i.Trim())
            );
        }
    }
}
