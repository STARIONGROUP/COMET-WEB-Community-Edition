﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IAutoRefreshService.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Services.SessionManagement
{
    using COMET.Web.Common.Utilities.DisposableObject;

    /// <summary>
    /// Service to enable auto-refresh of the opened session
    /// </summary>
    public interface IAutoRefreshService : IDisposableObject
    {
        /// <summary>
        /// Enable / disable auto-refresh for the ISession
        /// </summary>
        bool IsAutoRefreshEnabled { get; set; }

        /// <summary>
        /// Define the interval in sec to auto-refresh the session
        /// Set to 60s by default
        /// </summary>
        int AutoRefreshInterval { get; set; }

        /// <summary>
        /// Define seconds left in the timer before the next refresh
        /// </summary>
        int AutoRefreshSecondsLeft { get; }

        /// <summary>
        /// Sets the timer according to the appropriate setting
        /// </summary>
        void SetTimer();
    }
}
