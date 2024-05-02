// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICommonFileStoreTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.CommonFileStore
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    /// <summary>
    /// View model used to manage <see cref="CommonFileStore" />
    /// </summary>
    public interface ICommonFileStoreTableViewModel : IDeletableDataItemTableViewModel<CommonFileStore, CommonFileStoreRowViewModel>
    {
        /// <summary>
        /// Creates or edits a <see cref="CommonFileStore"/>
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="CommonFileStore"/> should be created</param>
        /// <returns>A <see cref="Task"/></returns>
        Task CreateOrEditCommonFileStore(bool shouldCreate);

        /// <summary>
        /// Sets the <see cref="CommonFileStoreTableViewModel.CurrentIteration"/> value
        /// </summary>
        /// <param name="iteration">The iteration to be set</param>
        void SetCurrentIteration(Iteration iteration);

        /// <summary>
        /// Gets a collection of all the available <see cref="DomainOfExpertise"/>
        /// </summary>
        IEnumerable<DomainOfExpertise> DomainsOfExpertise { get; }

        /// <summary>
        /// Gets or sets the value to verify if the <see cref="CommonFileStore"/> to create is private
        /// </summary>
        bool IsPrivate { get; set; }

        /// <summary>
        /// Gets the <see cref="IFolderFileStructureViewModel"/>
        /// </summary>
        IFolderFileStructureViewModel FolderFileStructureViewModel { get; }

        /// <summary>
        /// Loads the file structure handled by the <see cref="CommonFileStoreTableViewModel.FolderFileStructureViewModel"/>
        /// </summary>
        void LoadFileStructure();
    }
}
