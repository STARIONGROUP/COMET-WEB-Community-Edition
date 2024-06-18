// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenModel.razor.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Components
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.ViewModels.Components;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// Component used to open an <see cref="EngineeringModel" /> and select an <see cref="Iteration" />
    /// </summary>
    public partial class OpenModel
    {
        /// <summary>
        /// Gets or sets the <see cref="IStringTableService"/>
        /// </summary>
        [Inject]
        public IStringTableService ConfigurationService { get; set; }

        /// <summary>
        /// Value asserting that selectors for <see cref="EngineeringModel" /> and <see cref="Iteration" /> are enabled
        /// </summary>
        private bool selectorEnabled;

        /// <summary>
        /// The <see cref="IOpenModelViewModel" />
        /// </summary>
        [Inject]
        public IOpenModelViewModel ViewModel { get; set; }

        /// <summary>
        /// The <see cref="Guid" /> of a requested <see cref="EngineeringModelSetup" />
        /// </summary>
        [Parameter]
        public Guid ModelId { get; set; }

        /// <summary>
        /// The <see cref="Guid" /> of a requested <see cref="Iteration" />
        /// </summary>
        [Parameter]
        public Guid IterationId { get; set; }

        /// <summary>
        /// The <see cref="Guid" /> of a requested <see cref="DomainOfExpertise" />
        /// </summary>
        [Parameter]
        public Guid DomainId { get; set; }

        /// <summary>
        /// Value asserting that the button is enabled
        /// </summary>
        public bool ButtonEnabled => this.AreRequiredFieldSelected() && !this.ViewModel.IsOpeningSession;

        /// <summary>
        /// The display text of the button
        /// </summary>
        public string ButtonText => this.ViewModel.IsOpeningSession ? "Opening" : "Open";

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Initialize(this.ViewModel);
        }

        /// <summary>
        /// Initializes the current <see cref="ViewModel"/>
        /// </summary>
        /// <param name="viewModel">The <see cref="IOpenModelViewModel"/> to initialize</param>
        protected void Initialize(IOpenModelViewModel viewModel)
        {
            this.ViewModel = viewModel;
            this.ViewModel.InitializesProperties();
            this.Disposables.Add(this.ViewModel);

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.SelectedEngineeringModel,
                    x => x.ViewModel.SelectedIterationSetup,
                    x => x.ViewModel.SelectedDomainOfExpertise,
                    x => x.ViewModel.IsOpeningSession)
                .SubscribeAsync(_ => this.InvokeAsync(this.StateHasChanged)));
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            this.selectorEnabled = true;

            if (this.ModelId != Guid.Empty && this.IterationId != Guid.Empty && this.DomainId != Guid.Empty)
            {
                this.selectorEnabled = false;
                this.ViewModel.PreSelectIteration(this.ModelId, this.IterationId, this.DomainId);
            }
        }

        /// <summary>
        /// Verifies that all required field are selected
        /// </summary>
        /// <returns>True if all required field are selected</returns>
        protected virtual bool AreRequiredFieldSelected()
        {
            return this.ViewModel.SelectedEngineeringModel != null
                   && this.ViewModel.SelectedDomainOfExpertise != null
                   && this.ViewModel.SelectedIterationSetup != null;
        }
    }
}
