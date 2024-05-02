// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ServerConfiguration.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMET.Web.Common.Model.Configuration
{
    /// <summary>
    /// This class holds all of the configuration related properties that are stored on the local configuration files.
    /// </summary>
    public class ServerConfiguration
    {
        /// <summary>
        /// The Server Address to use
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        /// The configuration values for the Book feature
        /// </summary>
        public BookInputConfiguration BookInputConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EngineeringModelRdlFilter"/>. If not filters are specified all the available models are shown
        /// </summary>
        public EngineeringModelRdlFilter RdlFilter { get; set; }
    }
}
