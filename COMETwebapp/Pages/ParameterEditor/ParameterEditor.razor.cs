﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterEditor.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Pages.ParameterEditor
{
    using COMETwebapp.ViewModels.Pages.ParameterEditor;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Class for the <see cref="ParameterEditor"/> razor component
    /// </summary>
    public partial class ParameterEditor
    {
        /// <summary>
        /// Gets or sets the <see cref="IParameterEditorViewModel"/>
        /// </summary>
        [Inject]
        public IParameterEditorViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked after each time the component has been rendered. Note that the component does
        /// not automatically re-render after the completion of any returned <see cref="Task"/>, because
        /// that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="OnAfterRenderAsync(bool)"/> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="OnAfterRenderAsync(bool)"/> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender"/> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                this.ViewModel.InitializeViewModel();
                this.WhenAnyValue(x => x.ViewModel.FilteredElements.CountChanged).Subscribe(_=> this.InvokeAsync(this.StateHasChanged));

                this.WhenAnyValue(x => x.ViewModel.SelectedElementFilter,
                     x => x.ViewModel.SelectedOptionFilter,
                     x => x.ViewModel.SelectedParameterTypeFilter,
                     x => x.ViewModel.SelectedStateFilter).Subscribe(_ => this.ViewModel.ApplyFilters(this.ViewModel.Elements));
            }
        }
    }
}