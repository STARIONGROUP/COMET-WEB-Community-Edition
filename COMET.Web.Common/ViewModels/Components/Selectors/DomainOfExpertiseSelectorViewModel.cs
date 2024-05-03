// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DomainOfExpertiseSelectorViewModel.cs" company="Starion Group S.A.">
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

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;

    using Microsoft.AspNetCore.Components;

    using ReactiveUI;

    /// <summary>
    /// View Model that enables the user to select an <see cref="ActualFiniteState" />
    /// </summary>
    public class DomainOfExpertiseSelectorViewModel : BelongsToIterationSelectorViewModel, IDomainOfExpertiseSelectorViewModel
    {
        /// <summary>
        /// Backing field for <see cref="SelectedDomainOfExpertise" />
        /// </summary>
        private DomainOfExpertise selectedDomainOfExpertise;

        /// <summary>
        /// Creates a new instance of <see cref="DomainOfExpertiseSelectorViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        public DomainOfExpertiseSelectorViewModel(ISessionService sessionService, ICDPMessageBus messageBus)
        {
            this.SessionService = sessionService;
            this.AvailableDomainsOfExpertise = sessionService.GetSiteDirectory().Domain;

            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedDomainOfExpertise).SubscribeAsync(this.OnSelectedDomainOfExpertiseChange.InvokeAsync));
            this.Disposables.Add(messageBus.Listen<DomainChangedEvent>().Subscribe(this.OnDomainChanged));
        }

        /// <summary>
        /// Gets or sets the callback that is executed when the <see cref="SelectedDomainOfExpertise"/> property has changed
        /// </summary>
        public EventCallback<DomainOfExpertise> OnSelectedDomainOfExpertiseChange { get; set; }

        /// <summary>
        /// Gets the <see cref="ISessionService" />
        /// </summary>
        public ISessionService SessionService { get; private set; }

        /// <summary>
        /// A collection of available <see cref="DomainOfExpertise" />
        /// </summary>
        public IEnumerable<DomainOfExpertise> AvailableDomainsOfExpertise { get; set; }

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise" /> from the current <see cref="Iteration" />
        /// </summary>
        public DomainOfExpertise CurrentIterationDomain { get; set; }

        /// <summary>
        /// The currently selected <see cref="DomainOfExpertise" />
        /// </summary>
        public DomainOfExpertise SelectedDomainOfExpertise
        {
            get => this.selectedDomainOfExpertise;
            set => this.RaiseAndSetIfChanged(ref this.selectedDomainOfExpertise, value);
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        protected override void UpdateProperties()
        {
            this.CurrentIterationDomain = this.SessionService.GetDomainOfExpertise(this.CurrentIteration);
            this.SelectedDomainOfExpertise = this.CurrentIterationDomain;
        }

        /// <summary>
        /// Method executed when an iteration domain has changed
        /// </summary>
        /// <param name="domainChangedEvent">The <see cref="DomainChangedEvent"/> data</param>
        private void OnDomainChanged(DomainChangedEvent domainChangedEvent)
        {
            if (domainChangedEvent.Iteration.Iid == this.CurrentIteration.Iid)
            {
                this.CurrentIterationDomain = domainChangedEvent.SelectedDomain;
            }
        }
    }
}
