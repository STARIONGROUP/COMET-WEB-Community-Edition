// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SystemRepresentationBodyViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.ViewModels.Components.Common;

    using DynamicData;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// View Model that handle the logic for the System Representation application
    /// </summary>
    public class SystemRepresentationBodyViewModel : SingleIterationApplicationBaseViewModel, ISystemRepresentationBodyViewModel
    {
        /// <summary>
        /// The <see cref="ILogger{T}" /> used to record exceptions thrown by the drag-and-drop create pipeline.
        /// </summary>
        private readonly ILogger<SystemRepresentationBodyViewModel> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemRepresentationBodyViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="detailsPanelViewModel">The <see cref="IElementDetailsPanelViewModel" /> managing the editable element details panel.</param>
        /// <param name="logger">The <see cref="ILogger{T}" /> used to record drag-and-drop create-pipeline exceptions.</param>
        public SystemRepresentationBodyViewModel(ISessionService sessionService, ICDPMessageBus messageBus, IElementDetailsPanelViewModel detailsPanelViewModel, ILogger<SystemRepresentationBodyViewModel> logger) : base(sessionService, messageBus)
        {
            this.logger = logger;
            this.DetailsPanelViewModel = detailsPanelViewModel;
            this.Disposables.Add((IDisposable)this.DetailsPanelViewModel);
            this.DetailsPanelViewModel.AutoAddCreatedDefinitionAsUsage = true;

            this.ProductTreeViewModel = new SystemRepresentationTreeViewModel
            {
                OnClick = new EventCallbackFactory().Create<SystemNodeViewModel>(this, this.SelectElement),
                OnDrop = new EventCallbackFactory().Create<(SystemNodeViewModel From, SystemNodeViewModel To)>(this, this.OnElementDroppedAsync)
            };

            this.Disposables.Add(this.WhenAnyValue(x => x.OptionSelector.SelectedOption).Subscribe(option =>
            {
                this.DetailsPanelViewModel.CurrentOption = option;
                this.ApplyFilters();
                this.DetailsPanelViewModel.RefreshSelectedElement();
            }));
            this.Disposables.Add(this.WhenAnyValue(x => x.DetailsPanelViewModel.IsLoading).Subscribe(x => this.IsLoading = x));
            this.InitializeSubscriptions([typeof(ElementUsage), typeof(Parameter), typeof(ParameterOverride), typeof(ParameterSubscription), typeof(ParameterGroup), typeof(ElementDefinition)]);
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
        /// Gets the <see cref="IElementDetailsPanelViewModel" /> managing the editable element details panel.
        /// </summary>
        public IElementDetailsPanelViewModel DetailsPanelViewModel { get; }

        /// <summary>
        /// All <see cref="ElementBase" /> of the iteration
        /// </summary>
        public List<ElementBase> Elements { get; set; } = [];

        /// <summary>
        /// set the selected <see cref="SystemNodeViewModel" />
        /// </summary>
        /// <param name="selectedNode">The selected <see cref="SystemNodeViewModel" /></param>
        public void SelectElement(SystemNodeViewModel selectedNode)
        {
            var elementBase = this.Elements.Find(e => e.Iid == selectedNode.Thing.Iid);
            this.DetailsPanelViewModel.SelectElement(elementBase);
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
            this.Elements = this.Elements.DistinctBy(e => e.Iid).ToList();
            this.ProductTreeViewModel.CreateTree(this.Elements, this.OptionSelector.SelectedOption, new List<ActualFiniteState>());
        }

        /// <summary>
        /// Handles a drop event raised by the product tree: creates a new <see cref="ElementUsage" /> of the
        /// dragged node's <see cref="ElementDefinition" /> under the target node's
        /// <see cref="ElementDefinition" />.
        /// </summary>
        /// <param name="args">
        /// A tuple whose <c>From</c> member is the dragged <see cref="SystemNodeViewModel" /> and whose
        /// <c>To</c> member is the drop-target <see cref="SystemNodeViewModel" />.
        /// </param>
        /// <returns>A <see cref="Task" /> representing the asynchronous create operation.</returns>
        private async Task OnElementDroppedAsync((SystemNodeViewModel From, SystemNodeViewModel To) args)
        {
            var fromDefinition = args.From.Thing as ElementDefinition ?? (args.From.Thing as ElementUsage)?.ElementDefinition;
            var toDefinition = args.To.Thing as ElementDefinition ?? (args.To.Thing as ElementUsage)?.ElementDefinition;

            if (fromDefinition is null || toDefinition is null || this.CurrentDomain is null)
            {
                return;
            }

            this.IsLoading = true;

            try
            {
                var thingCreator = new ThingCreator();
                await thingCreator.CreateElementUsageAsync(toDefinition, fromDefinition, this.CurrentDomain, this.SessionService.Session);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error occurred while creating an Element Usage of '{From}' under '{To}' from a drag-and-drop operation", fromDefinition.ShortName, toDefinition.ShortName);
            }
            finally
            {
                this.IsLoading = false;
            }
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

            if (this.CurrentThing != null)
            {
                this.DetailsPanelViewModel.CurrentDomain = this.CurrentDomain;
                this.DetailsPanelViewModel.CurrentOption = this.OptionSelector.SelectedOption;
                this.DetailsPanelViewModel.Initialize(this.CurrentThing);
            }
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

            this.DetailsPanelViewModel.CurrentDomain = this.CurrentDomain;
            this.DetailsPanelViewModel.RefreshSelectedElement();
        }

        /// <summary>
        /// Handles the <c>SessionStatus.EndUpdate</c> message received, so that a change written by this or another
        /// open application is reflected here as well
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnEndUpdate()
        {
            return this.OnSessionRefreshed();
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed()
        {
            if (this.AddedThings.Count != 0 || this.UpdatedThings.Count != 0 || this.DeletedThings.Count != 0)
            {
                this.RefreshProductTree();
            }
            
            this.DetailsPanelViewModel.RefreshSelectedElement();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Applies the recorded <see cref="ElementUsage" /> changes to the product tree
        /// </summary>
        private void RefreshProductTree()
        {
            this.IsLoading = true;

            var addedElements = this.AddedThings.OfType<ElementUsage>().ToList();
            var deletedElements = this.DeletedThings.OfType<ElementUsage>().ToList();
            var updatedElements = this.UpdatedThings.OfType<ElementUsage>().ToList();
            var updatedElementBases = this.UpdatedThings.OfType<ElementBase>().ToList();

            this.Elements.AddRange(addedElements);
            this.Elements.RemoveMany(deletedElements);

            var selectedOption = this.OptionSelector.SelectedOption;
            var drawnUsageIids = this.ProductTreeViewModel.RootViewModel.GetFlatListOfDescendants(true)
                .Where(node => node.Thing != null)
                .Select(node => node.Thing.Iid)
                .ToHashSet();

            var optionMembershipChanged = updatedElements.Any(usage =>
            {
                var excluded = selectedOption != null && usage.ExcludeOption.Any(o => o.Iid == selectedOption.Iid);
                return excluded == drawnUsageIids.Contains(usage.Iid);
            });

            if (optionMembershipChanged)
            {
                this.ApplyFilters();
            }
            else
            {
                this.ProductTreeViewModel.AddElementsToTree(addedElements, selectedOption, []);
                this.ProductTreeViewModel.RemoveElementsFromTree(deletedElements);
                this.ProductTreeViewModel.UpdateElementsFromTree(updatedElementBases);
                this.ProductTreeViewModel.RootViewModel.OrderAllDescendantsByShortName();
            }

            this.ClearRecordedChanges();
            this.IsLoading = false;
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
