// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OptionsTable.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.EngineeringModel
{
    using System.ComponentModel.DataAnnotations;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Extensions;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Options;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Support class for the <see cref="OptionsTable" />
    /// </summary>
    public partial class OptionsTable : SelectedDataItemBase<Option, OptionRowViewModel>
    {
        /// <summary>
        /// The <see cref="IOptionsTableViewModel" /> for this component
        /// </summary>
        [Parameter]
        [Required]
        public IOptionsTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Initialize(this.ViewModel);
            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnDeletionMode).SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Method invoked every time a row is selected
        /// </summary>
        /// <param name="row">The selected row</param>
        protected override void OnSelectedDataItemChanged(OptionRowViewModel row)
        {
            base.OnSelectedDataItemChanged(row);
            this.ShouldCreateThing = false;
            this.ViewModel.CurrentThing = row.Thing.Clone(true);
        }

        /// <summary>
        /// Method invoked before creating a new thing
        /// </summary>
        private void OnAddThingClick()
        {
            this.ShouldCreateThing = true;
            this.IsOnEditMode = true;
            this.ViewModel.CurrentThing = new Option();
            this.InvokeAsync(this.StateHasChanged);
        }

        /// <summary>
        /// Method invoked when the deletion of a thing is confirmed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task OnDeletionConfirmed()
        {
            await this.ViewModel.OnConfirmPopupButtonClick();
            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Method invoked to "Show/Hide Deprecated Items"
        /// </summary>
        /// <param name="e">The <see cref="GridCustomizeElementEventArgs" /></param>
        private static void HighlightDefaultOptionRow(GridCustomizeElementEventArgs e)
        {
            if (e.ElementType == GridElementType.DataRow && (bool)e.Grid.GetRowValue(e.VisibleIndex, nameof(OptionRowViewModel.IsDefault)))
            {
                e.CssClass = "fw-bold";
            }
        }
    }
}
