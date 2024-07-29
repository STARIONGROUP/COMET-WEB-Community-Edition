// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTable.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ParameterEditor
{
    using COMET.Web.Common.Extensions;

    using COMETwebapp.Comparer;
    using COMETwebapp.ViewModels.Components.ParameterEditor;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Class for the component <see cref="ParameterTable" />
    /// </summary>
    public partial class ParameterTable
    {
        /// <summary>
        /// <see cref="EventCallback" /> to close the popup editor
        /// </summary>
        private EventCallback closeEditor;

        /// <summary>
        /// The <see cref="ParameterBaseRowViewModelComparer" />
        /// </summary>
        private readonly ParameterBaseRowViewModelComparer comparer = new();

        /// <summary>
        /// Gets or sets the <see cref="IParameterTableViewModel" />
        /// </summary>
        [Parameter]
        public IParameterTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnEditMode)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));

            this.closeEditor = new EventCallbackFactory().Create(this, () => { this.ViewModel.IsOnEditMode = false; });
        }

        /// <summary>
        /// Customizes the table rows
        /// </summary>
        /// <param name="e">The <see cref="GridCustomizeElementEventArgs" /></param>
        private void OnCustomizeElement(GridCustomizeElementEventArgs e)
        {
            if (e.ElementType == GridElementType.DataRow)
            {
                var row = (ParameterBaseRowViewModel)this.Grid.GetDataItem(e.VisibleIndex);

                if (row.IsPublishable)
                {
                    e.CssClass = "font-weight-bold";
                }
            }

            if (e.ElementType == GridElementType.GroupCell)
            {
                var elementBaseName = (string)e.Grid.GetRowValue(e.VisibleIndex, nameof(ParameterBaseRowViewModel.ElementBaseName));
                var isPublishableParameterInGroup = this.ViewModel.Rows.Items.Any(x => x.IsPublishable && x.ElementBaseName == elementBaseName);

                if (isPublishableParameterInGroup)
                {
                    e.CssClass = "font-weight-bold";
                }
            }

            if (e.ElementType == GridElementType.DataCell && e.Column.Caption == "Value")
            {
                e.CssClass = "overflow-visible";
            }
        }
    }
}
