// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BrowserSessionSettings.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Enumerations
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// En enumeration of possible keys to be used to store and retrieve cached BrowserSessionSettings
    /// </summary>
    public enum BrowserSessionSettingKey
    {
        /// <summary>
        /// Key to handle the last selected <see cref="EngineeringModel"/>
        /// </summary>
        LastUsedEngineeringModel,

        /// <summary>
        /// Key to handle the last selected <see cref="IterationSetup"/>
        /// </summary>
        LastUsedIterationData,

        /// <summary>
        /// Key to handle the last selected <see cref="DomainOfExpertise"/>
        /// </summary>
        LastUsedDomainOfExpertise
    }
}
