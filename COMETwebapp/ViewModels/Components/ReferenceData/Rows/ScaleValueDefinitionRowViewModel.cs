// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ScaleValueDefinitionRowViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ReferenceData.Rows
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.Rows;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for <see cref="ScaleValueDefinition" />s
    /// </summary>
    public class ScaleValueDefinitionRowViewModel : BaseDataItemRowViewModel<ScaleValueDefinition>
    {
        /// <summary>
        /// Backing field for <see cref="Value" />
        /// </summary>
        private string value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleValueDefinitionRowViewModel" /> class.
        /// </summary>
        /// <param name="scaleValueDefinition">The associated <see cref="ScaleValueDefinition" /></param>
        public ScaleValueDefinitionRowViewModel(ScaleValueDefinition scaleValueDefinition) : base(scaleValueDefinition)
        {
            this.Value = scaleValueDefinition.Value;
        }

        /// <summary>
        /// The value of the <see cref="ScaleValueDefinition" />
        /// </summary>
        public string Value
        {
            get => this.value;
            set => this.RaiseAndSetIfChanged(ref this.value, value);
        }
    }
}
