// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AnnotationKindSummary.cs" company="Starion Group S.A.">
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
    /// How many annotations of one <see cref="RequirementAnnotationKind" /> a requirement carries, and how many of them
    /// are still open. Drives the per-kind badges shown on a requirement row.
    /// </summary>
    /// <param name="Kind">The <see cref="RequirementAnnotationKind" /></param>
    /// <param name="Count">The number of annotations of that kind</param>
    /// <param name="OpenCount">The number of those annotations whose status is still open</param>
    public sealed record AnnotationKindSummary(RequirementAnnotationKind Kind, int Count, int OpenCount);
}
