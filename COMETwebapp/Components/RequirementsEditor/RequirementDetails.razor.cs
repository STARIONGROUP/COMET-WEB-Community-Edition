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
        /// The <see cref="RelationalExpression" /> currently linked through the parameter-link picker, or null when it
        /// is closed.
        /// </summary>
        private RelationalExpression linkingExpression;

        /// <summary>
        /// The <see cref="ParameterOrOverrideBase" />s the picker offers for <see cref="linkingExpression" />.
        /// </summary>
        private List<ParameterOrOverrideBase> linkCandidates = [];

        /// <summary>
        /// The parameters staged as bound to <see cref="linkingExpression" /> until the picker is confirmed.
        /// </summary>
        private HashSet<ParameterOrOverrideBase> stagedLinks = [];

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
        /// Gets a value indicating whether the parameter-link picker is open.
        /// </summary>
        public bool IsLinkDialogOpen => this.linkingExpression != null;

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
        /// Gets the link label of a related <see cref="Requirement" />: its specification's short name and its own
        /// short name or name, matching the table-of-contents ID/Name toggle so the two correspond.
        /// </summary>
        /// <param name="requirement">The related <see cref="Requirement" /></param>
        /// <returns>The label</returns>
        private string GetRequirementLabel(Requirement requirement)
        {
            var specification = requirement.GetContainerOfType<RequirementsSpecification>();
            var requirementLabel = this.ViewModel.TreeUsesShortName ? requirement.ShortName : requirement.Name;

            if (specification == null)
            {
                return requirementLabel;
            }

            var specificationLabel = this.ViewModel.TreeUsesShortName ? specification.ShortName : specification.Name;
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

        /// <summary>
        /// Gets the name of the <see cref="ElementDefinition" /> owning the given <paramref name="parameter" />, for
        /// display in the parameter-link picker.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase" /> candidate.</param>
        /// <returns>The owning element's name</returns>
        private static string GetOwningElementName(ParameterOrOverrideBase parameter)
        {
            return parameter.GetContainerOfType<ElementDefinition>()?.Name;
        }

        /// <summary>
        /// Opens the parameter-link picker for the given relational expression, staging its currently bound parameters.
        /// </summary>
        /// <param name="relational">The <see cref="RelationalExpression" /> to link.</param>
        private void OpenLinkDialog(RelationalExpression relational)
        {
            this.linkingExpression = relational;
            this.linkCandidates = this.ViewModel.GetLinkableParameters(relational).ToList();
            this.stagedLinks = [..this.ViewModel.GetBoundParameters(relational)];
        }

        /// <summary>
        /// Gets whether the given <paramref name="parameter" /> is staged as bound in the picker.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase" /> candidate.</param>
        /// <returns>true if staged</returns>
        private bool IsStaged(ParameterOrOverrideBase parameter)
        {
            return this.stagedLinks.Contains(parameter);
        }

        /// <summary>
        /// Adds or removes the given <paramref name="parameter" /> from the staged selection.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase" /> candidate.</param>
        /// <param name="isChecked">true to stage it, false to unstage it.</param>
        private void ToggleStaged(ParameterOrOverrideBase parameter, bool isChecked)
        {
            if (isChecked)
            {
                this.stagedLinks.Add(parameter);
            }
            else
            {
                this.stagedLinks.Remove(parameter);
            }
        }

        /// <summary>
        /// Persists the staged selection as the parameter links of <see cref="linkingExpression" /> and closes the
        /// picker.
        /// </summary>
        private async Task ConfirmLinksAsync()
        {
            await this.ViewModel.UpdateParameterLinksAsync(this.linkingExpression, [..this.stagedLinks]);
            this.linkingExpression = null;
        }

        /// <summary>
        /// Closes the picker without applying the staged selection.
        /// </summary>
        private void CancelLinks()
        {
            this.linkingExpression = null;
        }

        /// <summary>
        /// Handles the picker popup being closed via its close button.
        /// </summary>
        /// <param name="visible">The new visibility of the popup.</param>
        private void OnLinkDialogVisibleChanged(bool visible)
        {
            if (!visible)
            {
                this.linkingExpression = null;
            }
        }
    }
}
