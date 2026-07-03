// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementRowDisplayMode.cs" company="Starion Group S.A.">
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
    /// Defines how a requirement is rendered as a row in the document panel of the Requirements Editor.
    /// In every mode the definition text keeps the visual focus.
    /// </summary>
    public enum RequirementRowDisplayMode
    {
        /// <summary>
        /// The short name and the definition are shown on a single line (<c>ShortName - Definition</c>).
        /// </summary>
        ShortNameAndDefinition,

        /// <summary>
        /// The name and the definition are shown on a single line (<c>Name - Definition</c>).
        /// </summary>
        NameAndDefinition,

        /// <summary>
        /// The short name and name are shown on a header line, with the definition on the next line.
        /// </summary>
        ShortNameNameAndDefinition
    }
}
