// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleThingApplicationBase.razor.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Components.Applications
{
    using CDP4Common.CommonData;

    using COMET.Web.Common.ViewModels.Components.Applications;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Base component for any application that will need only one <see cref="Thing" />
    /// </summary>
    /// <typeparam name="TThing">A <see cref="Thing" /></typeparam>
    /// <typeparam name="TViewModel">An <see cref="ISingleThingApplicationBaseViewModel{TThing}" /></typeparam>
    public abstract partial class SingleThingApplicationBase<TThing, TViewModel>
    {
        /// <summary>
        /// The <typeparamref name="TThing" />
        /// </summary>
        [CascadingParameter]
        public Thing CurrentThing { get; set; }

        /// <summary>
        /// Handles the post-assignement flow of the <see cref="ApplicationBase{TViewModel}.ViewModel" /> property
        /// </summary>
        protected override void OnViewModelAssigned()
        {
            if (this.ViewModel != null && this.CurrentThing is TThing tthing)
            {
                this.ViewModel.CurrentThing = tthing;
            }

            base.OnViewModelAssigned();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.CurrentThing)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }
    }
}
