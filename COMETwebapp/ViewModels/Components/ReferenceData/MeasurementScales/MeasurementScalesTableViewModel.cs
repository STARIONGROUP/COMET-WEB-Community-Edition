// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementScalesTableViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.ReferenceData.MeasurementScales
{
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using MeasurementScale = CDP4Common.SiteDirectoryData.MeasurementScale;

    /// <summary>
    /// View model used to manage <see cref="MeasurementScale" />s
    /// </summary>
    public class MeasurementScalesTableViewModel : DeprecatableDataItemTableViewModel<MeasurementScale, MeasurementScaleRowViewModel>, IMeasurementScalesTableViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementScalesTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        public MeasurementScalesTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ICDPMessageBus messageBus,
            ILogger<MeasurementScalesTableViewModel> logger) : base(sessionService, messageBus, showHideDeprecatedThingsService, logger)
        {
        }

        /// <summary>
        /// Gets the available <see cref="ReferenceDataLibrary" />s
        /// </summary>
        public IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; private set; }

        /// <summary>
        /// Gets the available <see cref="MeasurementUnit" />s
        /// </summary>
        public IEnumerable<MeasurementUnit> MeasurementUnits { get; private set; }

        /// <summary>
        /// Gets the available <see cref="NumberSetKind" />s
        /// </summary>
        public IEnumerable<NumberSetKind> NumberSetKinds { get; private set; } = Enum.GetValues<NumberSetKind>();

        /// <summary>
        /// Initializes the <see cref="MeasurementScalesTableViewModel"/>
        /// </summary>
        public override void InitializeViewModel()
        {
            base.InitializeViewModel();
            this.ReferenceDataLibraries = this.SessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries();
        }
    }
}
