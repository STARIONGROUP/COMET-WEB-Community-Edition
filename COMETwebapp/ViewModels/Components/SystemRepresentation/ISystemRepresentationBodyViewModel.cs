// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ISystemRepresentationBodyViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.ViewModels.Components.SystemRepresentation
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Model;
    using COMETwebapp.ViewModels.Components.Shared;

    /// <summary>
    /// View Model that handle the logic for the System Representation application
    /// </summary>
    public interface ISystemRepresentationBodyViewModel: ISingleIterationApplicationBaseViewModel
    {
        /// <summary>
        /// The selected option
        /// </summary>
        Option OptionSelected { get; set; }

        /// <summary>
        /// Name of the selected domain
        /// </summary>
        DomainOfExpertise DomainSelected { get; set; }

        /// <summary>
        /// List of the names of available <see cref="Option" />
        /// </summary>
        List<string> Options { get; set; }

        /// <summary>
        /// List of the names of available <see cref="DomainOfExpertise" />
        /// </summary>
        List<string> Domains { get; set; }

        /// <summary>
        /// Gets or sets the total of domains in this <see cref="Iteration" />
        /// </summary>
        List<DomainOfExpertise> TotalDomains { get; }

        /// <summary>
        /// Represents the RootNode of the tree
        /// </summary>
        SystemNode RootNode { get; set; }

        /// <summary>
        /// The <see cref="ISystemTreeViewModel" />
        /// </summary>
        ISystemTreeViewModel SystemTreeViewModel { get; }

        /// <summary>
        /// The <see cref="IElementDefinitionDetailsViewModel" />
        /// </summary>
        IElementDefinitionDetailsViewModel ElementDefinitionDetailsViewModel { get; }

        /// <summary>
        /// All <see cref="ElementBase" /> of the iteration
        /// </summary>
        List<ElementBase> Elements { get; set; }

        /// <summary>
        /// Updates Elements list when a filter for option is selected
        /// </summary>
        /// <param name="option">Name of the selected Option</param>
        void OnOptionFilterChange(string option);

        /// <summary>
        /// Updates Elements list when a filter for domain is selected
        /// </summary>
        /// <param name="domain">Name of the selected Domain</param>
        void OnDomainFilterChange(string domain);
    }
}
