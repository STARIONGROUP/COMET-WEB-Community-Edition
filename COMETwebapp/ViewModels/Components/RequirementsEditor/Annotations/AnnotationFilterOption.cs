// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AnnotationFilterOption.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.RequirementsEditor.Annotations
{
    /// <summary>
    /// An entry of the Requirements Editor annotation filter: either one <see cref="RequirementAnnotationKind" />, or,
    /// when <see cref="Kind" /> is null, the requirements that still have an open annotation.
    /// </summary>
    /// <param name="Label">The text shown in the filter</param>
    /// <param name="Kind">The <see cref="RequirementAnnotationKind" /> to match, or null to match open annotations</param>
    public sealed record AnnotationFilterOption(string Label, RequirementAnnotationKind? Kind);
}
