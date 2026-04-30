// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CategoriesTable.razor.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Inline grid that manages the <see cref="ICategorizableThing.Category" /> collection of a parent
    /// <see cref="ICategorizableThing" />. The grid renders one row per available <see cref="Category" /> with a
    /// checkbox column; ticking or unticking a row mutates the parent's category list in place so the change is
    /// committed together with the parent when the surrounding form is saved. Selected categories are pulled to
    /// the top of the list and the remainder is sorted alphabetically by name.
    /// </summary>
    public partial class CategoriesTable : DisposableComponent
    {
        /// <summary>
        /// The parent <see cref="ICategorizableThing" /> whose <see cref="ICategorizableThing.Category" /> collection
        /// is being edited.
        /// </summary>
        [Parameter]
        public ICategorizableThing Thing { get; set; }

        /// <summary>
        /// The categories the user may pick from. The caller is expected to have already filtered this list by the
        /// applicable <see cref="ReferenceDataLibrary" /> chain and by <see cref="Category.PermissibleClass" /> for
        /// the parent's class kind.
        /// </summary>
        [Parameter]
        public IEnumerable<Category> AvailableCategories { get; set; } = Enumerable.Empty<Category>();

        /// <summary>
        /// Notifies the surrounding form that the parent's <see cref="ICategorizableThing.Category" /> collection
        /// has changed. Fired after each toggle of a row's selection.
        /// </summary>
        [Parameter]
        public EventCallback<ICategorizableThing> ThingChanged { get; set; }

        /// <summary>
        /// Persists the new selection back onto the parent's <see cref="ICategorizableThing.Category" /> list and
        /// fires <see cref="ThingChanged" />.
        /// </summary>
        /// <param name="items">The grid's currently selected data items.</param>
        /// <returns>A <see cref="Task" />.</returns>
        protected async Task OnSelectedCategoriesChanged(IReadOnlyList<object> items)
        {
            this.Thing.Category = items.Cast<Category>().ToList();
            await this.ThingChanged.InvokeAsync(this.Thing);
        }
    }
}
