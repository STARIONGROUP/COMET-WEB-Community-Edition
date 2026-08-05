// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FilterTagBox.razor.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Components.Selectors
{
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// The single reusable multi-select filter tag box used by every filter bar (dashboards, Parameter Editor,
    /// Requirements). It bakes in the shared behaviour (contains-filtering, a clear-all button, full-name display)
    /// so all filters look and behave identically. Wrap it in a <c>filter-bar-item</c> element to size it.
    /// </summary>
    /// <typeparam name="TItem">The type of the items presented and selected.</typeparam>
    public partial class FilterTagBox<TItem>
    {
        /// <summary>
        /// Gets or sets the collection of items available for selection.
        /// </summary>
        [Parameter]
        public IEnumerable<TItem> Data { get; set; }

        /// <summary>
        /// Gets or sets the collection of currently selected items.
        /// </summary>
        [Parameter]
        public IEnumerable<TItem> Values { get; set; }

        /// <summary>
        /// Gets or sets the callback invoked when the selected items change.
        /// </summary>
        [Parameter]
        public EventCallback<IEnumerable<TItem>> ValuesChanged { get; set; }

        /// <summary>
        /// Gets or sets the name of the item property shown as each tag's text.
        /// </summary>
        [Parameter]
        public string TextFieldName { get; set; }

        /// <summary>
        /// Gets or sets the placeholder text shown when nothing is selected.
        /// </summary>
        [Parameter]
        public string NullText { get; set; }

        /// <summary>
        /// Gets or sets any additional attributes (for example an <c>id</c>) splatted onto the underlying tag box.
        /// Only attributes the caller actually supplies are forwarded, so no empty or null <c>id</c> is ever set on
        /// the DxTagBox (which would make DevExpress build the invalid drop-down selector "#" and throw).
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> AdditionalAttributes { get; set; } = new();
    }
}
