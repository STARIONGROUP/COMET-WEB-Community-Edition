// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IFolderFileStructureViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.FileStore
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileHandler;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FolderHandler;

    /// <summary>
    /// View model used to manage the folder file structure
    /// </summary>
    public interface IFolderFileStructureViewModel : IApplicationBaseViewModel, IHaveReusableRows
    {
        /// <summary>
        /// The folder-file hierarchically structured
        /// </summary>
        List<FileFolderNodeViewModel> Structure { get; set; }

        /// <summary>
        /// Gets the <see cref="IFileHandlerViewModel" />
        /// </summary>
        IFileHandlerViewModel FileHandlerViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IFolderHandlerViewModel" />
        /// </summary>
        IFolderHandlerViewModel FolderHandlerViewModel { get; }

        /// <summary>
        /// Initializes the current <see cref="FolderFileStructureViewModel" />
        /// </summary>
        /// <param name="fileStore">The <see cref="FileStore" /> to be set</param>
        /// <param name="iteration">The current <see cref="Iteration" /></param>
        void InitializeViewModel(FileStore fileStore, Iteration iteration);
    }
}
