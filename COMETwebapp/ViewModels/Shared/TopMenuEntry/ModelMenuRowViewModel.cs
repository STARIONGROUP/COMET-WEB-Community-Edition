// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelMenuRowViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Shared.TopMenuEntry
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Extensions;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Row View Model that handle an opened <see cref="Iteration" /> into the <see cref="ModelMenuViewModel" />
    /// </summary>
    public class ModelMenuRowViewModel
    {
        /// <summary>
        /// The <see cref="EventCallback{TValue}" /> for closing the <see cref="Iteration" />
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
