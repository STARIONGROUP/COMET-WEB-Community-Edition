// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ICacheService.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
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

namespace COMET.Web.Common.Services.Cache;

using COMET.Web.Common.Enumerations;

/// <summary>
/// Defines the properties and methods of a class that implements the <see cref="ICacheService"/> interface
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Tries to get a stored value in the BrowserSessionSettings Dictionary
    /// </summary>
    /// <param name="browserSessionSettingKeyKey">The key of the BrowserSessionSetting</param>
    /// <param name="browserSessionSettingvalue">The value linked to the BrowserSessionSetting key in <see cref="CacheService.browserSessionSettings"/></param>
    /// <returns>true if the setting was found</returns>
    bool TryGetBrowserSessionSetting(BrowserSessionSettingKey browserSessionSettingKeyKey, out object browserSessionSettingvalue);

    /// <summary>
    /// Tries to get a stored value in the BrowserSessionSettings Dictionary and adds a new value if not present yet
    /// </summary>
    /// <param name="browserSessionSettingKeyKey">The key of the BrowserSessionSetting</param>
    /// <param name="defaultBrowserSessionSettingValue">The default value to return if the key of the BrowserSessionSetting value that will be returned and stored if the BrowserSessionSetting key was not present in <see cref="CacheService.browserSessionSettings"/> yet</param>
    /// <param name="browserSessionSettingvalue">The value linked to the BrowserSessionSetting key in <see cref="CacheService.browserSessionSettings"/></param>
    /// <returns>true if the setting was found</returns>
    bool TryGetOrAddBrowserSessionSetting(BrowserSessionSettingKey browserSessionSettingKeyKey, object defaultBrowserSessionSettingValue, out object browserSessionSettingvalue);

    /// <summary>
    /// Tries to store a value in the BrowserSessionSettings Dictionary. Overwrites the value if it already exists
    /// </summary>
    /// <param name="browserSessionSettingKeyKey">The key of the BrowserSessionSetting</param>
    /// <param name="browserSessionSettingValue">The value to store in the BrowserSessionSetting dictionary for a specific key</param>
    /// <returns>true if the setting was added</returns>
    void AddOrUpdateBrowserSessionSetting(BrowserSessionSettingKey browserSessionSettingKeyKey, object browserSessionSettingValue);
}
