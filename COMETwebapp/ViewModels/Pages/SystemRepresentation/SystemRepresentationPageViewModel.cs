// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemRepresentationPageViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
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
namespace COMETwebapp.ViewModels.Pages.SystemRepresentation
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.SystemRepresentation;
    using COMETwebapp.IterationServices;
    using COMETwebapp.Model;
    using COMETwebapp.Pages.SystemRepresentation;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    ///     View model for the <see cref="SystemRepresentation" /> page
    /// </summary>
    public class SystemRepresentationPageViewModel : ReactiveObject, ISystemRepresentationPageViewModel
    {
        /// <summary>
        /// All <see cref="ElementBase"> of the iteration
        /// </summary>
        public List<ElementBase> Elements { get; set; } = new List<ElementBase>();

        /// <summary>
        /// The selected option
        /// </summary>
        public Option OptionSelected { get; set; }

        /// <summary>
        /// Name of the selected domain
        /// </summary>
        public DomainOfExpertise DomainSelected { get; set; }

        /// <summary>
        /// List of the names of available <see cref="Option"/> 
        /// </summary>
        public List<string> Options { get; set; }

        /// <summary>
        /// List of the names of available <see cref="Option"/> 
        /// </summary>
        public List<string> Domains { get; set; }

        /// <summary>
        /// Gets or sets the total of domains in this <see cref="Iteration"/>
        /// </summary>
        public List<DomainOfExpertise> TotalDomains { get; private set; }

        /// <summary>
        /// Represents the RootNode of the tree
        /// </summary>
        public SystemNode RootNode { get; set; }

        /// <summary>
        /// Injected property to get access to <see cref="ISessionAnchor"/>
        /// </summary>
        private readonly ISessionAnchor SessionAnchor;

        /// <summary>
        ///     The <see cref="IIterationService" />
        /// </summary>
        private readonly IIterationService iterationService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemRepresentationPageViewModel" /> class.
        /// </summary>
        /// <param name="systemTreeViewModel">The <see cref="ISystemTreeViewModel" /></param>
        /// <param name="elementDefinitionDetailsViewModel">The <see cref="IElementDefinitionDetailsViewModel" /></param>
        /// <param name="iterationService">The <see cref="IIterationService" /></param>
        /// <param name="sessionAnchor">The <see cref="ISessionAnchor" /></param>
        public SystemRepresentationPageViewModel(ISystemTreeViewModel systemTreeViewModel, IElementDefinitionDetailsViewModel elementDefinitionDetailsViewModel, IIterationService iterationService, ISessionAnchor sessionAnchor)
        {
            this.iterationService = iterationService;
            this.SessionAnchor = sessionAnchor;

            this.SystemTreeViewModel = new SystemTreeViewModel
            {
                OnClick = new EventCallbackFactory().Create<SystemNode>(this, this.SelectElement)
            };

            this.ElementDefinitionDetailsViewModel = elementDefinitionDetailsViewModel;
        }

        /// <summary>
        ///     The <see cref="ISystemTreeViewModel" /> for the <see cref="SystemTree" /> component
        /// </summary>
        public ISystemTreeViewModel SystemTreeViewModel { get; }

        /// <summary>
        ///     The <see cref="IElementDefinitionDetailsViewModel" />
        /// </summary>
        public IElementDefinitionDetailsViewModel ElementDefinitionDetailsViewModel { get; }

        /// <summary>
        /// Initialize <see cref="ElementBase"> list
        /// </summary>
        private void InitializeElements()
        {
            var iteration = this.SessionAnchor?.OpenIteration;
            if (iteration != null)
            {
                if (iteration.TopElement != null)
                {
                    this.Elements.Add(iteration.TopElement);
                }
                iteration.Element.ForEach(e => this.Elements.AddRange(e.ContainedElement));
            }
        }

        /// <summary>
        /// Updates Elements list when a filter for option is selected
        /// </summary>
        /// <param name="option">Name of the selected Option</param>
        public void OnOptionFilterChange(string? option)
        {
            this.Elements.Clear();

            var iteration = this.SessionAnchor?.OpenIteration;
            var totalOptions = iteration?.Option.OrderBy(o => o.Name).ToList();
            this.OptionSelected = totalOptions?.FirstOrDefault(o => o.Name == option);
            
            var nestedElements = this.iterationService.GetNestedElementsByOption(this.SessionAnchor.OpenIteration, this.OptionSelected.Iid);
      
            var associatedElements = new List<ElementUsage>();
            nestedElements.ForEach(element =>
            {
                associatedElements.AddRange(element.ElementUsage);
            });
            associatedElements = associatedElements.Distinct().ToList();

            var elementsToRemove = new List<ElementBase>();
            this.Elements.ForEach(e =>
            {
                if (e.GetType().Equals(typeof(ElementUsage)) && !associatedElements.Contains(e))
                {
                    elementsToRemove.Add(e);
                }
            });
            this.Elements.RemoveAll(e => elementsToRemove.Contains(e));

            this.InitializeElements();
            this.CreateElementUsages(this.Elements);
            this.SystemTreeViewModel.SystemNodes = new List<SystemNode> { this.RootNode };
        }

        /// <summary>
        /// Updates Elements list when a filter for domain is selected
        /// </summary>
        /// <param name="domain">Name of the selected Domain</param>
        public void OnDomainFilterChange(string? domain)
        {
            if (domain != "All")
            {
                this.DomainSelected = this.TotalDomains.FirstOrDefault(d => d.Name == domain);
            }
            this.Elements.Clear();
            this.InitializeElements();
            this.CreateElementUsages(this.Elements);
            this.SystemTreeViewModel.SystemNodes = new List<SystemNode> { this.RootNode };
            this.DomainSelected = null;
        }
        
        /// <summary>
        /// Creates the <see cref="ElementUsage"/> used for the system tree nodes
        /// </summary>
        /// <param name="elements">the elements of the current <see cref="Iteration"/></param>
        private void CreateElementUsages(List<ElementBase> elements)
        {
            var topElement = elements.First();
            this.RootNode = new SystemNode(topElement.Name);
            this.CreateTreeRecursively(topElement, this.RootNode, null);
        }

        /// <summary>
        /// Creates the tree in a recursive way
        /// </summary>
        /// <param name="elementBase"></param>
        /// <param name="current"></param>
        /// <param name="parent"></param>  
        private void CreateTreeRecursively(ElementBase elementBase, SystemNode current, SystemNode? parent)
        {
            List<ElementUsage> childsOfElementBase = null;

            if (elementBase is ElementDefinition elementDefinition)
            {
                childsOfElementBase = DomainSelected != null ? elementDefinition.ContainedElement.Where(e => e.Owner == this.DomainSelected).ToList() : elementDefinition.ContainedElement;
            }
            else if (elementBase is ElementUsage elementUsage)
            {
                childsOfElementBase = DomainSelected != null ? elementUsage.ElementDefinition.ContainedElement.Where(e => e.Owner == this.DomainSelected).ToList() : elementUsage.ElementDefinition.ContainedElement;
            }

            if (childsOfElementBase is not null)
            {
                if (parent is not null)
                {
                    parent.AddChild(current);
                }

                foreach (var child in childsOfElementBase)
                {
                    this.CreateTreeRecursively(child, new SystemNode(child.Name), current);               
                }
            }
        }

        /// <summary>
        ///     Method invoked when the component is ready to start, having received its
        ///     initial parameters from its parent in the render tree.
        ///     Override this method if you will perform an asynchronous operation and
        ///     want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        public void OnInitializedAsync()
        {
            this.Elements.Clear();
            this.InitializeElements();

            this.Options = new List<string>();
            this.Domains = new List<string> { "All" };

            var engineeringModelSetup = this.SessionAnchor.GetSiteDirectory().Model.Find(m => m.Name.Equals(this.SessionAnchor.CurrentEngineeringModelName));
            this.TotalDomains = this.SessionAnchor.GetModelDomains(engineeringModelSetup).ToList();
            this.Domains.AddRange(this.TotalDomains.Select(d => d.Name));

            var iteration = this.SessionAnchor?.OpenIteration;
            iteration?.Option.OrderBy(o => o.Name).ToList().ForEach(o => this.Options.Add(o.Name));

            this.SessionAnchor.OnSessionRefreshed += (sender, args) =>
            {
                this.Elements.Clear();
                this.InitializeElements();
            };
        }

        /// <summary>
        ///     set the selected <see cref="SystemNode" />
        /// </summary>
        /// <param name="selectedNode">The selected <see cref="SystemNode" /></param>
        /// <returns>A <see cref="Task" /></returns>
        private void SelectElement(SystemNode selectedNode)
        {
            this.ElementDefinitionDetailsViewModel.SelectedSystemNode = this.Elements?.FirstOrDefault(e => e.Name.Equals(selectedNode.Title));
        }
    }
}
