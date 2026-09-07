// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ArchiveLogin.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMET.Web.Common.Components
{
    using COMET.Web.Common.ViewModels.Components;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;

    using ReactiveUI;

    /// <summary>
    /// This component enables the user to open a read-only session against an uploaded ECSS-E-TM-10-25 Annex C3 archive
    /// </summary>
    public partial class ArchiveLogin
    {
        /// <summary>
        /// The <see cref="IArchiveLoginViewModel" />
        /// </summary>
        [Inject]
        public IArchiveLoginViewModel ViewModel { get; set; }

        /// <summary>
        /// Asserts that the archive form has rendered and its editors are interactive. Exposed to the DOM as the
        /// application-owned <c>data-app-ready</c> readiness marker the end-to-end tests wait on before typing, because
        /// on a cold Blazor circuit a DevExpress editor re-renders as it wires up and wipes anything typed too early
        /// </summary>
        private bool formReady;

        /// <summary>
        /// Method invoked after each time the component has been rendered
        /// </summary>
        /// <param name="firstRender">A value indicating whether this is the first render of the component</param>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (!this.formReady)
            {
                this.formReady = true;
                await this.InvokeAsync(this.StateHasChanged);
            }
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(
                x => x.ViewModel.AuthenticationResult,
                x => x.ViewModel.IsLoading,
                x => x.ViewModel.SelectedFile
            ).Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Handles the selection of an archive by the user
        /// </summary>
        /// <param name="args">The <see cref="InputFileChangeEventArgs" /></param>
        private void OnArchiveSelected(InputFileChangeEventArgs args)
        {
            this.ViewModel.SelectedFile = args.File;
        }

        /// <summary>
        /// Executes the login process
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private Task ExecuteLogin()
        {
            return this.ViewModel.ExecuteLogin();
        }
    }
}
