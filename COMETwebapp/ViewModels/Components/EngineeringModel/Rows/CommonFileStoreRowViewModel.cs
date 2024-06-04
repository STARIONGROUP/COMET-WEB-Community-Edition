// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CommonFileStoreRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row View Model for  <see cref="CommonFileStore" />
    /// </summary>
    public class CommonFileStoreRowViewModel : BaseDataItemRowViewModel<CommonFileStore>
    {
        /// <summary>
        /// The backing field for <see cref="CreatedOn"/>
        /// </summary>
        private DateTime createdOn;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonFileStoreRowViewModel" /> class.
        /// </summary>
        /// <param name="commonFileStore">The associated <see cref="CommonFileStore" /></param>
        public CommonFileStoreRowViewModel(CommonFileStore commonFileStore) : base(commonFileStore)
        {
            this.CreatedOn = commonFileStore.CreatedOn;
        }

        /// <summary>
        /// The date and time when the <see cref="CommonFileStore"/> was created
        /// </summary>
        public DateTime CreatedOn
        {
            get => this.createdOn;
            set => this.RaiseAndSetIfChanged(ref this.createdOn, value);
        }
    }
}
