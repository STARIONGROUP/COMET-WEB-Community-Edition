// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Application.cs" company="Starion Group S.A.">
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
        /// The navigation url for the current application
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Value asserting that the current <see cref="Application"/> is currently disabled
        /// </summary>
        public bool IsDisabled { get; set; }
    }
}
