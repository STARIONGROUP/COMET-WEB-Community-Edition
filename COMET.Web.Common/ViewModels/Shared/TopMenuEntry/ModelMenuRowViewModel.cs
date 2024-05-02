// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelMenuRowViewModel.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.ViewModels.Shared.TopMenuEntry
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Extensions;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Row View Model that handle an opened <see cref="ModelMenuViewModel" /> into the <see cref="Iteration" />
    /// </summary>
    public class ModelMenuRowViewModel
    {
        /// <summary>
        /// The <see cref="Iteration" /> for closing the <see cref="EventCallback{TValue}" />
        /// </summary>
        private readonly EventCallback<Iteration> closeEventCallback;

        /// <summary>
        /// The opened <see cref="Iteration" />
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The <see cref="EventCallback{TValue}" /> to call when switching between <see cref="DomainOfExpertise" />
        /// </summary>
        private readonly EventCallback<Iteration> switchDomainEventCallback;

        /// <summary>
        /// Initializes a new <see cref="ModelMenuRowViewModel" />
        /// </summary>
        /// <param name="openedIteration">The opened <see cref="Iteration" /></param>
        /// <param name="closeEventCallback">
        /// The <see cref="EventCallback{TValue}" /> to call when closing the
        /// <see cref="Iteration" />
        /// </param>
        /// <param name="switchDomainEventCallback">
        /// The <see cref="EventCallback{TValue}" /> to call when switching between
        /// <see cref="DomainOfExpertise" />
        /// </param>
        public ModelMenuRowViewModel(Iteration openedIteration, EventCallback<Iteration> closeEventCallback, EventCallback<Iteration> switchDomainEventCallback)
        {
            this.iteration = openedIteration;
            this.closeEventCallback = closeEventCallback;
            this.switchDomainEventCallback = switchDomainEventCallback;
        }

        /// <summary>
        /// Gets the name of the <see cref="Iteration" />
        /// </summary>
        public string IterationName => this.iteration.GetName();

        /// <summary>
        /// Closes the current <see cref="Iteration" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public Task CloseIteration()
        {
            return this.closeEventCallback.InvokeAsync(this.iteration);
        }

        /// <summary>
        /// Switch between <see cref="DomainOfExpertise" /> for the current <see cref="Iteration" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public Task SwitchDomain()
        {
            return this.switchDomainEventCallback.InvokeAsync(this.iteration);
        }
    }
}
