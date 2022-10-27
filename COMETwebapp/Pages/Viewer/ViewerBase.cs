// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewerBase.cs" company="RHEA System S.A.">
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

    using COMETwebapp.Components.Viewer;
    using COMETwebapp.IterationServices;
    using COMETwebapp.Model;
    using COMETwebapp.SessionManagement;

    using Microsoft.AspNetCore.Components;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Support class for the <see cref="Viewer"/>
    /// </summary>
    public class ViewerBase : ComponentBase
    {
        /// <summary>
        /// The reference to the <see cref="BabylonCanvas"/> component
        /// </summary>
        [Parameter]
        public BabylonCanvas? CanvasComponentReference { get; set; }

        /// <summary>
        /// The filter on option
        /// </summary>
        [Parameter]
        [SupplyParameterFromQuery]
        public Guid? FilterOption { get; set; }

        [Parameter]
        [SupplyParameterFromQuery]
        public Guid? FilterElementBase { get; set; }

        /// <summary>
        /// All <see cref="ElementBase"> of the iteration
        /// </summary>
        public List<ElementBase> Elements { get; set; } = new List<ElementBase>();

        /// <summary>
        /// Name of the option selected
        /// </summary>
        public string? OptionSelected { get; set; }

        /// <summary>
        /// Name of the state selected
        /// </summary>
        public string? StateSelected { get; set; }

        /// <summary>
        /// Injected property to get access to <see cref="ISessionAnchor"/>
        /// </summary>
        [Inject]
        public ISessionAnchor? SessionAnchor { get; set; }

        /// <summary>
        /// Injected property to get access to <see cref="IIterationService"/>
        /// </summary>
        [Inject]
        public IIterationService? IterationService { get; set; }

        /// <summary>
        /// List of the names of <see cref="Option"/> available
        /// </summary>
        [Parameter]
        public List<string>? Options { get; set; }

        /// <summary>
        /// List of the names of <see cref="ActualFiniteState"/> 
        /// </summary>
        [Parameter]
        public List<string>? States { get; set; }


        /// <summary>
        /// List of the of <see cref="ActualFiniteStateList"/> 
        /// </summary>
        [Parameter]
        public List<ActualFiniteStateList>? ListActualFiniteStateLists { get; set; }

        /// <summary>
        /// The dictionary that keeps track of the filters
        /// </summary>
        private Dictionary<ActualFiniteStateList, ActualFiniteStateListFilterData> CheckboxStates_ActualFiniteStateList;

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
                this.Elements.Clear();
                this.InitializeElements();

                Options = new List<string>();
                States = new List<string>();
                var iteration = this.SessionAnchor?.OpenIteration;
                iteration?.Option.OrderBy(o => o.Name).ToList().ForEach(o => Options.Add(o.Name));

                this.ListActualFiniteStateLists = iteration?.ActualFiniteStateList?.ToList();

                iteration?.ActualFiniteStateList.ForEach(l =>
                {
                    l.ActualState.ForEach(s =>
                    {
                        States.Add(s.Name);
                    });
                });

                CheckboxStates_ActualFiniteStateList = new Dictionary<ActualFiniteStateList, ActualFiniteStateListFilterData>();
                this.ListActualFiniteStateLists?.ForEach(x =>
                {
                    var defaultState = x.ActualState.FirstOrDefault(afs => afs.IsDefault);
                    var data = new ActualFiniteStateListFilterData(defaultState);
                    CheckboxStates_ActualFiniteStateList.Add(x, data);
                });

                this.StateHasChanged();
            }
        }

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
        /// Filter <see cref="ElementBase"> to show in the tree
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public List<ElementBase> Filter(List<ElementBase> elements)
        {
            if (this.OptionSelected != null)
            {
                var option = this.SessionAnchor?.OpenIteration?.Option.ToList().Find(option => option.Name == this.OptionSelected)?.Iid;
                var nestedElements = this.IterationService?.GetNestedElementsByOption(this.SessionAnchor?.OpenIteration, option);

                var associatedElements = new List<ElementUsage>();
                nestedElements?.ForEach(element =>
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

            return elements;
        }

        private List<ElementUsage> CreateElementUsagesForScene(List<ElementBase> elements)
        {
            var optionId = this.SessionAnchor?.OpenIteration?.DefaultOption?.Iid;

            if(this.OptionSelected != null)
            {
                optionId = this.SessionAnchor?.OpenIteration?.Option.ToList().Find(option => option.Name == this.OptionSelected)?.Iid;
            }

            var nestedElements = this.IterationService?.GetNestedElementsByOption(this.SessionAnchor?.OpenIteration, optionId);

            var associatedElements = new List<ElementUsage>();
            nestedElements?.ForEach(element =>
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

            return elements.OfType<ElementUsage>().ToList();
        }

        /// <summary>
        /// Repopulates the scene with the specified <see cref="ElementUsage"/>
        /// </summary>
        /// <param name="elementUsages">the element usages to populate the scene with</param>
        private void RepopulateScene(List<ElementUsage> elementUsages)
        {
            var optionName = this.OptionSelected;
            var stateName = this.StateSelected;

            if(optionName is null)
            {
                optionName = this.SessionAnchor?.OpenIteration?.DefaultOption.Name;
            }
                        
            var option = this.SessionAnchor?.OpenIteration?.Option.FirstOrDefault(opt => opt.Name == optionName);
            List<ActualFiniteState> states = CheckboxStates_ActualFiniteStateList.Values.Select(x => x.GetStateToUse()).ToList();

            this.CanvasComponentReference?.RepopulateScene(elementUsages, option, states);
        }

        /// <summary>
        /// Updates Elements list when a filter for option is selected
        /// </summary>
        /// <param name="option">Name of the Option selected</param>
        public void OnOptionFilterChange(string? option)
        {
            this.OptionSelected = option;
            this.FilterOption = this.OptionSelected != null ? this.SessionAnchor?.OpenIteration?.Option.ToList().Find(o => o.Name == option)?.Iid : null;
            this.Elements.Clear();
            this.InitializeElements();
            var elementsOnScene = this.CreateElementUsagesForScene(this.Elements);
            this.RepopulateScene(elementsOnScene);
            this.StateHasChanged();
        }

        /// <summary>
        /// Event that is raised when the checkboxes of an <see cref="ActualFiniteStateList"/> change their state
        /// </summary>
        /// <param name="args">the arguments of the event</param>
        public void OnActualFiniteStateList_SelectionChanged(object sender, ChangeEventArgs args)
        {
            if(sender is ActualFiniteStateList actualFiniteStateList && args.Value is bool value)
            {
                if (CheckboxStates_ActualFiniteStateList.ContainsKey(actualFiniteStateList))
                {
                    CheckboxStates_ActualFiniteStateList[actualFiniteStateList].IsFilterActive = value;                    
                }                
            }
            var elementsOnScene = this.CreateElementUsagesForScene(this.Elements);
            this.RepopulateScene(elementsOnScene);
        }

        /// <summary>
        /// Event that is raised when the radiobuttons of an <see cref="ActualFiniteState"/> change their state
        /// </summary>
        /// <param name="args">the argument of the event</param>
        public void OnActualFiniteState_SelectionChanged(object sender, ChangeEventArgs args)
        {
            if(sender is ActualFiniteState actualFiniteState && actualFiniteState.Container is ActualFiniteStateList actualFiniteStateList)
            {
                if (CheckboxStates_ActualFiniteStateList.ContainsKey(actualFiniteStateList))
                {
                    CheckboxStates_ActualFiniteStateList[actualFiniteStateList].ActiveState = actualFiniteState;
                }
            }
            var elementsOnScene = this.CreateElementUsagesForScene(this.Elements);
            this.RepopulateScene(elementsOnScene);
        }
    }
}
