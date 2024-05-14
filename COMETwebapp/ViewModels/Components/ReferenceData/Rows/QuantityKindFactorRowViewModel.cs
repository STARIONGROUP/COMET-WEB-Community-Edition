// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="QuantityKindFactorRowViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ReferenceData.Rows
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.Common.Rows;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for <see cref="QuantityKindFactor" />
    /// </summary>
    public class QuantityKindFactorRowViewModel : BaseDataItemRowViewModel<QuantityKindFactor>
    {
        /// <summary>
        /// Backing field for <see cref="Exponent" />
        /// </summary>
        private string exponent;

        /// <summary>
        /// Backing field for <see cref="QuantityKind" />
        /// </summary>
        private string quantityKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityKindFactorRowViewModel" /> class.
        /// </summary>
        /// <param name="quantityKindFactor">The associated <see cref="QuantityKindFactor" /></param>
        public QuantityKindFactorRowViewModel(QuantityKindFactor quantityKindFactor) : base(quantityKindFactor)
        {
            this.QuantityKind = quantityKindFactor.QuantityKind?.Name;
            this.Exponent = quantityKindFactor.Exponent;
        }

        /// <summary>
        /// The quantity kind of the <see cref="QuantityKindFactor" />
        /// </summary>
        public string QuantityKind
        {
            get => this.quantityKind;
            set => this.RaiseAndSetIfChanged(ref this.quantityKind, value);
        }

        /// <summary>
        /// The exponent of the <see cref="QuantityKindFactor" />
        /// </summary>
        public string Exponent
        {
            get => this.exponent;
            set => this.RaiseAndSetIfChanged(ref this.exponent, value);
        }
    }
}
