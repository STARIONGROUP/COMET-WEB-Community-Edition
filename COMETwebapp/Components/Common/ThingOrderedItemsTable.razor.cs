// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ThingOrderedItemsTable.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.Common
{
    using CDP4Common.CommonData;
    using CDP4Common.Types;

    using COMET.Web.Common.Components;

    using COMETwebapp.Services.RowViewModelFactoryService;
    using COMETwebapp.ViewModels.Components.Common.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="ThingOrderedItemsTable{T,TItem,TItemRow}" />
    /// </summary>
    public abstract class ThingOrderedItemsTable<T, TItem, TItemRow> : DisposableComponent where T : Thing where TItem : Thing, new() where TItemRow : BaseDataItemRowViewModel<TItem>
    {
        /// <summary>
        /// Gets or sets the parameter type
        /// </summary>
        [Parameter]
        public T Thing { get; set; }

        /// <summary>
        /// The callback for when the parameter type has changed
        /// </summary>
        [Parameter]
        public EventCallback<T> ThingChanged { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if an item should be created
        /// </summary>
        public bool ShouldCreate { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether the form popup is visible for editing or creating.
        /// </summary>
        public bool IsOnEditMode { get; set; }

        /// <summary>
        /// The item that will be handled for both edit and add forms
        /// </summary>
        public TItem Item { get; protected set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        [Obsolete("Grid-editing members are obsolete. Use StartCreate() and StartEdit(TItemRow) instead.")]
        protected IGrid Grid { get; set; }

        /// <summary>
        /// Gets or sets the ordered list of items from the current <see cref="Thing" />
        /// </summary>
        public abstract OrderedItemList<TItem> OrderedItemsList { get; }

        /// <summary>
        /// Starts the creation flow for a new <typeparamref name="TItem" /> item.
        /// </summary>
        public virtual void StartCreate()
        {
            this.ShouldCreate = true;
            this.Item = new TItem { Iid = Guid.NewGuid() };
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Starts the edit flow for the specified <paramref name="row" />.
        /// </summary>
        /// <param name="row">The selected row to edit.</param>
        public virtual void StartEdit(TItemRow row)
        {
            if (row == null)
            {
                return;
            }

            this.ShouldCreate = false;
            this.Item = (TItem)row.Thing.Clone(true);
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Handles saving of the form popup, updating or adding the item to <see cref="OrderedItemsList" />.
        /// </summary>
        public virtual void OnSaved()
        {
            if (this.ShouldCreate)
            {
                this.OrderedItemsList.Add(this.Item);
            }
            else
            {
                var indexToUpdate = this.OrderedItemsList.FindIndex(x => x.Iid == this.Item.Iid);
                this.OrderedItemsList[indexToUpdate] = this.Item;
            }

            this.ThingChanged.InvokeAsync(this.Thing);
            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Handles cancellation of the form popup.
        /// </summary>
        public virtual void OnCanceled()
        {
            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Moves the selected row up
        /// </summary>
        /// <param name="row">The row to be moved</param>
        /// <returns>A <see cref="Task" /></returns>
        protected virtual async Task MoveUp(TItemRow row)
        {
            var currentIndex = this.OrderedItemsList.IndexOf(row.Thing);
            this.OrderedItemsList.Move(currentIndex, currentIndex - 1);
            await this.ThingChanged.InvokeAsync(this.Thing);
        }

        /// <summary>
        /// Moves the selected row down
        /// </summary>
        /// <param name="row">The row to be moved</param>
        /// <returns>A <see cref="Task" /></returns>
        protected virtual async Task MoveDown(TItemRow row)
        {
            var currentIndex = this.OrderedItemsList.IndexOf(row.Thing);
            this.OrderedItemsList.Move(currentIndex, currentIndex + 1);
            await this.ThingChanged.InvokeAsync(this.Thing);
        }

        /// <summary>
        /// Method that is invoked when an item row is being removed
        /// </summary>
        /// <param name="row">The row to be removed</param>
        protected async Task RemoveItem(TItemRow row)
        {
            this.OrderedItemsList.Remove(row.Thing);
            await this.ThingChanged.InvokeAsync(this.Thing);
        }

        /// <summary>
        /// Method that is invoked when the edit/add quantity kind factor form is being saved
        /// </summary>
        [Obsolete("Grid-editing members are obsolete. Use OnSaved() instead.")]
        protected void OnEditItemSaving()
        {
            if (this.ShouldCreate)
            {
                this.OrderedItemsList.Add(this.Item);
            }
            else
            {
                var indexToUpdate = this.OrderedItemsList.FindIndex(x => x.Iid == this.Item.Iid);
                this.OrderedItemsList[indexToUpdate] = this.Item;
            }

            this.ThingChanged.InvokeAsync(this.Thing);
        }

        /// <summary>
        /// Method used to retrieve the available rows, given the <see cref="Thing" />
        /// </summary>
        /// <returns>A collection of rows to display</returns>
        protected List<TItemRow> GetRows()
        {
            return this.OrderedItemsList?
                .Select(x => (TItemRow)RowViewModelFactory.CreateRow(x))
                .OrderBy(x => x?.Name, StringComparer.InvariantCultureIgnoreCase)
                .ToList();
        }
    }
}
