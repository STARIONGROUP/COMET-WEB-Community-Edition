// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SampledFunctionParameterTypeEditor.razor.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Components.ParameterTypeEditors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Component used to edit a <see cref="Parameter" /> defined with a <see cref="SampledFunctionParameterType" />
    /// </summary>
    public partial class SampledFunctionParameterTypeEditor
    {
        /// <summary>
        /// Gets or sets the <see cref="IParameterEditorBaseViewModel{T}" />
        /// </summary>
        [Parameter]
        public IParameterEditorBaseViewModel<SampledFunctionParameterType> ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="BindValueMode" /> used for the inputs
        /// </summary>
        [Parameter]
        public BindValueMode BindValueMode { get; set; } = BindValueMode.OnLostFocus;

        /// <summary>
        /// Value asserting that the component is on edit mode
        /// </summary>
        [Parameter]
        public bool IsOnEditMode { get; set; }

        /// <summary>
        /// <see cref="EventCallback" /> after cancelling edition
        /// </summary>
        [Parameter]
        public EventCallback OnAfterCancel { get; set; }

        /// <summary>
        /// <see cref="EventCallback"/> after edtiting
        /// </summary>
        [Parameter]
        public EventCallback OnAfterEdit { get; set; }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsReadOnly,
                x => x.ViewModel.ValueArray).Subscribe(_ => this.StateHasChanged()));
        }

        /// <summary>
        /// Updates values for the <see cref="SampledFunctionParameterType"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task UpdateValues()
        {
            await this.ViewModel.OnParameterValueChanged(null);
            await this.OnAfterEdit.InvokeAsync();
        }

        /// <summary>
        /// Cancel updates for the <see cref="SampledFunctionParameterType"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private Task CancelUpdate()
        {
            (this.ViewModel as ISampledFunctionParameterTypeEditorViewModel)?.ResetChanges();
            return this.OnAfterCancel.InvokeAsync();
        }
    }
}
