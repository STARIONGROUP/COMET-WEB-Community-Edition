// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementUnitsTableViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.ReferenceData
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DynamicData;

    using ReactiveUI;

    using MeasurementUnit = CDP4Common.SiteDirectoryData.MeasurementUnit;

    /// <summary>
    /// View model used to manage <see cref="MeasurementUnit" />s
    /// </summary>
    public class MeasurementUnitsTableViewModel : ReferenceDataItemViewModel<MeasurementUnit>, IMeasurementUnitsTableViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementUnitsTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        public MeasurementUnitsTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ICDPMessageBus messageBus, 
            ILogger<MeasurementUnitsTableViewModel> logger) : base(sessionService, messageBus, showHideDeprecatedThingsService, logger)
        {
        }

        /// <summary>
        /// Available <see cref="ReferenceDataLibrary" />s
        /// </summary>
        public IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; set; }

        /// <summary>
        /// A reactive collection of <see cref="MeasurementUnitRowViewModel" />
        /// </summary>
        public SourceList<MeasurementUnitRowViewModel> Rows { get; } = new();

        protected override void ConvertRowsToSpecificType()
        {
            var convertedItems = this.internalRows.Items.Select(x => new MeasurementUnitRowViewModel(x.Thing)
            {
                IsAllowedToWrite = x.IsAllowedToWrite
            });

            this.Rows.EditDiff(convertedItems);
        }

        /// <summary>
        /// Initializes the <see cref="MeasurementUnitsTableViewModel"/>
        /// </summary>
        public override void InitializeViewModel()
        {
            base.InitializeViewModel();
            this.ReferenceDataLibraries = this.SessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries();
        }
    }
}
