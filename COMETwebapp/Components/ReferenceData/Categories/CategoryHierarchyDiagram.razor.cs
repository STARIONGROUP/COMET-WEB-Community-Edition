// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CategoryHierarchyDiagram.razor.cs" company="Starion Group S.A.">
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
    using System.ComponentModel.DataAnnotations;

    using Blazor.Diagrams;
    using Blazor.Diagrams.Core.Geometry;
    using Blazor.Diagrams.Core.Models;
    using Blazor.Diagrams.Core.PathGenerators;
    using Blazor.Diagrams.Core.Routers;
    using Blazor.Diagrams.Options;

    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.ReferenceData.ParameterTypes;
    using COMETwebapp.Services.Interoperability;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="ParameterTypeTable" />
    /// </summary>
    public partial class CategoryHierarchyDiagram
    {
        /// <summary>
        /// The svg path of arrow
        /// </summary>
        private const string SvgArrowPath = "M -0.093 17.86 V -18.5 L 29.233 -0.32 L -0.093 17.86 z M 1.407 -15.806 v 30.9715 L 26.3865 -0.32 L 1.407 -15.806 z";

        /// <summary>
        /// Gets or sets the <see cref="Category" />
        /// </summary>
        [Parameter]
        [Required]
        public Category Category { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IJsUtilitiesService" />
        /// </summary>
        [Inject]
        public IJsUtilitiesService JsUtilitiesService { get; set; }

        /// <summary>
        /// Gets or sets the condition to check if the diagram is on details mode
        /// </summary>
        public bool IsOnDetailsMode { get; private set; }

        /// <summary>
        /// A collection of <see cref="Category" />
        /// </summary>
        private List<Category> SubCategories { get; set; } = [];

        /// <summary>
        /// The categories hierarchy <see cref="Diagram" /> to display
        /// </summary>
        public BlazorDiagram Diagram { get; private set; }

        /// <summary>
        /// Gets or sets the diagram dimensions
        /// </summary>
        private List<int> DiagramDimensions { get; set; } = [300, 350];

        /// <summary>
        /// Method invoked after each time the component has been rendered interactively and the UI has finished
        /// updating (for example, after elements have been added to the browser DOM). Any
        /// <see cref="T:Microsoft.AspNetCore.Components.ElementReference" />
        /// fields will be populated by the time this runs.
        /// This method is not invoked during prerendering or server-side rendering, because those processes
        /// are not attached to any live browser DOM and are already complete before the DOM is updated.
        /// Note that the component does not automatically re-render after the completion of any returned
        /// <see cref="T:System.Threading.Tasks.Task" />,
        /// because that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time
        /// <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and
        /// <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                var dimensions = await this.JsUtilitiesService.GetItemDimensions(".diagram-canvas");

                if (dimensions.Length == 2)
                {
                    this.DiagramDimensions = [.. dimensions];
                }

                this.SetupDiagram();
            }
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

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
            this.Diagram.SetZoom(0.7);
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (this.Category.Iid == Guid.Empty)
            {
                return;
            }

            this.Diagram.Nodes.Clear();
            this.SubCategories = ((Category)this.Category.Original).AllDerivedCategories().Distinct().ToList();
        }

        /// <summary>
        /// Setups the current diagram, creating diagram nodes and links
        /// </summary>
        private void SetupDiagram()
        {
            this.Diagram.Nodes.Clear();

            var centerNode = new CategoryNode(this.Category)
            {
                Position = new Point(this.DiagramDimensions[0] / 2d, this.DiagramDimensions[1] / 2d),
                Title = this.Category.Name,
                Highlighted = true
            };

            this.Diagram.Nodes.Add(centerNode);
            const int distanceBetweenNodes = 200;

            var superCategoryRowYAxis = (int)(centerNode.Position.Y - distanceBetweenNodes);
            var subCategoryRowYAxis = (int)(centerNode.Position.Y + distanceBetweenNodes);
            this.GenerateNodesRow(this.Category.SuperCategory, centerNode, superCategoryRowYAxis, distanceBetweenNodes);
            this.GenerateNodesRow(this.SubCategories, centerNode, subCategoryRowYAxis, distanceBetweenNodes);
        }

        /// <summary>
        /// Generates the row of nodes to be displayed in the diagram
        /// </summary>
        /// <param name="categories">The list of categories in the row</param>
        /// <param name="centralNode">The central node to be linked with the nodes</param>
        /// <param name="y">The y axis value</param>
        /// <param name="distanceBetweenNodes">The distance between nodes, in x axis</param>
        private void GenerateNodesRow(List<Category> categories, CategoryNode centralNode, int y, int distanceBetweenNodes)
        {
            var numberOfNodes = categories.Count;

            for (var nodeIndex = 0; nodeIndex < numberOfNodes; nodeIndex++)
            {
                var row = categories[nodeIndex];
                var xOffset = (nodeIndex - (numberOfNodes - 1) / 2) * distanceBetweenNodes;
                var position = new Point(centralNode.Position.X - xOffset, y);

                var node = new CategoryNode(row, position)
                {
                    Title = categories[nodeIndex].Name
                };

                this.Diagram.Nodes.Add(node);

                var linkModel = centralNode.Position.Y < y ? new LinkModel(node, centralNode) : new LinkModel(centralNode, node);
                linkModel.Router = new OrthogonalRouter();
                linkModel.PathGenerator = new StraightPathGenerator();
                linkModel.TargetMarker = new LinkMarker(SvgArrowPath, 30);
                this.Diagram.Links.Add(linkModel);
            }
        }
    }
}
