// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeComponentRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row View Model for  <see cref="ParameterTypeComponent" />
    /// </summary>
    public class ParameterTypeComponentRowViewModel : BaseDataItemRowViewModel<ParameterTypeComponent>
    {
        /// <summary>
        /// Backing field for <see cref="Coordinates" />
        /// </summary>
        private string coordinates;

        /// <summary>
        /// Backing field for <see cref="ParameterType" />
        /// </summary>
        private string parameterType;

        /// <summary>
        /// Backing field for <see cref="Scale" />
        /// </summary>
        private string scale;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeComponentRowViewModel" /> class.
        /// </summary>
        /// <param name="parameterTypeComponent">The associated <see cref="ParameterTypeComponent" /></param>
        public ParameterTypeComponentRowViewModel(ParameterTypeComponent parameterTypeComponent) : base(parameterTypeComponent)
        {
            this.Scale = parameterTypeComponent.Scale?.ShortName;
            this.ParameterType = parameterTypeComponent.ParameterType?.Name;
            this.Coordinates = parameterTypeComponent.Index.ToString();
        }

        /// <summary>
        /// The coordinates of the <see cref="ParameterTypeComponent" />
        /// </summary>
        public string Coordinates
        {
            get => this.coordinates;
            set => this.RaiseAndSetIfChanged(ref this.coordinates, value);
        }

        /// <summary>
        /// The parameter type of the <see cref="ParameterTypeComponent" />
        /// </summary>
        public string ParameterType
        {
            get => this.parameterType;
            set => this.RaiseAndSetIfChanged(ref this.parameterType, value);
        }

        /// <summary>
        /// The scale of the <see cref="ParameterTypeComponent" />
        /// </summary>
        public string Scale
        {
            get => this.scale;
            set => this.RaiseAndSetIfChanged(ref this.scale, value);
        }
    }
}
