// -----------------------------------------------------------------------------------
//   <copyright file="MultiComboBox.razor.cs" company="RHEA System S.A.">
//      Copyright (c) 2023 RHEA System S.A.
// 
//      Authors: Sam Gerené, Jaime Bernar
// 
//      This file is part of AIDA
//      European Space Agency Community License – v2.4 Permissive (Type 3)
//      See LICENSE file for details
// 
//   </copyright>
// -----------------------------------------------------------------------------------

namespace COMET.Web.Common.Components
{
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the multicombobox component
    /// </summary>
    public partial class MultiComboBox<TItem>
    {
        /// <summary>
        /// Gets or sets if the checkboxes for the selected items should be drawn
        /// </summary>
        [Parameter]
        public bool ShowCheckBoxes { get; set; }

        /// <summary>
        /// Gets or sets the item template for the selected items
        /// </summary>
        [Parameter]
        public RenderFragment<TItem> RowTemplate { get; set; }

        /// <summary>
        /// Gets or sets the data of the combobox
        /// </summary>
        [Parameter]
        public IEnumerable<TItem> Data { get; set; } = Enumerable.Empty<TItem>();

        /// <summary>
        /// Gets or sets if the component should show all the fields as readonly
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
        /// Handler for when the value of the component has changed
        /// </summary>
        /// <param name="newValue">the new value</param>
        /// <returns>an asynchronous operation</returns>
        private async Task ItemSelected(TItem newValue)
        {
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
