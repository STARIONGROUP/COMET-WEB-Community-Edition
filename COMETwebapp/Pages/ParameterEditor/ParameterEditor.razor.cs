// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterEditor.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
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

namespace COMETwebapp.Pages.ParameterEditor
{
    using System.Reactive.Linq;

    using CDP4Common.EngineeringModelData;
    
    using CDP4Dal;
    using CDP4Dal.Events;
    
    using COMETwebapp.SessionManagement;
    
    using Microsoft.AspNetCore.Components;
    
    /// <summary>
    /// Class for the <see cref="ParameterEditor"/> razor component
    /// </summary>
    public partial class ParameterEditor
    {
        /// <summary>
        /// The filter on option
        /// </summary>
        [Parameter]
        [SupplyParameterFromQuery]
        public Guid FilterOption { get; set; }

        [Parameter]
        [SupplyParameterFromQuery]
        public Guid FilterElementBase { get; set; }

        /// <summary>
        /// The selected <see cref="ElementBase"/> to filter
        /// </summary>
        private ElementBase SelectedElement { get; set; }

        /// <summary>
        /// All <see cref="ElementBase"/> of the iteration
        /// </summary>
        public List<ElementBase> Elements { get; set; } = new();

        /// <summary>
        /// Sets if only parameters owned by the active domain are shown
        /// </summary>
        private bool IsOwnedParameters { get; set; } = true;

        /// <summary>
        /// Name of the parameter type selected
        /// </summary>
        private string ParameterTypeSelected { get; set; }

        /// <summary>
        /// Name of the option selected
        /// </summary>
        private string OptionSelected { get; set; }

        /// <summary>
        /// Name of the state selected
        /// </summary>
        private string StateSelected { get; set; }

        /// <summary>
        /// Listeners for the components to update it with ISession
        /// </summary>
        private Dictionary<string, IDisposable> listeners = new ();

        /// <summary>
        /// All ParameterType names in the model
        /// </summary>
        public List<string> ParameterTypeNames = new();

        /// <summary>
        /// Initialize component at first render and after session update
        /// </summary>
        protected override void OnInitialized()
        {
            this.Elements.Clear();
            this.InitializeElements();

            this.ParameterTypeNames = new List<string>();
            this.IterationService.GetParameterTypes(this.SessionService.DefaultIteration).OrderBy(p => p.Name).ToList().ForEach(p => this.ParameterTypeNames.Add(p.Name));

            if (!this.listeners.TryGetValue("UpToDate", out var listener))
            {
                this.listeners.Add("UpToDate", CDPMessageBus.Current.Listen<SessionStateKind>().Where(x => x == SessionStateKind.UpToDate).Subscribe(x =>
                {
                    this.Elements.Clear();
                    this.InitializeElements();
                    this.StateHasChanged();
                }));
            }

            if (!this.listeners.TryGetValue("DomainChangedEvent", out listener))
            {
                this.listeners.Add("DomainChangedEvent", CDPMessageBus.Current.Listen<DomainChangedEvent>().Subscribe(x =>
                {
                    this.Elements.Clear();
                    this.InitializeElements();
                    this.StateHasChanged();
                }));
            }
        }

        /// <summary>
        /// Get selected filter on url page
        /// </summary>
        protected override void OnParametersSet()
        {
            if (this.FilterOption != null)
            {
                this.OptionSelected = this.SessionService.DefaultIteration?.Option.ToList().Find(o => o.Iid == this.FilterOption)?.Name;
            }
            else
            {
                this.OptionSelected = this.SessionService.DefaultIteration?.Option.First().Name;
            }
            
            if (this.FilterElementBase != null)
            {
                this.SelectedElement = this.Elements.Find(e => e.Iid == this.FilterElementBase);
            }
        }

