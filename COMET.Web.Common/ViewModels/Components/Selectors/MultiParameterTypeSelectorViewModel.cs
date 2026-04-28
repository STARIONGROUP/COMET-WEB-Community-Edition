// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MultiParameterTypeSelectorViewModel.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.ViewModels.Components.Selectors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using ReactiveUI;

    /// <summary>
    /// Default <see cref="IMultiParameterTypeSelectorViewModel" /> implementation. Mirrors the single-select
    /// <see cref="ParameterTypeSelectorViewModel" /> but exposes a collection-valued selection so callers can
    /// filter on more than one <see cref="ParameterType" /> at a time.
    /// </summary>
    public class MultiParameterTypeSelectorViewModel : BelongsToIterationSelectorViewModel, IMultiParameterTypeSelectorViewModel
    {
        /// <summary>
        /// The full set of <see cref="ParameterType" />s computed from the current <see cref="Iteration" />,
        /// before any user-side filtering or exclusion is applied.
        /// </summary>
        private IEnumerable<ParameterType> allAvailableParameterTypes = new List<ParameterType>();

        /// <summary>
        /// Backing field for <see cref="SelectedParameterTypes" />.
        /// </summary>
        private IEnumerable<ParameterType> selectedParameterTypes = new List<ParameterType>();

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="UpdateProperties" /> should restrict
        /// <see cref="AvailableParameterTypes" /> to the <see cref="ParameterType" />s actually used by parameters
        /// of the current <see cref="Iteration" /> (the default), or expose every <see cref="ParameterType" />
        /// reachable through the available reference data libraries.
        /// </summary>
        public bool QueryOnlyUsedParameterTypes { get; set; } = true;

        /// <summary>
        /// Gets or sets the currently selected <see cref="ParameterType" />s. An empty collection means
        /// "no filter" — every <see cref="ParameterType" /> in <see cref="AvailableParameterTypes" /> matches.
        /// </summary>
        public IEnumerable<ParameterType> SelectedParameterTypes
        {
            get => this.selectedParameterTypes;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameterTypes, value ?? new List<ParameterType>());
        }

        /// <summary>
        /// Gets the collection of <see cref="ParameterType" />s that the user can pick from.
        /// </summary>
        public IEnumerable<ParameterType> AvailableParameterTypes { get; private set; } = Enumerable.Empty<ParameterType>();

        /// <summary>
        /// Restricts the <see cref="AvailableParameterTypes" /> collection to only the entries whose
        /// <see cref="ParameterType.Iid" /> appears in <paramref name="parameterTypesId" />. Any entries in
        /// <see cref="SelectedParameterTypes" /> that no longer appear in <see cref="AvailableParameterTypes" />
        /// are dropped from the selection.
        /// </summary>
        /// <param name="parameterTypesId">A collection of <see cref="Guid" />s identifying the <see cref="ParameterType" />s to retain.</param>
        public void FilterAvailableParameterTypes(IEnumerable<Guid> parameterTypesId)
        {
            var allowedIds = parameterTypesId as ICollection<Guid> ?? parameterTypesId.ToList();
            this.AvailableParameterTypes = this.allAvailableParameterTypes.Where(x => allowedIds.Any(p => p == x.Iid)).ToList();
            this.SelectedParameterTypes = this.SelectedParameterTypes.Where(x => allowedIds.Contains(x.Iid)).ToList();
        }

        /// <summary>
        /// Excludes the entries whose <see cref="ParameterType.Iid" /> appears in <paramref name="parameterTypesId" />
        /// from <see cref="AvailableParameterTypes" />.
        /// </summary>
        /// <param name="parameterTypesId">A collection of <see cref="Guid" />s identifying the <see cref="ParameterType" />s to exclude.</param>
        public void ExcludeAvailableParameterTypes(IEnumerable<Guid> parameterTypesId)
        {
            var excluded = parameterTypesId as ICollection<Guid> ?? parameterTypesId.ToList();
            this.AvailableParameterTypes = this.allAvailableParameterTypes.Where(x => !excluded.Contains(x.Iid)).ToList();
            this.SelectedParameterTypes = this.SelectedParameterTypes.Where(x => !excluded.Contains(x.Iid)).ToList();
        }

        /// <summary>
        /// Recomputes <see cref="AvailableParameterTypes" /> from the current <see cref="Iteration" /> and
        /// resets <see cref="SelectedParameterTypes" /> to the empty selection.
        /// </summary>
        protected override void UpdateProperties()
        {
            this.SelectedParameterTypes = new List<ParameterType>();
            IEnumerable<ParameterType> parameterTypes;

            if (this.QueryOnlyUsedParameterTypes)
            {
                parameterTypes = this.CurrentIteration?
                    .QueryUsedParameterTypes()
                    .OrderBy(x => x.Name) ?? Enumerable.Empty<ParameterType>();
            }
            else
            {
                var siteDirectory = this.CurrentIteration.IterationSetup.GetContainerOfType<SiteDirectory>();

                parameterTypes = siteDirectory
                    .AvailableReferenceDataLibraries()
                    .SelectMany(x => x.ParameterType);
            }

            this.allAvailableParameterTypes = parameterTypes.OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase).ToList();
            this.AvailableParameterTypes = new List<ParameterType>(this.allAvailableParameterTypes);
        }
    }
}
