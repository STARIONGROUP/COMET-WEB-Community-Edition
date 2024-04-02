// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IterationsTableViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.Common.BaseDataItemTable;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    /// <summary>
    /// View model used to list <see cref="Iteration" />s
    /// </summary>
    public class IterationsTableViewModel : BaseDataItemTableViewModel<Iteration, IterationRowViewModel>, IIterationsTableViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantsTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        public IterationsTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<IterationsTableViewModel> logger) 
            : base(sessionService, messageBus, logger)
        {
        }

        /// <summary>
        /// Sets the model and filters the current Rows, keeping only the iterations associated with the given engineering model
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModelSetup"/> to get its iterations</param>
        public void SetEngineeringModel(EngineeringModelSetup model)
        {
            var iterationsAssociatedWithModel = this.DataSource.Items.Where(x => ((EngineeringModel)x.Container).EngineeringModelSetup.Iid == model.Iid);

            this.Rows.Edit(action =>
            {
                action.Clear();
                action.AddRange(iterationsAssociatedWithModel.Select(x => new IterationRowViewModel(x)));
            });
        }
    }
}
