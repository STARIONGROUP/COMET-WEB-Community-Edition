// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MappingToReferenceScaleRowViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.ReferenceData.Rows
{
    using CDP4Common.SiteDirectoryData;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for <see cref="MappingToReferenceScale" />s
    /// </summary>
    public class MappingToReferenceScaleRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Reference" />
        /// </summary>
        private string reference;

        /// <summary>
        /// Backing field for <see cref="ReferenceValue" />
        /// </summary>
        private string referenceValue;

        /// <summary>
        /// Backing field for <see cref="Dependent" />
        /// </summary>
        private string dependent;

        /// <summary>
        /// Backing field for <see cref="DependentValue" />
        /// </summary>
        private string dependentValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingToReferenceScaleRowViewModel" /> class.
        /// </summary>
        /// <param name="mappingToReferenceScale">The associated <see cref="MappingToReferenceScale" /></param>
        public MappingToReferenceScaleRowViewModel(MappingToReferenceScale mappingToReferenceScale)
        {
            this.Reference = mappingToReferenceScale.ReferenceScaleValue.ShortName;
            this.ReferenceValue = mappingToReferenceScale.ReferenceScaleValue.Value;
            this.Dependent = mappingToReferenceScale.DependentScaleValue.ShortName;
            this.DependentValue = mappingToReferenceScale.DependentScaleValue.Value;
        }

        /// <summary>
        /// Gets or sets the row's <see cref="MappingToReferenceScale"/>
        /// </summary>
        public MappingToReferenceScale MappingToReferenceScale { get; set; }

        /// <summary>
        /// The reference of the <see cref="MappingToReferenceScale" />
        /// </summary>
        public string Reference
        {
            get => this.reference;
            set => this.RaiseAndSetIfChanged(ref this.reference, value);
        }

        /// <summary>
        /// The reference value of the <see cref="MappingToReferenceScale" />
        /// </summary>
        public string ReferenceValue
        {
            get => this.referenceValue;
            set => this.RaiseAndSetIfChanged(ref this.referenceValue, value);
        }

        /// <summary>
        /// The dependent of the <see cref="MappingToReferenceScale" />
        /// </summary>
        public string Dependent
        {
            get => this.dependent;
            set => this.RaiseAndSetIfChanged(ref this.dependent, value);
        }

        /// <summary>
        /// The dependent value of the <see cref="MappingToReferenceScale" />
        /// </summary>
        public string DependentValue
        {
            get => this.dependentValue;
            set => this.RaiseAndSetIfChanged(ref this.dependentValue, value);
        }
    }
}
