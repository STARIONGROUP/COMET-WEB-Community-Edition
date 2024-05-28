// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CategoryHierarchyDiagramViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ReferenceData.Categories
{
    using Blazor.Diagrams;
    using Blazor.Diagrams.Core.Geometry;
    using Blazor.Diagrams.Core.Models;
    using Blazor.Diagrams.Core.PathGenerators;
    using Blazor.Diagrams.Core.Routers;
    using Blazor.Diagrams.Options;

    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.ReferenceData.Categories;

    using ReactiveUI;

    /// <summary>
    /// View model for the <see cref="CategoryHierarchyDiagram" /> component
    /// </summary>
    public class CategoryHierarchyDiagramViewModel : ReactiveObject, ICategoryHierarchyDiagramViewModel
    {
        /// <summary>
        /// The svg path of arrow
        /// </summary>
        private const string SvgArrowPath = "M -0.093 17.86 V -18.5 L 29.233 -0.32 L -0.093 17.86 z M 1.407 -15.806 v 30.9715 L 26.3865 -0.32 L 1.407 -15.806 z";

        /// <summary>
        /// Backing field for <see cref="SelectedCategory" />
        /// </summary>
        private Category selectedCategory;

        /// <summary>
        /// Creates a new instance of the class <see cref="CategoryHierarchyDiagramViewModel" />
        /// </summary>
        public CategoryHierarchyDiagramViewModel()
        {
            var options = new BlazorDiagramOptions
            {
                AllowMultiSelection = false,
                Zoom =
                {
                    Enabled = false
                }
            };

            this.Diagram = new BlazorDiagram(options);
            this.Diagram.RegisterComponent<CategoryNode, CategoryNodeComponent>();
        }

        /// <summary>
        /// The selected <see cref="Category" />
        /// </summary>
        public Category SelectedCategory
        {
            get => this.selectedCategory;
            set => this.RaiseAndSetIfChanged(ref this.selectedCategory, value);
        }

        /// <summary>
        /// A collection of <see cref="Category" />
        /// </summary>
        public IEnumerable<Category> Rows { get; set; }

        /// <summary>
        /// A collection of <see cref="Category" />
        /// </summary>
        public IEnumerable<Category> SubCategories { get; set; }

        /// <summary>
        /// The categories hierarchy <see cref="Diagram" /> to display
        /// </summary>
        public BlazorDiagram Diagram { get; set; }

        /// <summary>
        /// Gets or sets the diagram dimensions
        /// </summary>
        public List<int> DiagramDimensions { get; set; } = [300, 350];

        /// <summary>
        /// Clears the current <see cref="Diagram" /> nodes
        /// </summary>
        public void ClearDiagram()
        {
            this.Diagram.Nodes.Clear();
        }

        /// <summary>
        /// Create diagram nodes and links
        /// </summary>
        public void SetupDiagram()
        {
            this.Diagram.Nodes.Clear();
            this.Diagram.SetZoom(0.7);
            var centerPosition = new Point(this.DiagramDimensions[0] / 2d, this.DiagramDimensions[1] / 2d);

            var node12 = new CategoryNode(this.SelectedCategory, centerPosition)
            {
                Title = this.SelectedCategory.Name,
                Highlighted = true
            };

            this.Diagram.Nodes.Add(node12);
            var numberOfNodes = this.Rows.Count();
            const int distanceBetweenNodes = 200;

            foreach (var row in this.Rows)
            {
                var currentIndex = this.Rows.ToList().IndexOf(row);
                var xOffset = (currentIndex - (numberOfNodes - 1) / 2) * distanceBetweenNodes;
                var position = new Point(node12.Position.X - xOffset, node12.Position.Y - distanceBetweenNodes);

                var node = new CategoryNode(row, position)
                {
                    Title = row.Name
                };

                this.Diagram.Nodes.Add(node);

                this.Diagram.Links.Add(new LinkModel(node12, node)
                {
                    Router = new OrthogonalRouter(),
                    PathGenerator = new StraightPathGenerator(),
                    TargetMarker = new LinkMarker(SvgArrowPath, 30)
                });
            }

            var numberOfSubNodes = this.SubCategories.Count();

            // add subcategories
            foreach (var subCategory in this.SubCategories)
            {
                var currentIndex = this.SubCategories.ToList().IndexOf(subCategory);
                var xOffset = (currentIndex - (numberOfSubNodes - 1) / 2) * distanceBetweenNodes;
                var position2 = new Point(node12.Position.X + xOffset, node12.Position.Y + distanceBetweenNodes);

                var node2 = new CategoryNode(subCategory, position2)
                {
                    Title = subCategory.Name
                };

                this.Diagram.Nodes.Add(node2);

                this.Diagram.Links.Add(new LinkModel(node2, node12)
                {
                    Router = new OrthogonalRouter(),
                    PathGenerator = new StraightPathGenerator(),
                    TargetMarker = new LinkMarker(SvgArrowPath, 30)
                });
            }
        }
    }
}
