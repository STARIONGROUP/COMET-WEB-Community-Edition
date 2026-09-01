// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DefinitionsTable.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.Common
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.RowViewModelFactoryService;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Inline grid that manages the <see cref="DefinedThing.Definition" /> collection of a parent
    /// <see cref="DefinedThing" />. Stages additions, edits and removals on the in-memory parent so they are committed
    /// together with the parent when the surrounding form is saved.
    /// </summary>
    public partial class DefinitionsTable : DisposableComponent
    {
        /// <summary>
        /// The injected <see cref="ISessionService" />, used to assert whether the open session allows writing
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets a value indicating whether the open session forbids any modification, which is the case for a session
        /// opened from an ECSS-E-TM-10-25 Annex C3 archive. Create and delete controls bind their enabled state to
        /// the inverse of this, so the data can still be inspected but never modified
        /// </summary>
        public bool IsReadOnly => this.SessionService.IsReadOnly;

        /// <summary>
        /// The parent <see cref="DefinedThing" /> whose <see cref="DefinedThing.Definition" /> collection is being edited.
        /// </summary>
        [Parameter]
        public DefinedThing Thing { get; set; }

        /// <summary>
        /// The popup that asks the user to confirm the removal of a <see cref="Definition" /> before it is applied.
        /// </summary>
        public ConfirmRemovalPopup<DefinitionRowViewModel> RemovalPopup { get; private set; }

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
        /// Gets or sets a value indicating whether the form popup is visible for editing or creating.
        /// </summary>
        public bool IsOnEditMode { get; set; }

        /// <summary>
        /// The <see cref="Definition" /> currently bound to the popup edit form.
        /// </summary>
        public Definition Item { get; private set; }

        /// <summary>
        /// Starts the creation flow for a new <see cref="Definition" /> item.
        /// </summary>
        public void StartCreate()
        {
            this.ShouldCreate = true;
            this.Item = new Definition { Iid = Guid.NewGuid(), LanguageCode = "en-GB" };
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Starts the edit flow for the specified <paramref name="row" />.
        /// </summary>
        /// <param name="row">The selected row to edit.</param>
        public void StartEdit(DefinitionRowViewModel row)
        {
            if (row == null)
            {
                return;
            }

            this.ShouldCreate = false;
            this.Item = row.Thing.Clone(true);
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Persists the popup edit form's <see cref="Item" /> back onto the parent's <see cref="Definition" /> collection,
        /// either appending it (create) or replacing the matching existing entry in place (edit).
        /// </summary>
        public void OnSaved()
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

            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Handles cancellation of the form popup.
        /// </summary>
        public void OnCanceled()
        {
            this.IsOnEditMode = false;
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
        /// Builds the row view models displayed by the grid from the parent's current <see cref="Definition" /> list.
        /// </summary>
        /// <returns>Rows for the grid, ordered by language code.</returns>
        protected List<DefinitionRowViewModel> GetRows()
        {
            var rows = this.Thing?.Definition
                .Select(x => (DefinitionRowViewModel)RowViewModelFactory.CreateRow(x))
                .OrderBy(x => x.LanguageCode, StringComparer.InvariantCultureIgnoreCase)
                .ToList();

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    row.IsAllowedToWrite = this.SessionService.Session.PermissionService.CanWrite(row.Thing);
                }
            }

            return rows;
        }

        /// <summary>
        /// Removes the underlying <see cref="Definition" /> from the parent's collection, once the user has confirmed the
        /// removal in the <see cref="RemovalPopup" />.
        /// </summary>
        /// <param name="row">The row whose <see cref="Definition" /> should be removed.</param>
        protected void RemoveItem(DefinitionRowViewModel row)
        {
            this.Thing.Definition.Remove(row.Thing);
        }
    }
}
