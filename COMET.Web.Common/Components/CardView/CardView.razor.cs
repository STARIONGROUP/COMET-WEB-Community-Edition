// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CardView.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Components
{
    using COMET.Web.Common.Components.CardView;

    using FastMember;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web.Virtualization;

    using System.Linq.Dynamic;
    using System.Linq.Dynamic.Core;

    /// <summary>
    /// Component used to show a CardView based on a specific type
    /// </summary>
    public partial class CardView<T> : DisposableComponent
    {
        /// <summary>
        /// Gets or sets the item template for the list.
        /// </summary>
        [Parameter]
        public RenderFragment<T>? ItemTemplate { get; set; }

        /// <summary>
        /// Gets or sets the list of items of type T to use
        /// </summary>
        [Parameter]
        public ICollection<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Gets or sets a collection of propertynames of type <see cref="T"/> to perform search on
        /// </summary>
        public HashSet<string> SearchFields { get; set; } = [];

        /// <summary>
        /// Gets or sets a collection of propertynames of type <see cref="T"/> to perform sorting on
        /// </summary>
        public SortedSet<string> SortFields { get; set; } = [string.Empty];

        /// <summary>
        /// Gets or sets the fixed height of a Card, used to calculate the amout of items to load into the DOM in px
        /// </summary>
        [Parameter]
        public float ItemSize { get; set; } = 150;

        /// <summary>
        /// Gets or sets the minimum width of a Card
        /// </summary>
        [Parameter]
        public float MinWidth { get; set; } = 250;

        public bool AllowSort { get; set; } = false;

        public bool AllowSearch { get; set; } = false;

        /// <summary>
        /// A reference to the <see cref="Virtualize{T}"/> component for loading items
        /// </summary>
        private Virtualize<T>? virtualize; // Reference to the Virtualize component

        /// <summary>
        /// The FastMember <see cref="TypeAccessor"/> to use to perform actions on instances of <see cref="T"/>
        /// </summary>
        private TypeAccessor typeAccessor = TypeAccessor.Create(typeof(T));

        /// <summary>
        /// The selected Card in the CardView
        /// </summary>
        private T selected;

        /// <summary>
        /// Gets the dragged node used in drag and drop interactions
        /// </summary>
        private T draggedNode { get; set; }

        /// <summary>
        /// Gets or sets the term where to search/filter item of
        /// </summary>
        private string searchTerm { get; set; } = string.Empty;

        public string SelectedSortField { get; set; }

        /// <summary>
        /// Gets the class to visually show a Card to be selected or unselected
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        private string GetSelectedClass(T vm)
        {
            return vm.Equals(this.selected) ? "selected" : "";
        }

        /// <summary>
        /// Set the selected item
        /// </summary>
        /// <param name="item"></param>
        private void selectOption(T item)
        {
            this.selected = item;
        }

        /// <summary>
        /// Method invoked when a node is dragged
        /// </summary>
        /// <param name="node">The dragged node</param>
        private void OnDragNode(T node)
        {
            this.draggedNode = node;
        }

        /// <summary>
        /// Method invoked when a node is dropped
        /// </summary>
        /// <param name="targetNode">The target node where the <see cref="draggedNode"/> has been dropped</param>
        /// <returns>A <see cref="Task"/></returns>
        private async Task OnDropNode(T targetNode)
        {
            //TODO: Implement
            // var targetFolder = (Folder)targetNode.Thing;

            // switch (this.DraggedNode?.Thing)
            // {
            //     case File file:
            //         await this.ViewModel.FileHandlerViewModel.MoveFile(file, targetFolder);
            //         break;
            //     case Folder folder:
            //         await this.ViewModel.FolderHandlerViewModel.MoveFolder(folder, targetFolder);
            //         break;
            // }
        }

        /// <summary>
        /// Filters the list of items to show in the UI based on the <see cref="searchTerm"/>
        /// </summary>
        /// <param name="request">The request to perform filtering of the items list</param>
        /// <returns>an waitable <see cref="ValueTask"/></returns>
        private async ValueTask<ItemsProviderResult<T>> LoadItems(ItemsProviderRequest request)
        {
            // Filter items based on the SearchTerm
            var filteredItems = string.IsNullOrWhiteSpace(this.searchTerm)
                ? this.Items
                : this.Items.Where(item => this.FilterItem(item, this.searchTerm)).ToList();

            // Return paged items for virtualization
            var items = filteredItems.Skip(request.StartIndex).Take(request.Count);

            if (!string.IsNullOrWhiteSpace(this.SelectedSortField))
            {
                items = items.AsQueryable().OrderBy(this.SelectedSortField);
            }

            return new ItemsProviderResult<T>(items.ToList(), filteredItems.Count);
        }

        /// <summary>
        /// Used to filter items based on the <see cref="searchTerm"/>
        /// </summary>
        /// <param name="item">The item to perform searching on</param>
        /// <param name="query">The string to search for</param>
        /// <returns>True if the item's selected properties contain the value to search for, otherwise false</returns>
        private bool FilterItem(T item, string query)
        {
            var props = this.typeAccessor.GetMembers();

            return props.Where(x => this.SearchFields.Contains(x.Name)).Any(prop =>
            {
                var value = this.typeAccessor[item, prop.Name].ToString();
                return value != null && value.Contains(query, StringComparison.OrdinalIgnoreCase);
            });
        }

        /// <summary>
        /// A method that is executed when the user changes the search input element
        /// </summary>
        /// <param name="e">The <see cref="ChangeEventArgs"/> from the UI element's event</param>
        /// <returns></returns>
        private async Task OnSearchTextChanged(ChangeEventArgs e)
        {
            this.searchTerm = e.Value?.ToString() ?? string.Empty;

            if (this.virtualize != null)
            {
                await this.virtualize.RefreshDataAsync(); // Tell Virtualize to refresh data
            }
        }

        internal void RegisterCardField(CardField<T> cardField)
        {
            if (cardField.AllowSort)
            {
                if (this.SortFields.Add(cardField.FieldName))
                {
                    this.AllowSort = true;
                    this.StateHasChanged();
                }
            }

            if (cardField.AllowSearch)
            {
                if (this.SearchFields.Add(cardField.FieldName))
                {
                    this.AllowSearch = true;
                    this.StateHasChanged();
                }
            }
        }

        private void OnSelectedItemChanged(string arg)
        {
            this.SelectedSortField = arg ?? string.Empty;

            this.virtualize?.RefreshDataAsync(); // Tell Virtualize to refresh data
        }
    }
}
