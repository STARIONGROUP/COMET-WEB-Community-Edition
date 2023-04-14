// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IterationSelectorViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Model;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model that enable the user to select one selected <see cref="Iteration" />
    /// </summary>
    public class IterationSelectorViewModel : IIterationSelectorViewModel
    {
        /// <summary>
        /// The collection of available <see cref="Iteration" />
        /// </summary>
        private IEnumerable<Iteration> iterations;

        /// <summary>
        /// The selected <see cref="IterationData" />
        /// </summary>
        public IterationData SelectedIteration { get; set; }

        /// <summary>
        /// A collection of available <see cref="IterationData" />
        /// </summary>
        public IEnumerable<IterationData> AvailableIterations { get; set; }

        /// <summary>
        /// <see cref="EventCallback{TValue}" /> to call when the <see cref="Iteration" /> has been selected
        /// </summary>
        public EventCallback<Iteration> OnSubmit { get; set; }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="availableIterations">A collection of available <see cref="Iteration" /></param>
        public void UpdateProperties(IEnumerable<Iteration> availableIterations)
        {
            this.iterations = availableIterations;
            this.AvailableIterations = this.iterations.Select(x => new IterationData(x.IterationSetup, true));
            this.SelectedIteration = null;
        }

        /// <summary>
        /// Submit the selection of the <see cref="SelectedIteration" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public Task Submit()
        {
            var iteration = this.iterations.First(x => x.IterationSetup.Iid == this.SelectedIteration.IterationSetupId);
            return this.OnSubmit.InvokeAsync(iteration);
        }
    }
}
