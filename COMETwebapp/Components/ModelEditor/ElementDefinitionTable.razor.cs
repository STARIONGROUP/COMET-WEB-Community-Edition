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
    using COMETwebapp.Services.Interoperability;
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
        /// Gets or sets the <see cref="IDraggableElementService" />
        /// </summary>
        [Inject]
        public IDraggableElementService DraggableElementService { get; set; }

        /// <summary>
        /// Value indicating whether the dragging should be reinitialized.
        /// </summary>
        private bool ReInitializeDragging { get; set; }

        /// <summary>
        ///     Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid FirstGrid { get; set; }

        /// <summary>
        ///     Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid SecondGrid { get; set; }

        /// <summary>
        /// Helper reference to the DotNet object
        /// </summary>
        private DotNetObjectReference<ElementDefinitionTable> DotNetHelper { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

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
                this.DotNetHelper = DotNetObjectReference.Create(this);
                await this.DraggableElementService.LoadDotNetHelper(this.DotNetHelper);
                await this.DraggableElementService.InitDraggableGrids(GetGridSelector(FirstGrid), GetGridSelector(SecondGrid));
            }
            else
            {
                if (this.ReInitializeDragging)
                {
                    ReInitializeDragging = false;
                    await this.DraggableElementService.InitDraggableGrids(GetGridSelector(FirstGrid), GetGridSelector(SecondGrid));
                }
            }
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

            var copiedItem = new ElementDefinitionRowViewModel
            {
                ElementDefinitionName = targetItem.ElementDefinitionName,
                ElementUsageName = sourceItem.ElementDefinitionName,
            };

            targetItems.Add(copiedItem);

            this.ReInitializeDragging = true;
            InvokeAsync(() => StateHasChanged());
        }

        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {

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
