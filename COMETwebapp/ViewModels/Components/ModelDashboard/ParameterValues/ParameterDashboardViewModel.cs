// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterDashboardViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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
        /// <param name="selectedOption">The current <see cref="Option" /></param>
        /// <param name="selectedState">The current <see cref="ActualFiniteState" /></param>
        /// <param name="selectedParameterType">The current <see cref="ParameterType" /></param>
        /// <param name="currentDomain">The current <see cref="DomainOfExpertise" /></param>
        /// <param name="availableDomains">A collection of available <see cref="DomainOfExpertise" /></param>
        public void UpdateProperties(Iteration iteration, Option selectedOption, ActualFiniteState selectedState, ParameterType selectedParameterType, DomainOfExpertise currentDomain,
            IEnumerable<DomainOfExpertise> availableDomains)
        {
            this.ValueSets.Clear();

            if (iteration == null)
            {
                return;
            }

            this.CurrentDomain = currentDomain;
            this.AvailableDomains = availableDomains;
            this.ValueSets.AddRange(FilterValueSets(iteration, selectedOption, selectedState, selectedParameterType));
        }

        /// <summary>
        /// Filters the <see cref="ParameterValueSetBase" /> that are contained into an <see cref="Iteration" />
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <param name="selectedOption">The selected <see cref="Option" /></param>
        /// <param name="selectedState">The selected <see cref="ActualFiniteState" /></param>
        /// <param name="selectedParameterType">The selected <see cref="ParameterType" /></param>
        /// <returns>A collection of filtered <see cref="ParameterValueSetBase" /></returns>
        private static IEnumerable<ParameterValueSetBase> FilterValueSets(Iteration iteration, Option selectedOption,
            ActualFiniteState selectedState, ParameterType selectedParameterType)
        {
            var valuesSets = iteration.QueryParameterValueSetBase().ToList();

            if (selectedOption != null)
            {
                var nestedParameters = iteration.QueryNestedParameters(selectedOption).ToList();

                if (nestedParameters.Any())
                {
                    valuesSets = valuesSets.Where(x => nestedParameters.Select(p => p.ValueSet).Contains(x)).ToList();
                }
                else
                {
                    return Enumerable.Empty<ParameterValueSetBase>();
                }
            }

            if (selectedState != null)
            {
                valuesSets.RemoveAll(v => v.ActualState?.Iid != selectedState.Iid);
            }

            if (selectedParameterType != null)
            {
                valuesSets.RemoveAll(v => ((ParameterOrOverrideBase)v.Container).ParameterType.Iid != selectedParameterType.Iid);
            }

            return valuesSets;
        }
    }
}
