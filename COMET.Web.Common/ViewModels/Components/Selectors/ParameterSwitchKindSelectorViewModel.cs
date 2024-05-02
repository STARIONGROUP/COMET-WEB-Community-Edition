// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterSwitchKindSelectorViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.ViewModels.Components.Selectors
{
    using CDP4Common.EngineeringModelData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View Model to sets the current <see cref="ParameterSwitchKind" /> value
    /// </summary>
    public class ParameterSwitchKindSelectorViewModel : ReactiveObject, IParameterSwitchKindSelectorViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsReadOnly" />
        /// </summary>
        private bool isReadOnly;

        /// <summary>
        /// Backing field for <see cref="SwitchValue" />
        /// </summary>
        private ParameterSwitchKind switchValue;

        /// <summary>
        /// Creates a new instance of type <see cref="ParameterSwitchKindSelectorViewModel" />
        /// </summary>
        /// <param name="switchKind">The <see cref="ParameterSwitchKind" /></param>
        /// <param name="isReadOnly">The readonly state</param>
        public ParameterSwitchKindSelectorViewModel(ParameterSwitchKind switchKind, bool isReadOnly)
        {
            this.InitializesProperties(switchKind, isReadOnly);
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="parameterSwitchKind">The <see cref="ParameterSwitchKind" /></param>
        /// <param name="readOnly">The readonly state</param>
        public void UpdateProperties(ParameterSwitchKind parameterSwitchKind, bool readOnly)
        {
            this.InitializesProperties(parameterSwitchKind, readOnly);
        }

        /// <summary>
        /// The initial <see cref="ParameterSwitchKind" />
        /// </summary>
        public ParameterSwitchKind InitialSwitchValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EventCallback{TValue}" /> to be called to perfom an update
        /// </summary>
        public EventCallback OnUpdate { get; set; }

        /// <summary>
        /// Get or set the <see cref="ParameterSwitchKind" />
        /// </summary>
        public ParameterSwitchKind SwitchValue
        {
            get => this.switchValue;
            set => this.RaiseAndSetIfChanged(ref this.switchValue, value);
        }

        /// <summary>
        /// Gets or sets the readonly state
        /// </summary>
        public bool IsReadOnly
        {
            get => this.isReadOnly;
            set => this.RaiseAndSetIfChanged(ref this.isReadOnly, value);
        }

        /// <summary>
        /// Initializes this view model properties
        /// </summary>
        /// <param name="switchKind">The <see cref="ParameterSwitchKind" /></param>
        /// <param name="readOnly">The readonly state</param>
        private void InitializesProperties(ParameterSwitchKind switchKind, bool readOnly)
        {
            this.InitialSwitchValue = switchKind;
            this.SwitchValue = switchKind;
            this.IsReadOnly = readOnly;
        }
    }
}
