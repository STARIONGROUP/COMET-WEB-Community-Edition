// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsExportPayload.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Services.RequirementsEditor
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Model.RequirementsEditor.Export;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    /// <summary>
    /// Immutable snapshot of everything a requirements exporter needs to build its output, decoupling the exporter from
    /// the view model. The graph-walking that resolves a constraint's text and a requirement's relationships stays in
    /// the view model and is passed in as delegates, so exporters only own presentation and the
    /// <see cref="RequirementsExportConfiguration" />.
    /// </summary>
    /// <param name="Specifications">The <see cref="RequirementsSpecification" />s to export, in the order to write them</param>
    /// <param name="Configuration">The <see cref="RequirementsExportConfiguration" /> driving what is written</param>
    /// <param name="GetRelationshipDetails">
    /// Resolves the <see cref="RequirementRelationshipDetail" />s a <see cref="Requirement" /> participates in, one per
    /// matched rule, so the exporter can lay out a column per rule and direction
    /// </param>
    /// <param name="GetConstraintText">
    /// Renders a <see cref="ParametricConstraint" /> as text, already honouring
    /// <see cref="RequirementsExportConfiguration.IncludeConstraintLinkedElementAndValue" />
    /// </param>
    public record RequirementsExportPayload(
        IReadOnlyList<RequirementsSpecification> Specifications,
        RequirementsExportConfiguration Configuration,
        Func<Requirement, IReadOnlyList<RequirementRelationshipDetail>> GetRelationshipDetails,
        Func<ParametricConstraint, string> GetConstraintText);
}
