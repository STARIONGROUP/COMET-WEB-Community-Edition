// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EngineeringModelBodyViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.EngineeringModel
{
    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.ViewModels.Components.EngineeringModel.Options;
    using COMETwebapp.ViewModels.Components.EngineeringModel.Publications;

    /// <summary>
    /// View Model that handle the logic for the Engineering model body application
    /// </summary>
    public class EngineeringModelBodyViewModel : SingleIterationApplicationBaseViewModel, IEngineeringModelBodyViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="IOptionsTableViewModel"/>
        /// </summary>
        public IOptionsTableViewModel OptionsTableViewModel { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IPublicationsTableViewModel"/>
        /// </summary>
        public IPublicationsTableViewModel PublicationsTableViewModel { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelBodyViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="optionsTableViewModel">The <see cref="IOptionsTableViewModel"/></param>
        /// <param name="publicationsTableViewModel">The <see cref="IPublicationsTableViewModel"/></param>
        public EngineeringModelBodyViewModel(ISessionService sessionService, ICDPMessageBus messageBus, IOptionsTableViewModel optionsTableViewModel, IPublicationsTableViewModel publicationsTableViewModel) 
            : base(sessionService, messageBus)
        {
            this.OptionsTableViewModel = optionsTableViewModel;
            this.PublicationsTableViewModel = publicationsTableViewModel;
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed() => Task.CompletedTask;
        
        /// <summary>
        /// Update this view model properties
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            await base.OnThingChanged();

            this.OptionsTableViewModel.SetCurrentIteration(this.CurrentThing);
            this.PublicationsTableViewModel.SetCurrentIteration(this.CurrentThing);
            this.IsLoading = false;
        }
    }
}
