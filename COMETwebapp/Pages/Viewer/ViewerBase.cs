namespace COMETwebapp.Pages.Viewer
{
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Components.Viewer;
    using COMETwebapp.IterationServices;
    using COMETwebapp.SessionManagement;

    using Microsoft.AspNetCore.Components;

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

                iteration?.ActualFiniteStateList.ForEach(l =>
                {
                    l.ActualState.ForEach(s =>
                    {
                        States.Add(s.Name);
                    });
                });

                this.OptionSelected = iteration?.DefaultOption.Name;
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

            this.CanvasComponentReference?.RepopulateScene(elements.OfType<ElementUsage>().ToList());

            return elements;
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
            this.Filter(this.Elements);
            this.StateHasChanged();
        }

        /// <summary>
        /// Updates Elements list when a filter for parameter typestate is selected
        /// </summary>
        /// <param name="state">Name of the State selected</param>
        public void OnStateFilterChange(string? state)
        {
            this.StateSelected = state;
            this.Elements.Clear();
            this.InitializeElements();
            this.Filter(this.Elements);
            this.StateHasChanged();

        }
    }
}
