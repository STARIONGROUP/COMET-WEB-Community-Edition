// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MultiOptionSelectorViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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
    using CDP4Common.EngineeringModelData;

    using ReactiveUI;

    /// <summary>
    /// View Model that provide capability to select one or more <see cref="Option" />s
    /// </summary>
    public class MultiOptionSelectorViewModel : BelongsToIterationSelectorViewModel, IMultiOptionSelectorViewModel
    {
        /// <summary>
        /// Backing field for <see cref="SelectedOptions" />
        /// </summary>
        private IEnumerable<Option> selectedOptions = new List<Option>();

        /// <summary>
        /// The currently selected <see cref="Option" />s
        /// </summary>
        public IEnumerable<Option> SelectedOptions
        {
            get => this.selectedOptions;
            set => this.RaiseAndSetIfChanged(ref this.selectedOptions, value ?? new List<Option>());
        }

        /// <summary>
        /// A collection of available <see cref="Option" />
        /// </summary>
        public IEnumerable<Option> AvailableOptions { get; private set; } = Enumerable.Empty<Option>();

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        protected override void UpdateProperties()
        {
            this.SelectedOptions = new List<Option>();

            this.AvailableOptions = this.CurrentIteration?.Option.OrderBy(x => x.Name) ?? Enumerable.Empty<Option>();
        }
    }
}
