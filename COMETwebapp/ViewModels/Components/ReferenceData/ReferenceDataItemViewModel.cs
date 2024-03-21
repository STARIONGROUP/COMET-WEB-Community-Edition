// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ReferenceDataItemViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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


namespace COMETwebapp.ViewModels.Components.ReferenceData
{
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using DynamicData;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using ReactiveUI;
    using CDP4Dal.Events;
    using DevExpress.Drawing.Internal.Fonts.Interop;

    /// <summary>
    /// View model that provides the basic functionalities for a reference data item
    /// </summary>
    public abstract class ReferenceDataItemViewModel<T> : ApplicationBaseViewModel, IReferenceDataItemViewModel<T> where T : DefinedThing, IDeprecatableThing
    {
        /// <summary>
        /// A reactive collection of <see cref="MeasurementUnitRowViewModel" />
        /// </summary>
        protected SourceList<ReferenceDataItemRowViewModel<T>> internalRows { get; } = new();

        /// <summary>
        /// Backing field for <see cref="IsOnDeprecationMode" />
        /// </summary>
        private bool isOnDeprecationMode;

        /// <summary>
        /// Injected property to get access to <see cref="IPermissionService" />
        /// </summary>
        private readonly IPermissionService permissionService;

        /// <summary>
        /// A collection of <see cref="Type" /> used to create <see cref="ObjectChangedEvent" /> subscriptions
        /// </summary>
        private static readonly IEnumerable<Type> ObjectChangedTypesOfInterest = new List<Type>
        {
            typeof(T),
            typeof(PersonRole),
            typeof(ReferenceDataLibrary)
        };

        public ReferenceDataItemViewModel(ISessionService sessionService, ICDPMessageBus messageBus, IShowHideDeprecatedThingsService showHideDeprecatedThingsService,
            ILogger<ReferenceDataItemViewModel<T>> logger) : base(sessionService, messageBus)
        {
            this.permissionService = sessionService.Session.PermissionService;
            this.ShowHideDeprecatedThingsService = showHideDeprecatedThingsService;
            this.logger = logger;
            this.Disposables.Add(this.internalRows.Connect().AutoRefresh().Subscribe(_ => this.ConvertRowsToSpecificType()));

            this.InitializeSubscriptions(ObjectChangedTypesOfInterest);
            this.RegisterViewModelWithReusableRows(this);
        }

        protected abstract void ConvertRowsToSpecificType();

        /// <summary>
        /// Initializes the <see cref="MeasurementUnitsTableViewModel"/>
        /// </summary>
        public virtual void InitializeViewModel()
        {
            var listOfThings = this.SessionService.Session.Assembler.Cache.Values.Where(x => x.IsValueCreated).Select(x => x.Value).OfType<T>().ToList();
            this.DataSource.AddRange(listOfThings);
            this.internalRows.AddRange(this.DataSource.Items.Select(x => new ReferenceDataItemRowViewModel<T>(x)));
            this.RefreshAccessRight();
        }

        /// <summary>
        /// Injected property to get access to <see cref="IShowHideDeprecatedThingsService" />
        /// </summary>
        public IShowHideDeprecatedThingsService ShowHideDeprecatedThingsService { get; }

        /// <summary>
        /// Gets or sets the data source for the grid control.
        /// </summary>
        public SourceList<T> DataSource { get; } = new();

        /// <summary>
        /// The <see cref="ILogger{TCategoryName}"/>
        /// </summary>
        private readonly ILogger<ReferenceDataItemViewModel<T>> logger;

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
        /// Indicates if confirmation popup is visible
        /// </summary>
        public bool IsOnDeprecationMode
        {
            get => this.isOnDeprecationMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnDeprecationMode, value);
        }

