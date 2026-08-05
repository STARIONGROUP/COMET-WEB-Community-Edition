// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MultiElementBaseSelectorViewModel.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.Extensions;

    using ReactiveUI;

    /// <summary>
    /// ViewModel used to select one or more <see cref="ElementBase" />s through a collection of existing <see cref="ElementBase" />
    /// </summary>
    public class MultiElementBaseSelectorViewModel : BelongsToIterationSelectorViewModel, IMultiElementBaseSelectorViewModel
    {
        /// <summary>
        /// Backing field for <see cref="SelectedElementBases" />
        /// </summary>
        private IEnumerable<ElementBase> selectedElementBases = new List<ElementBase>();

        /// <summary>
        /// A collection of available <see cref="ElementBase" />
        /// </summary>
        public IEnumerable<ElementBase> AvailableElements { get; private set; } = Enumerable.Empty<ElementBase>();

        /// <summary>
        /// The currently selected <see cref="ElementBase" />s
        /// </summary>
        public IEnumerable<ElementBase> SelectedElementBases
        {
            get => this.selectedElementBases;
            set => this.RaiseAndSetIfChanged(ref this.selectedElementBases, value ?? new List<ElementBase>());
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        protected override void UpdateProperties()
        {
            this.SelectedElementBases = new List<ElementBase>();

            this.AvailableElements = this.CurrentIteration?.QueryUsedElementDefinitions().OrderBy(x => x.Name) ?? Enumerable.Empty<ElementBase>();
        }
    }
}
