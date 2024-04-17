// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BaseDataItemTableViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.Common.BaseDataItemTable
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.ViewModels.Components.Common.Rows;

    using DynamicData;

    /// <summary>
    /// View model that provides the basic functionalities for a base data item
    /// </summary>
    public abstract class BaseDataItemTableViewModel<T, TRow> : ApplicationBaseViewModel, IBaseDataItemTableViewModel<T, TRow> where T : Thing where TRow : BaseDataItemRowViewModel<T>
    {
        /// <summary>
        /// A collection of <see cref="Type" /> used to create <see cref="ObjectChangedEvent" /> subscriptions
        /// </summary>
        private static readonly IEnumerable<Type> ObjectChangedTypesOfInterest = new List<Type>
        {
            typeof(T),
            typeof(PersonRole),
            typeof(ReferenceDataLibrary)
        };

        /// <summary>
        /// Injected property to get access to <see cref="IPermissionService" />
        /// </summary>
        protected readonly IPermissionService PermissionService;

        /// <summary>
        /// The <see cref="ILogger{TCategoryName}" />
        /// </summary>
        protected readonly ILogger<BaseDataItemTableViewModel<T, TRow>> Logger;

        /// <summary>
        /// Creates a new instance of the <see cref="BaseDataItemTableViewModel{T,TRow}"/>
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService"/></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        protected BaseDataItemTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<BaseDataItemTableViewModel<T, TRow>> logger) 
            : base(sessionService, messageBus)
        {
            this.PermissionService = sessionService.Session.PermissionService;
            this.Logger = logger;

            this.InitializeSubscriptions(ObjectChangedTypesOfInterest);
            this.RegisterViewModelWithReusableRows(this);
        }

        /// <summary>
        /// The thing to create or edit
        /// </summary>
        public T Thing { get; set; }

        /// <summary>
        /// A reactive collection of things
        /// </summary>
        public SourceList<TRow> Rows { get; } = new();

        /// <summary>
        /// Gets or sets the data source for the grid control.
        /// </summary>
        public SourceList<T> DataSource { get; } = new();

        /// <summary>
        /// Initializes the <see cref="BaseDataItemTableViewModel{T,TRow}" />
        /// </summary>
        public virtual void InitializeViewModel()
        {
            this.IsLoading = true;

            this.Rows.Clear();
            this.DataSource.Clear();

            var listOfThings = this.SessionService.Session.Assembler.Cache.Values.Where(x => x.IsValueCreated).Select(x => x.Value).OfType<T>().ToList();
            this.DataSource.AddRange(listOfThings);
            this.Rows.AddRange(this.DataSource.Items.Select(CreateNewRow));
            this.RefreshAccessRight();

            this.IsLoading = false;
        }

        /// <summary>
        /// Add rows related to <see cref="CDP4Common.CommonData.Thing" /> that has been added
        /// </summary>
        /// <param name="addedThings">A collection of added <see cref="CDP4Common.CommonData.Thing" /></param>
        public void AddRows(IEnumerable<Thing> addedThings)
        {
            var addedThingOfTypeT = addedThings.OfType<T>().DistinctBy(x => x.Iid).ToList();
            this.Rows.AddRange(addedThingOfTypeT.Select(CreateNewRow));
        }

        /// <summary>
        /// Updates rows related to <see cref="CDP4Common.CommonData.Thing" /> that have been updated
        /// </summary>
        /// <param name="updatedThings">A collection of updated <see cref="CDP4Common.CommonData.Thing" /></param>
        public void UpdateRows(IEnumerable<Thing> updatedThings)
        {
            var updatedThingsList = updatedThings.ToList();

            var updatedThingOfTypeT = updatedThingsList.OfType<T>();
            var updatedPersonRoles = updatedThingsList.OfType<PersonRole>();
            var updatedRdls = updatedThingsList.OfType<ReferenceDataLibrary>();

            this.Rows.Edit(action =>
            {
                foreach (var updatedThing in updatedThingOfTypeT)
                {
                    var rowToUpdate = this.Rows.Items.FirstOrDefault(x => x.Thing.Iid == updatedThing.Iid);

                    if (rowToUpdate == null)
                    {
                        continue;
                    }

                    var updatedRow = CreateNewRow(updatedThing);
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
        /// Remove rows related to a <see cref="CDP4Common.CommonData.Thing" /> that has been deleted
        /// </summary>
        /// <param name="deletedThings">A collection of deleted <see cref="CDP4Common.CommonData.Thing" /></param>
        public void RemoveRows(IEnumerable<Thing> deletedThings)
        {
            var thingsIidsToRemove = deletedThings.OfType<T>().Select(x => x.Iid);
            var rowsToDelete = this.Rows.Items.Where(x => thingsIidsToRemove.Contains(x.Thing.Iid)).ToList();

            this.Rows.RemoveMany(rowsToDelete);
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
        /// Updates the active user access rights
        /// </summary>
        protected virtual void RefreshAccessRight()
        {
            this.IsLoading = true;

            foreach (var row in this.Rows.Items)
            {
                row.IsAllowedToWrite = this.PermissionService.CanWrite(row.Thing.ClassKind, row.Thing.Container);
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// Refresh the displayed container name for the things rows
        /// </summary>
        /// <param name="rdl">
        /// The updated <see cref="ReferenceDataLibrary" />.
        /// </param>
        private void RefreshContainerName(ReferenceDataLibrary rdl)
        {
            this.IsLoading = true;
            var rowsContainedByUpdatedRdl = this.Rows.Items.Where(x => x.Thing.Container.Iid == rdl.Iid);

            foreach (var thingRow in rowsContainedByUpdatedRdl)
            {
                thingRow.ContainerName = rdl.ShortName;
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// Creates a new row instance based on a given thing
        /// </summary>
        /// <param name="thing">The thing to create a row</param>
        /// <returns>The created row</returns>
        private static TRow CreateNewRow(T thing)
        {
            return (TRow)Activator.CreateInstance(typeof(TRow), thing);
        }
    }
}
