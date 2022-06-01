namespace COMETwebapp.IterationService
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using COMETwebapp.SessionManagement;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Service to access the opened iteration data
    /// </summary>
    public class IterationService
    {
        /// <summary>
        /// The actual opened iteration
        /// </summary>
        private readonly Iteration OpenedIteration;

        /// <summary>
        /// Initialize the service withe the opened iteration
        /// </summary>
        /// <param name="iteration">The iteration to read data</param>
        public IterationService(Iteration iteration)
        {
            OpenedIteration = iteration;
        }

        /// <summary>
        /// Get all <see cref="ParameterValueSet"/> of the opened iteration
        /// </summary>
        /// <returns>All <see cref="ParameterValueSet"/></returns>
        public List<ParameterValueSet> GetParameterValueSets()
        {
            List<ParameterValueSet> result = new List<ParameterValueSet>();
            this.OpenedIteration.Element.ForEach(e => e.Parameter.ForEach(p => result.AddRange(p.ValueSet)));
            return result;
        }

        /// <summary>
        /// Get the nested elements of the opened iteration for all options
        /// </summary>
        /// <returns></returns>
        public List<NestedElement> GetNestedElements()
        {
            NestedElementTreeGenerator nestedElementTreeGenerator = new NestedElementTreeGenerator();
            List<NestedElement> nestedElements = new List<NestedElement>();
            this.OpenedIteration.Option.ToList().ForEach(option => nestedElements.AddRange(nestedElementTreeGenerator.Generate(option)));
            return nestedElements;
        }

        /// <summary>
        /// Get the nested parameters from the given option
        /// </summary>
        /// <param name="optionIid">The Iid of the option</param>
        /// <returns>All <see cref="NestedParameter"/> of the given option</returns>
        public List<NestedParameter> GetNestedParameters(Guid? optionIid)
        {
            NestedElementTreeGenerator nestedElementTreeGenerator = new NestedElementTreeGenerator();
            List<NestedParameter> nestedParameters = new List<NestedParameter>();
            var option = this.OpenedIteration.Option.ToList().Find(o => o.Iid == optionIid);
            if (option != null)
            {
                nestedParameters.AddRange(nestedElementTreeGenerator.GetNestedParameters(option));
            }
            return nestedParameters;
        }

        /// <summary>
        /// Get unused elements defintion of the opened iteration
        /// An unused element is an element not used in an option
        /// </summary>
        /// <returns>All unused <see cref="ElementDefinition"/></returns>
        public List<ElementDefinition> GetUnusedElementDefinitions()
        {
            List<NestedElement> nestedElements = this.GetNestedElements();

            List<ElementDefinition> associatedElements = new List<ElementDefinition>();
            nestedElements.ForEach(element => {
                element.ElementUsage.ToList().ForEach(e => associatedElements.Add(e.ElementDefinition));
             });
            associatedElements = associatedElements.Distinct().ToList();

            List<ElementDefinition> unusedElementDefinitions = new List<ElementDefinition>();
            unusedElementDefinitions.AddRange(this.OpenedIteration.Element);

            unusedElementDefinitions.RemoveAll(e => associatedElements.Contains(e));

            return unusedElementDefinitions;
        }

        /// <summary>
        /// Get all the unreferenced element definitions in the opened iteration
        /// An unreferenced element is an element with no associated ElementUsage
        /// </summary>
        /// <returns>All unreferenced <see cref="ElementDefinition"/></returns>
        public List<ElementDefinition> GetUnreferencedElements()
        {
            List<ElementUsage> elementUsages = new List<ElementUsage>();
            this.OpenedIteration.Element.ForEach(e => elementUsages.AddRange(e.ContainedElement));

            List<ElementDefinition> associatedElementDefinitions = new List<ElementDefinition>();
            elementUsages.ForEach(e => associatedElementDefinitions.Add(e.ElementDefinition));

            List<ElementDefinition> unreferencedElementDefinitions = new List<ElementDefinition>();
            unreferencedElementDefinitions.AddRange(this.OpenedIteration.Element);

            unreferencedElementDefinitions.RemoveAll(e => associatedElementDefinitions.Contains(e));

            return unreferencedElementDefinitions;
        }
    }
}
