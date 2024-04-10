// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FolderFileStructureViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.CommonFileStore.FolderFileStructure
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    /// <summary>
    /// View model used to manage the folder file structure
    /// </summary>
    public class FolderFileStructureViewModel : IFolderFileStructureViewModel
    {
        /// <summary>
        /// Gets or sets the current <see cref="FileStore"/>
        /// </summary>
        private FileStore CurrentFileStore { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISessionService"/>
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderFileStructureViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        public FolderFileStructureViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<FolderFileStructureViewModel> logger)
        {
            this.sessionService = sessionService;
        }

        /// <summary>
        /// Gets a collection of the available <see cref="DomainOfExpertise"/>
        /// </summary>
        public IEnumerable<DomainOfExpertise> DomainsOfExpertise { get; private set; }

        /// <summary>
        /// The folder-file hierarchically structured
        /// </summary>
        public List<FileFolderNodeViewModel> Structure { get; set; } = [];

        /// <summary>
        /// Gets or sets the file to be created/edited
        /// </summary>
        public File File { get; set; } = new();

        /// <summary>
        /// Gets or sets the folder to be created/edited
        /// </summary>
        public Folder Folder { get; set; } = new();

        /// <summary>
        /// Gets or sets the condition to check if the file or folder to be created is locked
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Initializes the current <see cref="FolderFileStructureViewModel"/>
        /// </summary>
        /// <param name="fileStore">The <see cref="FileStore"/> to be set</param>
        public void InitializeViewModel(FileStore fileStore)
        {
            this.CurrentFileStore = fileStore;
            this.DomainsOfExpertise = this.sessionService.GetSiteDirectory().Domain;
            this.Structure = [];

            var rootNode = new FileFolderNodeViewModel();
            this.LoadFolderContent(rootNode);
            this.Structure.Add(rootNode);
        }

        /// <summary>
        /// The method used to load a folder's content, including files and subfolders
        /// </summary>
        /// <param name="folderNode">The folder node</param>
        private void LoadFolderContent(FileFolderNodeViewModel folderNode)
        {
            var nestedFiles = this.CurrentFileStore.File
                .Where(x => x.CurrentContainingFolder?.Iid == folderNode.Thing?.Iid)
                .Select(x => new FileFolderNodeViewModel(x))
                .ToList();

            var nestedFolders = this.CurrentFileStore.Folder
                .Where(x => x.ContainingFolder?.Iid == folderNode.Thing?.Iid)
                .Select(x => new FileFolderNodeViewModel(x))
                .ToList();

            folderNode.Content.AddRange(nestedFolders);
            folderNode.Content.AddRange(nestedFiles);

            foreach (var nestedFolder in nestedFolders)
            {
                this.LoadFolderContent(nestedFolder);
            }
        }
    }
}
