// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVSetupDialog.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.VandV
{
    using COMETwebapp.ViewModels.Components.VandV;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// The V&amp;V set-up dialog: it lists the reference data missing in the model's library, lets the user define the stage
    /// gates, and seeds what is missing; without the permission to do so it lists what an administrator must create.
    /// </summary>
    public partial class VandVSetupDialog
    {
        /// <summary>
        /// Gets or sets the <see cref="IVandVSetupViewModel" />.
        /// </summary>
        [Parameter]
        public IVandVSetupViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets a value indicating whether there is something the user may set up, so the dialog offers to do it.
        /// </summary>
        private bool CanConfirm => this.ViewModel.NeedsSetup && this.ViewModel.CanSeed;

        /// <summary>
        /// Method invoked when the component is ready to start, having received its initial parameters. The dialog
        /// re-renders when it opens or closes, while a write runs, and when a session refresh re-checked the reference data.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.IsVisible,
                    x => x.ViewModel.IsBusy,
                    x => x.ViewModel.Check,
                    x => x.ViewModel.CanSeed,
                    x => x.ViewModel.LastError)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Seeds the missing reference data; a failure is shown through <see cref="IVandVSetupViewModel.LastError" />.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private Task SeedAsync()
        {
            return this.ViewModel.SeedAsync();
        }
    }
}
