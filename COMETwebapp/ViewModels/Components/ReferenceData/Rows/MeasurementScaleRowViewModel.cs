// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MeasurementScaleRowViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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
    /// Row View Model for  <see cref="CDP4Common.SiteDirectoryData.MeasurementScale" />
    /// </summary>
    public class MeasurementScaleRowViewModel : DeprecatableDataItemRowViewModel<MeasurementScale>
    {
        /// <summary>
        /// Backing field for <see cref="Type" />
        /// </summary>
        private string type;

        /// <summary>
        /// Backing field for <see cref="NumberSet" />
        /// </summary>
        private string numberSet;

        /// <summary>
        /// Backing field for <see cref="Unit" />
        /// </summary>
        private string unit;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementScaleRowViewModel" /> class.
        /// </summary>
        /// <param name="measurementScale">The associated <see cref="MeasurementScale" /></param>
        public MeasurementScaleRowViewModel(MeasurementScale measurementScale) : base(measurementScale)
        {
            this.Type = measurementScale.ClassKind.ToString();
            this.NumberSet = measurementScale.NumberSet.ToString();
            this.Unit = measurementScale.Unit?.ShortName;
        }

        /// <summary>
        /// The <see cref="CDP4Common.SiteDirectoryData.MeasurementScale" /> type
        /// </summary>
        public string Type
        {
            get => this.type;
            set => this.RaiseAndSetIfChanged(ref this.type, value);
        }

        /// <summary>
        /// The <see cref="CDP4Common.SiteDirectoryData.MeasurementScale" /> number set
        /// </summary>
        public string NumberSet
        {
            get => this.numberSet;
            set => this.RaiseAndSetIfChanged(ref this.numberSet, value);
        }

        /// <summary>
        /// The <see cref="CDP4Common.SiteDirectoryData.MeasurementScale" /> measurement unit
        /// </summary>
        public string Unit
        {
            get => this.unit;
            set => this.RaiseAndSetIfChanged(ref this.unit, value);
        }
    }
}
