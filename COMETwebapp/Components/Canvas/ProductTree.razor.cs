// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductTree.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.Canvas
{
    using COMETwebapp.Model;
    using Microsoft.AspNetCore.Components;

    public partial class ProductTree
    {
        /// <summary>
        /// The root node of the tree
        /// </summary>
        [Parameter]
        public TreeNode? RootNode { get; set; }

        /// <summary>
        /// Gets or sets if the tree should show a complete tree
        /// </summary>
        public bool ShowNodesWithGeometry { get; private set; } = false;

        /// <summary>
        /// Value in the search filter
        /// </summary>
        public string SearchValue { get; private set; } = string.Empty;

        /// <summary>
        /// Calls the StateHasChanged method to refresh this component
        /// </summary>
        public void Refresh()
        {
            this.InvokeAsync(() => this.StateHasChanged());
        }

        /// <summary>
        /// Event for when the filter on the tree changes
        /// </summary>
        /// <param name="showNodeWithGeometry">if the tree should show a complete tree or just nodes with geometry</param>
        public void OnFilterChanged(bool showNodeWithGeometry)
        {
            this.ShowNodesWithGeometry = showNodeWithGeometry;
            var fullTree = this.RootNode?.GetFlatListOfDescendants(true);
            
            if (this.ShowNodesWithGeometry && fullTree is not null)
            {
                foreach (var node in fullTree)
                {
                    node.IsDrawn = node.SceneObject.Primitive != null;
                }
            }
            else
            {
                fullTree?.ForEach(x => x.IsDrawn = true);
            }

            this.Refresh();
        }

        /// <summary>
        /// Event for when the text of the search filter is changing
        /// </summary>
        /// <param name="e">the args of the event</param>
        public void OnSearchFilterChange(ChangeEventArgs e)
        {
            this.SearchValue = e.Value as string ?? string.Empty;
            var fullTree = this.RootNode?.GetFlatListOfDescendants(true);

            if (this.SearchValue == string.Empty)
            {
                fullTree?.ForEach(x => x.IsDrawn = true);
            }
            else
            {
                fullTree?.ForEach(x =>
                {
                    if (!x.Title.Contains(this.SearchValue, StringComparison.InvariantCultureIgnoreCase))
                    {
                        x.IsDrawn = false;
                    }
                    else
                    {
                        x.IsDrawn = true;
                    }
                });
            }

            this.Refresh();
        }
    }
}
