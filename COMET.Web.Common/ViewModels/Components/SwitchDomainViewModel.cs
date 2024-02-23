// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SwitchDomainViewModel.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.ViewModels.Components
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model that enable the user to select a different <see cref="DomainOfExpertise" /> for an <see cref="Iteration" />
    /// </summary>
    public class SwitchDomainViewModel : ISwitchDomainViewModel
    {
        /// <summary>
        /// A collection of available <see cref="DomainOfExpertise" />
        /// </summary>
        public IEnumerable<DomainOfExpertise> AvailableDomains { get; set; } = new List<DomainOfExpertise>();

        /// <summary>
        /// The currently selected <see cref="DomainOfExpertise" />
        /// </summary>
        public DomainOfExpertise SelectedDomainOfExpertise { get; set; }

        /// <summary>
        /// The <see cref="EventCallback{TValue}" /> to call when submitting the switch of <see cref="DomainOfExpertise" />
        /// </summary>
        public EventCallback<DomainOfExpertise> OnSubmit { get; set; }
    }
}
