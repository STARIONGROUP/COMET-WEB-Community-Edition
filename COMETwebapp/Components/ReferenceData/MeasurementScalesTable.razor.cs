// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementScalesTable.razor.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Components.ReferenceData
{
    using System.Threading.Tasks;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Extensions;

    using COMETwebapp.ViewModels.Components.ReferenceData;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="MeasurementScalesTable"/>
    /// </summary>
    public partial class MeasurementScalesTable
    {
        /// <summary>
        /// The <see cref="IMeasurementScalesTableViewModel" /> for this component
        /// </summary>
        [Inject]
        public IMeasurementScalesTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if a measurement scale should be created
        /// </summary>
        public bool ShouldCreateMeasurementScale { get; private set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

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
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.ViewModel.InitializeViewModel();

            this.Disposables.Add(this.ViewModel.Rows.CountChanged.SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
            this.Disposables.Add(this.ViewModel.Rows.Connect().AutoRefresh().SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
            this.Disposables.Add(this.ViewModel.WhenAnyValue(x => x.IsLoading).SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
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

        /// <summary>
        /// Method that is invoked when the edit/add measurement scale form is being saved
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private void OnEditMeasurementScaleSaving()
        {
            if (!this.ShouldCreateMeasurementScale)
            {
                // update measurement scale
            }

            // create measurement scale
        }

        /// <summary>
        /// Method invoked to highlight deprecated measurement scakes
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeElementEventArgs"/> </param>
        private static void DisableDeprecatedMeasurementScale(GridCustomizeElementEventArgs e)
        {
            if (e.ElementType == GridElementType.DataRow && (bool)e.Grid.GetRowValue(e.VisibleIndex, "IsDeprecated"))
            {
                e.CssClass = "highlighted-item";
            }
        }

        /// <summary>
        /// Method invoked when creating a new measurement scale
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditMeasurementScale(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (MeasurementScaleRowViewModel)e.DataItem;
            this.ShouldCreateMeasurementScale = e.IsNew;

            if (dataItem == null)
            {
                e.EditModel = new OrdinalScale();
                this.ViewModel.MeasurementScale = new OrdinalScale();
                return;
            }

            e.EditModel = dataItem;
            this.ViewModel.MeasurementScale = dataItem.MeasurementScale.Clone(true);
        }
    }
}
