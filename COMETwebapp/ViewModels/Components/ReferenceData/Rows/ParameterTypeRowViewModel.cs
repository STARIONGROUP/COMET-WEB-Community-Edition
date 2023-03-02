// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeRowViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.ReferenceData.Rows
{
    using CDP4Common.SiteDirectoryData;

    using ReactiveUI;

    /// <summary>
    /// Row View Model for  <see cref="ParameterType" />
    /// </summary>
    public class ParameterTypeRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="ContainerName" />
        /// </summary>
        private string containerName;

        /// <summary>
        /// Backing field for <see cref="DefaultScale" />
        /// </summary>
        private string defaultScale;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated" />
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="Name" />
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="ShortName" />
        /// </summary>
        private string shortName;

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
        public ParameterTypeRowViewModel(ParameterType parameterType)
        {
            this.ParameterType = parameterType;
            this.Name = parameterType.Name;
            this.ShortName = parameterType.ShortName;
            this.Symbol = parameterType.Symbol;
            var scale = parameterType is QuantityKind quantityKind ? quantityKind.DefaultScale : null;
            this.DefaultScale = scale?.ShortName;
            this.Type = parameterType.ClassKind.ToString();
            var container = (ReferenceDataLibrary)parameterType.Container;
            this.ContainerName = container.ShortName;
            this.IsDeprecated = parameterType.IsDeprecated;
        }

        /// <summary>
        /// The associated <see cref="ParameterType" />
        /// </summary>
        public ParameterType ParameterType { get; private set; }

        /// <summary>
        /// The name of the <see cref="ParameterType" />
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// The short name of the <see cref="ParameterType" />
        /// </summary>
        public string ShortName
        {
            get => this.shortName;
            set => this.RaiseAndSetIfChanged(ref this.shortName, value);
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
        /// The <see cref="ParameterType" /> container name
        /// </summary>
        public string ContainerName
        {
            get => this.containerName;
            set => this.RaiseAndSetIfChanged(ref this.containerName, value);
        }

        /// <summary>
        /// Value indicating if the <see cref="ParameterType" /> is deprecated
        /// </summary>
        public bool IsDeprecated
        {
            get => this.isDeprecated;
            set => this.RaiseAndSetIfChanged(ref this.isDeprecated, value);
        }

        /// <summary>
        /// Update this row view model properties
        /// </summary>
        /// <param name="parameterTypeRow">The <see cref="ParameterTypeRowViewModel" /> to use for updating</param>
        public void UpdateProperties(ParameterTypeRowViewModel parameterTypeRow)
        {
            this.Name = parameterTypeRow.Name;
            this.ShortName = parameterTypeRow.ShortName;
            this.Symbol = parameterTypeRow.Symbol;
            this.DefaultScale = parameterTypeRow.DefaultScale;
            this.Type = parameterTypeRow.Type;
            this.ContainerName = parameterTypeRow.ContainerName;
            this.IsDeprecated = parameterTypeRow.IsDeprecated;
        }
    }
}