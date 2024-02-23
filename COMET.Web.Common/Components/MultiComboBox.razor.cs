// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MultiComboBox.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Jaime Bernar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Components
{
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the multicombobox component
    /// </summary>
    public partial class MultiComboBox<TItem>
    {
        /// <summary>
        /// The last selected value of the combobox
        /// </summary>
        private TItem lastSelectedValue;

        /// <summary>
        /// Gets or sets the maximum number of chips that the combo should show
        /// </summary>
        [Parameter]
        public int MaxNumberOfChips { get; set; } = 3;

        /// <summary>
        /// Gets or sets if the checkboxes for the selected items should be drawn
        /// </summary>
        [Parameter]
        public bool ShowCheckBoxes { get; set; } = true;

        /// <summary>
        /// Gets or sets the item template for the selected items
        /// </summary>
        [Parameter]
        public RenderFragment<TItem> RowTemplate { get; set; }

        /// <summary>
        /// Gets or sets item template to show when the number of selected items is greater or equal to the <see cref="MaxNumberOfChips"/>
        /// </summary>
        [Parameter]
        public RenderFragment EditorTextTemplate { get; set; }

        /// <summary>
        /// Gets or sets the data of the combobox
        /// </summary>
        [Parameter]
        public IEnumerable<TItem> Data { get; set; } = Enumerable.Empty<TItem>();

        /// <summary>
        /// Gets or sets if the component should show all the fields as enabled/disabled.
        /// If a component is disabled, the user can't select the values within the component.
        /// </summary>
        [Parameter]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the value of the selected items
        /// </summary>
        [Parameter]
        public List<TItem> Values { get; set; } = new();

        /// <summary>
        /// Gets or sets the callback used to update the component value
        /// </summary>
        [Parameter]
        public EventCallback<List<TItem>> ValuesChanged { get; set; }
        
        /// <summary>
        /// Gets or sets if the component is read only.
        /// If a component is read only, the user can select the values but not edit them.
        /// </summary>
        [Parameter]
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Handler for when the value of the component has changed
        /// </summary>
        /// <param name="newValue">the new value</param>
        /// <returns>an asynchronous operation</returns>
        private async Task ItemSelected(TItem newValue)
        {
            this.lastSelectedValue = default;

            if(this.Values.Contains(newValue))
            {
                this.Values.Remove(newValue);
            }
            else
            {
                this.Values.Add(newValue);
            }

            await this.ValuesChanged.InvokeAsync(this.Values);
        }
    }
}
