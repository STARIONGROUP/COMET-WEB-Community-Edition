// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterDashboardViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
//
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.ModelDashboard.ParameterValues
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Extensions;

    using DynamicData;

    /// <summary>
    /// View model that provides information related to <see cref="ParameterValueSetBase" />
    /// </summary>
    public class ParameterDashboardViewModel : IParameterDashboardViewModel
    {
        /// <summary>
        /// A collection of <see cref="ParameterValueSetBase" />
        /// </summary>
        public SourceList<ParameterValueSetBase> ValueSets { get; } = new();

        /// <summary>
        /// A collection of available <see cref="DomainOfExpertise" />
        /// </summary>
        public IEnumerable<DomainOfExpertise> AvailableDomains { get; private set; }

        /// <summary>
        /// The current <see cref="DomainOfExpertise" />
        /// </summary>
        public DomainOfExpertise CurrentDomain { get; private set; }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="iteration">The current <see cref="Iteration" /></param>
        /// <param name="selectedOptions">
        /// The collection of selected <see cref="Option" />s. <c>null</c> or an empty collection means no option
        /// filtering is applied.
        /// </param>
        /// <param name="selectedStates">
        /// The collection of selected <see cref="ActualFiniteState" />s. <c>null</c> or an empty collection means
        /// no state filtering is applied.
        /// </param>
        /// <param name="selectedParameterTypes">
        /// The collection of selected <see cref="ParameterType" />s. <c>null</c> or an empty collection means no
        /// parameter-type filtering is applied.
        /// </param>
        /// <param name="currentDomain">The current <see cref="DomainOfExpertise" /></param>
        /// <param name="availableDomains">A collection of available <see cref="DomainOfExpertise" /></param>
        public void UpdateProperties(Iteration iteration, IEnumerable<Option> selectedOptions, IEnumerable<ActualFiniteState> selectedStates, IEnumerable<ParameterType> selectedParameterTypes, DomainOfExpertise currentDomain,
            IEnumerable<DomainOfExpertise> availableDomains)
        {
            this.ValueSets.Clear();

            if (iteration == null)
            {
                return;
            }

            this.CurrentDomain = currentDomain;
            this.AvailableDomains = availableDomains;
            this.ValueSets.AddRange(FilterValueSets(iteration, selectedOptions, selectedStates, selectedParameterTypes));
        }

        /// <summary>
        /// Filters the <see cref="ParameterValueSetBase" /> that are contained into an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <param name="selectedOptions">
        /// The collection of selected <see cref="Option" />s. <c>null</c> or an empty collection means no option
        /// filtering is applied.
        /// </param>
        /// <param name="selectedStates">
        /// The collection of selected <see cref="ActualFiniteState" />s. <c>null</c> or an empty collection means
        /// no state filtering is applied.
        /// </param>
        /// <param name="selectedParameterTypes">
        /// The collection of selected <see cref="ParameterType" />s. <c>null</c> or an empty collection means no
        /// parameter-type filtering is applied.
        /// </param>
        /// <returns>A collection of filtered <see cref="ParameterValueSetBase" /></returns>
        private static IEnumerable<ParameterValueSetBase> FilterValueSets(Iteration iteration, IEnumerable<Option> selectedOptions,
            IEnumerable<ActualFiniteState> selectedStates, IEnumerable<ParameterType> selectedParameterTypes)
        {
            var valuesSets = iteration.QueryParameterValueSetBase().ToList();

            var options = selectedOptions?.ToList() ?? new List<Option>();

            if (options.Count > 0)
            {
                var nestedValueSetIds = options.SelectMany(option => iteration.QueryNestedParameters(option))
                    .Select(p => ((ParameterValueSetBase)p.ValueSet).Iid)
                    .ToHashSet();

                if (nestedValueSetIds.Count > 0)
                {
                    valuesSets = valuesSets.Where(x => nestedValueSetIds.Contains(x.Iid)).ToList();
                }
                else
                {
                    return Enumerable.Empty<ParameterValueSetBase>();
                }
            }

            var stateIds = selectedStates?.Select(x => x.Iid).ToHashSet() ?? new HashSet<Guid>();

            if (stateIds.Count > 0)
            {
                valuesSets.RemoveAll(v => v.ActualState == null || !stateIds.Contains(v.ActualState.Iid));
            }

            var parameterTypeIds = selectedParameterTypes?.Select(x => x.Iid).ToHashSet() ?? new HashSet<Guid>();

            if (parameterTypeIds.Count > 0)
            {
                valuesSets.RemoveAll(v => !parameterTypeIds.Contains(((ParameterOrOverrideBase)v.Container).ParameterType.Iid));
            }

            return valuesSets;
        }
    }
}
