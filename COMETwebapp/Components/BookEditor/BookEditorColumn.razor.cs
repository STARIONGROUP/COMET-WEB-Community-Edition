// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BookEditorColumn.razor.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Components.BookEditor
{
    using CDP4JsonSerializer.JsonConverter;

    using COMETwebapp.Services.Interoperability;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the BookEditorColumn component
    /// </summary>
    public partial class BookEditorColumn<TItem>
    {
        /// <summary>
        /// Gets or sets the <see cref="IDomDataService"/>
        /// </summary>
        [Inject]
        public IDomDataService DomDataService { get; set; }

        /// <summary>
        /// Gets or sets the class to use for the collapse button
        /// </summary>
        [Parameter]
        public string CollapseButtonIconClass { get; set; }

        /// <summary>
        /// Gets or sets if the lines should be drawn in the right side
        /// </summary>
        [Parameter]
        public bool LinesOnRight { get; set; }

        /// <summary>
        /// Gets or sets if the first horizontal line should be the half width
        /// </summary>
        [Parameter]
        public bool HorizontalLineHalfWidth { get; set; }

        /// <summary>
        /// Gets or sets the title of the header
        /// </summary>
        [Parameter]
        public string HeaderTitle { get; set; }

        /// <summary>
        /// Gets or sets the color of the header in hexadecimal format
        /// </summary>
        [Parameter]
        public string HeaderHexColor { get; set; }

        /// <summary>
        /// Gets or sets if the column is collapsed.
        /// </summary>
        [Parameter]
        public bool IsCollapsed { get; set; }
        
        /// <summary>
        /// Gets or set the collection of items
        /// </summary>
        [Parameter]
        public ICollection<TItem> Items { get; set; } = new List<TItem>();

        /// <summary>
        /// Gets or sets the selected value
        /// </summary>
        [Parameter]
        public TItem SelectedValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EventCallback"/> for when the <see cref="SelectedValue"/> changes
        /// </summary>
        [Parameter]
        public EventCallback<TItem> SelectedValueChanged { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EventCallback"/> for when a new item was requested
        /// </summary>
        [Parameter]
        public EventCallback OnCreateNewItemClick { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EventCallback"/> for when the user request to collapse the column
        /// </summary>
        [Parameter]
        public EventCallback OnCollapseClicked { get; set; }

        /// <summary>
        /// Gets or sets the content template to render if any
        /// </summary>
        [Parameter]
        public RenderFragment<TItem> ContentTemplate { get; set; }

        private float[] firstItemSizeAndPosition = {};

        private float[] sizeAndPosition = {};

        [Parameter]
        public string CssClass { get; set; }

        /// <summary>
        /// Hanlder for when the selected value changes
        /// </summary>
        /// <param name="item">the item selected</param>
        /// <returns>an asynchronous operation</returns>
        private async Task OnSelectedValueChanged(TItem item, int itemIndex)
        {
            try
            {
                this.firstItemSizeAndPosition = await this.DomDataService.GetElementSizeAndPosition(0, this.CssClass);
                this.sizeAndPosition = await this.DomDataService.GetElementSizeAndPosition(itemIndex, this.CssClass);
                await this.SelectedValueChanged.InvokeAsync(item);
            }
            catch (Exception ex) 
            {

            }
        }

        /// <summary>
        /// Generate the path points for the polyline
        /// </summary>
        /// <returns>the path</returns>
        private string GeneratePathPoints()
        {
            if (this.sizeAndPosition.Length < 4 || this.firstItemSizeAndPosition.Length < 4)
            {
                return string.Empty;
            }

            var left = this.sizeAndPosition[0];
            var top = this.sizeAndPosition[1];
            var width = this.sizeAndPosition[2];
            var height = this.sizeAndPosition[3];

            var finalTop = (int)(this.firstItemSizeAndPosition[1] + this.firstItemSizeAndPosition[3] / 2.0f);

            var x1 = (int)(width*0.6);
            var y1 = (int)(top + height/2.0f);
            var x2 = (int)(width*0.9);
            var y2 = (int)(top + height / 2.0f);
            var x3 = (int)(width*0.9);
            var y3 = finalTop;
            var x4 = (int)(width);
            var y4 = finalTop;

            return $"{x1},{y1},{x2},{y2},{x3},{y3},{x4},{y4}";
        }
    }
}
