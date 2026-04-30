// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDomainsOfExpertiseTableViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.SiteDirectory.DomainsOfExpertise
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    /// <summary>
    /// View model used to manage <see cref="DomainOfExpertise" />
    /// </summary>
    public interface IDomainsOfExpertiseTableViewModel : IDeprecatableDataItemTableViewModel<DomainOfExpertise, DomainOfExpertiseRowViewModel>
    {
        /// <summary>
        /// Gets the <see cref="Category" />s eligible for the current <see cref="DomainOfExpertise" />, drawn from
        /// the chain of <see cref="ReferenceDataLibrary" />s available in the <see cref="SiteDirectory" /> and
        /// filtered by <see cref="Category.PermissibleClass" />.
        /// </summary>
        IEnumerable<Category> Categories { get; }

        /// <summary>
        /// Creates or edits a <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="DomainOfExpertise"/> should be created</param>
        /// <returns>A <see cref="Task"/></returns>
        Task CreateOrEditDomainOfExpertise(bool shouldCreate);
    }
}
