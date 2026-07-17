// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SystemRepresentationBody.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.SystemRepresentation
{
    using COMET.Web.Common.Components.Applications;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Utilities;

    using COMETwebapp.Utilities;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    using ReactiveUI;

    /// <summary>
    /// Core component for the System Representation application
    /// </summary>
    public partial class SystemRepresentationBody
    {
        /// <summary>
        /// Gets or sets the <see cref="IJSRuntime" /> used to initialise the column resizer.
        /// </summary>
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        /// <summary>
        /// Initialises the column-resizer JS interop on first render.
        /// </summary>
        /// <param name="firstRender"><see langword="true" /> on the first render cycle.</param>
        /// <returns>A <see cref="Task" />.</returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                try
                {
                      await this.JsRuntime.InvokeVoidAsync("cometResizer.init", "col-resizer", "leftColumn", 260);
                }
                catch (Exception)
                {
                    // JS interop failures during pre-rendering or test environments are non-fatal.
                }
            }
        }

        /// <summary>
        /// Handles the post-assignement flow of the <see cref="ApplicationBase{TViewModel}.ViewModel" /> property
        /// </summary>
        protected override void OnViewModelAssigned()
        {
            base.OnViewModelAssigned();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.OptionSelector.SelectedOption)
                .Subscribe(_ => this.UpdateUrl()));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsLoading)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue(QueryKeys.OptionKey, out var option))
            {
                this.ViewModel.OptionSelector.SelectedOption = this.ViewModel.OptionSelector.AvailableOptions.FirstOrDefault(x => x.Iid == option.FromShortGuid());
            }
        }

        /// <summary>
        /// Sets the url of the <see cref="NavigationManager" /> based on the current values
        /// </summary>
        private void UpdateUrl()
        {
            var additionalParameters = new Dictionary<string, string>();

            if (this.ViewModel.OptionSelector.SelectedOption != null)
            {
                additionalParameters[QueryKeys.OptionKey] = this.ViewModel.OptionSelector.SelectedOption.Iid.ToShortGuid();
            }

            this.ViewModel.ApplyFilters();
            this.UpdateUrlWithParameters(additionalParameters, WebAppConstantValues.SystemRepresentationPage);
        }
    }
}
