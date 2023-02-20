// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterEditorViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Pages.ParameterEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Extensions;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Services.SubscriptionService;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Pages.ParameterEditor.ParameterEditor"/>
    /// </summary>
    public class ParameterEditorViewModel : ReactiveObject, IParameterEditorViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ISubscriptionService"/>
        /// </summary>
        [Inject]
        public ISubscriptionService SubscriptionService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISessionService"/>
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// All <see cref="ElementBase"/> of the iteration without filtering
        /// </summary>
        public List<ElementBase> Elements { get; set; } = new();

        /// <summary>
        /// Gets or sets the filtered <see cref="ElementBase"/>
        /// </summary>
        public SourceList<ElementBase> FilteredElements { get; set; } = new();
        
        /// <summary>
        /// Sets if only parameters owned by the active domain are shown
        /// </summary>
        public bool IsOwnedParameters { get; set; } 

        /// <summary>
        /// Backing field for the <see cref="SelectedElementFilter"/> property
        /// </summary>
        private ElementBase selectedElementFilter;

        /// <summary>
        /// The selected <see cref="ElementBase"/> to filter
        /// </summary>
        public ElementBase SelectedElementFilter
        {
            get => this.selectedElementFilter;
            set => this.RaiseAndSetIfChanged(ref this.selectedElementFilter, value);
        }

        /// <summary>
        /// Backing field for the <see cref="SelectedParameterTypeFilter"/> property
        /// </summary>
        private ParameterType selectedParameterTypeFilterFilter;

        /// <summary>
        /// Name of the parameter type selected
        /// </summary>
        public ParameterType SelectedParameterTypeFilter
        {
            get => this.selectedParameterTypeFilterFilter;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameterTypeFilterFilter, value);
        }

        /// <summary>
        /// Backing field for the <see cref="SelectedOptionFilter"/> property
        /// </summary>
        private Option selectedOptionFilterFilter;

        /// <summary>
        /// Name of the option selected
        /// </summary>
        public Option SelectedOptionFilter
        {
            get => this.selectedOptionFilterFilter;
            set => this.RaiseAndSetIfChanged(ref this.selectedOptionFilterFilter, value);
        }

        /// <summary>
        /// Backing field for the <see cref="SelectedStateFilter"/> property
        /// </summary>
        private ActualFiniteState selectedStateFilterFilter;

        /// <summary>
        /// Name of the state selected
        /// </summary>
        public ActualFiniteState SelectedStateFilter
        {
            get => this.selectedStateFilterFilter;
            set => this.RaiseAndSetIfChanged(ref this.selectedStateFilterFilter, value);
        }

        /// <summary>
        /// All ParameterType names in the model
        /// </summary>
        public List<ParameterType> ParameterTypes { get; set; } = new();

        /// <summary>
        /// Creates a new instance of <see cref="ParameterEditorViewModel"/>
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService"/></param>
        /// <param name="subscriptionService">the <see cref="ISubscriptionService"/></param>
        public ParameterEditorViewModel(ISessionService sessionService, ISubscriptionService subscriptionService)
        {
            this.SessionService = sessionService;
            this.SubscriptionService = subscriptionService;
        }

        /// <summary>
        /// Initializes the <see cref="ParameterEditorViewModel"/>
        /// </summary>
        public void InitializeViewModel()
        {
            this.Elements = this.SessionService.DefaultIteration.QueryElementsBase().ToList();
            this.SelectedOptionFilter = this.SessionService.DefaultIteration.DefaultOption;

            this.ParameterTypes = this.SessionService.DefaultIteration.QueryUsedParameterTypes().OrderBy(p => p.Name).ToList();
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
        /// Filters the <param name="elements"/> by the <see cref="SelectedElementFilter"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        private IEnumerable<ElementBase> FilterByElement(IEnumerable<ElementBase> elements)
        {
            if (this.SelectedElementFilter is null)
            {
                return elements;
            }

            return elements.Where(x => x == this.selectedElementFilter);
        }

        /// <summary>
        /// Filters the <param name="elements"/> by the <see cref="SelectedParameterTypeFilter"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        private IEnumerable<ElementBase> FilterByParameterType(IEnumerable<ElementBase> elements)
        {
            var filteredElements = new List<ElementBase>();

            if (this.SelectedParameterTypeFilter is null)
            {
                return elements;
            }

            foreach (var element in elements)
            {
                if (element is ElementDefinition elementDefinition)
                {
                    if (elementDefinition.Parameter.Any(p => p.ParameterType.Name == this.SelectedParameterTypeFilter.Name))
                    {
                        filteredElements.Add(elementDefinition);
                    }
                }
                else if (element is ElementUsage elementUsage)
                {
                    if (elementUsage.ElementDefinition.Parameter.Any(p => p.ParameterType.Name == this.SelectedParameterTypeFilter.Name))
                    {
                        filteredElements.Add(elementUsage);
                    }
                    else if (elementUsage.ParameterOverride.Any(p => p.ParameterType.Name == this.SelectedParameterTypeFilter.Name))
                    {
                        filteredElements.Add(elementUsage);
                    }
                }
            }

            return filteredElements;
        }

        /// <summary>
        /// Filters the <param name="elements"/> by the <see cref="SelectedOptionFilter"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        private IEnumerable<ElementBase> FilterByOption(IEnumerable<ElementBase> elements)
        {
            if (this.SelectedOptionFilter is null)
            {
                return elements;
            }
            
            var nestedElements = this.SessionService.DefaultIteration.QueryNestedElements(this.selectedOptionFilterFilter);

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
        /// Filters the <param name="elements"/> by the <see cref="SelectedStateFilter"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        private IEnumerable<ElementBase> FilterByState(IEnumerable<ElementBase> elements)
        {
            if (this.SelectedStateFilter is null)
            {
                return elements;
            }
            
            return elements.FilterByState(this.SelectedStateFilter);
        }

        /// <summary>
        /// Filters the <param name="elements"/> by the <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        private IEnumerable<ElementBase> FilterByOwnedByActiveDomain(IEnumerable<ElementBase> elements)
        {
            if (!this.IsOwnedParameters)
            {
                return elements;
            }

            var filteredElements = new List<ElementBase>();
            var domainOfExpertise = this.SessionService.GetDomainOfExpertise(this.SessionService.DefaultIteration);

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
    }
}