        /// <summary>
        /// The <see cref="MeasurementUnit" /> to create or edit
        /// </summary>
        public T MeasurementUnit { get; set; }

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
        public void OnDeprecateUnDeprecateButtonClick(ReferenceDataItemRowViewModel<T> measurementUnitRow)
        {
            this.MeasurementUnit = measurementUnitRow.Thing;
            this.PopupDialog = this.MeasurementUnit.IsDeprecated ? "You are about to un-deprecate the measurement unit: " + measurementUnitRow.Name : "You are about to deprecate the measurement unit: " + measurementUnitRow.Name;
            this.IsOnDeprecationMode = true;
        }

        /// <summary>
        /// Tries to deprecate or undeprecate a <see cref="MeasurementUnit" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task DeprecatingOrUnDeprecatingMeasurementUnit()
        {
            var siteDirectoryClone = this.SessionService.GetSiteDirectory().Clone(false);
            var clonedMeasurementUnit = this.MeasurementUnit.Clone(false);
            ((IDeprecatableThing)clonedMeasurementUnit).IsDeprecated = !((IDeprecatableThing)clonedMeasurementUnit).IsDeprecated;

            try
            {
                await this.SessionService.UpdateThings(siteDirectoryClone, clonedMeasurementUnit);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An error has occurred while trying to deprecating or un-deprecating the measurement unit {unitName}", clonedMeasurementUnit.ShortName);
            }
        }

        /// <summary>
        /// Add rows related to <see cref="Thing" /> that has been added
        /// </summary>
        /// <param name="addedThings">A collection of added <see cref="Thing" /></param>
        public void AddRows(IEnumerable<Thing> addedThings)
        {
            var addedMeasurementUnits = addedThings.OfType<T>().ToList();
            this.internalRows.AddRange(addedMeasurementUnits.Select(x => new ReferenceDataItemRowViewModel<T>(x)));
        }

        /// <summary>
        /// Updates rows related to <see cref="Thing" /> that have been updated
        /// </summary>
        /// <param name="updatedThings">A collection of updated <see cref="Thing" /></param>
        public void UpdateRows(IEnumerable<Thing> updatedThings)
        {
            var updatedThingsList = updatedThings.ToList();

            var updatedMeasurementUnits = updatedThingsList.OfType<T>();
            var updatedPersonRoles = updatedThingsList.OfType<PersonRole>();
            var updatedRdls = updatedThingsList.OfType<ReferenceDataLibrary>();

            this.internalRows.Edit(action =>
            {
                foreach (var updatedMeasurementUnit in updatedMeasurementUnits)
                {
                    var updatedRow = new ReferenceDataItemRowViewModel<T>(updatedMeasurementUnit);
                    var rowToUpdate = this.internalRows.Items.First(x => x.Thing.Iid == updatedMeasurementUnit.Iid);
                    action.Replace(rowToUpdate, updatedRow);
                }
            });

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
            var measurementUnitsIidsToRemove = deletedThings.OfType<T>().Select(x => x.Iid);
            var rowsToDelete = this.internalRows.Items.Where(x => measurementUnitsIidsToRemove.Contains(x.Thing.Iid)).ToList();

            this.internalRows.RemoveMany(rowsToDelete);
        }

        /// <summary>
        /// popup message dialog
        /// </summary>
        public string PopupDialog { get; set; }

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
        /// Refresh the displayed container name for the measurement unit rows
        /// </summary>
        /// <param name="rdl">
        /// The updated <see cref="ReferenceDataLibrary" />.
        /// </param>
        private void RefreshContainerName(ReferenceDataLibrary rdl)
        {
            this.IsLoading = true;
            var rowsContainedByUpdatedRdl = this.internalRows.Items.Where(x => x.Thing.Container.Iid == rdl.Iid);

            foreach (var measurementUnit in rowsContainedByUpdatedRdl)
            {
                measurementUnit.ContainerName = rdl.ShortName;
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// Updates the active user access rights
        /// </summary>
        protected void RefreshAccessRight()
        {
            this.IsLoading = true;

            foreach (var row in this.internalRows.Items)
            {
                row.IsAllowedToWrite = this.permissionService.CanWrite(row.Thing.ClassKind, row.Thing.Container);
            }

            this.IsLoading = false;
        }
    }
}