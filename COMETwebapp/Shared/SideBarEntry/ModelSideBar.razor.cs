﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelSideBar.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Shared.SideBarEntry
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Menu entry to access to the opened <see cref="Iteration" />(s) content and to open new <see cref="Iteration" />
    /// </summary>
    public partial class ModelSideBar
    {
        /// <summary>
        /// The <see cref="IModelMenuViewModel" />
        /// </summary>
        [Inject]
        public IModelMenuViewModel ViewModel { get; set; }

        /// <summary>
        /// The <see cref="IStringTableService" />
        /// </summary>
        [Inject]
        public IStringTableService ConfigurationService { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.ViewModel);

            this.Disposables.Add(this.ViewModel.SessionService.OpenIterations.CountChanged
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnSwitchDomainMode,
                    x => x.ViewModel.IsOnOpenIterationMode)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }
    }
}
