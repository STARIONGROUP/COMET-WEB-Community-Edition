// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTableViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.ViewModels.Components.ModelEditor
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;

    using COMETwebapp.Components.ModelEditor;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using Microsoft.JSInterop;

    using System.Collections.ObjectModel;

    /// <summary>
    /// ViewModel for the <see cref="ElementDefinitionTable" />
    /// </summary>
    public class ElementDefinitionTableViewModel : DisposableObject, IElementDefinitionTableViewModel
    {
        /// <summary>
        /// All <see cref="ElementBase" /> of the iteration
        /// </summary>
        public List<ElementBase> Elements { get; set; } = new();

        /// <summary>
        /// The current <see cref="Iteration" />
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// Gets or sets the <see cref="IDraggableElementService"/>
        /// </summary>
        public IDraggableElementService DraggableElementService { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="ElementDefinitionTableViewModel" />
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService" /></param>
        /// <param name="draggableElementService">the <see cref="IDraggableElementService" /></param>
        public ElementDefinitionTableViewModel(ISessionService sessionService, IDraggableElementService draggableElementService)
        {
            this.iteration = sessionService.OpenIterations.Items.FirstOrDefault();
            this.DraggableElementService = draggableElementService;
            this.InitializeElements();
            this.PopulateRows();
        }

        /// <summary>
        /// Set the dotnet helper
        /// </summary>
        /// <param name="dotNetHelper">the dotnet helper</param>
        public async Task LoadDotNetHelper(DotNetObjectReference<ElementDefinitionTable> dotNetHelper)
        {
            await this.DraggableElementService.LoadDotNetHelper(dotNetHelper);
        }

        /// <summary>
        ///  Method used to initialize the draggable grids
        /// </summary>
        /// <param name="firstGrid">the first grid</param>
        /// <param name="secondGrid">the second grid</param>
        public async Task InitDraggableGrids(string firstGrid, string secondGrid)
        {
            await this.DraggableElementService.InitDraggableGrids(firstGrid, secondGrid);
        }

        /// <summary>
        /// Gets the collection of the <see cref="ElementDefinitionRowViewModel" /> for target model
        /// </summary>
        public ObservableCollection<ElementDefinitionRowViewModel> RowsTarget { get; } = new();

        /// <summary>
        /// Gets the collection of the <see cref="ElementDefinitionRowViewModel" /> for source model
        /// </summary>
        public ObservableCollection<ElementDefinitionRowViewModel> RowsSource { get; } = new();

        public void PopulateRows()
        {
            this.RowsTarget.Clear();
            this.RowsSource.Clear();
            this.Elements.ForEach(e => this.RowsTarget.Add(new ElementDefinitionRowViewModel(e)));
            this.Elements.ForEach(e => this.RowsSource.Add(new ElementDefinitionRowViewModel(e)));
        }

        /// <summary>
        /// Initialize <see cref="ElementBase" /> list
        /// </summary>
        private void InitializeElements()
        {
            this.iteration.Element.ForEach(e => 
            {
                this.Elements.Add(e);
                this.Elements.AddRange(e.ContainedElement);
            });
        } 
    }
}
