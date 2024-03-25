// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MeasurementUnitRowViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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
    /// Row View Model for  <see cref="CDP4Common.SiteDirectoryData.MeasurementUnit" />
    /// </summary>
    public class MeasurementUnitRowViewModel : ReferenceDataItemRowViewModel<MeasurementUnit>
    {
        /// <summary>
        /// Backing field for <see cref="Type" />
        /// </summary>
        private string type;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementUnitRowViewModel" /> class.
        /// </summary>
        /// <param name="measurementUnit">The associated <see cref="MeasurementUnit" /></param>
        public MeasurementUnitRowViewModel(MeasurementUnit measurementUnit) : base(measurementUnit)
        {
            this.Type = measurementUnit.ClassKind.ToString();
        }

        /// <summary>
        /// The <see cref="CDP4Common.SiteDirectoryData.MeasurementUnit" /> type
        /// </summary>
        public string Type
        {
            get => this.type;
            set => this.RaiseAndSetIfChanged(ref this.type, value);
        }
    }
}
