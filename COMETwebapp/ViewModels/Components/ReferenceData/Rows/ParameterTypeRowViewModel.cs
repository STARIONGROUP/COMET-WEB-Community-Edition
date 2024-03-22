// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row View Model for  <see cref="ParameterType" />
    /// </summary>
    public class ParameterTypeRowViewModel : ReferenceDataItemRowViewModel<ParameterType>
    {
        /// <summary>
        /// Backing field for <see cref="DefaultScale" />
        /// </summary>
        private string defaultScale;

        /// <summary>
        /// Backing field for <see cref="Symbol" />
        /// </summary>
        private string symbol;

        /// <summary>
        /// Backing field for <see cref="Type" />
        /// </summary>
        private string type;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeRowViewModel" /> class.
        /// </summary>
        /// <param name="parameterType">The associated <see cref="ParameterType" /></param>
        public ParameterTypeRowViewModel(ParameterType parameterType) : base(parameterType)
        {
            this.Symbol = parameterType.Symbol;
            var scale = parameterType is QuantityKind quantityKind ? quantityKind.DefaultScale : null;
            this.DefaultScale = scale?.ShortName;
            this.Type = parameterType.ClassKind.ToString();
        }

        /// <summary>
        /// The symbol of the <see cref="ParameterType" />
        /// </summary>
        public string Symbol
        {
            get => this.symbol;
            set => this.RaiseAndSetIfChanged(ref this.symbol, value);
        }

        /// <summary>
        /// The default scale of the <see cref="ParameterType" />
        /// </summary>
        public string DefaultScale
        {
            get => this.defaultScale;
            set => this.RaiseAndSetIfChanged(ref this.defaultScale, value);
        }

        /// <summary>
        /// The <see cref="ParameterType" /> type
        /// </summary>
        public string Type
        {
            get => this.type;
            set => this.RaiseAndSetIfChanged(ref this.type, value);
        }

        /// <summary>
        /// Update this row view model properties
        /// </summary>
        /// <param name="parameterTypeRow">The <see cref="ParameterTypeRowViewModel" /> to use for updating</param>
        public void UpdateProperties(ParameterTypeRowViewModel parameterTypeRow)
        {
            base.UpdateProperties(parameterTypeRow);
            this.Symbol = parameterTypeRow.Symbol;
            this.DefaultScale = parameterTypeRow.DefaultScale;
            this.Type = parameterTypeRow.Type;
        }
    }
}
