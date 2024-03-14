// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserManagementTable.razor.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Components.UserManagement
{
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Extensions;

    using COMETwebapp.ViewModels.Components.UserManagement.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="UserManagementTable" />
    /// </summary>
    public partial class UserManagementTable
    {
        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if a person should be created
        /// </summary>
        private bool ShouldCreatePerson { get; set; } = true;

        /// <summary>
        /// Method invoked when a custom summary calculation is required, allowing you to
        /// perform custom calculations based on the data displayed in the grid.
        /// </summary>
        /// <param name="e">A <see cref="GridCustomSummaryEventArgs" /></param>
        public static void CustomSummary(GridCustomSummaryEventArgs e)
        {
            switch (e.SummaryStage)
            {
                case GridCustomSummaryStage.Start:
                    e.TotalValue = 0;
                    break;
                case GridCustomSummaryStage.Calculate:
                    if (e.DataItem is PersonRowViewModel { IsActive: false })
                    {
                        e.TotalValue = (int)e.TotalValue + 1;
                    }

                    break;
                case GridCustomSummaryStage.Finalize:
                default:
                    break;
            }
        }

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

            this.ViewModel.OnInitialized();

            this.Disposables.Add(this.ViewModel.Rows.CountChanged.SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
            this.Disposables.Add(this.ViewModel.WhenAnyValue(x => x.Person).SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
            this.Disposables.Add(this.ViewModel.Rows.Connect().AutoRefresh().SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Method invoked when creating a new person
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        private void CustomizeEditPerson(GridCustomizeEditModelEventArgs e)
        {
            var dataItem = (PersonRowViewModel)e.DataItem;
            this.ShouldCreatePerson = e.IsNew;

            if (dataItem == null)
            {
                e.EditModel = new Person();
                this.ViewModel.Person = new Person();
                return;
            }

            e.EditModel = dataItem;
            this.ViewModel.Person = dataItem.Person.Clone(true);
        }

        /// <summary>
        /// Method that is invoked when the edit/add person model form is being saved
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        private async Task OnEditModelSaving()
        {
            if (!this.ShouldCreatePerson)
            {
                await this.ViewModel.EditingPerson();
                return;
            }

            await this.ViewModel.AddingPerson();
        }

        /// <summary>
        /// Method invoked when the summary text of a summary item is being displayed, allowing you to customize
        /// the text as needed. Override this method to modify the summary text based on specific conditions.
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeSummaryDisplayTextEventArgs" /></param>
        private static void CustomizeSummaryDisplayText(GridCustomizeSummaryDisplayTextEventArgs e)
        {
            if (e.Item.Name == "Inactive")
            {
                e.DisplayText = $"{e.Value} Inactive";
            }
        }

        /// <summary>
        /// Method invoked to highlight deprecated persons
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeElementEventArgs" /></param>
        private static void DisableDeprecatedPerson(GridCustomizeElementEventArgs e)
        {
            if (e.ElementType == GridElementType.DataRow && (bool)e.Grid.GetRowValue(e.VisibleIndex, "IsDeprecated"))
            {
                e.CssClass = "highlighted-item";
            }
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
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {
           //in this case, we don't have selectors that needs parameters from the url
        }
    }
}
