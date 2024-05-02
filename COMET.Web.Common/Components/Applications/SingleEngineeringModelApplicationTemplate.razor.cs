// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleEngineeringModelApplicationTemplate.razor.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Components.Applications
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Utilities;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Shared component that will englobe all applications where only one <see cref="EngineeringModel" /> needs to be selected
    /// </summary>
    public partial class SingleEngineeringModelApplicationTemplate
    {
        /// <summary>
        /// The <see cref="Guid" /> of selected <see cref="EngineeringModel" />
        /// </summary>
        [Parameter]
        public Guid EngineeringModelId { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            this.UpdateProperties();
            base.OnInitialized();
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            this.UpdateProperties();
        }

        /// <summary>
        /// Update properties of the viewmodel based on provided parameters
        /// </summary>
        private void UpdateProperties()
        {
            if (this.EngineeringModelId == Guid.Empty)
            {
                switch (this.ViewModel.SessionService.OpenEngineeringModels.Count)
                {
                    case 1:
                        this.ViewModel.OnThingSelect(this.ViewModel.SessionService.OpenEngineeringModels.First());
                        break;
                    case > 1:
                        this.ViewModel.AskToSelectThing();
                        break;
                }
            }
            else if (this.EngineeringModelId != Guid.Empty && this.ViewModel.SelectedThing == null)
            {
                this.ViewModel.OnThingSelect(this.ViewModel.SessionService.OpenEngineeringModels.FirstOrDefault(x => x.EngineeringModelSetup.Iid == this.EngineeringModelId));
            }

            this.EngineeringModelId = this.ViewModel.SelectedThing?.Iid ?? Guid.Empty;
        }

        /// <summary>
        /// Set URL parameters based on the current context
        /// </summary>
        /// <param name="currentOptions">A <see cref="Dictionary{TKey,TValue}" /> of URL parameters</param>
        protected override void SetUrlParameters(Dictionary<string, string> currentOptions)
        {
            base.SetUrlParameters(currentOptions);

            currentOptions[QueryKeys.ModelKey] = this.ViewModel.SelectedThing.EngineeringModelSetup.Iid.ToShortGuid();
        }
    }
}
