// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleEngineeringModelApplicationTemplateViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.ViewModels.Components.Applications
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    /// <summary>
    /// ViewModel that will englobe all applications where only one <see cref="EngineeringModel" /> needs to be selected
    /// </summary>
    public class SingleEngineeringModelApplicationTemplateViewModel : SingleThingApplicationTemplateViewModel<EngineeringModel>, ISingleEngineeringModelApplicationTemplateViewModel
    {
        /// <summary>
        /// Initializes a new <see cref="SingleEngineeringModelApplicationTemplateViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="modelSelector">The <see cref="IEngineeringModelSelectorViewModel" /></param>
        public SingleEngineeringModelApplicationTemplateViewModel(ISessionService sessionService, IEngineeringModelSelectorViewModel modelSelector) : base(sessionService, modelSelector)
        {
            this.Disposables.Add(this.SessionService.OpenIterations.CountChanged.Subscribe(_ => this.OnOpenIterationCountChanged()));
        }

        /// <summary>
        /// Gets the <see cref="IEngineeringModelSelectorViewModel" />
        /// </summary>
        public IEngineeringModelSelectorViewModel EngineeringModelSelectorViewModel => this.SelectorViewModel as IEngineeringModelSelectorViewModel;

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        protected override void UpdateProperties()
        {
            this.SelectorViewModel.UpdateProperties(this.SessionService.OpenEngineeringModels);
        }

        /// <summary>
        /// Handles the change of opened <see cref="Iteration" />
        /// </summary>
        private void OnOpenIterationCountChanged()
        {
            if (this.SessionService.OpenEngineeringModels.Count is 0 or 1)
            {
                this.SelectedThing = this.SessionService.OpenEngineeringModels.FirstOrDefault();
            }
        }
    }
}