        /// <summary>
        /// Initialize <see cref="ElementBase"/> list
        /// </summary>
        private void InitializeElements()
        {
            var iteration = this.SessionService.DefaultIteration;
            
            if (iteration != null)
            {
                if (this.IsOwnedParameters == true)
                {
                    if (iteration.TopElement != null && iteration.TopElement.Parameter.FindAll(p => p.Owner == this.SessionService.GetDomainOfExpertise(this.SessionService.DefaultIteration)).Count != 0)
                    {
                        this.Elements.Add(iteration.TopElement);
                    }
                    
                    iteration.Element.ForEach(e =>
                    {
                        e.ContainedElement.ForEach(containedElement =>
                        {
                            if (containedElement.ParameterOverride.Count == 0)
                            {
                                if (containedElement.ElementDefinition.Parameter.FindAll(p => p.Owner == this.SessionService.GetDomainOfExpertise(this.SessionService.DefaultIteration)).Count != 0)
                                {
                                    this.Elements.Add(containedElement);
                                }
                            }
                            else
                            {
                                if (containedElement.ParameterOverride.FindAll(p => p.Owner == this.SessionService.GetDomainOfExpertise(this.SessionService.DefaultIteration)).Count != 0)
                                {
                                    this.Elements.Add(containedElement);
                                }
                                
                                if (!this.Elements.Contains(containedElement) && containedElement.ElementDefinition.Parameter.FindAll(p => p.Owner == this.SessionService.GetDomainOfExpertise(this.SessionService.DefaultIteration)).Count != 0)
                                {
                                    this.Elements.Add(containedElement);
                                }
                            }
                        });
                    });
                }
                else
                {
                    if (iteration.TopElement != null)
                    {
                        this.Elements.Add(iteration.TopElement);
                    }
                    
                    iteration.Element.ForEach(e => this.Elements.AddRange(e.ContainedElement));
                }
            }
        }

        /// <summary>
        /// Filter <see cref="ElementBase"/> to show in the tree
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public List<ElementBase> Filter(List<ElementBase> elements)
        {
            if (this.ParameterTypeSelected != null)
            {
                var elementsToRemove = new List<ElementBase>();
                
                elements.ForEach(e =>
                {
                    if (e.GetType().Equals(typeof(ElementDefinition)))
                    {
                        var elementDefinition = (ElementDefinition)e;
                        
                        if (elementDefinition.Parameter.FindAll(p => p.ParameterType.Name.Equals(this.ParameterTypeSelected)).Count == 0)
                        {
                            elementsToRemove.Add(e);
                        }
                    }
                    else if (e.GetType().Equals(typeof(ElementUsage)))
                    {
                        var elementUsage = (ElementUsage)e;
                        
                        if (elementUsage.ParameterOverride.Count == 0 && elementUsage.ElementDefinition.Parameter.FindAll(p => p.ParameterType.Name.Equals(this.ParameterTypeSelected)).Count == 0)
                        {
                            elementsToRemove.Add(e);
                        }
                        else if (elementUsage.ParameterOverride.Count != 0)
                        {
                            if (elementUsage.ParameterOverride.FindAll(p => p.ParameterType.Name.Equals(this.ParameterTypeSelected)).Count == 0 &&
                                elementUsage.ElementDefinition.Parameter.FindAll(p => p.ParameterType.Name.Equals(this.ParameterTypeSelected)).Count == 0)
                            {
                                elementsToRemove.Add(e);
                            }
                        }
                    }
                });
                
                elements.RemoveAll(e => elementsToRemove.Contains(e));
            }

            if (this.OptionSelected != null)
            {
                var option = this.SessionService.DefaultIteration?.Option.ToList().Find(option => option.Name == this.OptionSelected)?.Iid;
                var nestedElements = this.IterationService.GetNestedElementsByOption(this.SessionService.DefaultIteration, option);

                var associatedElements = new List<ElementUsage>();
                
                nestedElements.ForEach(element =>
                {
                    associatedElements.AddRange(element.ElementUsage);
                });
                
                associatedElements = associatedElements.Distinct().ToList();

                var elementsToRemove = new List<ElementBase>();
                
                elements.ForEach(e =>
                {
                    if (e.GetType().Equals(typeof(ElementUsage)) && !associatedElements.Contains(e))
                    {
                        elementsToRemove.Add(e);
                    }
                });
                
                elements.RemoveAll(e => elementsToRemove.Contains(e));
            }

            if (this.StateSelected != null)
            {
                var elementsToRemove = new List<ElementBase>();
                
                elements.ForEach(e =>
                {
                    if (e.GetType().Equals(typeof(ElementDefinition)))
                    {
                        var elementDefinition = (ElementDefinition)e;
                        var actualStates = new List<ActualFiniteState>();
                        
                        elementDefinition.Parameter.ForEach(p =>
                        {
                            p.ValueSet.ForEach(v =>
                            {
                                if (v.ActualState != null && v.ActualState.Name.Equals(this.StateSelected))
                                {
                                    actualStates.Add(v.ActualState);
                                }
                            });
                        });
                        
                        if (actualStates.Count == 0)
                        {
                            elementsToRemove.Add(e);
                        }
                    }
                    else if (e.GetType().Equals(typeof(ElementUsage)))
                    {
                        var elementUsage = (ElementUsage)e;
                        var actualStates = new List<ActualFiniteState>();
                        
                        if (elementUsage.ParameterOverride.Count == 0)
                        {
                            elementUsage.ElementDefinition.Parameter.ForEach(p =>
                            {
                                p.ValueSet.ForEach(v =>
                                {
                                    if (v.ActualState != null && v.ActualState.Name.Equals(this.StateSelected))
                                    {
                                        actualStates.Add(v.ActualState);
                                    }
                                });
                            });
                            
                            if (actualStates.Count == 0)
                            {
                                elementsToRemove.Add(e);
                            }
                        }
                        else if (elementUsage.ParameterOverride.Count != 0)
                        {
                            elementUsage.ParameterOverride.ForEach(p =>
                            {
                                p.ValueSet.ForEach(v =>
                                {
                                    if (v.ActualState != null && v.ActualState.Name.Equals(this.StateSelected))
                                    {
                                        actualStates.Add(v.ActualState);
                                    }
                                });
                            });
                            
                            elementUsage.ElementDefinition.Parameter.ForEach(p =>
                            {
                                p.ValueSet.ForEach(v =>
                                {
                                    if (v.ActualState != null && v.ActualState.Name.Equals(this.StateSelected))
                                    {
                                        actualStates.Add(v.ActualState);
                                    }
                                });
                            });
                            
                            if (actualStates.Count == 0)
                            {
                                elementsToRemove.Add(e);
                            }
                        }
                    }
                });
                
                elements.RemoveAll(e => elementsToRemove.Contains(e));
            }

            if (this.SelectedElement != null)
            {
                return elements.FindAll(e => e.Iid == this.SelectedElement?.Iid);
            }
            else
            {
                return elements;
            }
        }

