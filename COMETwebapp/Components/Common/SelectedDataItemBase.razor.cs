﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SelectedDataItemBase.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.Common
{
    using CDP4Common.CommonData;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Extensions;

    using COMETwebapp.ViewModels.Components.Common.BaseDataItemTable;
    using COMETwebapp.ViewModels.Components.Common.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="SelectedDataItemBase{T,TRow}" />
    /// </summary>
    public abstract class SelectedDataItemBase<T, TRow> : DisposableComponent where T : Thing where TRow : BaseDataItemRowViewModel<T>
    {
        /// <summary>
        /// The <see cref="IBaseDataItemTableViewModel{T,TRow}" /> for this component
        /// </summary>
        private IBaseDataItemTableViewModel<T, TRow> ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if a thing should be created
        /// </summary>
        public bool ShouldCreateThing { get; protected set; }

        /// <summary>
        /// Gets or sets the condition to check if the current component is on edit mode
        /// </summary>
        public bool IsOnEditMode { get; protected set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        protected IGrid Grid { get; set; }

        /// <summary>
        /// Method used to initialize the <see cref="ViewModel" />
        /// </summary>
        protected void Initialize(IBaseDataItemTableViewModel<T, TRow> viewModel)
        {
            this.ViewModel = viewModel;
            this.ViewModel.InitializeViewModel();

            this.Disposables.Add(this.ViewModel.WhenAnyValue(
                    x => x.IsLoading,
                    x => x.CurrentThing)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.ViewModel.Rows.CountChanged.SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
            this.Disposables.Add(this.ViewModel.Rows.Connect().AutoRefresh().SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Method invoked when creating a new thing
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        protected virtual void CustomizeEditThing(GridCustomizeEditModelEventArgs e)
        {
        }

        /// <summary>
        /// Method that is invoked when the edit/add thing form is being saved
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected virtual Task OnEditThingSaving()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Method invoked every time a row is selected
        /// </summary>
        /// <param name="row">The selected row</param>
        protected virtual void OnSelectedDataItemChanged(TRow row)
        {
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Method invoked whenever a form is saved
        /// </summary>
        protected void OnSaved()
        {
            if (!this.ShouldCreateThing)
            {
                return;
            }

            this.ShouldCreateThing = false;
            var createdRow = this.ViewModel.Rows.Items.FirstOrDefault(x => x.Thing.Iid == this.ViewModel.CurrentThing.Iid);

            if (createdRow is null)
            {
                return;
            }

            this.OnSelectedDataItemChanged(createdRow);
        }
    }
}
