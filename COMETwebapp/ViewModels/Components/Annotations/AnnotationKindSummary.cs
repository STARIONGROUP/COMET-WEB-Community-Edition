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

namespace COMETwebapp.ViewModels.Components.Annotations
{
    /// <summary>
    /// How many annotations of one <see cref="AnnotationKind" /> a thing carries, and how many of them still need
    /// attention. Drives the per-kind badges shown next to an annotated thing.
    /// </summary>
    /// <param name="Kind">The <see cref="AnnotationKind" /></param>
    /// <param name="Count">The number of annotations of that kind</param>
    /// <param name="AttentionCount">
    /// The number of those annotations that still need attention: open themselves, or a change request with a change
    /// proposal that is still open
    /// </param>
    public sealed record AnnotationKindSummary(AnnotationKind Kind, int Count, int AttentionCount);
}
