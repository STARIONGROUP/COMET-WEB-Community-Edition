// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="INamingConventionsService.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMET.Web.Common.Services.NamingConventionService
{
    /// <summary>
    /// The <see cref="INamingConventionService"/> provides static information based on defined naming convention, like for names of <see cref="Category"/> to use for example
    /// </summary>
    public interface INamingConventionService<TEnum> where TEnum : Enum
    {
        /// <summary>
        /// Initializes this service
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task InitializeService();

        /// <summary>
        /// Gets the value for naming convention
        /// </summary>
        /// <param name="namingConventionKey">The naming convention key</param>
        /// <returns>The defined naming convention, if exists</returns>
        string GetNamingConventionValue(string namingConventionKey);

        /// <summary>
        /// Gets the value for naming convention
        /// </summary>
        /// <param name="namingConventionKind">The <see cref="NamingConventionKind" /></param>
        /// <returns>The defined naming convention, if exists</returns>
        string GetNamingConventionValue(TEnum namingConventionKind);
    }
}
