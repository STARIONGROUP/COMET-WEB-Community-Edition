// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IDomainFileStoreTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.DomainFileStore
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    /// <summary>
    /// View model used to manage <see cref="DomainFileStore" />
    /// </summary>
    public interface IDomainFileStoreTableViewModel : IDeletableDataItemTableViewModel<DomainFileStore, DomainFileStoreRowViewModel>
    {
        /// <summary>
        /// Gets or sets the value to verify if the <see cref="DomainFileStore" /> to create is private
        /// </summary>
        bool IsPrivate { get; set; }

        /// <summary>
        /// Gets the <see cref="IFolderFileStructureViewModel" />
        /// </summary>
        IFolderFileStructureViewModel FolderFileStructureViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IDomainOfExpertiseSelectorViewModel" />
        /// </summary>
        IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; }

        /// <summary>
        /// Creates or edits a <see cref="DomainFileStore" />
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="DomainFileStore" /> should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        Task CreateOrEditDomainFileStore(bool shouldCreate);

        /// <summary>
        /// Sets the <see cref="DomainFileStoreTableViewModel.CurrentIteration" /> value
        /// </summary>
        /// <param name="iteration">The iteration to be set</param>
        void SetCurrentIteration(Iteration iteration);
    }
}
