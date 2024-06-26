﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BaseDataItemTableViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Services.RowViewModelFactoryService;
    using COMETwebapp.ViewModels.Components.Common.Rows;

    using DynamicData;

    /// <summary>
    /// View model that provides the basic functionalities for a base data item
    /// </summary>
    public abstract class BaseDataItemTableViewModel<T, TRow> : SingleThingApplicationBaseViewModel<T>, IBaseDataItemTableViewModel<T, TRow> where T : Thing where TRow : BaseDataItemRowViewModel<T>
    {
        /// <summary>
        /// The <see cref="ILogger{TCategoryName}" />
        /// </summary>
        protected readonly ILogger<BaseDataItemTableViewModel<T, TRow>> Logger;

        /// <summary>
        /// Injected property to get access to <see cref="IPermissionService" />
        /// </summary>
        protected readonly IPermissionService PermissionService;

        /// <summary>
        /// Creates a new instance of the <see cref="BaseDataItemTableViewModel{T,TRow}" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        /// <param name="extraTypesOfInterest">The collection of extra types of interest to subscribe</param>
        protected BaseDataItemTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<BaseDataItemTableViewModel<T, TRow>> logger, IEnumerable<Type> extraTypesOfInterest = null)
            : base(sessionService, messageBus)
        {
            this.PermissionService = sessionService.Session.PermissionService;
            this.Logger = logger;
            var objectChangedTypesOfInterest = new List<Type>() { typeof(T) };

            if (extraTypesOfInterest != null)
            {
                objectChangedTypesOfInterest.AddRange(extraTypesOfInterest);
            }

            this.InitializeSubscriptions(objectChangedTypesOfInterest);
            this.RegisterViewModelWithReusableRows(this);
        }

        /// <summary>
        /// A reactive collection of things
        /// </summary>
        public SourceList<TRow> Rows { get; } = new();

        /// <summary>
        /// Initializes the <see cref="BaseDataItemTableViewModel{T,TRow}" />
        /// </summary>
        public virtual void InitializeViewModel()
        {
            this.IsLoading = true;

            this.Rows.Clear();
            var listOfThings = this.QueryListOfThings() ?? Enumerable.Empty<T>();
            this.Rows.AddRange(listOfThings.Select(CreateNewRow).OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase));
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
        /// Queries a list of things of the current type
        /// </summary>
        /// <returns>A list of things</returns>
        protected abstract List<T> QueryListOfThings();

        /// <summary>
        /// Update this view model properties when the <see cref="SingleThingApplicationBaseViewModel{TThing}.CurrentThing" /> has
        /// changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override Task OnThingChanged()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the message for the success notification
        /// </summary>
        /// <param name="created">The value to check if the thing was created</param>
        /// <returns>The message</returns>
        protected virtual NotificationDescription GetNotificationDescription(bool created)
        {
            var notificationDescription = new NotificationDescription
            {
                OnSuccess = $"The {typeof(T).Name} {this.CurrentThing.GetShortNameOrName()} was {(created ? "added" : "updated")}",
                OnError = $"Error while {(created ? "adding" : "updating")} the {typeof(T).Name} {this.CurrentThing.GetShortNameOrName()}"
            };

            return notificationDescription;
        }

        /// <summary>
        /// Handles the <see cref="SessionStatus.EndUpdate" /> message received
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnEndUpdate()
        {
            await this.OnSessionRefreshed();
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

            this.UpdateInnerComponents();
            this.RefreshAccessRight();
            this.ClearRecordedChanges();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates the active user access rights
        /// </summary>
        protected virtual void RefreshAccessRight()
        {
            foreach (var row in this.Rows.Items)
            {
                row.IsAllowedToWrite = this.PermissionService.CanWrite(row.Thing.ClassKind, row.Thing.Container);
            }
        }

        /// <summary>
        /// Refresh the displayed container name for the things rows
        /// </summary>
        /// <param name="container">
        /// The updated container, which is a <see cref="DefinedThing" />
        /// </param>
        protected void RefreshContainerName(DefinedThing container)
        {
            var containedRows= this.Rows.Items.Where(x => x.Thing.Container.Iid == container.Iid);

            foreach (var thingRow in containedRows)
            {
                thingRow.ContainerName = container.ShortName;
            }
        }

        /// <summary>
        /// Creates a new row instance based on a given thing
        /// </summary>
        /// <param name="thing">The thing to create a row</param>
        /// <returns>The created row</returns>
        private static TRow CreateNewRow(T thing)
        {
            return (TRow)RowViewModelFactory.CreateRow(thing);
        }
    }
}
