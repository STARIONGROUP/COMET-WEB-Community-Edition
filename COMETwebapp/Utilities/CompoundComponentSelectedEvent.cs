// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CompoundComponentSelectedEvent.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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
using COMETwebapp.ViewModels.Components.Shared.ParameterEditors;

namespace COMETwebapp.Utilities
{
    /// <summary>
    /// Class used to notify an observer that the <see cref="CompoundParameterTypeEditorViewModel"/> is selected.
    /// </summary>
    public class CompoundComponentSelectedEvent
    {
        /// <summary>
        /// Gets or sets the <see cref="CompoundParameterTypeEditorViewModel" />
        /// </summary>
        public CompoundParameterTypeEditorViewModel CompoundParameterTypeEditorViewModel { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundComponentSelectedEvent" /> class.
        /// </summary>
        /// <param name="compoundParameterTypeEditorViewModel">The <see cref="CompoundParameterTypeEditorViewModel" /></param>
        public CompoundComponentSelectedEvent(CompoundParameterTypeEditorViewModel compoundParameterTypeEditorViewModel)
        {
            this.CompoundParameterTypeEditorViewModel = compoundParameterTypeEditorViewModel;
        }
    }
}
