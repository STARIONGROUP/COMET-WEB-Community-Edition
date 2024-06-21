// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Application.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMET.Web.Common.Model
{
    /// <summary>
    /// Define application information
    /// </summary>
    public class Application
    {
        /// <summary>
        /// Name of the application
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A little description of the application
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The color of the icon to represent the application
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Icon in the card to represent the application
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The icon type to be displayed as a dynamic component, instead of a css class
        /// </summary>
        /// <remarks>Setting this value will likely override the selected <see cref="Icon" /> property when displayed</remarks>
        public Type IconType { get; set; }

        /// <summary>
        /// The navigation url for the current application
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Value asserting that the current <see cref="Application" /> is currently disabled
        /// </summary>
        public bool IsDisabled { get; set; }
    }
}
