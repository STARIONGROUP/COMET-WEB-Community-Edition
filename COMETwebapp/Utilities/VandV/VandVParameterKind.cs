// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVParameterKind.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Utilities.VandV
{
    /// <summary>
    /// The kind of a V&amp;V <see cref="CDP4Common.SiteDirectoryData.ParameterType" />. Restricted to kinds that need no
    /// measurement scale, so the reference data carries no unit dependency.
    /// </summary>
    public enum VandVParameterKind
    {
        /// <summary>
        /// A <see cref="CDP4Common.SiteDirectoryData.TextParameterType" />.
        /// </summary>
        Text,

        /// <summary>
        /// A <see cref="CDP4Common.SiteDirectoryData.DateParameterType" />.
        /// </summary>
        Date,

        /// <summary>
        /// A <see cref="CDP4Common.SiteDirectoryData.BooleanParameterType" />.
        /// </summary>
        Boolean,

        /// <summary>
        /// An <see cref="CDP4Common.SiteDirectoryData.EnumerationParameterType" />.
        /// </summary>
        Enumeration
    }
}
