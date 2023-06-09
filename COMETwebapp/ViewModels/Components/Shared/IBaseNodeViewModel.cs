// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IBaseNodeViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.Shared
{
    /// <summary>
    /// ViewModel that handle information related to <see cref="IBaseNodeViewModel"/> inside a tree
    /// </summary>
    public interface IBaseNodeViewModel
    {
        /// <summary>
        /// Level of the tree. Increases by one for each nested element
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the title of this <see cref="BaseNodeViewModel{T}"/>
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="BaseNodeViewModel{T}"/> is expanded
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="BaseNodeViewModel{T}"/> is drawn
        /// </summary>
        public bool IsDrawn { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="BaseNodeViewModel{T}"/> is selected
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
