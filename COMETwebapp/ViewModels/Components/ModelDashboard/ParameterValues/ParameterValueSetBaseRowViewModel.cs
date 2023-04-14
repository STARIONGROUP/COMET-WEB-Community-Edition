// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterValueSetBaseRowViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.ModelDashboard.ParameterValues
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    /// <summary>
    /// Row view model for the <see cref="ParameterValueSetBase" />
    /// </summary>
    public class ParameterValueSetBaseRowViewModel
    {
        /// <summary>
        /// Initializes a new <see cref="ParameterValueSetBaseRowViewModel" />
        /// </summary>
        /// <param name="valueSet">The <see cref="ParameterValueSetBase" /></param>
        public ParameterValueSetBaseRowViewModel(ParameterValueSetBase valueSet)
        {
            this.ElementName = ((ElementBase)valueSet.Container.Container).Name;
            var parameter = (ParameterOrOverrideBase)valueSet.Container;
            this.ParameterName = parameter.ParameterType.Name;

            if (parameter.Scale != null)
            {
                this.ParameterName += $" [{parameter.Scale.ShortName}]";
            }

            this.ParameterType = parameter.ParameterType;
            this.OptionName = valueSet.ActualOption?.Name ?? "-";
            this.ActualFiniteStateName = valueSet.ActualState?.Name ?? "-";
            this.ActualValue = valueSet.ActualValue;
            this.PublishedValue = valueSet.Published;
            this.ModelCode = valueSet.ModelCode().Split("\\")[0];
            this.Owner = valueSet.Owner.ShortName;
            this.Scale = parameter.Scale;
        }

        /// <summary>
        /// The <see cref="MeasurementScale" />
        /// </summary>
        public MeasurementScale Scale { get; }

        /// <summary>
        /// The shortname of the <see cref="DomainOfExpertise" /> owner the <see cref="ParameterValueSetBase" />
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// The model code of the <see cref="ParameterValueSetBase" />
        /// </summary>
        public string ModelCode { get; }

        /// <summary>
        /// The <see cref="ParameterType" />
        /// </summary>
        public ParameterType ParameterType { get; set; }

        /// <summary>
        /// The published value
        /// </summary>
        public ValueArray<string> PublishedValue { get; }

        /// <summary>
        /// The actual value
        /// </summary>
        public ValueArray<string> ActualValue { get; }

        /// <summary>
        /// The name of the <see cref="ParameterType" />
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// The name of the <see cref="Option" />, if any
        /// </summary>
        public string OptionName { get; }

        /// <summary>
        /// The name of the <see cref="ActualFiniteState" />, if any
        /// </summary>
        public string ActualFiniteStateName { get; }

        /// <summary>
        /// The name of the <see cref="ElementBase" />
        /// </summary>
        public string ElementName { get; }
    }
}
