// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsDocument.razor.cs" company="Starion Group S.A.">
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
    /// Renders a <see cref="RequirementsSpecification" /> as a document: the specification header, its requirements,
    /// and its <see cref="RequirementsGroup" /> hierarchy (recursively) with the requirements filed under each group.
    /// </summary>
    public partial class RequirementsDocument
    {
        /// <summary>
        /// The number of simple-parameter-value columns that still fit inline beside the definition and pills; above
        /// this the value grid drops to its own full-width line below the row. A count heuristic — it does not measure
        /// the actual panel width — tuned to 5 so the common case stays inline even with the table of contents open.
        /// The grid also wraps at 5 columns per line (see .req-row-values max-width) so it never runs off the row.
        /// </summary>
        private const int MaxInlineValueColumns = 5;

        /// <summary>
        /// Gets or sets the <see cref="IRequirementsEditorBodyViewModel" />.
        /// </summary>
        [Parameter]
        public IRequirementsEditorBodyViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RequirementsContainer" /> rendered by this instance. When <c>null</c>, the
        /// selected specification (the document root) is rendered.
        /// </summary>
        [Parameter]
        public RequirementsContainer Container { get; set; }

        /// <summary>
        /// Gets or sets the nesting level of the current group, used to size its header (0 for the specification root).
        /// </summary>
        [Parameter]
        public int Level { get; set; }

        /// <summary>
        /// Gets the HTML anchor id used to scroll to the given <paramref name="group" /> from the table of contents.
        /// </summary>
        /// <param name="group">The <see cref="RequirementsGroup" /></param>
        /// <returns>The anchor id</returns>
        public static string GroupAnchorId(RequirementsGroup group)
        {
            return $"req-group-{group.Iid}";
        }

        /// <summary>
        /// Gets the HTML anchor id used to scroll to the given <paramref name="requirement" /> from a traceability link.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>The anchor id</returns>
        public static string RequirementAnchorId(Requirement requirement)
        {
            return $"req-{requirement.Iid}";
        }

        /// <summary>
        /// The <see cref="CDP4Common.CommonData.Thing.Iid" /> of the requirement whose definition is currently being edited inline, or null.
        /// </summary>
        private Guid? editingRequirementIid;

        /// <summary>
        /// The staged definition text of the requirement currently being edited inline.
        /// </summary>
        private string editingDefinition;

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
        /// Gets the definition text of the given <paramref name="requirement" /> for display, or a placeholder when empty.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>The definition content, or a click-to-add placeholder</returns>
        private static string GetDefinitionDisplay(Requirement requirement)
        {
            var content = GetDefinition(requirement);
            return string.IsNullOrWhiteSpace(content) ? "Click to add a definition" : content;
        }

        /// <summary>
        /// Gets whether the given <paramref name="requirement" />'s definition is currently being edited inline.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>true if it is being edited</returns>
        private bool IsEditingDefinition(Requirement requirement)
        {
            return this.editingRequirementIid == requirement.Iid;
        }

        /// <summary>
        /// Gets whether the inline definition edit of the given <paramref name="requirement" /> has an unsaved change,
        /// so the editor can show the same "dirty" highlight as the simple-parameter-value editor.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>true when the staged text differs from the saved definition</returns>
        private bool IsDefinitionDirty(Requirement requirement)
        {
            return !string.Equals(this.editingDefinition ?? string.Empty, GetDefinition(requirement), StringComparison.Ordinal);
        }

        /// <summary>
        /// Starts the inline edit of the given <paramref name="requirement" />'s definition.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        private void StartDefinitionEdit(Requirement requirement)
        {
            this.editingRequirementIid = requirement.Iid;
            this.editingDefinition = GetDefinition(requirement);
        }

        /// <summary>
        /// Cancels the current inline definition edit without saving.
        /// </summary>
        private void CancelDefinitionEdit()
        {
            this.editingRequirementIid = null;
            this.editingDefinition = null;
        }

        /// <summary>
        /// Commits the current inline definition edit.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /> being edited</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task ConfirmDefinitionEdit(Requirement requirement)
        {
            var content = this.editingDefinition;
            this.editingRequirementIid = null;
            this.editingDefinition = null;
            await this.ViewModel.SaveInlineDefinitionAsync(requirement, content);
        }

        /// <summary>
        /// Cancels the inline definition edit when the Escape key is pressed.
        /// </summary>
        /// <param name="eventArgs">The <see cref="KeyboardEventArgs" /></param>
        private void OnDefinitionEditorKeyDown(KeyboardEventArgs eventArgs)
        {
            if (eventArgs.Key == "Escape")
            {
                this.CancelDefinitionEdit();
            }
        }
    }
}
