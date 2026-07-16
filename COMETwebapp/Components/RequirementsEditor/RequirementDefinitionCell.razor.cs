// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementDefinitionCell.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.RequirementsEditor
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;

    /// <summary>
    /// The definition of a <see cref="Requirement" />: a click-to-edit display span that turns into a compact inline
    /// editor. Each instance owns its own edit state, so only one cell is ever in edit mode at a time.
    /// </summary>
    public partial class RequirementDefinitionCell
    {
        /// <summary>
        /// The staged definition text while editing, or <c>null</c> when this cell is not being edited.
        /// </summary>
        private string editingDefinition;

        /// <summary>
        /// Gets or sets the <see cref="IRequirementsEditorBodyViewModel" />.
        /// </summary>
        [Parameter]
        public IRequirementsEditorBodyViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Requirement" /> whose definition is rendered.
        /// </summary>
        [Parameter]
        public Requirement Requirement { get; set; }

        /// <summary>
        /// Gets a value indicating whether this cell is currently in edit mode.
        /// </summary>
        private bool IsEditing => this.editingDefinition != null;

        /// <summary>
        /// Gets whether the current inline edit has an unsaved change, so the editor can show the same "dirty"
        /// highlight as the simple-parameter-value editor.
        /// </summary>
        private bool IsDirty => !string.Equals(this.editingDefinition ?? string.Empty, GetDefinition(this.Requirement), StringComparison.Ordinal);

        /// <summary>
        /// Gets the definition text of the given <paramref name="requirement" /> (its first definition).
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>The first definition's content, or an empty string</returns>
        private static string GetDefinition(Requirement requirement)
        {
            return requirement.Definition.FirstOrDefault()?.Content ?? string.Empty;
        }

        /// <summary>
        /// Gets the definition text of <see cref="Requirement" /> for display, or a placeholder when empty.
        /// </summary>
        /// <returns>The definition content, or a click-to-add placeholder</returns>
        private string GetDefinitionDisplay()
        {
            var content = GetDefinition(this.Requirement);
            return string.IsNullOrWhiteSpace(content) ? "Click to add a definition" : content;
        }

        /// <summary>
        /// Starts the inline edit of this cell's definition.
        /// </summary>
        private void StartEdit()
        {
            this.editingDefinition = GetDefinition(this.Requirement);
        }

        /// <summary>
        /// Cancels the current inline definition edit without saving.
        /// </summary>
        private void Cancel()
        {
            this.editingDefinition = null;
        }

        /// <summary>
        /// Commits the current inline definition edit.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task ConfirmAsync()
        {
            var content = this.editingDefinition;
            this.editingDefinition = null;
            await this.ViewModel.SaveInlineDefinitionAsync(this.Requirement, content);
        }

        /// <summary>
        /// Cancels the inline definition edit when the Escape key is pressed.
        /// </summary>
        /// <param name="eventArgs">The <see cref="KeyboardEventArgs" /></param>
        private void OnKeyDown(KeyboardEventArgs eventArgs)
        {
            if (eventArgs.Key == "Escape")
            {
                this.Cancel();
            }
        }
    }
}
