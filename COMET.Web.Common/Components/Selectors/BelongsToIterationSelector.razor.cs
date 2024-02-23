// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BelongsToIterationSelector.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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
    /// Base component for selector that belongs to an <see cref="Iteration" />
    /// </summary>
    /// <typeparam name="TViewModel">An <see cref="IBelongsToIterationSelectorViewModel" /></typeparam>
    public abstract partial class BelongsToIterationSelector<TViewModel>
    {
        /// <summary>
        /// The <typeparamref name="TViewModel" />
        /// </summary>
        [Parameter]
        public TViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            this.Disposables.Add(this.ViewModel);

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.CurrentIteration)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }
    }
}
