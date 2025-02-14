// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AuthenticationDto.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Model.DTO
{
    /// <summary>
    /// Authentication information to connect to an E-TM-10-25 data source
    /// </summary>
    public class AuthenticationDto
    {
        /// <summary>
        /// Gets or sets the address of the datasource to connect to
        /// </summary>
        public string SourceAddress { get; set; }

        /// <summary>
        /// Gets or sets the username to authenticate with
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password to authenticate with
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if full trust is active. This option allows connecting to servers with self signed certificates
        /// </summary>
        public bool FullTrust { get; set; } = false;
        
        /// <summary>
        /// Asserts that credentials should be validated or no 
        /// </summary>
        public bool ShouldValidateCredentials { get; set; } = true;
    }
}
