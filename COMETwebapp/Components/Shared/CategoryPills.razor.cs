// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CategoryPills.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Components.Shared
{
    using CDP4Common.SiteDirectoryData;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Renders a list of category pills, showing at most <see cref="MaxVisible" /> and collapsing the remainder
    /// into a single "+N" pill whose tooltip lists the hidden categories. Keeps rows readable when a thing carries
    /// many categories (which would otherwise push the node text out of view).
    /// </summary>
    public partial class CategoryPills
    {
        /// <summary>
        /// The categories to render.
        /// </summary>
        [Parameter]
        public IEnumerable<Category> Categories { get; set; }

        /// <summary>
        /// Whether the pills show the category short name (when <see langword="true" />) or the full name.
        /// </summary>
        [Parameter]
        public bool UseShortName { get; set; } = true;

        /// <summary>
        /// The maximum number of individual category pills to render before collapsing the rest into a "+N" pill.
        /// </summary>
        [Parameter]
        public int MaxVisible { get; set; } = 2;

        /// <summary>
        /// Gets the display label of a category, honouring <see cref="UseShortName" />.
        /// </summary>
        /// <param name="category">The <see cref="Category" />.</param>
        /// <returns>The short name or the full name.</returns>
        private string Label(Category category)
        {
            return this.UseShortName ? category.ShortName : category.Name;
        }
    }
}
