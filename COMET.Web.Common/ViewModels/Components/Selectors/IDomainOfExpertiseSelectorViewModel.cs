﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IDomainOfExpertiseSelectorViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMET.Web.Common.ViewModels.Components.Selectors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View Model that enables the user to select an <see cref="DomainOfExpertise" />
    /// </summary>
    public interface IDomainOfExpertiseSelectorViewModel : IBelongsToIterationSelectorViewModel
    {
        /// <summary>
        /// A collection of available <see cref="DomainOfExpertise" />
        /// </summary>
        IEnumerable<DomainOfExpertise> AvailableDomainsOfExpertise { get; set; }

        /// <summary>
        /// The currently selected <see cref="DomainOfExpertise" />
        /// </summary>
        DomainOfExpertise SelectedDomainOfExpertise { get; set; }

        /// <summary>
        /// Gets the <see cref="ISessionService" />
        /// </summary>
        ISessionService SessionService { get; }

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise" /> from the current <see cref="Iteration"/>
        /// </summary>
        DomainOfExpertise CurrentIterationDomain { get; set; }

        /// <summary>
        /// Gets or sets the callback that is executed when the <see cref="SelectedDomainOfExpertise"/> property has changed
        /// </summary>
        EventCallback<DomainOfExpertise> OnSelectedDomainOfExpertiseChange { get; set; }

        /// <summary>
        /// Sets the <see cref="SelectedDomainOfExpertise"/> property or resets its value
        /// </summary>
        /// <param name="reset">The condition to check if the value should be reset</param>
        /// <param name="domainOfExpertise">The <see cref="DomainOfExpertise"/> to be set</param>
        /// <returns>A <see cref="Task"/></returns>
        Task SetSelectedDomainOfExpertiseOrReset(bool reset, DomainOfExpertise domainOfExpertise = null);
    }
}