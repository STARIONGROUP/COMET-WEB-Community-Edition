﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CategoryNode.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ReferenceData.Categories
{
    using Blazor.Diagrams.Core.Geometry;
    using Blazor.Diagrams.Core.Models;

    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// represent the Category diagram node
    /// </summary>
    public class CategoryNode : NodeModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryNode" /> class.
        /// </summary>
        /// <param name="category">The <see cref="Category" /></param>
        /// <param name="position">The <see cref="Point" /></param>
        public CategoryNode(Category category, Point position = null) : base(position)
        {
            this.Category = category;
        }

        /// <summary>
        /// Get or set the <see cref="Category" />
        /// </summary>
        public Category Category { get; set; }

        /// <summary>
        /// The condition to check if the node should be highlighted
        /// </summary>
        public bool Highlighted { get; set; }
    }
}
