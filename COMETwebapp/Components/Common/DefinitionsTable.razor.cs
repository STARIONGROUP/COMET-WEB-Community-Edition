// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DefinitionsTable.razor.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components;

    using COMETwebapp.Services.RowViewModelFactoryService;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Inline grid that manages the <see cref="DefinedThing.Definition" /> collection of a parent
    /// <see cref="DefinedThing" />. Stages additions, edits and removals on the in-memory parent so they are committed
    /// together with the parent when the surrounding form is saved.
    /// </summary>
    public partial class DefinitionsTable : DisposableComponent
    {
        /// <summary>
        /// The parent <see cref="DefinedThing" /> whose <see cref="DefinedThing.Definition" /> collection is being edited.
        /// </summary>
        [Parameter]
        public DefinedThing Thing { get; set; }

        /// <summary>
        /// Notifies the surrounding form that the parent <see cref="DefinedThing" />'s definition collection has changed.
        /// </summary>
        [Parameter]
        public EventCallback<DefinedThing> ThingChanged { get; set; }

        /// <summary>
        /// The <see cref="NaturalLanguage" />s the user may pick from. When empty, the language is edited as free text
        /// (backward-compatible); when provided, the language is a dropdown that excludes languages already used by the
        /// parent so a <see cref="DefinedThing" /> keeps at most one <see cref="Definition" /> per language.
        /// </summary>
        [Parameter]
        public IEnumerable<NaturalLanguage> AvailableLanguages { get; set; } = [];

        /// <summary>
        /// True when the popup edit form is creating a new <see cref="Definition" /> rather than editing an existing one.
        /// </summary>
        public bool ShouldCreate { get; private set; }

        /// <summary>
        /// The <see cref="Definition" /> currently bound to the popup edit form.
        /// </summary>
        public Definition Item { get; private set; }

        /// <summary>
        /// The <see cref="DxGrid" /> hosting the table — kept so that the toolbar buttons can drive its edit state.
        /// </summary>
        public IGrid Grid { get; private set; }

        /// <summary>
        /// Builds the row view models displayed by the grid from the parent's current <see cref="Definition" /> list.
        /// </summary>
        /// <returns>Rows for the grid, ordered by language code.</returns>
        protected List<DefinitionRowViewModel> GetRows()
        {
            return this.Thing?.Definition
                .Select(x => (DefinitionRowViewModel)RowViewModelFactory.CreateRow(x))
                .OrderBy(x => x.LanguageCode, StringComparer.InvariantCultureIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Adds a freshly-created <see cref="Definition" /> when the grid asks for the popup edit form's bound item, or
        /// hands it a clone of the selected row's underlying <see cref="Definition" /> when editing an existing one.
        /// </summary>
        /// <param name="e">The grid customize-edit-model event.</param>
        protected void CustomizeEditDefinition(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (DefinitionRowViewModel)e.DataItem;
            this.ShouldCreate = e.IsNew;

            this.Item = dataItem == null
                ? new Definition { Iid = Guid.NewGuid(), LanguageCode = "en-GB" }
                : dataItem.Thing.Clone(true);

            e.EditModel = this.Item;
        }

        /// <summary>
        /// Persists the popup edit form's <see cref="Item" /> back onto the parent's <see cref="Definition" /> collection,
        /// either appending it (create) or replacing the matching existing entry in place (edit).
        /// </summary>
        protected void OnEditItemSaving()
        {
            if (this.ShouldCreate)
            {
                this.Thing.Definition.Add(this.Item);
            }
            else
            {
                var indexToUpdate = this.Thing.Definition.FindIndex(x => x.Iid == this.Item.Iid);
                this.Thing.Definition[indexToUpdate] = this.Item;
            }

            this.ThingChanged.InvokeAsync(this.Thing);
        }

        /// <summary>
        /// Gets the <see cref="NaturalLanguage" />s selectable for the definition currently being edited: every
        /// <see cref="AvailableLanguages" /> entry except those already used by another definition of the parent (the
        /// edited definition's own language stays selectable so an existing definition can keep its language).
        /// </summary>
        /// <returns>The selectable languages.</returns>
        public IEnumerable<NaturalLanguage> GetSelectableLanguages()
        {
            var usedLanguages = this.Thing.Definition
                .Where(x => x.Iid != this.Item?.Iid)
                .Select(x => x.LanguageCode)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return this.AvailableLanguages.Where(x => !usedLanguages.Contains(x.LanguageCode));
        }

        /// <summary>
        /// Removes the underlying <see cref="Definition" /> from the parent's collection.
        /// </summary>
        /// <param name="row">The row whose <see cref="Definition" /> should be removed.</param>
        /// <returns>A <see cref="Task" />.</returns>
        protected async Task RemoveItem(DefinitionRowViewModel row)
        {
            this.Thing.Definition.Remove(row.Thing);
            await this.ThingChanged.InvokeAsync(this.Thing);
        }
    }
}
