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

namespace COMET.Web.Common.Components.CardView
{
    using System.Linq.Dynamic.Core;

    using FastMember;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web.Virtualization;

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
        /// Gets or sets the fixed height of a Card, used to calculate the amout of items to load into the DOM in px
        /// </summary>
        [Parameter]
        public float ItemSize { get; set; } = 150;

        /// <summary>
        /// Gets or sets the minimum width of a Card
        /// </summary>
        [Parameter]
        public float MinWidth { get; set; } = 250;

        /// <summary>
        /// Gets or sets a collection of propertynames of type T to perform search on
        /// </summary>
        public HashSet<string> SearchFields { get; private set; } = [];

        /// <summary>
        /// Gets or sets a collection of propertynames of type T to perform sorting on
        /// </summary>
        public SortedSet<string> SortFields { get; private set; } = [string.Empty];

        /// <summary>
        /// Gets or sets a value indication that sorting is allowed
        /// </summary>
        public bool AllowSort { get; set; }

        /// <summary>
        /// Gets or sets a value indication that searching is allowed
        /// </summary>
        public bool AllowSearch { get; set; }

        /// <summary>
        /// hold a reference to the previously selected Item.
        /// Typically used to check for changes in Items collection
        /// </summary>
        private ICollection<T> previousItems;

        /// <summary>
        /// A reference to the <see cref="Virtualize{T}"/> component for loading items
        /// </summary>
        private Virtualize<T>? virtualize; // Reference to the Virtualize component

        /// <summary>
        /// The FastMember <see cref="FastMember.TypeAccessor"/> to use to perform actions on instances of type T
        /// </summary>
        private TypeAccessor typeAccessor = TypeAccessor.Create(typeof(T));

        /// <summary>
        /// The selected Card in the CardView
        /// </summary>
        private T selected;

        /// <summary>
        /// Gets or sets the term where to search/filter items on
        /// </summary>
        public string SearchTerm { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the term where to sort items on
        /// </summary>
        public string SelectedSortField { get; set; } = string.Empty;

        /// <summary>
        /// Gets the class to visually show a Card to be selected or unselected
        /// </summary>
        /// <param name="vm"></param>
        /// <returns>A string that retrurns the css class for selected Card</returns>
        private string GetSelectedClass(T vm)
        {
            return vm.Equals(this.selected) ? "selected" : "";
        }

        /// <summary>
        /// Set the selected item
        /// </summary>
        /// <param name="item">The item of type T</param>
        public void SelectItem(T item)
        {
            this.selected = item;
        }

        /// <summary>
        /// Filters the list of items to show in the UI based on the <see cref="SearchTerm"/>
        /// </summary>
        /// <param name="request">The request to perform filtering of the items list</param>
        /// <returns>an waitable <see cref="ValueTask"/></returns>
        private ValueTask<ItemsProviderResult<T>> LoadItemsAsync(ItemsProviderRequest request)
        {
            // Filter items based on the SearchTerm
            var filteredItems = !this.AllowSearch || string.IsNullOrWhiteSpace(this.SearchTerm)
                ? this.Items
                : this.Items.Where(item => this.FilterItem(item, this.SearchTerm)).ToList();

            // Return paged items for virtualization
            var items = filteredItems.Skip(request.StartIndex).Take(request.Count);

            if (this.AllowSort && !string.IsNullOrWhiteSpace(this.SelectedSortField))
            {
                items = items.AsQueryable().OrderBy(this.SelectedSortField);
            }

            return new ValueTask<ItemsProviderResult<T>>(new ItemsProviderResult<T>(items.ToList(), filteredItems.Count));
        }

        /// <summary>
        /// Used to filter items based on the <see cref="SearchTerm"/>
        /// </summary>
        /// <param name="item">The item to perform searching on</param>
        /// <param name="query">The string to search for</param>
        /// <returns>True if the item's selected properties contain the value to search for, otherwise false</returns>
        private bool FilterItem(T item, string query)
        {
            if (this.AllowSearch)
            {
                var props = this.typeAccessor.GetMembers();

                return props.Where(x => this.SearchFields.Contains(x.Name)).Any(prop =>
                {
                    var value = this.typeAccessor[item, prop.Name].ToString();
                    return value != null && value.Contains(query, StringComparison.OrdinalIgnoreCase);
                });
            }

            return true;
        }

        /// <summary>
        /// A method that is executed when the user changes the search input element
        /// </summary>
        /// <param name="value">The text from the UI element's event</param>
        private void OnSearchTextChanged(string value)
        {
            this.SearchTerm = value ?? string.Empty;

            this.virtualize?.RefreshDataAsync(); // Tell Virtualize to refresh data
        }

        /// <summary>
        /// Initializes the <see cref="CardField{T}"/>
        /// </summary>
        /// <param name="cardField">the <see cref="CardField{T}"/></param>
        internal void InitializeCardField(CardField<T> cardField)
        {
            if (cardField.AllowSort && this.SortFields.Add(cardField.FieldName))
            {
                this.AllowSort = true;
                this.StateHasChanged();
            }

            if (cardField.AllowSearch && this.SearchFields.Add(cardField.FieldName))
            {
                this.AllowSearch = true;
                this.StateHasChanged();
            }
        }

        /// <summary>
        /// Handles the selection of a the Sort item
        /// </summary>
        /// <param name="newVal"></param>
        public void OnSelectedSortItemChanged(string newVal)
        {
            this.SelectedSortField = newVal ?? string.Empty;

            this.virtualize?.RefreshDataAsync(); // Tell Virtualize to refresh data
        }

        /// <summary>
        /// Raised when a parameter is set
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (!Equals(this.previousItems, this.Items) && this.virtualize != null)
            {
                await this.virtualize.RefreshDataAsync(); // Tell Virtualize to refresh data
            }

            this.previousItems = this.Items;
        }
    }
}
