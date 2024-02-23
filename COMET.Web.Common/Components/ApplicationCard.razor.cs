// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ApplicationCard.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Components
{
    using COMET.Web.Common.Model;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component to display a <see cref="Application" />
    /// </summary>
    public partial class ApplicationCard
    {
        /// <summary>
        /// The HTML id for the current component
        /// </summary>
        private string cardId;

        /// <summary>
        /// The <see cref="Application" /> to display
        /// </summary>
        [Parameter]
        public Application CurrentApplication { get; set; }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (this.CurrentApplication != null)
            {
                this.cardId = this.CurrentApplication.Name.Replace(" ", string.Empty).ToLower();
            }
        }
    }
}
