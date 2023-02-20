// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterEditorBodyViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.ParameterEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Extensions;
    using COMETwebapp.IterationServices;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.ViewModels.Components.Shared;
    using COMETwebapp.ViewModels.Components.Shared.Selectors;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Components.ParameterEditor.ParameterEditorBody"/>
    /// </summary>
    public class ParameterEditorBodyViewModel : SingleIterationApplicationBaseViewModel, IParameterEditorBodyViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ISubscriptionService"/>
        /// </summary>
        [Inject]
        public ISubscriptionService SubscriptionService { get; set; }
        public IIterationService IterationService { get; set; }

        /// <summary>
        /// Gets the <see cref="IElementBaseSelectorViewModel"/>
        /// </summary>
        public IElementBaseSelectorViewModel ElementSelector { get; private set; } = new ElementBaseSelectorViewModel();

        /// <summary>
        /// Gets the <see cref="IOptionSelectorViewModel" />
        /// </summary>
        public IOptionSelectorViewModel OptionSelector { get; private set; } = new OptionSelectorViewModel();

        /// <summary>
        /// Gets the <see cref="IFiniteStateSelectorViewModel" />
        /// </summary>
        public IFiniteStateSelectorViewModel FiniteStateSelector { get; private set; } = new FiniteStateSelectorViewModel();

        /// <summary>
        /// Gets the <see cref="IParameterTypeSelectorViewModel" />
        /// </summary>
        public IParameterTypeSelectorViewModel ParameterTypeSelector { get; private set; } = new ParameterTypeSelectorViewModel();

        /// <summary>
        /// All <see cref="ElementBase"/> of the iteration without filtering
        /// </summary>
        public List<ElementBase> Elements { get; private set; } = new();

        /// <summary>
        /// Gets or sets the filtered <see cref="ElementBase"/>
        /// </summary>
        public SourceList<ElementBase> FilteredElements { get; } = new();

        /// <summary>
        /// Backing field for the <see cref="IsOwnedParameters"/>
        /// </summary>
        private bool isOwnedParameters;

        /// <summary>
        /// Sets if only parameters owned by the active domain are shown
        /// </summary>
        public bool IsOwnedParameters
        {
            get => this.isOwnedParameters;
            set => this.RaiseAndSetIfChanged(ref this.isOwnedParameters, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="IParameterTableViewModel"/>
        /// </summary>
        public IParameterTableViewModel ParameterTableViewModel { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="ParameterEditorBodyViewModel"/>
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService"/></param>
        /// <param name="iterationService">the <see cref="IIterationService"/></param>
        public ParameterEditorBodyViewModel(ISessionService sessionService, IIterationService iterationService) : base(sessionService)
        {
            this.IterationService = iterationService;
            this.ParameterTableViewModel = new ParameterTableViewModel(sessionService);

            this.Disposables.Add(this.FilteredElements.CountChanged.Subscribe(_ =>
            {
                this.ParameterTableViewModel.InitializeViewModel(this.FilteredElements.Items);
            }));

            this.Disposables.Add(this.WhenAnyValue(x=>x.ElementSelector.SelectedElementBase,
                x=>x.OptionSelector.SelectedOption,
                x=>x.FiniteStateSelector.SelectedActualFiniteState,
                x=>x.ParameterTypeSelector.SelectedParameterType,
                x=>x.IsOwnedParameters).Subscribe(_ =>
            {
                this.InitializeViewModel();
            }));
        }

        /// <summary>
        /// Update this view model properties
        /// </summary>
        protected override void OnIterationChanged()
        {
            this.ElementSelector.CurrentIteration = this.CurrentIteration;
            this.OptionSelector.CurrentIteration = this.CurrentIteration;
            this.FiniteStateSelector.CurrentIteration = this.CurrentIteration;
            this.ParameterTypeSelector.CurrentIteration = this.CurrentIteration;
            this.InitializeViewModel();
        }

        /// <summary>
        /// Initializes the <see cref="ParameterEditorBodyViewModel"/>
        /// </summary>
        public void InitializeViewModel()
        {
            this.Elements = this.CurrentIteration?.QueryElementsBase().ToList() ?? new List<ElementBase>();
            this.ApplyFilters(this.Elements);
        }

        /// <summary>
        /// Apply all the filters selected in the <param name="elements"/> and replace the data of <see cref="FilteredElements"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        public void ApplyFilters(IEnumerable<ElementBase> elements)
        {
            this.FilteredElements.Clear();
            var filteredElements = this.FilterByElement(elements);
            filteredElements = this.FilterByOption(filteredElements);
            filteredElements = this.FilterByParameterType(filteredElements);
            filteredElements = this.FilterByState(filteredElements);
            filteredElements = this.FilterByOwnedByActiveDomain(filteredElements);
            this.FilteredElements.AddRange(filteredElements);
        }

        /// <summary>
        /// Filters the <param name="elements"/> by the <see cref="ElementSelector"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        public IEnumerable<ElementBase> FilterByElement(IEnumerable<ElementBase> elements)
        {
            if (this.ElementSelector.SelectedElementBase is null)
            {
                return elements;
            }

            return elements.Where(x => x == this.ElementSelector.SelectedElementBase);
        }

        /// <summary>
        /// Filters the <param name="elements"/> by the <see cref="ParameterTypeSelector"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        public IEnumerable<ElementBase> FilterByParameterType(IEnumerable<ElementBase> elements)
        {
            var filteredElements = new List<ElementBase>();

            if (this.ParameterTypeSelector.SelectedParameterType is null)
            {
                return elements;
            }

            foreach (var element in elements)
            {
                if (element is ElementDefinition elementDefinition)
                {
                    if (elementDefinition.Parameter.Any(p => p.ParameterType.Name == this.ParameterTypeSelector.SelectedParameterType.Name))
                    {
                        filteredElements.Add(elementDefinition);
                    }
                }
                else if (element is ElementUsage elementUsage)
                {
                    if (elementUsage.ElementDefinition.Parameter.Any(p => p.ParameterType.Name == this.ParameterTypeSelector.SelectedParameterType.Name))
                    {
                        filteredElements.Add(elementUsage);
                    }
                    else if (elementUsage.ParameterOverride.Any(p => p.ParameterType.Name == this.ParameterTypeSelector.SelectedParameterType.Name))
                    {
                        filteredElements.Add(elementUsage);
                    }
                }
            }

            return filteredElements;
        }

        /// <summary>
        /// Filters the <param name="elements"/> by the <see cref="OptionSelector"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        public IEnumerable<ElementBase> FilterByOption(IEnumerable<ElementBase> elements)
        {
            if (this.OptionSelector.SelectedOption is null)
            {
                return elements;
            }

            var nestedElements = this.CurrentIteration.QueryNestedElements(this.OptionSelector.SelectedOption);

            var associatedElements = new List<ElementUsage>();
            nestedElements.ToList().ForEach(element => associatedElements.AddRange(element.ElementUsage));

            associatedElements = associatedElements.Distinct().ToList();

            var elementsToRemove = new List<ElementBase>();

            var filteredElements = elements.ToList();

            foreach (var element in filteredElements)
            {
                if (element is ElementUsage elementUsage && !associatedElements.Contains(elementUsage))
                {
                    elementsToRemove.Add(elementUsage);
                }
            }

            filteredElements.RemoveAll(e => elementsToRemove.Contains(e));

            return filteredElements;
        }

        /// <summary>
        /// Filters the <param name="elements"/> by the <see cref="FiniteStateSelector"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        public IEnumerable<ElementBase> FilterByState(IEnumerable<ElementBase> elements)
        {
            if (this.FiniteStateSelector.SelectedActualFiniteState is null)
            {
                return elements;
            }

            return elements.FilterByState(this.FiniteStateSelector.SelectedActualFiniteState);
        }

        /// <summary>
        /// Filters the <param name="elements"/> by the <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        public IEnumerable<ElementBase> FilterByOwnedByActiveDomain(IEnumerable<ElementBase> elements)
        {
            if (!this.IsOwnedParameters)
            {
                return elements;
            }

            var filteredElements = new List<ElementBase>();
            var domainOfExpertise = this.SessionService.GetDomainOfExpertise(this.CurrentIteration);

            foreach (var element in elements)
            {
                if (element is ElementDefinition elementDefinition)
                {
                    if (elementDefinition.Parameter.Any(p => p.Owner == domainOfExpertise))
                    {
                        filteredElements.Add(elementDefinition);
                    }
                }
                else if (element is ElementUsage elementUsage)
                {
                    if (!elementUsage.ParameterOverride.Any())
                    {
                        if (elementUsage.ElementDefinition.Parameter.Any(p => p.Owner == domainOfExpertise))
                        {
                            filteredElements.Add(elementUsage);
                        }
                    }
                    else
                    {
                        if (elementUsage.ParameterOverride.Any(p => p.Owner == domainOfExpertise))
                        {
                            filteredElements.Add(elementUsage);
                        }

                        if (elementUsage.ElementDefinition.Parameter.Any(p => p.Owner == domainOfExpertise) && !filteredElements.Contains(elementUsage))
                        {
                            filteredElements.Add(elementUsage);
                        }
                    }
                }
            }

            return filteredElements;
        }

        /// <summary>
        /// Queries the <see cref="DomainOfExpertise"/> of the current <see cref="Iteration"/>
        /// </summary>
        /// <returns>the name of the <see cref="DomainOfExpertise"/></returns>
        public string QueryDomainOfExpertiseName()
        {
            return this.SessionService.GetDomainOfExpertise(this.CurrentIteration)?.Name ?? string.Empty;
        }
    }
}
