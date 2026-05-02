// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PersonEditForm.razor.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Components.Shared.PersonEdit
{
    using System.Collections.Specialized;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry.PersonEdit;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Code-behind for <see cref="PersonEditForm" />, the self-service profile editor surfaced from the
    /// session menu. Hosts a tabbed <c>DxFormLayout</c> bound to a <see cref="IPersonEditViewModel" />
    /// and re-renders whenever the underlying VM signals a change worth showing.
    /// </summary>
    public partial class PersonEditForm : DisposableComponent
    {
        /// <summary>
        /// Gets or sets the view model driving the form.
        /// </summary>
        [Parameter]
        public IPersonEditViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the callback invoked when the user clicks Cancel. Bound by the host popup so it
        /// can close itself.
        /// </summary>
        [Parameter]
        public EventCallback OnCancel { get; set; }

        /// <summary>
        /// Subscribes to the VM properties that influence the form's enabled / disabled state and the
        /// row collections so additions / removals re-render the e-mail and telephone tabs.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (this.ViewModel is null)
            {
                return;
            }

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.IsValid,
                    x => x.ViewModel.IsLoading,
                    x => x.ViewModel.IsPasswordEditEnabled,
                    x => x.ViewModel.GivenName,
                    x => x.ViewModel.Surname)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            this.ViewModel.EmailAddresses.CollectionChanged += this.OnRowCollectionChanged;
            this.ViewModel.TelephoneNumbers.CollectionChanged += this.OnRowCollectionChanged;
        }

        /// <summary>
        /// Detaches the row-collection handlers so the component does not leak event subscriptions when
        /// the popup is recycled.
        /// </summary>
        /// <param name="disposing">Whether managed resources should be disposed.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && this.ViewModel is not null)
            {
                this.ViewModel.EmailAddresses.CollectionChanged -= this.OnRowCollectionChanged;
                this.ViewModel.TelephoneNumbers.CollectionChanged -= this.OnRowCollectionChanged;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Re-renders the form when the user adds or removes a contact row.
        /// </summary>
        /// <param name="sender">The originating collection.</param>
        /// <param name="args">The change arguments.</param>
        private void OnRowCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            this.InvokeAsync(this.StateHasChanged);
        }
    }
}
