// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Viewer.razor.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Pages.Viewer
{
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Components.Canvas;
    using COMETwebapp.Interoperability;
    using COMETwebapp.IterationServices;
    using COMETwebapp.Model;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Utilities;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="Viewer"/>
    /// </summary>
    public partial class ViewerPage
    {
        /// <summary>
        /// Gets or sets the property used for the Interoperability
        /// </summary>
        [Inject]
        public IJSInterop JSInterop { get; set; }

        /// <summary>
        /// The reference to the <see cref="CanvasComponent"/> component
        /// </summary>
        public CanvasComponent? CanvasComponent { get; set; }

        /// <summary>
        /// Gets or sets the reference of the <see cref="ProductTree"/>
        /// </summary>
        public ProductTree? ProductTree { get; set; }

        /// <summary>
        /// The filter on option
        /// </summary>
        [Parameter]
        [SupplyParameterFromQuery]
        public Guid? FilterOption { get; set; }

        /// <summary>
        /// All <see cref="ElementBase"> of the iteration
        /// </summary>
        public List<ElementBase> Elements { get; set; } = new List<ElementBase>();

        /// <summary>
        /// All the <see cref="ElementUsage"/> that are on the 3D Scene
        /// </summary>
        public List<ElementUsage> ElementUsagesOnScreen { get; set; } = new List<ElementUsage>();

        /// <summary>
        /// Gets or sets the current selected <see cref="Option"/>
        /// </summary>
        public Option SelectedOption { get; private set; }

        /// <summary>
        /// Gets or sets the total of options in this <see cref="Iteration"/>
        /// </summary>
        public List<Option> TotalOptions { get; private set; }

        /// <summary>
        /// Injected property to get access to <see cref="ISessionAnchor"/>
        /// </summary>
        [Inject]
        public ISessionAnchor SessionAnchor { get; set; }

        /// <summary>
        /// Injected property to get access to <see cref="IIterationService"/>
        /// </summary>
        [Inject]
        public IIterationService? IterationService { get; set; }

        /// <summary>
        /// List of the of <see cref="ActualFiniteStateList"/> 
        /// </summary>        
        public List<ActualFiniteStateList>? ListActualFiniteStateLists { get; set; }

        /// <summary>
        /// Gets or sets the Selected <see cref="ActualFiniteState"/>
        /// </summary>
        public List<ActualFiniteState> SelectedActualFiniteStates { get; private set; }

        /// <summary>
        /// Represents the RootNode of the tree
        /// </summary>
        public TreeNode RootNode { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISelectionMediator"/>
        /// </summary>
        [Inject]
        public ISelectionMediator SelectionMediator { get; set; }

        /// <summary>
        /// Method invoked after each time the component has been rendered. Note that the component does
        /// not automatically re-render after the completion of any returned <see cref="Task"/>, because
        /// that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see cref="OnAfterRenderAsync(bool)"/> has been invoked
        /// on this component instance; otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="OnAfterRenderAsync(bool)"/> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender"/> parameter to ensure that initialization work is only performed
        /// once.
        /// </remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                this.Elements = this.InitializeElements();
                var elementUsages = this.Elements.OfType<ElementUsage>().ToList();

                var iteration = this.SessionAnchor?.OpenIteration;
                this.TotalOptions = iteration?.Option.OrderBy(o => o.Name).ToList();
                var defaultOption = this.SessionAnchor?.OpenIteration?.DefaultOption;
                this.SelectedOption = defaultOption != null ? defaultOption : this.TotalOptions.First();
                this.ListActualFiniteStateLists = iteration?.ActualFiniteStateList?.ToList();
                this.SelectedActualFiniteStates = this.ListActualFiniteStateLists.SelectMany(x => x.ActualState).Where(x => x.IsDefault).ToList();

                await this.RepopulateScene(elementUsages);

                this.CreateTree(this.Elements);

                this.SessionAnchor.OnSessionRefreshed += async (sender, args) =>
                {
                    this.Elements = this.InitializeElements();
                    var elementsOnScene = this.Elements.OfType<ElementUsage>().ToList();
                    await this.RepopulateScene(elementsOnScene);
                };

                this.SelectionMediator.OnModelSelectionChanged += (sender, sceneObject) =>
                {
                    var treeNodes = this.RootNode.GetFlatListOfDescendants();
                    treeNodes.ForEach(x => x.IsSelected = false);
                    if(sceneObject != null)
                    {
                        var node = treeNodes.FirstOrDefault(x => x.SceneObject == sceneObject);
                        if (node is not null)
                        {
                            node.IsSelected = true;
                        }
                    } 
                    this.Refresh();
                };

                await this.InvokeAsync(() => this.StateHasChanged());
            }
        }

        /// <summary>
        /// Initialize <see cref="ElementBase"> list
        /// </summary>
        private List<ElementBase> InitializeElements()
        {
            var elements = new List<ElementBase>();
            var iteration = this.SessionAnchor?.OpenIteration;
            if (iteration != null)
            {
                if (iteration.TopElement != null)
                {
                    elements.Add(iteration.TopElement);
                }
                iteration.Element.ForEach(e => elements.AddRange(e.ContainedElement));              
            }
            return elements;
        }

        /// <summary>
        /// Creates the product tree
        /// </summary>
        /// <param name="elementUsages">the element usages used in the product tree</param>
        private void CreateTree(List<ElementBase> elementUsages)
        {
            //Order sceneObjects by the name
            var topElement = elementUsages.First();
            var topSceneObject = this.GetSceneObjectByElementBase(topElement);
            this.RootNode = new TreeNode(topSceneObject);
            this.CreateTreeRecursively(topElement, this.RootNode, null);
            this.RootNode.OrderAllDescendantsByShortName();
        }

        /// <summary>
        /// Creates the tree in a recursive way
        /// </summary>
        /// <param name="elementBase"></param>
        /// <param name="current"></param>
        /// <param name="parent"></param>
        private void CreateTreeRecursively(ElementBase elementBase, TreeNode current, TreeNode? parent)
        {
            List<ElementUsage> childsOfElementBase = null;

            if (elementBase is ElementDefinition elementDefinition)
            {
                childsOfElementBase = elementDefinition.ContainedElement;
            }
            else if (elementBase is ElementUsage elementUsage)
            {
                childsOfElementBase = elementUsage.ElementDefinition.ContainedElement;
            }

            if (childsOfElementBase is not null)
            {
                current.Parent = parent;
                if (parent is not null)
                {
                    parent.Children.Add(current);
                }

                foreach (var child in childsOfElementBase)
                {
                    var sceneObject = this.GetSceneObjectByElementBase(child);
                    if (sceneObject is not null)
                    {
                        this.CreateTreeRecursively(child, new TreeNode(sceneObject), current);
                    }
                }
            }
        }

        /// <summary>
        /// Tries to get a scene object from the specified element base
        /// </summary>
        /// <param name="elementBase"></param>
        /// <returns></returns>
        private SceneObject? GetSceneObjectByElementBase(ElementBase elementBase)
        {
            if (elementBase is ElementDefinition elementDefinition)
            {
                return this.CanvasComponent.GetAllSceneObjects().FirstOrDefault(x => x.ElementUsage.ElementDefinition.Iid == elementDefinition.Iid, new SceneObject(null));
            }
            else if (elementBase is ElementUsage elementUsage)
            {
                return this.CanvasComponent.GetAllSceneObjects().FirstOrDefault(x => x.ElementUsage.Iid == elementUsage.Iid, new SceneObject(null));
            }

            return new SceneObject(null);
        }

        /// <summary>
        /// Repopulates the scene with the specified <see cref="ElementUsage"/>
        /// </summary>
        /// <param name="elementUsages">the element usages to populate the scene with</param>
        private async Task<List<SceneObject>> RepopulateScene(List<ElementUsage> elementUsages)
        {
            return await this.CanvasComponent?.RepopulateScene(elementUsages, this.SelectedOption, this.SelectedActualFiniteStates);
        }

        /// <summary>
        /// Updates Elements list when a filter for option is selected
        /// </summary>
        /// <param name="option">Name of the Option selected</param>
        public async void OnOptionFilterChange(string? option)
        {
            var defaultOption = this.SessionAnchor?.OpenIteration?.DefaultOption;
            this.SelectedOption = this.TotalOptions.FirstOrDefault(x => x.Name == option, defaultOption);

            this.Elements = this.InitializeElements();
            var elementsOnScene = this.Elements.OfType<ElementUsage>().ToList();
            await this.RepopulateScene(elementsOnScene);
            await this.InvokeAsync(() => this.StateHasChanged());
        }

        /// <summary>
        /// Event raised when an actual finite state has changed
        /// </summary>
        /// <param name="selectedActiveFiniteStates"></param>
        public async void ActualFiniteStateChanged(List<ActualFiniteState> selectedActiveFiniteStates)
        {
            this.Elements = this.InitializeElements();
            this.SelectedActualFiniteStates = selectedActiveFiniteStates;
            var elementsOnScene = this.Elements.OfType<ElementUsage>().ToList();
            await this.RepopulateScene(elementsOnScene);
        }

        /// <summary>
        /// Calls the StateHasChanged method to refresh the view
        /// </summary>
        public void Refresh()
        {
            this.InvokeAsync(() => this.StateHasChanged());
        }
    }
}
