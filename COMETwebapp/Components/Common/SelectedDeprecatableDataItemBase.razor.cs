﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectedDeprecatableDataItemBase.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.Common
{
    using CDP4Common.CommonData;

    using COMET.Web.Common.Extensions;

    using COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable;
    using COMETwebapp.ViewModels.Components.Common.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="SelectedDeprecatableDataItemBase{T,TRow}" />
    /// </summary>
    public abstract partial class SelectedDeprecatableDataItemBase<T, TRow> : SelectedDataItemBase<T, TRow> where T : Thing, IShortNamedThing, INamedThing, IDeprecatableThing where TRow : DeprecatableDataItemRowViewModel<T>
    {
        /// <summary>
        /// The <see cref="IDeprecatableDataItemTableViewModel{T,TRow}" /> for this component
        /// </summary>
        private IDeprecatableDataItemTableViewModel<T, TRow> ViewModel { get; set; }

        /// <summary>
        /// Method invoked to "Show/Hide Deprecated Items"
        /// </summary>
        public void HideOrShowDeprecatedItems()
        {
            if (this.ViewModel.ShowHideDeprecatedThingsService.ShowDeprecatedThings)
            {
                this.Grid.ClearFilter();
            }
            else
            {
                this.Grid.FilterBy("IsDeprecated", GridFilterRowOperatorType.Equal, false);
            }
        }

        /// <summary>
        /// Method used to initialize the <see cref="ViewModel"/>
        /// </summary>
        protected void Initialize(IDeprecatableDataItemTableViewModel<T, TRow> viewModel)
        {
            this.ViewModel = viewModel;
            base.Initialize(this.ViewModel);

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnDeprecationMode).SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Method invoked after each time the component has been rendered. Note that the component does
        /// not automatically re-render after the completion of any returned <see cref="Task" />, because
        /// that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="ComponentBase.OnAfterRender(bool)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="ComponentBase.OnAfterRender(bool)" /> and <see cref="OnAfterRenderAsync(bool)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.ShowHideDeprecatedThingsService.ShowDeprecatedThings)
                    .Subscribe(_ => this.HideOrShowDeprecatedItems()));
            }
        }

        /// <summary>
        /// Method invoked to highlight deprecated a thing
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeElementEventArgs" /> </param>
        protected static void DisableDeprecatedThing(GridCustomizeElementEventArgs e)
        {
            if (e.ElementType == GridElementType.DataRow && (bool)e.Grid.GetRowValue(e.VisibleIndex, "IsDeprecated"))
            {
                e.CssClass = "highlighted-item";
            }
        }
    }
}
