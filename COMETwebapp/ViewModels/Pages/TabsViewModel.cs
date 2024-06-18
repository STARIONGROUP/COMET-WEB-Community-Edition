// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TabsViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Pages
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Model;

    using DynamicData;
    using DynamicData.Binding;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="TabsViewModel" /> contains logic and behavior that are required to support multi-tabs application
    /// </summary>
    public class TabsViewModel : DisposableObject, ITabsViewModel
    {
        /// <summary>
        /// Gets the injected <see cref="IServiceProvider" />
        /// </summary>
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Gets the injected <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Backing field for <see cref="SelectedApplication" />
        /// </summary>
        private TabbedApplication selectedApplication;

        /// <summary>
        /// Initializes a new instance of <see cref="TabsViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider" /></param>
        public TabsViewModel(ISessionService sessionService, IServiceProvider serviceProvider)
        {
            this.sessionService = sessionService;
            this.serviceProvider = serviceProvider;
            this.Disposables.Add(this.WhenPropertyChanged(x => x.SelectedApplication).Subscribe(_ => this.InitializeViewModelBasedOnApplication()));
            this.Disposables.Add(this.sessionService.OpenIterations.CountChanged.Subscribe(_ => this.CloseTabIfIterationClosed()));

            this.Disposables.Add(this.OpenTabs.Connect().WhereReasonsAre(ListChangeReason.Remove, ListChangeReason.RemoveRange).Subscribe(changeSet =>
            {
                foreach (var result in changeSet)
                {
                    result.Item.Current.ApplicationBaseViewModel.IsAllowedToDispose = true;
                }
            }));
        }

        /// <summary>
        /// Gets the collection of all <see cref="TabbedApplicationInformation" />
        /// </summary>
        public SourceList<TabbedApplicationInformation> OpenTabs { get; } = new();

        /// <summary>
        /// Gets or sets the current tab
        /// </summary>
        public TabbedApplicationInformation CurrentTab { get; set; }

        /// <summary>
        /// Gets the collection of available <see cref="TabbedApplication" />
        /// </summary>
        public IEnumerable<TabbedApplication> AvailableApplications => Applications.ExistingApplications.OfType<TabbedApplication>();

        /// <summary>
        /// Gets or sets the current selected <see cref="TabbedApplication" />
        /// </summary>
        public TabbedApplication SelectedApplication
        {
            get => this.selectedApplication;
            set
            {
                this.selectedApplication = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the <see cref="TabbedApplicationInformation" /> based on the selected <see cref="TabbedApplication" />
        /// </summary>
        private void InitializeViewModelBasedOnApplication()
        {
            this.OpenTabs.Clear();
            
            if (this.SelectedApplication == null)
            {
                return;
            }

            if (this.serviceProvider.GetService(this.SelectedApplication.ViewModelType) is not IApplicationBaseViewModel viewModel)
            {
                return;
            }

            viewModel.IsAllowedToDispose = false;
            object thingOfInterest = default;

            if (this.SelectedApplication.ThingTypeOfInterest == typeof(Iteration))
            {
                thingOfInterest = this.sessionService.OpenIterations.Items.FirstOrDefault();
            }

            if (this.SelectedApplication.ThingTypeOfInterest == typeof(EngineeringModel))
            {
                thingOfInterest = this.sessionService.OpenEngineeringModels.FirstOrDefault();
            }

            if (thingOfInterest != null)
            {
                this.OpenTabs.Add(new TabbedApplicationInformation(viewModel, this.SelectedApplication.ComponentType, thingOfInterest));
            }

            this.CurrentTab = this.OpenTabs.Items.FirstOrDefault();
        }

        /// <summary>
        /// Closes a tab if its iteration has been closed
        /// </summary>
        private void CloseTabIfIterationClosed()
        {
            var tabsToClose = this.OpenTabs.Items.Where(x => !this.sessionService.OpenIterations.Items.Contains(x.ObjectOfInterest));
            this.OpenTabs.RemoveMany(tabsToClose);
        }
    }
}
