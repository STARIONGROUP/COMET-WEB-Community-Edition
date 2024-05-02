// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileRevisionRowViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.Rows
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.Common.Rows;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for  <see cref="FileRevision" />
    /// </summary>
    public class FileRevisionRowViewModel : ReactiveObject
    {
        /// <summary>
        /// The backing field for <see cref="CreatedOn"/>
        /// </summary>
        private string createdOn;

        /// <summary>
        /// The backing field for <see cref="Path"/>
        /// </summary>
        private string path;

        /// <summary>
        /// The backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileRevisionRowViewModel" /> class.
        /// </summary>
        /// <param name="fileRevision">The associated <see cref="FileRevision" /></param>
        public FileRevisionRowViewModel(FileRevision fileRevision)
        {
            this.Thing = fileRevision;
            this.CreatedOn = fileRevision.CreatedOn.ToString("dd/MM/yyyy HH:mm:ss");
            this.Path = fileRevision.Path;
            this.Name = fileRevision.Name;
        }

        /// <summary>
        /// Gets or sets the current <see cref="FileRevision"/>
        /// </summary>
        public FileRevision Thing { get; set; }

        /// <summary>
        /// The date and time when the <see cref="FileRevision"/> was created, as a <see cref="string"/>
        /// </summary>
        public string CreatedOn
        {
            get => this.createdOn;
            set => this.RaiseAndSetIfChanged(ref this.createdOn, value);
        }

        /// <summary>
        /// The path of the <see cref="FileRevision"/>
        /// </summary>
        public string Path
        {
            get => this.path;
            set => this.RaiseAndSetIfChanged(ref this.path, value);
        }

        /// <summary>
        /// The name of the <see cref="FileRevision"/>
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }
    }
}
