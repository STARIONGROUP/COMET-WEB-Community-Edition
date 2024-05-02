﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TextParameterTypeEditor.razor.cs" company="Starion Group S.A.">
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
    /// Component used to edit a <see cref="Parameter" /> defined with a <see cref="TextParameterType" />
    /// </summary>
    public partial class TextParameterTypeEditor
    {
        /// <summary>
        /// Gets or sets the <see cref="IParameterEditorBaseViewModel{T}" />
        /// </summary>
        [Parameter]
        public IParameterEditorBaseViewModel<TextParameterType> ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="BindValueMode" /> used for the inputs
        /// </summary>
        [Parameter]
        public BindValueMode BindValueMode { get; set; }

        private DxMemo multiLineEditor;

        /// <summary>
        /// Value asserting that the component is on edit mode
        /// </summary>
        public bool IsOnEditMode { get; set; }

        /// <summary>
        /// opens the memo popup
        /// </summary>
        private void OpenMemoPopup()
        {
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// set focus on the memo popup
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        public async Task OnShown()
        {
            await multiLineEditor.FocusAsync();
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsReadOnly,
                    x => x.ViewModel.ValueArray)
                .Subscribe(_ => this.StateHasChanged()));
        }
    }
}
