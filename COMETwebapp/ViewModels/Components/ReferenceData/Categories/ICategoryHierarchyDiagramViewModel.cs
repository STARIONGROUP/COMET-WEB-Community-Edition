// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICategoryHierarchyDiagramViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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
namespace COMETwebapp.ViewModels.Components.ReferenceData.Categories
{
    using Blazor.Diagrams.Core;

    using CDP4Common.SiteDirectoryData;

    /// <summary>
    ///     Interface definition for <see cref="CategoryHierarchyDiagramViewModel" />
    /// </summary>
    public interface ICategoryHierarchyDiagramViewModel
    {
        /// <summary>
        ///     The selected <see cref="Category"/>
        /// </summary>
        Category SelectedCategory { get; set; }

        /// <summary>
        /// A collection of <see cref="Category" />
        /// </summary>
        IEnumerable<Category> Rows { get; set; }

        /// <summary>
        /// A collection of <see cref="Category" />
        /// </summary>
        IEnumerable<Category> SubCategories { get; set; }

        /// <summary>
        /// The categories hierarchy <see cref="Diagram" /> to display
        /// </summary>
        Diagram Diagram { get; set; }

        /// <summary>
        /// Gets or sets the diagram dimensions
        /// </summary>
        List<int> DiagramDimensions { get; set; }

        /// <summary>
        /// Create diagram nodes and links
        /// </summary>
        void SetupDiagram();
    }
}
