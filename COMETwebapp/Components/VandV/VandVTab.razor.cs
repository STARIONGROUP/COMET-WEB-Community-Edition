// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVTab.razor.cs" company="Starion Group S.A.">
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
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.VandV;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// The V&amp;V tab of the Requirements Editor: the set-up empty state on a model without V&amp;V, otherwise the register,
    /// with a banner when some reference data is missing.
    /// </summary>
    public partial class VandVTab
    {
        /// <summary>
        /// Gets or sets the <see cref="IVandVViewModel" />.
        /// </summary>
        [Parameter]
        public IVandVViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISessionService" />, used to assert whether the open session allows writing.
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets a value indicating whether the open session forbids any modification, as an Annex C3 archive does.
        /// </summary>
        private bool IsReadOnly => this.SessionService.IsReadOnly;

        /// <summary>
        /// Method invoked when the component is ready to start, having received its initial parameters. The tab re-renders
        /// when the snapshot is rebuilt, which every iteration reload and every V&amp;V set-up does.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.Model, x => x.ViewModel.LoadError)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }
    }
}
