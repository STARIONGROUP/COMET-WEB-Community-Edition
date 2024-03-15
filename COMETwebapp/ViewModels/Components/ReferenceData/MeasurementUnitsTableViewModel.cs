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
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DynamicData;

    using ReactiveUI;

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
        /// A collection of all <see cref="MeasurementUnitRowViewModel" />
        /// </summary>
        private IEnumerable<MeasurementUnitRowViewModel> allRows = new List<MeasurementUnitRowViewModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementUnitsTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        public MeasurementUnitsTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ICDPMessageBus messageBus) : base(sessionService, messageBus)
        {
            this.sessionService = sessionService;
            this.permissionService = sessionService.Session.PermissionService;
            this.ShowHideDeprecatedThingsService = showHideDeprecatedThingsService;

            this.Disposables.Add(
                this.MessageBus.Listen<ObjectChangedEvent>(typeof(MeasurementUnit))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added &&
                                           objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as MeasurementUnit)
                    .SubscribeAsync(this.AddNewMeasurementUnit));

            this.Disposables.Add(
                this.MessageBus.Listen<ObjectChangedEvent>(typeof(MeasurementUnit))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                           objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as MeasurementUnit)
                    .SubscribeAsync(this.UpdateMeasurementUnit));

            this.Disposables.Add(
                this.MessageBus.Listen<ObjectChangedEvent>(typeof(ReferenceDataLibrary))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                           objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ReferenceDataLibrary)
                    .SubscribeAsync(this.RefreshContainerName));

            this.Disposables.Add(this.MessageBus.Listen<ObjectChangedEvent>(typeof(PersonRole))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                       objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                .Select(x => x.ChangedThing as PersonRole)
                .SubscribeAsync(_ => this.RefreshAccessRight()));
        }

        /// <summary>
        /// The <see cref="MeasurementUnit" /> to create or edit
        /// </summary>
        public MeasurementUnit MeasurementUnit { get; set; }

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
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// Override this method if you will perform an asynchronous operation and
        /// want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        public async Task OnInitializedAsync()
        {
            foreach (var referenceDataLibrary in this.sessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries())
            {
                this.DataSource.AddRange(referenceDataLibrary.Unit);
            }

            this.ReferenceDataLibraries = this.sessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries();
            await this.UpdateProperties(this.DataSource.Items);
            await this.RefreshAccessRight();
        }

        /// <summary>
        /// Adds a new <see cref="MeasurementUnit" />
        /// </summary>
        public Task AddNewMeasurementUnit(MeasurementUnit measurementUnit)
        {
            var newRows = new List<MeasurementUnitRowViewModel>(this.allRows)
            {
                new(measurementUnit)
            };

            this.UpdateRows(newRows);

            return this.RefreshAccessRight();
        }

        /// <summary>
        /// Updates the <see cref="MeasurementUnit" />
        /// </summary>
        public Task UpdateMeasurementUnit(MeasurementUnit measurementUnit)
        {
            var updatedRows = new List<MeasurementUnitRowViewModel>(this.allRows);
            var index = updatedRows.FindIndex(x => x.MeasurementUnit.Iid == measurementUnit.Iid);
            updatedRows[index] = new MeasurementUnitRowViewModel(measurementUnit);
            this.UpdateRows(updatedRows);

            return this.RefreshAccessRight();
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="measurementUnits">A collection of <see cref="MeasurementUnit" /></param>
        public async Task UpdateProperties(IEnumerable<MeasurementUnit> measurementUnits)
        {
            this.IsLoading = true;
            await Task.Delay(1);
            this.allRows = measurementUnits.Select(x => new MeasurementUnitRowViewModel(x));
            this.UpdateRows(this.allRows);
            this.IsLoading = false;
        }

        /// <summary>
        /// Method invoked when confirming the deprecation/un-deprecation of a <see cref="MeasurementUnit" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task OnConfirmPopupButtonClick()
        {
            if (this.MeasurementUnit.IsDeprecated)
            {
                await this.UnDeprecatingMeasurementUnit();
            }
            else
            {
                await this.DeprecatingMeasurementUnit();
            }

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
        protected override async Task OnSessionRefreshed()
        {
            this.IsLoading = true;
            await Task.Delay(1);
            this.IsLoading = false;
        }

        /// <summary>
        /// Tries to undeprecate a <see cref="MeasurementUnit" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task UnDeprecatingMeasurementUnit()
        {
            var measurementUnitsToUnDeprecate = new List<MeasurementUnit>();
            var clonedMeasurementUnit = this.MeasurementUnit.Clone(false);
            clonedMeasurementUnit.IsDeprecated = false;
            measurementUnitsToUnDeprecate.Add(clonedMeasurementUnit);

            try
            {
                await this.sessionService.UpdateThings(this.sessionService.GetSiteDirectory(), measurementUnitsToUnDeprecate);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }

            this.IsOnDeprecationMode = false;
        }

        /// <summary>
        /// Tries to deprecate a <see cref="MeasurementUnit" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task DeprecatingMeasurementUnit()
        {
            var measurementUnitsToDeprecate = new List<MeasurementUnit>();
            var clonedMeasurementUnit = this.MeasurementUnit.Clone(false);
            clonedMeasurementUnit.IsDeprecated = true;
            measurementUnitsToDeprecate.Add(clonedMeasurementUnit);

            try
            {
                await this.sessionService.UpdateThings(this.sessionService.GetSiteDirectory(), measurementUnitsToDeprecate);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }

            this.IsOnDeprecationMode = false;
        }

        /// <summary>
        /// Refresh the displayed container name for the measurement unit rows
        /// </summary>
        /// <param name="rdl">
        /// The updated <see cref="ReferenceDataLibrary" />.
        /// </param>
        private async Task RefreshContainerName(ReferenceDataLibrary rdl)
        {
            this.IsLoading = true;
            await Task.Delay(1);

            foreach (var measurementUnit in this.Rows.Items)
            {
                if (measurementUnit.ContainerName != rdl.ShortName)
                {
                    measurementUnit.ContainerName = rdl.ShortName;
                }
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// Updates the active user access rights
        /// </summary>
        private async Task RefreshAccessRight()
        {
            this.IsLoading = true;
            await Task.Delay(1);

            foreach (var row in this.Rows.Items)
            {
                row.IsAllowedToWrite = this.permissionService.CanWrite(ClassKind.MeasurementUnit, row.MeasurementUnit.Container);
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// Update the <see cref="Rows" /> collection based on a collection of
        /// <see cref="MeasurementUnitRowViewModel" /> to display.
        /// </summary>
        /// <param name="rowsToDisplay">A collection of <see cref="MeasurementUnitRowViewModel" /></param>
        private void UpdateRows(IEnumerable<MeasurementUnitRowViewModel> rowsToDisplay)
        {
            rowsToDisplay = rowsToDisplay.ToList();

            var deletedRows = this.Rows.Items.Where(x => rowsToDisplay.All(r => r.MeasurementUnit.Iid != x.MeasurementUnit.Iid)).ToList();
            var addedRows = rowsToDisplay.Where(x => this.Rows.Items.All(r => r.MeasurementUnit.Iid != x.MeasurementUnit.Iid)).ToList();
            var existingRows = rowsToDisplay.Where(x => this.Rows.Items.Any(r => r.MeasurementUnit.Iid == x.MeasurementUnit.Iid)).ToList();

            this.Rows.RemoveMany(deletedRows);
            this.Rows.AddRange(addedRows);

            foreach (var existingRow in existingRows)
            {
                this.Rows.Items.First(x => x.MeasurementUnit.Iid == existingRow.MeasurementUnit.Iid).UpdateProperties(existingRow);
            }
        }
    }
}
