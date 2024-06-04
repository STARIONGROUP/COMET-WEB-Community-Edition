// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFolderHandlerViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FolderHandler
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    /// <summary>
    /// View model used to manage the folders in Filestores
    /// </summary>
    public interface IFolderHandlerViewModel : IApplicationBaseViewModel
    {
        /// <summary>
        /// Initializes the current <see cref="FolderHandlerViewModel"/>
        /// </summary>
        /// <param name="fileStore">The <see cref="FileStore"/> to be set</param>
        /// <param name="iteration">The current <see cref="Iteration"/></param>
        void InitializeViewModel(FileStore fileStore, Iteration iteration);

        /// <summary>
        /// Gets or sets the folder to be created/edited
        /// </summary>
        Folder CurrentThing { get; set; }

        /// <summary>
        /// Gets or sets a collection of the available <see cref="Folder" />s
        /// </summary>
        IEnumerable<Folder> Folders { get; set; }

        /// <summary>
        /// Gets the <see cref="IDomainOfExpertiseSelectorViewModel"/>
        /// </summary>
        IDomainOfExpertiseSelectorViewModel DomainOfExpertiseSelectorViewModel { get; }

        /// <summary>
        /// Creates or edits a folder
        /// </summary>
        /// <param name="shouldCreate">the value to check if the <see cref="CurrentThing"/> should be created or edited</param>
        /// <returns>A <see cref="Task"/></returns>
        Task CreateOrEditFolder(bool shouldCreate);

        /// <summary>
        /// Moves a folder to a target folder
        /// </summary>
        /// <param name="folder">The folder to be moved</param>
        /// <param name="targetFolder">the target folders</param>
        /// <returns>A <see cref="Task"/></returns>
        Task MoveFolder(Folder folder, Folder targetFolder);

        /// <summary>
        /// Deletes the current folder
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        Task DeleteFolder();
    }
}
