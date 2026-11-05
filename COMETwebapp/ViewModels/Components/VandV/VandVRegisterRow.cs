// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVRegisterRow.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.VandV
{
    using CDP4Common.CommonData;

    using COMETwebapp.Utilities.VandV;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    /// <summary>
    /// One row of the V&amp;V register, which is rendered as a flat list of rows indented by <see cref="Depth" />.
    /// </summary>
    /// <param name="Kind">The <see cref="VandVRegisterRowKind" />.</param>
    /// <param name="Thing">The specification, group, requirement, item or step.</param>
    /// <param name="Depth">The indentation level, 0 for a specification.</param>
    /// <param name="HasChildren">Whether the row can be expanded.</param>
    /// <param name="IsExpanded">Whether the row's children are shown.</param>
    /// <param name="Coverage">The coverage text of a specification, group or requirement; empty for an item or step.</param>
    /// <param name="RollUp">The roll-up of a requirement's items, or null for any other row.</param>
    /// <param name="Outcome">The outcome of an item, or null for any other row.</param>
    /// <param name="IsInherited">Whether a step belongs to the activity performing the item rather than to the item.</param>
    /// <param name="Fields">The field cells of an item, for the field groups shown; null for any other row.</param>
    /// <param name="Traceability">The relationships of a requirement or item while traceability is shown; otherwise null.</param>
    public sealed record VandVRegisterRow(
        VandVRegisterRowKind Kind,
        Thing Thing,
        int Depth,
        bool HasChildren,
        bool IsExpanded,
        string Coverage,
        VandVRollUp? RollUp = null,
        VandVItemOutcome? Outcome = null,
        bool IsInherited = false,
        IReadOnlyList<VandVField> Fields = null,
        IReadOnlyList<RequirementRelationshipRow> Traceability = null);
}
