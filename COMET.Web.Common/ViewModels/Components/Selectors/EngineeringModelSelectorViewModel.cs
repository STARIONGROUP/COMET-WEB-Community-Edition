// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EngineeringModelSelectorViewModel.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.ViewModels.Components.Selectors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model that enable the user to select one selected <see cref="EngineeringModelSetup" />
    /// </summary>
    public class EngineeringModelSelectorViewModel : IEngineeringModelSelectorViewModel
    {
        /// <summary>
        /// A collection of available <see cref="EngineeringModel"/>
        /// </summary>
        private IEnumerable<EngineeringModel> engineeringModels;

        /// <summary>
        /// The selected <see cref="EngineeringModelSetup" />
        /// </summary>
        public EngineeringModelSetup SelectedEngineeringModelSetup { get; set; }

        /// <summary>
        /// A collection of available <see cref="EngineeringModelSetup" />
        /// </summary>
        public IEnumerable<EngineeringModelSetup> AvailableEngineeringModelSetups { get; set; }

        /// <summary>
        /// <see cref="EventCallback{TValue}" /> to call when the <see cref="EngineeringModel" /> has been selected
        /// </summary>
        public EventCallback<EngineeringModel> OnSubmit { get; set; }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="availableEngineeringModels">A collection of available <see cref="EngineeringModel" /></param>
        public void UpdateProperties(IEnumerable<EngineeringModel> availableEngineeringModels)
        {
            this.engineeringModels = availableEngineeringModels;
            this.AvailableEngineeringModelSetups = this.engineeringModels.Select(x => x.EngineeringModelSetup);
            this.SelectedEngineeringModelSetup = null;
        }

        /// <summary>
        /// Submit the selection of the <see cref="SelectedEngineeringModelSetup" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public Task Submit()
        {
            return this.OnSubmit.InvokeAsync(this.engineeringModels.First(x => x.Iid == this.SelectedEngineeringModelSetup.EngineeringModelIid));
        }
    }
}
