// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewerViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.ViewModels.Pages.Viewer
{
    using CDP4Common.EngineeringModelData;
    
    using COMETwebapp.Model;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer.Canvas;
    
    using Microsoft.AspNetCore.Components;
    
    using ReactiveUI;

    /// <summary>
    /// View Model for the <see cref="COMETwebapp.Pages.Viewer.ViewerPage"/> component
    /// </summary>
    public class ViewerViewModel : ReactiveObject, IViewerViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ISessionService"/>
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISelectionMediator"/>
        /// </summary>
        [Inject]
        public ISelectionMediator SelectionMediator { get; set; }

        /// <summary>
        /// Backing field for the <see cref="SelectedOption"/>
        /// </summary>
        private Option selectedOption;

        /// <summary>
        /// Gets or sets the selected <see cref="Option"/>
        /// </summary>
        public Option SelectedOption
        {
            get => this.selectedOption;
            set => this.RaiseAndSetIfChanged(ref this.selectedOption, value);
        }

        /// <summary>
        /// Backing field for the <see cref="RootNodeViewModel"/>
        /// </summary>
        private INodeComponentViewModel rootNodeViewModel;

        /// <summary>
        /// Gets or sets the root VM of the <see cref="COMETwebapp.Components.Viewer.Canvas.ProductTree"/>
        /// </summary>
        public INodeComponentViewModel RootNodeViewModel
        {
            get => this.rootNodeViewModel;
            set => this.RaiseAndSetIfChanged(ref this.rootNodeViewModel, value);
        }

        /// <summary>
        /// Gets or sets the list of the available <see cref="Option"/>
        /// </summary>
        public List<Option> TotalOptions { get; set; }

        /// <summary>
        /// All <see cref="ElementBase"/> of the iteration
        /// </summary>
        public List<ElementBase> Elements { get; set; } = new();

        /// <summary>
        /// List of the of <see cref="ActualFiniteStateList"/> 
        /// </summary>        
        public List<ActualFiniteStateList> ListActualFiniteStateLists { get; set; }

        /// <summary>
        /// Gets or sets the Selected <see cref="ActualFiniteState"/>
        /// </summary>
        public List<ActualFiniteState> SelectedActualFiniteStates { get; private set; }

        /// <summary>
        /// Creates a new instance of type <see cref="ViewerViewModel"/>
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService"/></param>
        public ViewerViewModel(ISessionService sessionService, ISelectionMediator selectionMediator)
        {
            this.SessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.SelectionMediator = selectionMediator ?? throw new ArgumentNullException(nameof(selectionMediator));

            this.Elements = this.InitializeElements();

            var iteration = this.SessionService.DefaultIteration;
            this.TotalOptions = iteration.Option.OrderBy(o => o.Name).ToList();
            var defaultOption = this.SessionService.DefaultIteration.DefaultOption;
            this.SelectedOption = defaultOption != null ? defaultOption : this.TotalOptions.First();
            this.ListActualFiniteStateLists = iteration.ActualFiniteStateList?.ToList();
            this.SelectedActualFiniteStates = this.ListActualFiniteStateLists?.SelectMany(x => x.ActualState).Where(x => x.IsDefault).ToList();

            this.CreateTree(this.Elements);

            this.SessionService.OnSessionRefreshed += (sender, args) =>
            {
                this.Elements = this.InitializeElements();
                this.CreateTree(this.Elements);
            };

            this.SelectionMediator.OnModelSelectionChanged += (sceneObject) =>
            {
                var treeNodes = this.RootNodeViewModel.GetFlatListOfDescendants();
                treeNodes.ForEach(x => x.IsSelected = false);
                
                if (sceneObject != null)
                {
                    var node = treeNodes.FirstOrDefault(x => x.Node.SceneObject == sceneObject);
                    
                    if (node is not null)
                    {
                        node.IsSelected = true;
                    }
                }
            };

            this.WhenAnyValue(x => x.SelectedOption).Subscribe(o => this.OnOptionChange(o));
        }

        /// <summary>
        /// Initialize <see cref="ElementBase"/> list
        /// </summary>
        private List<ElementBase> InitializeElements()
        {
            var elements = new List<ElementBase>();
            var iteration = this.SessionService.DefaultIteration;

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
        /// <param name="productTreeElements">the product tree elements</param>
        private void CreateTree(IEnumerable<ElementBase> productTreeElements)
        {
            if (productTreeElements.Any())
            {
                var topElement = productTreeElements.First();
                var topSceneObject = SceneObject.Create(topElement, this.SelectedOption, this.SelectedActualFiniteStates);
                this.RootNodeViewModel = new NodeComponentViewModel(new TreeNode(topSceneObject), this.SelectionMediator);
                this.CreateTreeRecursively(topElement, this.RootNodeViewModel, null);
                this.RootNodeViewModel.OrderAllDescendantsByShortName();
            }
        }

        /// <summary>
        /// Creates the tree in a recursive way
        /// </summary>
        /// <param name="elementBase"></param>
        /// <param name="current"></param>
        /// <param name="parent"></param>
        private void CreateTreeRecursively(ElementBase elementBase, INodeComponentViewModel current, INodeComponentViewModel parent)
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
                if (parent is not null)
                {
                    parent.AddChild(current);
                }

                foreach (var child in childsOfElementBase)
                {
                    var sceneObject = SceneObject.Create(child, this.SelectedOption, this.SelectedActualFiniteStates);
                    if (sceneObject is not null)
                    {
                        var nodeViewModel = new NodeComponentViewModel(new TreeNode(sceneObject), this.SelectionMediator);
                        this.CreateTreeRecursively(child, nodeViewModel, current);
                    }
                }
            }
        }

        /// <summary>
        /// Event for when the selected <see cref="Option"/> has changed
        /// </summary>
        /// <param name="option">the new selected option</param>
        public void OnOptionChange(Option option)
        {
            var defaultOption = this.SessionService.DefaultIteration.DefaultOption;
            this.SelectedOption = this.TotalOptions.FirstOrDefault(x => x == option, defaultOption);
            this.Elements = this.InitializeElements();
            this.CreateTree(this.Elements);
        }

        /// <summary>
        /// Event raised when an actual finite state has changed
        /// </summary>
        /// <param name="selectedActiveFiniteStates"></param>
        public void ActualFiniteStateChanged(List<ActualFiniteState> selectedActiveFiniteStates)
        {
            this.SelectedActualFiniteStates = selectedActiveFiniteStates;
            this.Elements = this.InitializeElements();
            this.CreateTree(this.Elements);
        }
    }
}