        /// <summary>
        /// Stop and clear listeners of the component
        /// </summary>
        public void Dispose()
        {
            this.listeners.Values.ToList().ForEach(l => l.Dispose());
            this.listeners.Clear();
        }

        /// <summary>
        /// Updates Elements list when a filter for owner is selected
        /// </summary>
        /// <param name="e"><see cref="ChangeEventArgs"/> from the selector</param>
        public void OnOwnerFilterChange(ChangeEventArgs e)
        {
            this.Elements.Clear();
            this.InitializeElements();
        }

        /// <summary>
        /// Updates Elements list when a filter for parameter type is selected
        /// </summary>
        /// <param name="parameterType">Name of the ParameterType selected</param>
        public void OnParameterFilterChange(string parameterType)
        {
            this.ParameterTypeSelected = parameterType;
            this.Elements.Clear();
            this.InitializeElements();
        }

        /// <summary>
        /// Updates Elements list when a filter for option is selected
        /// </summary>
        /// <param name="option">Name of the Option selected</param>
        public void OnOptionFilterChange(string option)
        {
            this.OptionSelected = option;
            
            if (this.OptionSelected != null)
            {
                this.FilterOption = this.SessionService.DefaultIteration.Option.ToList().Find(o => o.Name == option).Iid;
            }
            else
            {
                this.FilterOption = Guid.Empty;
            }
            
            this.NavigationManager.NavigateTo($"/ParameterEditor?filteroption={this.FilterOption}");
        }

        /// <summary>
        /// Updates Elements list when a filter for parameter typestate is selected
        /// </summary>
        /// <param name="state">Name of the State selected</param>
        public void OnStateFilterChange(string state)
        {
            this.StateSelected = state;
            this.Elements.Clear();
            this.InitializeElements();
        }
    }
}
