// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SearchBar.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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
    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// A reusable search text box with a leading magnifying-glass icon. Gives every search field across the
    /// application the same appearance and behaviour; supports two-way binding through <c>@bind-Text</c>.
    /// </summary>
    public partial class SearchBar
    {
        /// <summary>
        /// The current search text. Supports two-way binding via <c>@bind-Text</c>.
        /// </summary>
        [Parameter]
        public string Text { get; set; }

        /// <summary>
        /// Invoked when <see cref="Text" /> changes, enabling <c>@bind-Text</c>.
        /// </summary>
        [Parameter]
        public EventCallback<string> TextChanged { get; set; }

        /// <summary>
        /// The placeholder shown when the box is empty.
        /// </summary>
        [Parameter]
        public string Placeholder { get; set; } = "Search...";

        /// <summary>
        /// When the bound value is pushed back: on every keystroke or after a typing pause. Defaults to
        /// <see cref="DevExpress.Blazor.BindValueMode.OnDelayedInput" />.
        /// </summary>
        [Parameter]
        public BindValueMode BindValueMode { get; set; } = BindValueMode.OnDelayedInput;

        /// <summary>
        /// The debounce, in milliseconds, applied when <see cref="BindValueMode" /> is
        /// <see cref="DevExpress.Blazor.BindValueMode.OnDelayedInput" />.
        /// </summary>
        [Parameter]
        public int InputDelay { get; set; } = 500;

        /// <summary>
        /// Additional CSS classes appended to the search box.
        /// </summary>
        [Parameter]
        public string CssClass { get; set; }

        /// <summary>
        /// Gets the CSS class applied to the underlying text box: the shared inline-search-icon styling plus
        /// any caller-supplied <see cref="CssClass" />.
        /// </summary>
        private string EffectiveCssClass => string.IsNullOrWhiteSpace(this.CssClass) ? "inline-search-icon" : $"inline-search-icon {this.CssClass}";

        /// <summary>
        /// Propagates a text change from the underlying text box to the bound value.
        /// </summary>
        /// <param name="value">The new search text.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        private async Task OnTextChangedAsync(string value)
        {
            this.Text = value;
            await this.TextChanged.InvokeAsync(value);
        }
    }
}
