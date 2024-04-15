// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileTypeRowViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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

namespace COMETwebapp.ViewModels.Components.EngineeringModel.Rows
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.Rows;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for  <see cref="FileType" />
    /// </summary>
    public class FileTypeRowViewModel : BaseDataItemRowViewModel<FileType>
    {
        /// <summary>
        /// The backing field for <see cref="Extension"/>
        /// </summary>
        private string extension;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTypeRowViewModel" /> class.
        /// </summary>
        /// <param name="fileType">The associated <see cref="FileType" /></param>
        public FileTypeRowViewModel(FileType fileType) : base(fileType)
        {
            this.Extension = fileType.Extension;
        }

        /// <summary>
        /// The file type extension
        /// </summary>
        public string Extension
        {
            get => this.extension;
            set => this.RaiseAndSetIfChanged(ref this.extension, value);
        }
    }
}
