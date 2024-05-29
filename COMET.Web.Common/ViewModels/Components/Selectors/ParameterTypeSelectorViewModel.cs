// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeSelectorViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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
    /// View Model that enables the user to select an <see cref="ParameterType" />
    /// </summary>
    public class ParameterTypeSelectorViewModel : BelongsToIterationSelectorViewModel, IParameterTypeSelectorViewModel
    {
        /// <summary>
        /// A collection of all available <see cref="ParameterType" />
        /// </summary>
        private IEnumerable<ParameterType> allAvailableParameterTypes = new List<ParameterType>();

        /// <summary>
        /// Backing field for <see cref="SelectedParameterType" />
        /// </summary>
        private ParameterType selectedParameterType;

        /// <summary>
        /// Gets or sets the value to check if only the parameter types used in the current <see cref="Iteration" /> should be
        /// queried
        /// </summary>
        public bool QueryOnlyUsedParameterTypes { get; set; } = true;

        /// <summary>
        /// The currently selected <see cref="ParameterType" />
        /// </summary>
        public ParameterType SelectedParameterType
        {
            get => this.selectedParameterType;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameterType, value);
        }

        /// <summary>
        /// A collection of available <see cref="ParameterType" />
        /// </summary>
        public IEnumerable<ParameterType> AvailableParameterTypes { get; private set; } = Enumerable.Empty<ParameterType>();

        /// <summary>
        /// Filter the collection of the <see cref="IParameterTypeSelectorViewModel.AvailableParameterTypes" /> with provided
        /// values
        /// </summary>
        /// <param name="parameterTypesId">A collection of <see cref="Guid" /> for <see cref="ParameterType" /></param>
        public void FilterAvailableParameterTypes(IEnumerable<Guid> parameterTypesId)
        {
            this.AvailableParameterTypes = this.allAvailableParameterTypes.Where(x => parameterTypesId.Any(p => p == x.Iid));
            this.SelectedParameterType = this.AvailableParameterTypes.FirstOrDefault(x => x.Iid == this.SelectedParameterType?.Iid);
        }

        /// <summary>
        /// Excludes a collection of <see cref="ParameterType" />s from the <see cref="AvailableParameterTypes" />
        /// </summary>
        /// <param name="parameterTypesId">A collection of <see cref="Guid" /> for <see cref="ParameterType" /></param>
        public void ExcludeAvailableParameterTypes(IEnumerable<Guid> parameterTypesId)
        {
            this.AvailableParameterTypes = this.allAvailableParameterTypes.Where(x => !parameterTypesId.Contains(x.Iid));
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        protected override void UpdateProperties()
        {
            this.SelectedParameterType = null;
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

            this.allAvailableParameterTypes = parameterTypes.OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
            this.AvailableParameterTypes = new List<ParameterType>(this.allAvailableParameterTypes);
        }
    }
}
