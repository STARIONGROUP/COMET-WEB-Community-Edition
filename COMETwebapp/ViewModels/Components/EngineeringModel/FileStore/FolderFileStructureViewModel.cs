// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FolderFileStructureViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FileHandler;
    using COMETwebapp.ViewModels.Components.EngineeringModel.FileStore.FolderHandler;

    /// <summary>
    /// View model used to manage the folder file structure
    /// </summary>
    public class FolderFileStructureViewModel : ApplicationBaseViewModel, IFolderFileStructureViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FolderFileStructureViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="fileHandlerViewModel">The <see cref="IFileHandlerViewModel" /></param>
        /// <param name="folderHandlerViewModel">The <see cref="IFolderHandlerViewModel" /></param>
        public FolderFileStructureViewModel(ISessionService sessionService, ICDPMessageBus messageBus, IFileHandlerViewModel fileHandlerViewModel, IFolderHandlerViewModel folderHandlerViewModel)
            : base(sessionService, messageBus)
        {
            this.FileHandlerViewModel = fileHandlerViewModel;
            this.FolderHandlerViewModel = folderHandlerViewModel;
            this.InitializeSubscriptions([typeof(File), typeof(Folder), typeof(FileStore)]);
            this.RegisterViewModelWithReusableRows(this);
        }

        /// <summary>
        /// Gets or sets the current <see cref="FileStore" />
        /// </summary>
        private FileStore CurrentFileStore { get; set; }

        /// <summary>
        /// Gets the <see cref="IFileHandlerViewModel" />
        /// </summary>
        public IFileHandlerViewModel FileHandlerViewModel { get; private set; }

        /// <summary>
        /// Gets the <see cref="IFolderHandlerViewModel" />
        /// </summary>
        public IFolderHandlerViewModel FolderHandlerViewModel { get; private set; }

        /// <summary>
        /// The folder-file hierarchically structured
        /// </summary>
        public List<FileFolderNodeViewModel> Structure { get; set; } = [];

        /// <summary>
        /// Initializes the current <see cref="FolderFileStructureViewModel" />
        /// </summary>
        /// <param name="fileStore">The <see cref="FileStore" /> to be set</param>
        /// <param name="iteration">The current <see cref="Iteration" /></param>
        public void InitializeViewModel(FileStore fileStore, Iteration iteration)
        {
            this.CurrentFileStore = fileStore;
            this.FileHandlerViewModel.InitializeViewModel(this.CurrentFileStore, iteration);
            this.FolderHandlerViewModel.InitializeViewModel(this.CurrentFileStore, iteration);
            this.SetFolders(this.CurrentFileStore.Folder);
            this.CreateStructureTree();
        }

        /// <summary>
        /// Add rows related to <see cref="Thing" /> that has been added
        /// </summary>
        /// <param name="addedThings">A collection of added <see cref="Thing" /></param>
        public void AddRows(IEnumerable<Thing> addedThings)
        {
            var addedThingsList = addedThings.Where(x => x is File or Folder && x.Container?.Iid == this.CurrentFileStore.Iid).ToList();
            var flatListOfNodes = this.Structure[0].GetFlatListOfChildrenNodes(true).ToList();

            foreach (var addedThing in addedThingsList)
            {
                var parentNode = GetContainingFolderNodeFromList(flatListOfNodes, addedThing);
                var nodeToAdd = new FileFolderNodeViewModel(addedThing);
                parentNode.Content.Add(nodeToAdd);
            }
        }

        /// <summary>
        /// Updates rows related to <see cref="Thing" /> that have been updated
        /// </summary>
        /// <param name="updatedThings">A collection of updated <see cref="Thing" /></param>
        public void UpdateRows(IEnumerable<Thing> updatedThings)
        {
            var updatedThingsList = updatedThings.Where(x => x is File or Folder && x.Container?.Iid == this.CurrentFileStore.Iid).ToList();
            var rootNode = this.Structure[0];
            var flatListOfNodes = rootNode.GetFlatListOfChildrenNodes(true).ToList();

            foreach (var updatedThing in updatedThingsList)
            {
                var nodeToUpdate = flatListOfNodes.First(x => x.Thing?.Iid == updatedThing?.Iid);
                var parentNode = GetContainingFolderNodeFromList(flatListOfNodes, updatedThing);

                rootNode.RemoveChildNode(nodeToUpdate);
                nodeToUpdate.UpdateThing(updatedThing);
                parentNode.Content.Add(nodeToUpdate);
            }

            var updatedFileStore = this.UpdatedThings.OfType<FileStore>().FirstOrDefault(x => x.Iid == this.CurrentFileStore?.Iid);

            if (updatedFileStore != null)
            {
                this.SetFolders(updatedFileStore.Folder);
            }
        }

        /// <summary>
        /// Remove rows related to a <see cref="Thing" /> that has been deleted
        /// </summary>
        /// <param name="deletedThings">A collection of deleted <see cref="Thing" /></param>
        public void RemoveRows(IEnumerable<Thing> deletedThings)
        {
            var rootNode = this.Structure[0];
            var flatListOfNodes = rootNode.GetFlatListOfChildrenNodes(true).ToList();

            var deletedThingsList = this.DeletedThings.Where(x => x is File or Folder && x.Container?.Iid == this.CurrentFileStore.Iid)
                .Select(deletedThing => flatListOfNodes.FirstOrDefault(x => x.Thing?.Iid == deletedThing?.Iid))
                .ToList();

            foreach (var nodeToDelete in deletedThingsList)
            {
                rootNode.RemoveChildNode(nodeToDelete);
            }
        }

        /// <summary>
        /// Handles the <see cref="SessionStatus.EndUpdate" /> message received
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnEndUpdate()
        {
            await this.OnSessionRefreshed();
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed()
        {
            if ((this.AddedThings.Count == 0 && this.UpdatedThings.Count == 0 && this.DeletedThings.Count == 0) || this.CurrentFileStore is null)
            {
                return Task.CompletedTask;
            }

            this.IsLoading = true;

            this.UpdateInnerComponents();
            this.ClearRecordedChanges();

            this.IsLoading = false;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates the structure tree, present in <see cref="Structure" />
        /// </summary>
        private void CreateStructureTree()
        {
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
                .OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase)
                .ToList();

            var nestedFolders = this.CurrentFileStore.Folder
                .Where(x => x.ContainingFolder?.Iid == folderNode.Thing?.Iid)
                .Select(x => new FileFolderNodeViewModel(x))
                .OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase)
                .ToList();

            folderNode.Content.AddRange(nestedFolders);
            folderNode.Content.AddRange(nestedFiles);

            foreach (var nestedFolder in nestedFolders)
            {
                this.LoadFolderContent(nestedFolder);
            }
        }

        /// <summary>
        /// Sets the collection of <see cref="Folder" />s with a null value appended for the contained view models that need it
        /// </summary>
        /// <param name="folders">The collection of folders to set</param>
        private void SetFolders(IEnumerable<Folder> folders)
        {
            var resultingFolders = folders.ToList();
            resultingFolders.Add(null);
            this.FileHandlerViewModel.Folders = resultingFolders;
            this.FolderHandlerViewModel.Folders = resultingFolders;
        }

        /// <summary>
        /// Gets the containing folder node for a given node, search in a given flat list of nodes
        /// </summary>
        /// <param name="listOfNodes">The collection of nodes to search for the containing folder node</param>
        /// <param name="node">The node for which the containing folder will be searched for</param>
        /// <returns>The containing folder node</returns>
        private static FileFolderNodeViewModel GetContainingFolderNodeFromList(IEnumerable<FileFolderNodeViewModel> listOfNodes, Thing node)
        {
            FileFolderNodeViewModel parentNode;

            switch (node)
            {
                case File file:
                    parentNode = listOfNodes.First(x => x.Thing?.Iid == file.CurrentContainingFolder?.Iid);
                    break;
                case Folder folder:
                    parentNode = listOfNodes.First(x => x.Thing?.Iid == folder.ContainingFolder?.Iid);
                    break;
                default:
                    return null;
            }

            return parentNode;
        }
    }
}
