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
    using COMETwebapp.Model;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.ViewModels.Components.SystemRepresentation;
    using COMETwebapp.Components.SystemRepresentation;
    using ReactiveUI;

    /// <summary>
    ///     View model for the <see cref="ProjectPage" /> page
    /// </summary>
    public class SystemRepresentationPageViewModel : ReactiveObject, ISystemRepresentationPageViewModel
    {

        /// <summary>
        ///     The <see cref="ISystemTreeViewModel" /> for the <see cref="SystemTree" /> component
        /// </summary>
        public ISystemTreeViewModel SystemTreeViewModel { get; }

        /// <summary>
        /// All <see cref="ElementBase"> of the iteration
        /// </summary>
        public List<ElementBase> Elements { get; set; } = new List<ElementBase>();

        /// <summary>
        /// Name of the option selected
        /// </summary>
        public string? OptionSelected { get; set; }

        /// <summary>
        /// Name of the domain selected
        /// </summary>
        public DomainOfExpertise DomainSelected { get; set; }

        /// <summary>
        /// List of the names of <see cref="Option"/> available
        /// </summary>
        public List<string>? Options { get; set; }

        /// <summary>
        /// List of the names of <see cref="Option"/> available
        /// </summary>
        public List<string>? Domains { get; set; }

        /// <summary>
        /// Represents the RootNode of the tree
        /// </summary>
        public SystemNode? RootNode { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemRepresentationPageViewModel" /> class.
        /// </summary>
        /// <param name="systemTreeViewModel">The <see cref="ISystemTreeViewModel" /></param>
        public SystemRepresentationPageViewModel(ISystemTreeViewModel systemTreeViewModel)
        {
            this.SystemTreeViewModel = systemTreeViewModel;
        }

        /// <summary>
        /// Initialize <see cref="ElementBase"> list
        /// </summary>
        private void InitializeElements(ISessionAnchor session)
        {
            var iteration = session?.OpenIteration;
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
        /// <param name="option">Name of the Option selected</param>
        public void OnOptionFilterChange(string? option, ISessionAnchor session)
        {
            this.OptionSelected = option;
            this.Elements.Clear();
            this.InitializeElements(session);
            var elementsOnScene = this.CreateElementUsagesForScene(this.Elements);
            this.SystemTreeViewModel.SystemNodes.Clear();
            this.SystemTreeViewModel.SystemNodes.Add(this.RootNode);
        }

        /// <summary>
        /// Updates Elements list when a filter for option is selected
        /// </summary>
        /// <param name="option">Name of the Option selected</param>
        public void OnDomainFilterChange(DomainOfExpertise? domain, ISessionAnchor session)
        {
            this.DomainSelected = domain;
            this.Elements.Clear();
            session.SwitchDomain(domain);
            this.InitializeElements(session);
            var elementsOnScene = this.CreateElementUsagesForScene(this.Elements);
            this.SystemTreeViewModel.SystemNodes.Clear();
            this.SystemTreeViewModel.SystemNodes.Add(this.RootNode);
        }

        /// <summary>
        /// Creates the <see cref="ElementUsage"/> that need to be used fot populating the scene
        /// </summary>
        /// <param name="elements">the elements of the current <see cref="Iteration"/></param>
        /// <returns>the <see cref="ElementUsage"/> used in the scene</returns>
        private List<ElementUsage> CreateElementUsagesForScene(List<ElementBase> elements)
        {
            var topElement = elements.First();
            this.RootNode = new SystemNode(topElement.Name);
            this.CreateTreeRecursively(topElement, this.RootNode, null);

            return elements.OfType<ElementUsage>().ToList();
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
        /// <param name="session">The <see cref="ISessionAnchor" /></param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        public async Task OnInitializedAsync(ISessionAnchor session)
        {
            this.Elements.Clear();
            this.InitializeElements(session);

            this.Options = new List<string>();
            this.Domains = new List<string>();

            var engineeringModelSetup = session.GetSiteDirectory().Model.Find(m => m.Name.Equals(session.CurrentEngineeringModelName));
            var domains = session.GetModelDomains(engineeringModelSetup);
            this.Domains.AddRange(domains.Select(d => d.Name));

            var iteration = session?.OpenIteration;
            iteration?.Option.OrderBy(o => o.Name).ToList().ForEach(o => this.Options.Add(o.Name));

            session.OnSessionRefreshed += (sender, args) =>
            {
                this.Elements.Clear();
                this.InitializeElements(session);
            };
        }
    }
}
