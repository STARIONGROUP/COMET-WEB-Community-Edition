// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoriesTable.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
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

namespace COMETwebapp.Components.ReferenceData
{
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    
    using CDP4Common.SiteDirectoryData;
    
    using COMETwebapp.ViewModels.Components.ReferenceData;
    using COMETwebapp.Wrappers;
    
    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;
    
    using ReactiveUI;

    /// <summary>
    ///     Support class for the <see cref="CategoriesTable"/>
    /// </summary>
    public partial class CategoriesTable
    {
        /// <summary>
        ///     The <see cref="ICategoriesTableViewModel" /> for this component
        /// </summary>
        [Inject]
        public ICategoriesTableViewModel ViewModel { get; set; }

        /// <summary>
        ///     Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        ///     Method invoked to highlight deprecated categories
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeElementEventArgs"/> </param>
        private static void DisableDeprecatedCategories(GridCustomizeElementEventArgs e)
        {
            if (e.ElementType == GridElementType.DataRow && (bool)e.Grid.GetRowValue(e.VisibleIndex, "IsDeprecated"))
            {
                e.CssClass = "highlighted-item";
            }
        }

        /// <summary>
        ///     Method invoked when creating a new category
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs"/></param>
        private void CustomizeEditCategory(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (Category)e.DataItem;

            if (dataItem == null)
            {
                e.EditModel = new Category();
            }

            this.ViewModel.Category = new Category();
            this.ViewModel.SelectedPermissibleClasses = new List<ClassKindWrapper>();
            this.ViewModel.SelectedSuperCategories = new List<Category>();
        }

        /// <summary>
        ///     Method invoked to "Show/Hide Deprecated Items" 
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
        ///     Method invoked when the component is ready to start, having received its
        ///     initial parameters from its parent in the render tree.
        ///     Override this method if you will perform an asynchronous operation and
        ///     want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            await this.ViewModel.OnInitializedAsync();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnDeprecationMode)
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.ViewModel.Rows.CountChanged.Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
            this.Disposables.Add(this.ViewModel.Rows.Connect().AutoRefresh().Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Method invoked after each time the component has been rendered. Note that the component does
        /// not automatically re-render after the completion of any returned <see cref="Task"/>, because
        /// that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="ComponentBase.OnAfterRender(bool)"/> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="ComponentBase.OnAfterRender(bool)"/> and <see cref="OnAfterRenderAsync(bool)"/> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender"/> parameter to ensure that initialization work is only performed
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
    }
}
