// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeTableViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
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

    /// <summary>
    /// View model used to manage <see cref="ParameterType" />
    /// </summary>
    public class ParameterTypeTableViewModel : ApplicationBaseViewModel, IParameterTypeTableViewModel
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
        /// A collection of all <see cref="ParameterTypeRowViewModel" />
        /// </summary>
        private IEnumerable<ParameterTypeRowViewModel> allRows = new List<ParameterTypeRowViewModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        public ParameterTypeTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ICDPMessageBus messageBus) : base(sessionService, messageBus)
        {
            this.sessionService = sessionService;
            this.permissionService = sessionService.Session.PermissionService;
            this.ShowHideDeprecatedThingsService = showHideDeprecatedThingsService;

            this.Disposables.Add(
                this.MessageBus.Listen<ObjectChangedEvent>(typeof(ParameterType))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added &&
                                           objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ParameterType)
                    .SubscribeAsync(this.AddNewParameterType));

            this.Disposables.Add(
                this.MessageBus.Listen<ObjectChangedEvent>(typeof(ParameterType))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                           objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ParameterType)
                    .SubscribeAsync(this.UpdateParameterType));

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
        /// Available <see cref="ReferenceDataLibrary" />s
        /// </summary>
        public IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; set; }

        /// <summary>
        /// Injected property to get access to <see cref="IShowHideDeprecatedThingsService" />
        /// </summary>
        public IShowHideDeprecatedThingsService ShowHideDeprecatedThingsService { get; }

        /// <summary>
        /// A reactive collection of <see cref="ParameterTypeRowViewModel" />
        /// </summary>
        public SourceList<ParameterTypeRowViewModel> Rows { get; } = new();

        /// <summary>
        /// Gets or sets the data source for the grid control.
        /// </summary>
        public SourceList<ParameterType> DataSource { get; } = new();

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
                this.DataSource.AddRange(referenceDataLibrary.ParameterType);
            }

            this.ReferenceDataLibraries = this.sessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries();
            await this.UpdateProperties(this.DataSource.Items);
            await this.RefreshAccessRight();
        }

        /// <summary>
        /// Adds a new <see cref="ParameterType" />
        /// </summary>
        public Task AddNewParameterType(ParameterType parameterType)
        {
            var newRows = new List<ParameterTypeRowViewModel>(this.allRows)
            {
                new(parameterType)
            };

            this.UpdateRows(newRows);

            return this.RefreshAccessRight();
        }

        /// <summary>
        /// Updates the <see cref="ParameterType" />
        /// </summary>
        public Task UpdateParameterType(ParameterType parameterType)
        {
            var updatedRows = new List<ParameterTypeRowViewModel>(this.allRows);
            var index = updatedRows.FindIndex(x => x.ParameterType.Iid == parameterType.Iid);
            updatedRows[index] = new ParameterTypeRowViewModel(parameterType);
            this.UpdateRows(updatedRows);

            return this.RefreshAccessRight();
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="parameterTypes">A collection of <see cref="ParameterType" /></param>
        public async Task UpdateProperties(IEnumerable<ParameterType> parameterTypes)
        {
            this.IsLoading = true;
            await Task.Delay(1);
            this.allRows = parameterTypes.Select(x => new ParameterTypeRowViewModel(x));
            this.UpdateRows(this.allRows);
            this.IsLoading = false;
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
        /// Refresh the displayed container name for the category rows
        /// </summary>
        /// <param name="rdl">
        /// The updated <see cref="ReferenceDataLibrary" />.
        /// </param>
        private async Task RefreshContainerName(ReferenceDataLibrary rdl)
        {
            this.IsLoading = true;
            await Task.Delay(1);

            foreach (var parameter in this.Rows.Items)
            {
                if (parameter.ContainerName != rdl.ShortName)
                {
                    parameter.ContainerName = rdl.ShortName;
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
                row.IsAllowedToWrite = this.permissionService.CanWrite(ClassKind.Category, row.ParameterType.Container);
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// Update the <see cref="Rows" /> collection based on a collection of
        /// <see cref="ParameterTypeRowViewModel" /> to display.
        /// </summary>
        /// <param name="rowsToDisplay">A collection of <see cref="ParameterTypeRowViewModel" /></param>
        private void UpdateRows(IEnumerable<ParameterTypeRowViewModel> rowsToDisplay)
        {
            rowsToDisplay = rowsToDisplay.ToList();

            var deletedRows = this.Rows.Items.Where(x => rowsToDisplay.All(r => r.ParameterType.Iid != x.ParameterType.Iid)).ToList();
            var addedRows = rowsToDisplay.Where(x => this.Rows.Items.All(r => r.ParameterType.Iid != x.ParameterType.Iid)).ToList();
            var existingRows = rowsToDisplay.Where(x => this.Rows.Items.Any(r => r.ParameterType.Iid == x.ParameterType.Iid)).ToList();

            this.Rows.RemoveMany(deletedRows);
            this.Rows.AddRange(addedRows);

            foreach (var existingRow in existingRows)
            {
                this.Rows.Items.First(x => x.ParameterType.Iid == existingRow.ParameterType.Iid).UpdateProperties(existingRow);
            }
        }
    }
}
