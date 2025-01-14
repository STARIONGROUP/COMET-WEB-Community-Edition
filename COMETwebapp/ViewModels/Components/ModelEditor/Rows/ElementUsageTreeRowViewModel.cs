// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementUsageTreeRowViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ModelEditor.Rows
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Row View Model for  <see cref="ElementUsage" />
    /// </summary>
    public class ElementUsageTreeRowViewModel : ElementBaseTreeRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementUsageTreeRowViewModel" /> class.
        /// <param name="elementUsage">the <see cref="ElementUsage" /></param>
        /// </summary>
        public ElementUsageTreeRowViewModel(ElementUsage elementUsage) : base(elementUsage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementUsageTreeRowViewModel" /> class.
        /// </summary>
        public ElementUsageTreeRowViewModel()
        {
        }

        /// <summary>
        /// Update this row view model properties
        /// </summary>
        /// <param name="elementUsageTreeRow">The <see cref="ElementUsageTreeRowViewModel" /> to use for updating</param>
        public void UpdateProperties(ElementUsageTreeRowViewModel elementUsageTreeRow)
        {
            base.UpdateProperties(elementUsageTreeRow);
        }
    }
}
