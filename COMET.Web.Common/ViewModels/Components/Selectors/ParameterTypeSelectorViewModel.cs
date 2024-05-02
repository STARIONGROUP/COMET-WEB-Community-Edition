// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeSelectorViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.ViewModels.Components.Selectors
{
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
        /// Filter the collection of the <see cref="IParameterTypeSelectorViewModel.AvailableParameterTypes" /> with provided values
        /// </summary>
        /// <param name="parameterTypesId">A collection of <see cref="Guid" /> for <see cref="ParameterType" /></param>
        public void FilterAvailableParameterTypes(IEnumerable<Guid> parameterTypesId)
        {
            this.AvailableParameterTypes = this.allAvailableParameterTypes.Where(x => parameterTypesId.Any(p => p == x.Iid));
            this.SelectedParameterType = this.AvailableParameterTypes.FirstOrDefault(x => x.Iid == this.SelectedParameterType?.Iid);
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        protected override void UpdateProperties()
        {
            this.SelectedParameterType = null;
            this.allAvailableParameterTypes = this.CurrentIteration?.QueryUsedParameterTypes().OrderBy(x => x.Name) ?? Enumerable.Empty<ParameterType>();
            this.AvailableParameterTypes = new List<ParameterType>(this.allAvailableParameterTypes);
        }
    }
}
