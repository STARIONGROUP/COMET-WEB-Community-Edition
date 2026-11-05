// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementTraceability.razor.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Renders the relationships a <see cref="Requirement" /> participates in, shared by the Requirements document and the
    /// V&amp;V register.
    /// </summary>
    public partial class RequirementTraceability
    {
        /// <summary>
        /// Gets or sets the relationship rows to render.
        /// </summary>
        [Parameter]
        public IReadOnlyList<RequirementRelationshipRow> Rows { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether links are labelled by short name (true) or name (false).
        /// </summary>
        [Parameter]
        public bool UsesShortName { get; set; }

        /// <summary>
        /// Gets or sets the callback invoked when the link of a related requirement is clicked.
        /// </summary>
        [Parameter]
        public EventCallback<Requirement> OnNavigate { get; set; }

        /// <summary>
        /// Gets the arrow symbol for the given <paramref name="direction" />.
        /// </summary>
        /// <param name="direction">The <see cref="RelationshipDirection" /></param>
        /// <returns>The symbol</returns>
        private static string GetDirectionSymbol(RelationshipDirection direction)
        {
            return direction switch
            {
                RelationshipDirection.Outgoing => "→",
                RelationshipDirection.Incoming => "←",
                _ => "↔"
            };
        }

        /// <summary>
        /// Gets the tooltip for the given <paramref name="direction" />.
        /// </summary>
        /// <param name="direction">The <see cref="RelationshipDirection" /></param>
        /// <returns>The tooltip text</returns>
        private static string GetDirectionTitle(RelationshipDirection direction)
        {
            return direction switch
            {
                RelationshipDirection.Outgoing => "Outgoing relationship",
                RelationshipDirection.Incoming => "Incoming relationship",
                _ => "Multi relationship"
            };
        }

        /// <summary>
        /// Gets the link label of a related <see cref="Requirement" />: its specification's short name and its own
        /// short name or name, matching the ID/Name toggle so the two correspond.
        /// </summary>
        /// <param name="requirement">The related <see cref="Requirement" /></param>
        /// <returns>The label</returns>
        private string GetRequirementLabel(Requirement requirement)
        {
            var specification = requirement.GetContainerOfType<RequirementsSpecification>();
            var requirementLabel = this.UsesShortName ? requirement.ShortName : requirement.Name;

            if (specification == null)
            {
                return requirementLabel;
            }

            var specificationLabel = this.UsesShortName ? specification.ShortName : specification.Name;
            return $"{specificationLabel} · {requirementLabel}";
        }

        /// <summary>
        /// Gets the first definition text of a related <see cref="Requirement" />, shown after its link in the
        /// remaining space (truncated) so a traceability row hints at what it points to.
        /// </summary>
        /// <param name="requirement">The related <see cref="Requirement" /></param>
        /// <returns>The definition content, or an empty string</returns>
        private static string GetRequirementDefinitionText(Requirement requirement)
        {
            return requirement?.Definition.FirstOrDefault()?.Content ?? string.Empty;
        }

        /// <summary>
        /// Gets the display label of a related <see cref="Thing" /> that is not a <see cref="Requirement" />;
        /// element definitions and usages include their model code.
        /// </summary>
        /// <param name="thing">The related <see cref="Thing" /></param>
        /// <returns>The label</returns>
        private static string GetThingLabel(Thing thing)
        {
            return thing switch
            {
                ElementDefinition elementDefinition => $"{elementDefinition.Name} ({elementDefinition.ModelCode()})",
                ElementUsage elementUsage => $"{elementUsage.Name} ({elementUsage.ModelCode()})",
                DefinedThing definedThing => $"{definedThing.ShortName} – {definedThing.Name}",
                _ => thing.UserFriendlyName
            };
        }
    }
}
