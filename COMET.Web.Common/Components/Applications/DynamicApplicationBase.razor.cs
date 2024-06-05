// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DynamicApplicationBase.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//     Annex A and Annex C.
// 
//     Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
// 
//         http://www.apache.org/licenses/LICENSE-2.0
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.
// 
//   </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Components.Applications
{
    using System.ComponentModel.DataAnnotations;

    using CDP4Common.CommonData;

    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component that allows the rendering of any <see cref="ApplicationBase{TViewModel}"/> dynamically
    /// </summary>
    public partial class DynamicApplicationBase
    {
        /// <summary>
        /// Gets or sets the <see cref="Type"/> of the <see cref="ApplicationBase{TViewModel}"/> to render
        /// </summary>
        [Parameter]
        [Required]
        public Type ApplicationBaseType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IApplicationBaseViewModel"/>
        /// </summary>
        [Parameter]
        [Required]
        public IApplicationBaseViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the optional <see cref="Thing" />
        /// </summary>
        [Parameter]
        public Thing CurrentThing { get; set; }

        /// <summary>
        /// Gets the <see cref="Dictionary{TKey,TValue}"/> of parameters that have to be passed to the <see cref="DynamicComponent"/>
        /// </summary>
        private readonly Dictionary<string, object> parameters = [];

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            this.ValidateParameters();
            this.parameters["ParameterizedViewModel"] = this.ViewModel;
        }

        /// <summary>
        /// Validates that provided parameters are correct for the current component
        /// </summary>
        /// <exception cref="InvalidOperationException">If the provided <see cref="ViewModel"/> is not valid for the provided <see cref="ApplicationBaseType"/>
        /// or if the <see cref="ApplicationBaseType"/> is not a subclass of <see cref="ApplicationBase{TViewModel}"/></exception>
        private void ValidateParameters()
        {
            if (!TypeChecker.IsSubclassOfRawGeneric(this.ApplicationBaseType, typeof(ApplicationBase<>), out var viewModelType))
            {
                throw new InvalidOperationException($"The provided {nameof(this.ApplicationBaseType)} is not an ApplicationBase");
            }

            if (!viewModelType.IsInstanceOfType(this.ViewModel))
            {
                throw new InvalidOperationException($"The provided {nameof(this.ViewModel)} does not matches the required Type: Expected {viewModelType.Name}, received {this.ViewModel.GetType().Name}");
            }
        }
    }
}
