// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementUnitsTableViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar, João Rua
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
    public class MeasurementUnitsTableViewModel : ApplicationBaseViewModel, IMeasurementUnitsTableViewModel
    {
        /// <summary>
        /// Injected property to get access to <see cref="IPermissionService" />
        /// </summary>
        private readonly IPermissionService permissionService;

        /// <summary>
        /// Injected property to get access to <see cref="ISessionService" />
        /// </summary>
        private readonly ISessionService sessionService;

        /// <summary>
        /// Backing field for <see cref="IsOnDeprecationMode" />
        /// </summary>
        private bool isOnDeprecationMode;

        /// <summary>
        /// The <see cref="ILogger{TCategoryName}"/>
        /// </summary>
        private readonly ILogger<MeasurementUnitsTableViewModel> logger;

        /// <summary>
        /// A collection of <see cref="Type" /> used to create <see cref="ObjectChangedEvent" /> subscriptions
        /// </summary>
        private static readonly IEnumerable<Type> ObjectChangedTypesOfInterest = new List<Type>
        {
            typeof(MeasurementUnit),
            typeof(PersonRole),
            typeof(ReferenceDataLibrary)
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementUnitsTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        public MeasurementUnitsTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ICDPMessageBus messageBus, 
            ILogger<MeasurementUnitsTableViewModel> logger) : base(sessionService, messageBus)
        {
            this.sessionService = sessionService;
            this.permissionService = sessionService.Session.PermissionService;
            this.ShowHideDeprecatedThingsService = showHideDeprecatedThingsService;
            this.logger = logger;

            this.InitializeSubscriptions(ObjectChangedTypesOfInterest);
            this.RegisterViewModelWithReusableRows(this);
        }

        /// <summary>
        /// The <see cref="MeasurementUnit" /> to create or edit
        /// </summary>
        public MeasurementUnit MeasurementUnit { get; set; } = new SimpleUnit();

        /// <summary>
        /// Available <see cref="ReferenceDataLibrary" />s
        /// </summary>
        public IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; set; }

        /// <summary>
        /// Injected property to get access to <see cref="IShowHideDeprecatedThingsService" />
        /// </summary>
        public IShowHideDeprecatedThingsService ShowHideDeprecatedThingsService { get; }

        /// <summary>
        /// A reactive collection of <see cref="MeasurementUnitRowViewModel" />
        /// </summary>
        public SourceList<MeasurementUnitRowViewModel> Rows { get; } = new();

        /// <summary>
        /// Gets or sets the data source for the grid control.
        /// </summary>
        public SourceList<MeasurementUnit> DataSource { get; } = new();

        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        public bool IsOnDeprecationMode
        {
            get => this.isOnDeprecationMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnDeprecationMode, value);
        }

        /// <summary>
        /// popup message dialog
        /// </summary>
        public string PopupDialog { get; set; }

        /// <summary>
        /// Initializes the <see cref="MeasurementUnitsTableViewModel"/>
        /// </summary>
        public void InitializeViewModel()
        {
            foreach (var referenceDataLibrary in this.sessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries())
            {
                this.DataSource.AddRange(referenceDataLibrary.Unit);
            }

            this.ReferenceDataLibraries = this.sessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries();
            this.Rows.AddRange(this.DataSource.Items.Select(x => new MeasurementUnitRowViewModel(x)));
            this.RefreshAccessRight();
        }

        /// <summary>
        /// Method invoked when confirming the deprecation/un-deprecation of a <see cref="MeasurementUnit" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task OnConfirmPopupButtonClick()
        {
            await this.DeprecatingOrUnDeprecatingMeasurementUnit();
            this.IsOnDeprecationMode = false;
        }

        /// <summary>
        /// Method invoked when canceling the deprecation/un-deprecation of a <see cref="MeasurementUnit" />
        /// </summary>
        public void OnCancelPopupButtonClick()
        {
            this.IsOnDeprecationMode = false;
        }

        /// <summary>
        /// Action invoked when the deprecate or undeprecate button is clicked
        /// </summary>
        /// <param name="measurementUnitRow"> The <see cref="MeasurementUnitRowViewModel" /> to deprecate or undeprecate </param>
        public void OnDeprecateUnDeprecateButtonClick(MeasurementUnitRowViewModel measurementUnitRow)
        {
            this.MeasurementUnit = measurementUnitRow.MeasurementUnit;
            this.PopupDialog = this.MeasurementUnit.IsDeprecated ? "You are about to un-deprecate the measurement unit: " + measurementUnitRow.Name : "You are about to deprecate the measurement unit: " + measurementUnitRow.Name;
            this.IsOnDeprecationMode = true;
        }

        /// <summary>
        /// Handles the refresh of the current <see cref="ISession" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnSessionRefreshed()
        {
            if (this.AddedThings.Count == 0 && this.UpdatedThings.Count == 0 && this.DeletedThings.Count == 0)
            {
                return Task.CompletedTask;
            }

            this.IsLoading = true;
            this.UpdateInnerComponents();
            this.RefreshAccessRight();
            this.ClearRecordedChanges();
            this.IsLoading = false;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Tries to deprecate or undeprecate a <see cref="MeasurementUnit" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task DeprecatingOrUnDeprecatingMeasurementUnit()
        {
            var siteDirectoryClone = this.sessionService.GetSiteDirectory().Clone(false);
            var clonedMeasurementUnit = this.MeasurementUnit.Clone(false);
            clonedMeasurementUnit.IsDeprecated = !clonedMeasurementUnit.IsDeprecated;

            try
            {
                await this.sessionService.UpdateThings(siteDirectoryClone, clonedMeasurementUnit);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error has occurred while trying to deprecating or un-deprecating the measurement unit {unitName}", clonedMeasurementUnit.ShortName);
            }
        }

        /// <summary>
        /// Refresh the displayed container name for the measurement unit rows
        /// </summary>
        /// <param name="rdl">
        /// The updated <see cref="ReferenceDataLibrary" />.
        /// </param>
        private void RefreshContainerName(ReferenceDataLibrary rdl)
        {
            this.IsLoading = true;
            var rowsContainedByUpdatedRdl = this.Rows.Items.Where(x => x.MeasurementUnit.Container.Iid == rdl.Iid);

            foreach (var measurementUnit in rowsContainedByUpdatedRdl)
            {
                measurementUnit.ContainerName = rdl.ShortName;
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// Updates the active user access rights
        /// </summary>
        private void RefreshAccessRight()
        {
            this.IsLoading = true;

            foreach (var row in this.Rows.Items)
            {
                row.IsAllowedToWrite = this.permissionService.CanWrite(ClassKind.MeasurementUnit, row.MeasurementUnit.Container);
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// Add rows related to <see cref="Thing" /> that has been added
        /// </summary>
        /// <param name="addedThings">A collection of added <see cref="Thing" /></param>
        public void AddRows(IEnumerable<Thing> addedThings)
        {
            var addedMeasurementUnits = addedThings.OfType<MeasurementUnit>().ToList();
            this.Rows.AddRange(addedMeasurementUnits.Select(x => new MeasurementUnitRowViewModel(x)));
        }

        /// <summary>
        /// Updates rows related to <see cref="Thing" /> that have been updated
        /// </summary>
        /// <param name="updatedThings">A collection of updated <see cref="Thing" /></param>
        public void UpdateRows(IEnumerable<Thing> updatedThings)
        {
            var updatedThingsList = updatedThings.ToList();

            var updatedMeasurementUnits = updatedThingsList.OfType<MeasurementUnit>();
            var updatedPersonRoles = updatedThingsList.OfType<PersonRole>();
            var updatedRdls = updatedThingsList.OfType<ReferenceDataLibrary>();

            foreach (var updatedMeasurementUnit in updatedMeasurementUnits)
            {
                var updatedRow = new MeasurementUnitRowViewModel(updatedMeasurementUnit);
                this.Rows.Items.First(x => x.MeasurementUnit.Iid == updatedMeasurementUnit.Iid).UpdateProperties(updatedRow);
            }

            foreach (var rdl in updatedRdls)
            {
                this.RefreshContainerName(rdl);
            }

            if (updatedPersonRoles.Any())
            {
                this.RefreshAccessRight();
            }
        }

        /// <summary>
        /// Remove rows related to a <see cref="Thing" /> that has been deleted
        /// </summary>
        /// <param name="deletedThings">A collection of deleted <see cref="Thing" /></param>
        public void RemoveRows(IEnumerable<Thing> deletedThings)
        {
            var measurementUnitsIidsToRemove = deletedThings.OfType<MeasurementUnit>().ToList().Select(x => x.Iid);
            var rowsToDelete = this.Rows.Items.Where(x => measurementUnitsIidsToRemove.Contains(x.MeasurementUnit.Iid)).ToList();

            this.Rows.RemoveMany(rowsToDelete);
        }
    }
}
