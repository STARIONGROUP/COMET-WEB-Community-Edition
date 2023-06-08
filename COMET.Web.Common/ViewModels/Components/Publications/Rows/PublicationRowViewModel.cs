// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PublicationRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.ViewModels.Components.Publications.Rows
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the rows in the Publications component
    /// </summary>
    public class PublicationRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Gets or sets the <see cref="ParameterOrOverride"/> that this row represents
        /// </summary>
        public ParameterOrOverrideBase ParameterOrOverride { get; private set; }

        /// <summary>
        /// Gets or sets the name of the <see cref="DomainOfExpertise"/> of this publication
        /// </summary>
        public string Domain { get; private set; }

        /// <summary>
        /// Gets or sets the new value of the publication
        /// </summary>
        public string NewValue { get; private set; }

        /// <summary>
        /// Gets or sets the old value of the publication
        /// </summary>
        public string OldValue { get; private set; }

        /// <summary>
        /// Gets or sets the model code 
        /// </summary>
        public string ModelCode { get; private set; }

        /// <summary>
        /// Gets or sets the short name of the <see cref="ElementBase"/> that contains the publishable value
        /// </summary>
        public string ElementShortName { get; private set; }

        /// <summary>
        /// Gets or sets the name of the <see cref="ParameterType"/> that contains the publishable value
        /// </summary>
        public string ParameterType { get; private set; }

        /// <summary>
        /// Backing field for the <see cref="IsSelected"/> property
        /// </summary>
        private bool isSelected;

        /// <summary>
        /// Gets or sets if the row is selected. Only selected rows will be published.
        /// </summary>
        public bool IsSelected
        {
            get => this.isSelected;
            set => this.RaiseAndSetIfChanged(ref this.isSelected, value);
        }

        /// <summary>
        /// Creates a new instance of type <see cref="PublicationsViewModel"/>
        /// </summary>
        /// <param name="parameter">the <see cref="ParameterOrOverrideBase"/></param>
        /// <param name="valueSet">the <see cref="IValueSet"/></param>
        public PublicationRowViewModel(ParameterOrOverrideBase parameter, IValueSet valueSet)
        {
            if (valueSet is not ParameterValueSet parameterValueSet)
            {
                throw new ArgumentException($"The {valueSet} must be of type ParameterValueSet", nameof(valueSet));
            }
            
            this.ParameterOrOverride = parameter;
            this.Domain = parameter.Owner.Name;
            this.ElementShortName = ((ElementBase)parameter.Container).ShortName;
            this.ModelCode = parameterValueSet.ModelCode();
            this.NewValue = parameterValueSet.ActualValue.ToString();
            this.OldValue = parameterValueSet.Published.ToString();
            this.ParameterType = parameter.ParameterType.Name;
        }
    }
}
