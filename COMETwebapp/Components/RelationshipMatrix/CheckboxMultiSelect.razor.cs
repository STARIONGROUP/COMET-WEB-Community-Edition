// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CheckboxMultiSelect.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.RelationshipMatrix
{
    using COMET.Web.Common.Components;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// A multi-select dropdown that shows its options as checkboxes (so several values can be picked in a single
    /// opening) while keeping a compact single-line summary of the selection in the closed trigger. Mirrors the
    /// COMET-IME category/owner pickers.
    /// </summary>
    /// <typeparam name="T">The type of the selectable items</typeparam>
    public partial class CheckboxMultiSelect<T> : DisposableComponent
    {
        /// <summary>
        /// A stable identifier used both as the trigger element id and as the <see cref="DxDropDown" /> position target.
        /// </summary>
        private readonly string triggerId = $"cms-{Guid.NewGuid():N}";

        /// <summary>
        /// A value indicating whether the dropdown is open.
        /// </summary>
        private bool isOpen;

        /// <summary>
        /// Gets or sets the selectable items.
        /// </summary>
        [Parameter]
        public IEnumerable<T> Data { get; set; } = [];

        /// <summary>
        /// Gets or sets the currently selected items.
        /// </summary>
        [Parameter]
        public IEnumerable<T> Values { get; set; } = [];

        /// <summary>
        /// Gets or sets the callback invoked when the selection changes.
        /// </summary>
        [Parameter]
        public EventCallback<IEnumerable<T>> ValuesChanged { get; set; }

        /// <summary>
        /// Gets or sets the name of the property used to label each item in the list.
        /// </summary>
        [Parameter]
        public string TextFieldName { get; set; }

        /// <summary>
        /// Gets or sets the function used to compute the label of an item in the closed-trigger summary.
        /// </summary>
        [Parameter]
        public Func<T, string> TextSelector { get; set; }

        /// <summary>
        /// Gets or sets the text shown in the trigger when nothing is selected.
        /// </summary>
        [Parameter]
        public string NullText { get; set; } = "Select...";

        /// <summary>
        /// Gets the identifier of the trigger element.
        /// </summary>
        private string TriggerId => this.triggerId;

        /// <summary>
        /// Gets the CSS selector pointing at the trigger element, used as the dropdown's position target.
        /// </summary>
        private string TriggerSelector => $"#{this.triggerId}";

        /// <summary>
        /// Gets the single-line summary shown in the closed trigger.
        /// </summary>
        private string SummaryText
        {
            get
            {
                var selected = this.Values?.ToList() ?? [];
                return selected.Count == 0 ? this.NullText : string.Join("; ", selected.Select(x => this.TextSelector is null ? x?.ToString() : this.TextSelector(x)));
            }
        }

        /// <summary>
        /// Gets a value indicating whether every available item is selected.
        /// </summary>
        private bool AreAllSelected
        {
            get
            {
                var data = this.Data?.ToList() ?? [];
                return data.Count != 0 && data.All(x => this.Values?.Contains(x) == true);
            }
        }

        /// <summary>
        /// Forwards a list-box selection change to <see cref="ValuesChanged" />.
        /// </summary>
        /// <param name="values">The newly selected items</param>
        private Task OnValuesChanged(IEnumerable<T> values)
        {
            return this.ValuesChanged.InvokeAsync(values?.ToList() ?? []);
        }

        /// <summary>
        /// Selects or clears every available item in response to the "(Select All)" checkbox.
        /// </summary>
        /// <param name="selectAll">Whether every item should be selected</param>
        private Task OnSelectAllChanged(bool selectAll)
        {
            return this.ValuesChanged.InvokeAsync(selectAll ? this.Data?.ToList() ?? [] : []);
        }
    }
}
