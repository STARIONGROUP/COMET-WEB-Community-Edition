﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EnumerationParameterTypeEditor.razor.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Components.ParameterTypeEditors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Component used to edit a <see cref="Parameter" /> defined with a <see cref="EnumerationParameterType" />
    /// </summary>
    public partial class EnumerationParameterTypeEditor
    {
        /// <summary>
        /// The unique id of the <see cref="DxDropDown" /> target
        /// </summary>
        private readonly string targetId = $"dropdown-customization-target-container-{Guid.NewGuid()}";

        /// <summary>
        /// The css selector for <see cref="targetId" />
        /// </summary>
        private string TargetIdSelector => $"#{this.targetId}";

        /// <summary>
        /// Gets or sets the <see cref="IParameterEditorBaseViewModel{T}" />
        /// </summary>
        [Parameter]
        public IParameterEditorBaseViewModel<EnumerationParameterType> ViewModel { get; set; }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (this.ViewModel is IEnumerationParameterTypeEditorViewModel vm)
            {
                this.Disposables.Add(vm.WhenAnyValue(x => x.IsOnEditMode, x => x.SelectAllChecked)
                    .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
            }

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsReadOnly,
                    x => x.ViewModel.ValueArray)
                .Subscribe(_ => this.StateHasChanged()));
        }
    }
}
