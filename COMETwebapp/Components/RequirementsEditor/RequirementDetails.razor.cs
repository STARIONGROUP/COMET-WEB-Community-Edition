// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementDetails.razor.cs" company="Starion Group S.A.">
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
    /// Renders the block detail sections of a <see cref="Requirement" /> in the document: its
    /// <see cref="ParametricConstraint" /> expression trees and the relationships it participates in, side by side,
    /// driven by the "Constraints" and "Traceability" toolbar toggles. The <see cref="SimpleParameterValue" /> columns
    /// are rendered separately on the requirement row by <see cref="RequirementValueCell" />.
    /// </summary>
    public partial class RequirementDetails
    {
        /// <summary>
        /// Gets or sets the <see cref="IRequirementsEditorBodyViewModel" />.
        /// </summary>
        [Parameter]
        public IRequirementsEditorBodyViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Requirement" /> whose details are rendered.
        /// </summary>
        [Parameter]
        public Requirement Requirement { get; set; }

        /// <summary>
        /// Gets the operator label of the given non-leaf <paramref name="expression" />.
        /// </summary>
        /// <param name="expression">The <see cref="BooleanExpression" /></param>
        /// <returns>The label</returns>
        private static string GetExpressionLabel(BooleanExpression expression)
        {
            return expression switch
            {
                AndExpression => "AND",
                OrExpression => "OR",
                ExclusiveOrExpression => "XOR",
                NotExpression => "NOT",
                _ => expression.ClassKind.ToString()
            };
        }

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
        /// Gets the link label of a related <see cref="Requirement" />: its specification's short name and its own.
        /// </summary>
        /// <param name="requirement">The related <see cref="Requirement" /></param>
        /// <returns>The label</returns>
        private static string GetRequirementLabel(Requirement requirement)
        {
            var specification = requirement.GetContainerOfType<RequirementsSpecification>();
            return specification == null ? requirement.ShortName : $"{specification.ShortName} · {requirement.ShortName}";
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
