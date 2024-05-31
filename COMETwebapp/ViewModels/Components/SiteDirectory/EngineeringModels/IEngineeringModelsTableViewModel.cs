// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEngineeringModelsTableViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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

namespace COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    /// <summary>
    /// View model used to manage <see cref="EngineeringModelSetup" />
    /// </summary>
    public interface IEngineeringModelsTableViewModel : IDeletableDataItemTableViewModel<EngineeringModelSetup, EngineeringModelRowViewModel>
    {
        /// <summary>
        /// Gets a collection of the available engineering models
        /// </summary>
        IEnumerable<EngineeringModelSetup> EngineeringModels { get; }

        /// <summary>
        /// Gets a collection of all the possible model kinds
        /// </summary>
        IEnumerable<EngineeringModelKind> ModelKinds { get; }

        /// <summary>
        /// Gets a collection of all the possible study phase kinds
        /// </summary>
        IEnumerable<StudyPhaseKind> StudyPhases { get; }

        /// <summary>
        /// Gets a collection of the available site reference data libraries
        /// </summary>
        IEnumerable<SiteReferenceDataLibrary> SiteRdls { get; }

        /// <summary>
        /// Gets a collection of the available domains of expertise
        /// </summary>
        IEnumerable<DomainOfExpertise> DomainsOfExpertise { get; }

        /// <summary>
        /// Gets a collection of the available organizations
        /// </summary>
        IEnumerable<Organization> Organizations { get; }

        /// <summary>
        /// Gets or sets the collection of selected organizations
        /// </summary>
        IEnumerable<Organization> SelectedOrganizations { get; set; }

        /// <summary>
        /// Gets or sets the selected model admin organization
        /// </summary>
        Organization SelectedModelAdminOrganization { get; set; }

        /// <summary>
        /// Gets or sets the selected site reference data library
        /// </summary>
        SiteReferenceDataLibrary SelectedSiteRdl { get; set; }

        /// <summary>
        /// Gets or sets the selected source <see cref="EngineeringModelSetup"/>
        /// </summary>
        EngineeringModelSetup SelectedSourceModel { get; set; }

        /// <summary>
        /// Gets a collection of the available <see cref="IterationRowViewModel"/>s
        /// </summary>
        IEnumerable<IterationRowViewModel> IterationRows { get; }

        /// <summary>
        /// Gets the <see cref="IOrganizationalParticipantsTableViewModel"/>
        /// </summary>
        IOrganizationalParticipantsTableViewModel OrganizationalParticipantsTableViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IOrganizationalParticipantsTableViewModel"/>
        /// </summary>
        IParticipantsTableViewModel ParticipantsTableViewModel { get; }

        /// <summary>
        /// Creates a new <see cref="EngineeringModelSetup"/>
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="EngineeringModelSetup"/> should be created</param>
        /// <returns>A <see cref="Task"/></returns>
        Task CreateOrEditEngineeringModel(bool shouldCreate);
    }
}
