// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IActiveDomainsTableViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.BaseDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    /// <summary>
    /// View model used to manage active <see cref="DomainOfExpertise" /> related to an engineering model
    /// </summary>
    public interface IActiveDomainsTableViewModel : IBaseDataItemTableViewModel<DomainOfExpertise, DomainOfExpertiseRowViewModel>
    {
        /// <summary>
        /// Filters the current Rows, keeping only the Domains of Expertise associated with the given engineering model
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModelSetup"/> to get its participants</param>
        void SetEngineeringModel(EngineeringModelSetup model);

        /// <summary>
        /// Gets a collection of all the available <see cref="DomainOfExpertise"/>s
        /// </summary>
        IEnumerable<DomainOfExpertise> DomainsOfExpertise { get; }

        /// <summary>
        /// Gets or sets a collection of all the selected active <see cref="DomainOfExpertise"/>s for the engineering model
        /// </summary>
        IEnumerable<DomainOfExpertise> SelectedDomainsOfExpertise { get; set; }

        /// <summary>
        /// Edit the active domains related with the <see cref="ActiveDomainsTableViewModel.CurrentModel"/>, using the <see cref="ActiveDomainsTableViewModel.SelectedDomainsOfExpertise"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        Task EditActiveDomains();

        /// <summary>
        /// Resets the <see cref="ActiveDomainsTableViewModel.SelectedDomainsOfExpertise"/> value based on the <see cref="ActiveDomainsTableViewModel.CurrentModel"/>
        /// </summary>
        void ResetSelectedDomainsOfExpertise();
    }
}
