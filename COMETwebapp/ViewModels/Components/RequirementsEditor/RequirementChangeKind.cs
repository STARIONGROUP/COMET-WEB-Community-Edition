// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementChangeKind.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.RequirementsEditor
{
    /// <summary>
    /// Enumerates the kinds of change that the requirements changelog can report for a single element
    /// between two iterations.
    /// </summary>
    public enum RequirementChangeKind
    {
        /// <summary>
        /// The element exists in the newer iteration but not in the baseline iteration.
        /// </summary>
        Created,

        /// <summary>
        /// The element exists in the baseline iteration but not in the newer iteration.
        /// </summary>
        Deleted,

        /// <summary>
        /// The element exists in both iterations but one or more of its values changed.
        /// </summary>
        Modified,

        /// <summary>
        /// The element was made deprecated between the baseline and the newer iteration.
        /// </summary>
        Deprecated,

        /// <summary>
        /// The element was restored from a deprecated state between the baseline and the newer iteration.
        /// </summary>
        Restored
    }
}
