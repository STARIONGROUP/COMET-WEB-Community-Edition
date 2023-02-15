// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleIterationApplicationBaseViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.ViewModels.Components.Shared
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Utilities.DisposableObject;
    using DynamicData.Binding;

    using ReactiveUI;

    /// <summary>
    /// Base view model for any application that will need only one <see cref="Iteration" />
    /// </summary>
    public abstract class SingleIterationApplicationBaseViewModel : DisposableObject, ISingleIterationApplicationBaseViewModel
    {
        /// <summary>
        /// Backing field for <see cref="CurrentIteration" />
        /// </summary>
        private Iteration currentIteration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleIterationApplicationBaseViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        protected SingleIterationApplicationBaseViewModel(ISessionService sessionService)
        {
            this.Disposables.Add(this.WhenAnyPropertyChanged(nameof(this.CurrentIteration))
                .Subscribe(_ => this.OnIterationChanged()));

            this.Disposables.Add(CDPMessageBus.Current.Listen<DomainChangedEvent>().Subscribe(_ => this.OnDomainChanged()));

            this.SessionService = sessionService;
        }

        /// <summary>
        /// The <see cref="ISessionService" />
        /// </summary>
        protected ISessionService SessionService { get; private set; }

        /// <summary>
        /// Gets the current <see cref="DomainOfExpertise" />
        /// </summary>
        public DomainOfExpertise CurrentDomain { get; protected set; }

        /// <summary>
        /// The current <see cref="Iteration" /> to work with
        /// </summary>
        public Iteration CurrentIteration
        {
            get => this.currentIteration;
            set => this.RaiseAndSetIfChanged(ref this.currentIteration, value);
        }

        /// <summary>
        /// Handles the change of <see cref="DomainOfExpertise" />
        /// </summary>
        protected virtual void OnDomainChanged()
        {
            this.CurrentDomain = this.CurrentIteration == null ? null : this.SessionService.GetDomainOfExpertise(this.CurrentIteration);
        }

        /// <summary>
        /// Update this view model properties when the <see cref="Iteration" /> has changed
        /// </summary>
        protected abstract void OnIterationChanged();
    }
}
