// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActiveDomainsTableViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels
{
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.Common.BaseDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    /// <summary>
    /// View model used to manage active <see cref="DomainOfExpertise" /> related to an engineering model
    /// </summary>
    public class ActiveDomainsTableViewModel : BaseDataItemTableViewModel<DomainOfExpertise, DomainOfExpertiseRowViewModel>, IActiveDomainsTableViewModel
    {
        /// <summary>
        /// Gets or sets the current <see cref="EngineeringModelSetup"/>
        /// </summary>
        private EngineeringModelSetup CurrentModel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDomainsTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        public ActiveDomainsTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<ActiveDomainsTableViewModel> logger) 
            : base(sessionService, messageBus, logger)
        {
        }

        /// <summary>
        /// Gets a collection of all the available <see cref="DomainOfExpertise"/>s
        /// </summary>
        public IEnumerable<DomainOfExpertise> DomainsOfExpertise { get; private set; }

        /// <summary>
        /// Gets or sets a collection of all the selected active <see cref="DomainOfExpertise"/>s for the engineering model
        /// </summary>
        public IEnumerable<DomainOfExpertise> SelectedDomainsOfExpertise { get; set; }

        /// <summary>
        /// Initializes the <see cref="BaseDataItemTableViewModel{T,TRow}" />
        /// </summary>
        public override void InitializeViewModel()
        {
            base.InitializeViewModel();

            this.DomainsOfExpertise = this.SessionService.GetSiteDirectory().Domain;
        }

        /// <summary>
        /// Sets the model and filters the current Rows, keeping only the active domains associated with the given engineering model
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModelSetup"/> to get its active domains</param>
        public void SetEngineeringModel(EngineeringModelSetup model)
        {
            this.CurrentModel = model;
            var domainsAssociatedWithModel = this.DataSource.Items.Where(x => this.CurrentModel.ActiveDomain.Contains(x));

            this.Rows.Edit(action =>
            {
                action.Clear();
                action.AddRange(domainsAssociatedWithModel.Select(x => new DomainOfExpertiseRowViewModel(x)));
            });

            this.RefreshAccessRight();
            this.ResetSelectedDomainsOfExpertise();
        }

        /// <summary>
        /// Resets the <see cref="SelectedDomainsOfExpertise"/> value based on the <see cref="CurrentModel"/>
        /// </summary>
        public void ResetSelectedDomainsOfExpertise()
        {
            this.SelectedDomainsOfExpertise = this.CurrentModel.ActiveDomain;
        }

        /// <summary>
        /// Edit the active domains related with the <see cref="CurrentModel"/>, using the <see cref="SelectedDomainsOfExpertise"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        public async Task EditActiveDomains()
        {
            if (this.CurrentModel.ActiveDomain.SequenceEqual(this.SelectedDomainsOfExpertise))
            {
                return;
            }

            this.IsLoading = true;

            try
            {
                var modelClone = this.CurrentModel.Clone(false);
                modelClone.ActiveDomain = this.SelectedDomainsOfExpertise.ToList();
                await this.SessionService.CreateOrUpdateThingsWithNotification(modelClone.Container.Clone(false), [modelClone]);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "An error has occurred while saving the active domains");
            }

            this.IsLoading = false;
        }
    }
}
