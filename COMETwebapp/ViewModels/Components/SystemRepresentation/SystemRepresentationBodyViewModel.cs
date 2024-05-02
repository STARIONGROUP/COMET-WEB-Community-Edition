// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SystemRepresentationBodyViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.SystemRepresentation
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.ViewModels.Components.SystemRepresentation.Rows;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View Model that handle the logic for the System Representation application
    /// </summary>
    public class SystemRepresentationBodyViewModel : SingleIterationApplicationBaseViewModel, ISystemRepresentationBodyViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleIterationApplicationBaseViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        public SystemRepresentationBodyViewModel(ISessionService sessionService, ICDPMessageBus messageBus) : base(sessionService, messageBus)
        {
            this.ProductTreeViewModel = new SystemRepresentationTreeViewModel
            {
                OnClick = new EventCallbackFactory().Create<SystemNodeViewModel>(this, this.SelectElement)
            };

            this.Disposables.Add(this.WhenAnyValue(x => x.OptionSelector.SelectedOption).Subscribe(_ => this.ApplyFilters()));
            this.InitializeSubscriptions([typeof(ElementUsage)]);
        }

        /// <summary>
        /// Gets the <see cref="IOptionSelectorViewModel" />
        /// </summary>
        public IOptionSelectorViewModel OptionSelector { get; private set; } = new OptionSelectorViewModel(false);

        /// <summary>
        /// Represents the RootNode of the tree
        /// </summary>
        public SystemNodeViewModel RootNode { get; set; }

        /// <summary>
        /// The <see cref="SystemRepresentationTreeViewModel" />
        /// </summary>
        public SystemRepresentationTreeViewModel ProductTreeViewModel { get; }

        /// <summary>
        /// The <see cref="IElementDefinitionDetailsViewModel" />
        /// </summary>
        public IElementDefinitionDetailsViewModel ElementDefinitionDetailsViewModel { get; } = new ElementDefinitionDetailsViewModel();

        /// <summary>
        /// All <see cref="ElementBase" /> of the iteration
        /// </summary>
        public List<ElementBase> Elements { get; set; } = new();

        /// <summary>
        /// set the selected <see cref="SystemNodeViewModel" />
        /// </summary>
        /// <param name="selectedNode">The selected <see cref="SystemNodeViewModel" /></param>
        /// <returns>A <see cref="Task" /></returns>
        public void SelectElement(SystemNodeViewModel selectedNode)
        {
            this.ElementDefinitionDetailsViewModel.SelectedSystemNode = this.Elements.Find(e => e.Iid == selectedNode.Thing.Iid);

            this.ElementDefinitionDetailsViewModel.Rows = this.ElementDefinitionDetailsViewModel.SelectedSystemNode switch
            {
                ElementDefinition elementDefinition => elementDefinition.Parameter.Select(x => new ElementDefinitionDetailsRowViewModel(x)).ToList(),
                ElementUsage elementUsage => elementUsage.ElementDefinition.Parameter.Select(x => new ElementDefinitionDetailsRowViewModel(x)).ToList(),
                _ => null
            };
        }

        /// <summary>
        /// Apply all the filters on the <see cref="SystemRepresentationTreeViewModel" />
        /// </summary>
        public void ApplyFilters()
        {
            if (this.CurrentThing == null)
            {
                return;
            }

            this.IsLoading = true;
            this.OnOptionFilterChange(this.OptionSelector.SelectedOption);
            this.IsLoading = false;
        }

        /// <summary>
        /// Updates Elements list when a filter for option is selected
        /// </summary>
        /// <param name="selectedOption">the selected <see cref="Option" /></param>
        public void OnOptionFilterChange(Option selectedOption)
        {
            var nestedElements = this.CurrentThing.QueryNestedElements(selectedOption).ToList();

            var associatedElements = new List<ElementUsage>();
            associatedElements.AddRange(nestedElements.SelectMany(x => x.ElementUsage));
            associatedElements = associatedElements.Distinct().ToList();

            var elementsToRemove = new List<ElementBase>();

            this.Elements.ForEach(e =>
            {
                if (e.GetType() == typeof(ElementUsage) && !associatedElements.Contains(e))
                {
                    elementsToRemove.Add(e);
                }
            });

            this.Elements.RemoveAll(e => elementsToRemove.Contains(e));

            this.InitializeElements();
            this.ProductTreeViewModel.CreateTree(this.Elements, this.OptionSelector.SelectedOption, new List<ActualFiniteState>());
        }

        /// <summary>
        /// Update this view model properties
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            await base.OnThingChanged();

            this.Elements.Clear();
            this.OptionSelector.CurrentIteration = this.CurrentThing;
            this.InitializeElements();
            this.ApplyFilters();
            this.IsLoading = false;
        }

        /// <summary>
        /// Handles the change of <see cref="DomainOfExpertise" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnDomainChanged()
        {
            await base.OnDomainChanged();

            if (this.CurrentDomain != null)
            {
                this.IsLoading = true;
                this.ApplyFilters();
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed()
        {
            if (this.AddedThings.Count == 0 && this.UpdatedThings.Count == 0 && this.DeletedThings.Count == 0)
            {
                return Task.CompletedTask;
            }

            this.IsLoading = true;

            var addedElements = this.AddedThings.OfType<ElementUsage>().ToList();
            var deletedElements = this.DeletedThings.OfType<ElementUsage>().ToList();
            var updatedElements = this.UpdatedThings.OfType<ElementUsage>().ToList();

            this.Elements.AddRange(addedElements);
            this.Elements.RemoveMany(deletedElements);

            this.ProductTreeViewModel.AddElementsToTree(addedElements, this.OptionSelector.SelectedOption, []);
            this.ProductTreeViewModel.RemoveElementsFromTree(deletedElements);
            this.ProductTreeViewModel.UpdateElementsFromTree(updatedElements);

            this.ProductTreeViewModel.RootViewModel.OrderAllDescendantsByShortName();

            this.ClearRecordedChanges();
            this.IsLoading = false;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Initialize <see cref="ElementBase" /> list
        /// </summary>
        private void InitializeElements()
        {
            if (this.CurrentThing.TopElement != null)
            {
                this.Elements.Add(this.CurrentThing.TopElement);
            }

            this.CurrentThing.Element.ForEach(e => this.Elements.AddRange(e.ContainedElement));
        }
    }
}
