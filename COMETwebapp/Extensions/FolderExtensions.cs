// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FolderExtensions.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Extensions
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    ///  Extension class for Folder
    /// </summary>
    public static class FolderExtensions
    {
        /// <summary>
        /// Gets the given folder path, including the folder itself
        /// </summary>
        /// <param name="folder">The folder to get the path</param>
        /// <returns>The path</returns>
        public static string GetFolderPath(this Folder folder)
        {
            var path = $"{folder.Path}/{folder.Name}/";
            return path;
        }
    }
}
