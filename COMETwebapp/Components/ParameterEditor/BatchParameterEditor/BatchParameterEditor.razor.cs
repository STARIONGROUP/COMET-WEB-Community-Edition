// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BatchParameterEditor.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ParameterEditor.BatchParameterEditor
{
    using COMET.Web.Common.Extensions;

    using COMETwebapp.ViewModels.Components.ModelDashboard.ParameterValues;
    using COMETwebapp.ViewModels.Components.ParameterEditor.BatchParameterEditor;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Class for the component <see cref="BatchParameterEditor" />
    /// </summary>
    public partial class BatchParameterEditor
    {
        /// <summary>
        /// Gets or sets the <see cref="IBatchParameterEditorViewModel" />
        /// </summary>
        [Parameter]
        public IBatchParameterEditorViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the grid control that is being customized.
        /// </summary>
        private IGrid Grid { get; set; }

        /// <summary>
        /// Gets the condition to check if the apply button should be enabled
        /// </summary>
        private bool IsApplyButtonEnabled => this.ViewModel.ParameterTypeSelectorViewModel.SelectedParameterType != null && this.ViewModel.SelectedValueSetsRowsToUpdate.Count > 0;

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(
                    x => x.ViewModel.ParameterTypeSelectorViewModel.SelectedParameterType,
                    x => x.ViewModel.OptionSelectorViewModel.SelectedOption,
                    x => x.ViewModel.FiniteStateSelectorViewModel.SelectedActualFiniteState,
                    x => x.ViewModel.SelectedCategory,
                    x => x.ViewModel.DomainOfExpertiseSelectorViewModel.SelectedDomainOfExpertise)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Gets the group check value based on the given <see cref="ViewModel" />
        /// </summary>
        /// <param name="context">The column group context</param>
        /// <returns>The check box value</returns>
        private bool? GetGroupCheckBoxChecked(GridDataColumnGroupRowTemplateContext context)
        {
            var groupedRows = this.GetGroupDataItems(context).OfType<ParameterValueSetBaseRowViewModel>().ToList();
            var numberOfSelectedGroupedRows = groupedRows.Count(this.Grid.IsDataItemSelected);

            if (numberOfSelectedGroupedRows == groupedRows.Count)
            {
                return true;
            }

            if (numberOfSelectedGroupedRows == 0)
            {
                return false;
            }

            return null;
        }

        /// <summary>
        /// Method executed when the checkbox value has changed for a given group
        /// </summary>
        /// <param name="value">The new checkbox value</param>
        /// <param name="context">The group context</param>
        private void GroupCheckBox_CheckedChanged(bool? value, GridDataColumnGroupRowTemplateContext context)
        {
            var items = this.GetGroupDataItems(context);
            context.Grid.SelectDataItems(items, value != null && value.Value);
        }

        /// <summary>
        /// Gets the <see cref="ParameterValueSetBaseRowViewModel" />s from the selected element name group
        /// </summary>
        /// <param name="context">The <see cref="GridDataColumnGroupRowTemplateContext" /></param>
        /// <returns>A collection of data items contained in the given group</returns>
        private IEnumerable<object> GetGroupDataItems(GridDataColumnGroupRowTemplateContext context)
        {
            var groupElementName = (string)this.Grid.GetRowValue(context.VisibleIndex, nameof(ParameterValueSetBaseRowViewModel.ElementName));
            return this.ViewModel.Rows.Items.Where(x => x.ElementName == groupElementName);
        }
    }
}
