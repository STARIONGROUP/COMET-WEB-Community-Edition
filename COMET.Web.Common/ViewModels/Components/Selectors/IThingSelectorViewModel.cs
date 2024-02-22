// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IThingSelectorViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.ViewModels.Components.Selectors
{
    using CDP4Common.CommonData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model that provide selection capabilities for a <typeparamref name="TThing"/>
    /// </summary>
    /// <typeparam name="TThing">Any <see cref="Thing"/></typeparam>
    public interface IThingSelectorViewModel<TThing> where TThing: Thing
    {
        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="availableThings">A collection of available <typeparamref name="TThing"/></param>
        void UpdateProperties(IEnumerable<TThing> availableThings);

        /// <summary>
        /// <see cref="EventCallback{TValue}" /> to call when the <typeparamref name="TThing"/> has been selected
        /// </summary>
        EventCallback<TThing> OnSubmit { get; set; }
    }
}
