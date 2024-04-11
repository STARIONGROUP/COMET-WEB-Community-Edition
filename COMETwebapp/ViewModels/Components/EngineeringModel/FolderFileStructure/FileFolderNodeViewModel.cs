// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileFolderNodeViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.EngineeringModel.FolderFileStructure
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for a node in the File-Folder structure tree
    /// </summary>
    public class FileFolderNodeViewModel : ReactiveObject
    {
        /// <summary>
        /// The backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// The list containing the sub content of the current node
        /// </summary>
        public List<FileFolderNodeViewModel> Content { get; private set; } = [];

        /// <summary>
        /// The thing associated with the model, which can be either a <see cref="File"/> or a <see cref="Folder"/>
        /// </summary>
        public Thing Thing { get; set; }

        /// <summary>
        /// The name of the current node
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// The icon css class of the current node, used to display the icon as a html class
        /// </summary>
        public string IconCssClass { get; private set; }

        /// <summary>
        /// Creates a new instance of type <see cref="FileFolderNodeViewModel"/>
        /// </summary>
        /// <param name="thing">The thing to set the current <see cref="Thing"/>, which can be both <see cref="File"/> or <see cref="Folder"/></param>
        /// <param name="content">A collection containing all the node's content</param>
        public FileFolderNodeViewModel(Thing thing, IEnumerable<FileFolderNodeViewModel> content = null)
        {
            this.InitializeProperties(thing, content);
        }

        /// <summary>
        /// Creates a new instance of type <see cref="FileFolderNodeViewModel"/>, as a root node
        /// </summary>
        public FileFolderNodeViewModel()
        {
            this.Name = "root";
            this.IconCssClass = "oi oi-target";
        }

        /// <summary>
        /// Updates the contained <see cref="Thing"/>
        /// </summary>
        /// <param name="thing">The thing to be set</param>
        public void UpdateThing(Thing thing)
        {
            if (this.Thing.GetType() != thing.GetType())
            {
                throw new InvalidCastException("Cannot change current thing type");
            }

            this.InitializeProperties(thing);
        }

        /// <summary>
        /// Removes a given node from the nested content
        /// </summary>
        /// <param name="node">The node to be removed</param>
        public void RemoveChildNode(FileFolderNodeViewModel node)
        {
            var wasNodeRemoved = this.Content.Remove(node);

            if (wasNodeRemoved)
            {
                return;
            }

            foreach (var folderNodes in this.Content.Where(x => x.Thing is Folder))
            {
                folderNodes.RemoveChildNode(node);
            }
        }

        /// <summary>
        /// Gets a flat list with all the children nodes
        /// </summary>
        /// <param name="includeSelf">The value to set if current node should be included in the result</param>
        /// <returns>The collection of children</returns>
        public IEnumerable<FileFolderNodeViewModel> GetFlatListOfChildrenNodes(bool includeSelf = false)
        {
            var children = new List<FileFolderNodeViewModel>();

            if (includeSelf)
            {
                children.Add(this);
            }

            children.AddRange(this.Content);

            foreach (var content in this.Content)
            {
                children.AddRange(content.GetFlatListOfChildrenNodes());
            }

            return children;
        }

        /// <summary>
        /// Initializes the node's properties
        /// </summary>
        /// <param name="thing">The thing to set the current <see cref="Thing"/></param>
        /// <param name="content">The collection of <see cref="FileFolderNodeViewModel"/> to set the current <see cref="Content"/></param>
        private void InitializeProperties(Thing thing, IEnumerable<FileFolderNodeViewModel> content = null)
        {
            switch (thing)
            {
                case File file:
                    this.Name = file.CurrentFileRevision.Name;
                    this.IconCssClass = "oi oi-file";
                    break;
                case Folder folder:
                    this.Name = folder.Name;
                    this.IconCssClass = "oi oi-folder";
                    break;
                default:
                    throw new InvalidDataException("The given thing should be either a File or a Folder");
            }

            this.Thing = thing;

            if (content != null)
            {
                this.Content = content.ToList();
            }
        }
    }
}
