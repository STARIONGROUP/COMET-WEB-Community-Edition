// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterSwitchKindSelector.razor.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Components.Selectors
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Component used to select a <see cref="ParameterSwitchKind" />
    /// </summary>
    public partial class ParameterSwitchKindSelector
    {
        /// <summary>
        /// A collection of all available <see cref="ParameterSwitchKind" />
        /// </summary>
        private readonly IEnumerable<ParameterSwitchKind> availableValues = Enum.GetValues(typeof(ParameterSwitchKind)).Cast<ParameterSwitchKind>();

        /// <summary>
        /// Gets or sets the <see cref="IParameterSwitchKindSelectorViewModel" />
        /// </summary>
        [Parameter]
        public IParameterSwitchKindSelectorViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsReadOnly)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Gets the css class based on the current value of the <see cref="ParameterSwitchKind" />
        /// </summary>
        /// <param name="switchKind">The <see cref="ParameterSwitchKind" /></param>
        /// <returns>
        /// A css class if the <see cref="ParameterSwitchKind" /> correspond to the
        /// <see cref="IParameterSwitchKindSelectorViewModel.InitialSwitchValue" />
        /// </returns>
        private string GetCssClass(ParameterSwitchKind switchKind)
        {
            return switchKind == this.ViewModel.InitialSwitchValue && switchKind != this.ViewModel.SwitchValue ? "highlighted" : string.Empty;
        }
    }
}
