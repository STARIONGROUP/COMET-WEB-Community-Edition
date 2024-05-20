// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="UnitFactorRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row View Model for <see cref="UnitFactor" />s
    /// </summary>
    public class UnitFactorRowViewModel : BaseDataItemRowViewModel<UnitFactor>
    {
        /// <summary>
        /// Backing field for <see cref="MeasurementUnit" />
        /// </summary>
        private string measurementUnit;

        /// <summary>
        /// Backing field for <see cref="Exponent" />
        /// </summary>
        private string exponent;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitFactorRowViewModel" /> class.
        /// </summary>
        /// <param name="unitFactor">The associated <see cref="UnitFactor" /></param>
        public UnitFactorRowViewModel(UnitFactor unitFactor) : base(unitFactor)
        {
            this.MeasurementUnit = unitFactor.Unit.ShortName;
            this.Exponent = unitFactor.Exponent;
            this.UnitFactor = unitFactor;
        }

        /// <summary>
        /// Gets or sets the row's <see cref="UnitFactor"/>
        /// </summary>
        public UnitFactor UnitFactor { get; set; }

        /// <summary>
        /// The measurement unit of the <see cref="UnitFactor" />
        /// </summary>
        public string MeasurementUnit
        {
            get => this.measurementUnit;
            set => this.RaiseAndSetIfChanged(ref this.measurementUnit, value);
        }

        /// <summary>
        /// The exponent of the <see cref="UnitFactor" />
        /// </summary>
        public string Exponent
        {
            get => this.exponent;
            set => this.RaiseAndSetIfChanged(ref this.exponent, value);
        }
    }
}
