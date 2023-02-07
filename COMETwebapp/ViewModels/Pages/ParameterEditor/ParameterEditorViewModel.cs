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
    using COMETwebapp.IterationServices;
    using COMETwebapp.Services.SessionManagement;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Pages.ParameterEditor.ParameterEditor"/>
    /// </summary>
    public class ParameterEditorViewModel : ReactiveObject, IParameterEditorViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="IIterationService"/>
        /// </summary>
        [Inject]
        public IIterationService IterationService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISessionService"/>
        /// </summary>
        [Inject]
        public ISessionService SessionService { get; set; }

        /// <summary>
        /// The selected <see cref="ElementBase"/> to filter
        /// </summary>
        public ElementBase SelectedElement { get; set; }

        /// <summary>
        /// All <see cref="ElementBase"/> of the iteration without filtering
        /// </summary>
        public List<ElementBase> Elements { get; set; } = new();

        /// <summary>
        /// Backing field for the <see cref="FilteredElements"/>
        /// </summary>
        private List<ElementBase> filteredElements = new();

        /// <summary>
        /// Gets or sets the filtered <see cref="ElementBase"/>
        /// </summary>
        public List<ElementBase> FilteredElements
        {
            get => this.filteredElements;
            set => this.RaiseAndSetIfChanged(ref this.filteredElements, value);
        }

        /// <summary>
        /// Sets if only parameters owned by the active domain are shown
        /// </summary>
        public bool IsOwnedParameters { get; set; } = true;

        /// <summary>
        /// Name of the parameter type selected
        /// </summary>
        public ParameterType SelectedParameterType { get; set; }

        /// <summary>
        /// Name of the option selected
        /// </summary>
        public Option SelectedOption { get; set; }

        /// <summary>
        /// Name of the state selected
        /// </summary>
        public ActualFiniteState SelectedState { get; set; }

        /// <summary>
        /// All ParameterType names in the model
        /// </summary>
        public List<ParameterType> ParameterTypes { get; set; } = new();

        /// <summary>
        /// Creates a new instance of <see cref="ParameterEditorViewModel"/>
        /// </summary>
        /// <param name="sessionService">the <see cref="ISessionService"/></param>
        /// <param name="iterationService">the <see cref="IIterationService"/></param>
        public ParameterEditorViewModel(ISessionService sessionService, IIterationService iterationService)
        {
            this.SessionService = sessionService;
            this.IterationService = iterationService;
        }

        /// <summary>
        /// Initializes the <see cref="ParameterEditorViewModel"/>
        /// </summary>
        public void InitializeViewModel()
        {
            this.Elements = this.SessionService.DefaultIteration.GetElementsOfIteration().ToList();
            this.SelectedOption = this.SessionService.DefaultIteration.DefaultOption;
            this.ParameterTypes = this.IterationService.GetParameterTypes(this.SessionService.DefaultIteration).OrderBy(p => p.Name).ToList();
            this.FilteredElements = this.ApplyFilters(this.Elements).ToList();
        }

        /// <summary>
        /// Apply all the filters selected in the <param name="elements"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        public IEnumerable<ElementBase> ApplyFilters(IEnumerable<ElementBase> elements)
        {
            var filteredElements = this.FilterByOption(elements);
            filteredElements = this.FilterByParameterType(filteredElements);
            filteredElements = this.FilterByState(filteredElements);
            return filteredElements;
        }

        /// <summary>
        /// Filters the <param name="elements"/> by the <see cref="SelectedParameterType"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        public IEnumerable<ElementBase> FilterByParameterType(IEnumerable<ElementBase> elements)
        {
            var filteredElements = new List<ElementBase>();

            if (this.SelectedParameterType is null)
            {
                return elements;
            }

            foreach (var element in elements)
            {
                if (element is ElementDefinition elementDefinition)
                {
                    if (elementDefinition.Parameter.Any(p => p.ParameterType == this.SelectedParameterType))
                    {
                        filteredElements.Add(elementDefinition);
                    }
                }
                else if (element is ElementUsage elementUsage)
                {
                    if (!elementUsage.ParameterOverride.Any() && elementUsage.ElementDefinition.Parameter.Any(p => p.ParameterType == this.SelectedParameterType))
                    {
                        filteredElements.Add(elementUsage);
                    }
                    else if (elementUsage.ParameterOverride.Any() && elementUsage.ParameterOverride.Any(p => p.ParameterType == this.SelectedParameterType))
                    {
                        filteredElements.Add(elementUsage);
                    }
                }
            }

            return filteredElements;
        }

        /// <summary>
        /// Filters the <param name="elements"/> by the <see cref="SelectedOption"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        public IEnumerable<ElementBase> FilterByOption(IEnumerable<ElementBase> elements)
        {
            if (this.SelectedOption is null)
            {
                return elements;
            }

            var nestedElements = this.IterationService.GetNestedElementsByOption(this.SessionService.DefaultIteration, this.SelectedOption.Iid);

            var associatedElements = new List<ElementUsage>();
            nestedElements.ForEach(element => associatedElements.AddRange(element.ElementUsage));

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
        /// Filters the <param name="elements"/> by the <see cref="SelectedState"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        public IEnumerable<ElementBase> FilterByState(IEnumerable<ElementBase> elements)
        {
            if (this.SelectedState is null)
            {
                return elements;
            }
            
            return elements.FilterByState(this.SelectedState);
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

            //var iteration = this.SessionService.DefaultIteration;

            //if (iteration.TopElement != null && iteration.TopElement.Parameter.FindAll(p => p.Owner == this.SessionService.GetDomainOfExpertise(this.SessionService.DefaultIteration)).Count != 0)
            //{
            //    this.Elements.Add(iteration.TopElement);
            //}

            //iteration.Element.ForEach(e =>
            //{
            //    e.ContainedElement.ForEach(containedElement =>
            //    {
            //        if (containedElement.ParameterOverride.Count == 0)
            //        {
            //            if (containedElement.ElementDefinition.Parameter.FindAll(p => p.Owner == this.SessionService.GetDomainOfExpertise(this.SessionService.DefaultIteration)).Count != 0)
            //            {
            //                this.Elements.Add(containedElement);
            //            }
            //        }
            //        else
            //        {
            //            if (containedElement.ParameterOverride.FindAll(p => p.Owner == this.SessionService.GetDomainOfExpertise(this.SessionService.DefaultIteration)).Count != 0)
            //            {
            //                this.Elements.Add(containedElement);
            //            }

            //            if (!this.Elements.Contains(containedElement) && containedElement.ElementDefinition.Parameter.FindAll(p => p.Owner == this.SessionService.GetDomainOfExpertise(this.SessionService.DefaultIteration)).Count != 0)
            //            {
            //                this.Elements.Add(containedElement);
            //            }
            //        }
            //    });
            //});

            return elements;
        }
    }
}
