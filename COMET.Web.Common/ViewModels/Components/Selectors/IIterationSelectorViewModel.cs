﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IIterationSelectorViewModel.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.Model;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model that enable the user to select one selected <see cref="Iteration" />
    /// </summary>
    public interface IIterationSelectorViewModel: IThingSelectorViewModel<Iteration>
    {
        /// <summary>
        /// The selected <see cref="IterationData" />
        /// </summary>
        IterationData SelectedIteration { get; set; }

        /// <summary>
        /// A collection of available <see cref="IterationData" />
        /// </summary>
        IEnumerable<IterationData> AvailableIterations { get; set; }

        /// <summary>
        /// Submit the selection of the <see cref="IterationSelectorViewModel.SelectedIteration" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task Submit();
    }
}
