// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="Publications.razor.cs" company="RHEA System S.A.">
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
namespace COMET.Web.Common.Components.Publications
{
    using COMET.Web.Common.ViewModels.Components.Publications;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Support class for the Publications component
    /// </summary>
    public partial class Publications
    {
        /// <summary>
        /// Gets or sets the <see cref="IPublicationsViewModel"/>
        /// </summary>
        [Inject]
        public IPublicationsViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.ViewModel.Rows.Connect()
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(x=>x.ViewModel.CanPublish)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }
    }
}
