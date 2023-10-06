// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleIterationApplicationTemplate.razor.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Components
{
    using System;
    using System.Reactive.Linq;

    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.WebUtilities;

    using ReactiveUI;

    /// <summary>
    /// Shared component that will englobe all applications where only one <see cref="Iteration" /> needs to be selected
    /// </summary>
    public partial class SingleIterationApplicationTemplate: DisposableComponent
    {
        /// <summary>
        /// Body of the application
        /// </summary>
        [Parameter]
        public RenderFragment Body { get; set; }

        /// <summary>
        /// A property to show the active model
        /// </summary>
        [Parameter]
        public bool ShowActiveModel { get; set; }

        /// <summary>
        /// The <see cref="Guid" /> of selected <see cref="Iteration" />
        /// </summary>
        [Parameter]
        public Guid IterationId { get; set; }

        /// <summary>
        /// The <see cref="ISingleIterationApplicationTemplateViewModel" />
        /// </summary>
        [Inject]
        public ISingleIterationApplicationTemplateViewModel ViewModel { get; set; }

        /// <summary>
        /// The <see cref="NavigationManager" />
        /// </summary>
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        /// <summary>
        /// The <see cref="IConfigurationService" />
        /// </summary>
        [Inject]
        public IConfigurationService ServerConnectionService { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.IsOnIterationSelectionMode)
                .Merge(this.ViewModel.SessionService.OpenIterations.CountChanged.Select(_ => true))
                .Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));

            this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.SelectedIteration)
                .Select(_ => true)
                .Merge(CDPMessageBus.Current.Listen<DomainChangedEvent>().Select(_ => true))
                .Subscribe(_ => this.InvokeAsync(this.SetCorrectUrl)));
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (this.IterationId == Guid.Empty)
            {
                switch (this.ViewModel.SessionService.OpenIterations.Count)
                {
                    case 1:
                        this.ViewModel.SelectIteration(this.ViewModel.SessionService.OpenIterations.Items.First());
                        break;
                    case > 1:
                        this.ViewModel.AskToSelectIteration();
                        break;
                }
            }
            else if(this.IterationId != Guid.Empty && this.ViewModel.SelectedIteration == null)
            {
                var iteration = this.ViewModel.SessionService.OpenIterations.Items.FirstOrDefault(x => x.Iid == this.IterationId);

                if (iteration != null)
                {
                    this.ViewModel.SelectIteration(iteration);
                }
                else
                {
                    this.IterationId = Guid.Empty;
                }
            }
        }

        /// <summary>
        /// Sets the correct url based on the selected <see cref="Iteration" />
        /// </summary>
        internal void SetCorrectUrl()
        {
            var urlPage = this.NavigationManager.Uri.Replace(this.NavigationManager.BaseUri, string.Empty).Split('?')[0];

            var currentOptions = this.NavigationManager.Uri.GetParametersFromUrl();

            if (this.ViewModel.SelectedIteration != null)
            {
                currentOptions[QueryKeys.IterationKey] = this.ViewModel.SelectedIteration.Iid.ToShortGuid();
                currentOptions[QueryKeys.ModelKey] = this.ViewModel.SelectedIteration.IterationSetup.Container.Iid.ToShortGuid();

                if (string.IsNullOrEmpty(this.ServerConnectionService.ServerConfiguration.ServerAddress))
                {
                    currentOptions[QueryKeys.ServerKey] = this.ViewModel.SessionService.Session.DataSourceUri;

                }
                currentOptions[QueryKeys.DomainKey] = this.ViewModel.SessionService.GetDomainOfExpertise(this.ViewModel.SelectedIteration).Iid.ToShortGuid();
            }
            else
            {
                currentOptions.Clear();
            }

            var targetOptions = new Dictionary<string, string>();

            foreach (var currentOption in currentOptions.Where(x => !string.IsNullOrEmpty(x.Value)))
            {
                targetOptions[currentOption.Key] = currentOption.Value;
            }

            var targetUrl = QueryHelpers.AddQueryString(urlPage, targetOptions);
            this.NavigationManager.NavigateTo(targetUrl);
        }
    }
}
