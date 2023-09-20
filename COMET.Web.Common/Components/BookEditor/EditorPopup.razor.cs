// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="InputEditor.razor.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Components.BookEditor
{
    using CDP4Common.Helpers;

    using COMET.Web.Common.Services;
    using COMET.Web.Common.ViewModels.Components.BookEditor;

    using Microsoft.AspNetCore.Components;
    
    /// <summary>
    /// Support class for the BookEditorPopup component
    /// </summary>
    public partial class EditorPopup
    {
        /// <summary>
        /// Gets or sets the <see cref="IEditorPopupViewModel"/>
        /// </summary>
        [Parameter]
        public IEditorPopupViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        ///
        /// Override this method if you will perform an asynchronous operation and
        /// want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            this.Disposables.Add(this.ViewModel.ValidationErrors.Connect().Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Handler for when the confirm button has been clicked
        /// </summary>
        private async Task OnConfirmClick()
        {
            var validationErrors = ValidationService.ValidateThing(this.ViewModel.Item);

            this.ViewModel.ValidationErrors.Edit(inner =>
            {
                inner.Clear();
                inner.AddRange(validationErrors);
            });

            if (!this.ViewModel.ValidationErrors.Items.Any())
            {
                await this.ViewModel.OnConfirmClick.InvokeAsync();
            }
        }
    }
}
