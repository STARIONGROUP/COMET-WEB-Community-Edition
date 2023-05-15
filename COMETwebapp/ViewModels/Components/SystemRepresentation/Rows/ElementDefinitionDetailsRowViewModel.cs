// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionDetailsRowViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.ViewModels.Components.SystemRepresentation.Rows
{
	using CDP4Common.EngineeringModelData;
	using CDP4Common.SiteDirectoryData;

	using ReactiveUI;

	/// <summary>
	/// Row View Model for  <see cref="ElementDefinition" />
	/// </summary>
	public class ElementDefinitionDetailsRowViewModel : ReactiveObject
	{
		/// <summary>
		/// Backing field for <see cref="ParameterTypeName" />
		/// </summary>
		private string parameterTypeName;

		/// <summary>
		/// Backing field for <see cref="ShortName" />
		/// </summary>
		private string shortName;

		/// <summary>
		/// Backing field for <see cref="ActualValue" />
		/// </summary>
		private string actualValue;

		/// <summary>
		/// Backing field for <see cref="PublishedValue" />
		/// </summary>
		private string publishedValue;

		/// <summary>
		/// Backing field for <see cref="Owner" />
		/// </summary>
		private string owner;

        /// <summary>
        /// Backing field for <see cref="Switch" />
        /// </summary>
        private string switchValue;

        /// <summary>
        /// Backing field for <see cref="modelCode" />
        /// </summary>
        private string modelCode;

        /// <summary>
        /// Backing field for <see cref="Parameter" />
        /// </summary>
        private Parameter parameter;

		/// <summary>
		/// Initializes a new instance of the <see cref="ElementDefinitionDetailsRowViewModel" /> class.
		/// </summary>
		public ElementDefinitionDetailsRowViewModel(Parameter parameter)
		{
			this.ParameterTypeName = parameter.ParameterType.Name;
			this.ShortName = parameter.ParameterType.ShortName;
			this.ActualValue = parameter.ValueSet.FirstOrDefault()?.ActualValue.ToString() + @parameter.Scale?.ShortName;
			this.PublishedValue = parameter.ValueSet.FirstOrDefault()?.Published.ToString() + @parameter.Scale?.ShortName;
			this.Owner = parameter.Owner.ShortName;
			this.SwitchValue = parameter.ValueSet.FirstOrDefault()?.ValueSwitch.ToString();
			this.ModelCode = parameter.ModelCode();
            this.Parameter = parameter;
		}

		/// <summary>
		/// The Name of the <see cref="ParameterType" />
		/// </summary>
		public string ParameterTypeName
		{
			get => this.parameterTypeName;
			set => this.RaiseAndSetIfChanged(ref this.parameterTypeName, value);
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
		/// The actual value of the <see cref="Parameter" />
		/// </summary>
		public string ActualValue
		{
			get => this.actualValue;
			set => this.RaiseAndSetIfChanged(ref this.actualValue, value);
		}

		/// <summary>
		/// The published value of the <see cref="Parameter" />
		/// </summary>
		public string PublishedValue
		{
			get => this.publishedValue;
			set => this.RaiseAndSetIfChanged(ref this.publishedValue, value);
		}

		/// <summary>
		/// The short name of the Owner of the <see cref="Parameter" />
		/// </summary>
		public string Owner
		{
			get => this.owner;
			set => this.RaiseAndSetIfChanged(ref this.owner, value);
		}

        /// <summary>
        /// The Switch Value of the <see cref="Parameter" />
        /// </summary>
        public string SwitchValue
		{
            get => this.switchValue;
            set => this.RaiseAndSetIfChanged(ref this.switchValue, value);
        }

        /// <summary>
        /// The model code of the <see cref="Parameter" />
        /// </summary>
        public string ModelCode
        {
            get => this.modelCode;
            set => this.RaiseAndSetIfChanged(ref this.modelCode, value);
        }

        /// <summary>
        /// The <see cref="Parameter" /> of selected <see cref="ElementDefinition" />
        /// </summary>
        public Parameter Parameter
		{
			get => this.parameter;
			set => this.RaiseAndSetIfChanged(ref this.parameter, value);
		}
	}
}
