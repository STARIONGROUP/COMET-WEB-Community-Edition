// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ConfirmRemovalPopup.razor.cs" company="Starion Group S.A.">
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
    using COMET.Web.Common.Components;
    using COMET.Web.Common.ViewModels.Components;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Gates the removal of a table row behind the shared <see cref="ConfirmCancelPopup" />: the host asks for a removal
    /// with <see cref="Request" /> and only gets its <see cref="OnRemove" /> callback once the user confirms.
    /// </summary>
    /// <typeparam name="TItem">The reference type of the item being removed.</typeparam>
    public partial class ConfirmRemovalPopup<TItem> : DisposableComponent where TItem : class
    {
        /// <summary>
        /// The item whose removal is pending the user's confirmation, or null when no removal is pending.
        /// </summary>
        private TItem itemToRemove;

        /// <summary>
        /// The header of the confirmation popup.
        /// </summary>
        [Parameter]
        public string HeaderText { get; set; }

        /// <summary>
        /// The question asked in the body of the confirmation popup.
        /// </summary>
        [Parameter]
        public string ContentText { get; set; }

        /// <summary>
        /// Invoked with the requested item once the user confirms its removal.
        /// </summary>
        [Parameter]
        public EventCallback<TItem> OnRemove { get; set; }

        /// <summary>
        /// The <see cref="IConfirmCancelPopupViewModel" /> driving the shared <see cref="ConfirmCancelPopup" />.
        /// </summary>
        public IConfirmCancelPopupViewModel ViewModel { get; } = new ConfirmCancelPopupViewModel
        {
            ConfirmRenderStyle = ButtonRenderStyle.Danger
        };

        /// <summary>
        /// Gets a value indicating whether the confirmation popup is shown.
        /// </summary>
        public bool IsVisible => this.ViewModel.IsVisible;

        /// <summary>
        /// Asks the user to confirm the removal of the given <paramref name="item" />.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void Request(TItem item)
        {
            this.itemToRemove = item;
            this.ViewModel.IsVisible = true;
        }

        /// <summary>
        /// Confirms the pending removal, handing the item to <see cref="OnRemove" />.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task Confirm()
        {
            var item = this.itemToRemove;
            this.Cancel();

            if (item != null)
            {
                await this.OnRemove.InvokeAsync(item);
            }
        }

        /// <summary>
        /// Dismisses the confirmation popup without removing anything.
        /// </summary>
        public void Cancel()
        {
            this.itemToRemove = null;
            this.ViewModel.IsVisible = false;
        }

        /// <summary>
        /// Wires the popup's texts and its Cancel and Confirm callbacks.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            this.ViewModel.HeaderText = this.HeaderText;
            this.ViewModel.ContentText = this.ContentText;
            this.ViewModel.OnCancel = EventCallback.Factory.Create(this, this.Cancel);
            this.ViewModel.OnConfirm = EventCallback.Factory.Create(this, this.Confirm);
        }
    }
}
