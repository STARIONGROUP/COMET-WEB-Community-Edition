// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeTableViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
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

    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.Utilities.DisposableObject;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DynamicData;

    /// <summary>
    /// View model used to manage <see cref="ParameterType" />
    /// </summary>
    public class ParameterTypeTableViewModel : DisposableObject, IParameterTypeTableViewModel
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
        /// Injected property to get access to <see cref="IShowHideDeprecatedThingsService" />
        /// </summary>
        public IShowHideDeprecatedThingsService showHideDeprecatedThingsService { get; }

        /// <summary>
        /// A collection of all <see cref="ParameterTypeRowViewModel" />
        /// </summary>
        private IEnumerable<ParameterTypeRowViewModel> allRows = new List<ParameterTypeRowViewModel>();

        /// <summary>
        ///    Available <see cref="ReferenceDataLibrary"/>s
        /// </summary>
        public IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        public ParameterTypeTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService)
        {
            this.sessionService = sessionService;
            this.permissionService = sessionService.Session.PermissionService;
            this.showHideDeprecatedThingsService = showHideDeprecatedThingsService;

            this.Disposables.Add(
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ParameterType))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added &&
                                           objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ParameterType)
                    .Subscribe(this.AddNewParameterType));

            this.Disposables.Add(
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ParameterType))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                           objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ParameterType)
                    .Subscribe(this.UpdateParameterType));

            this.Disposables.Add(
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ReferenceDataLibrary))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                           objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ReferenceDataLibrary)
                    .Subscribe(this.RefreshContainerName));

            this.Disposables.Add(CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(PersonRole))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                       objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                .Select(x => x.ChangedThing as PersonRole)
                .Subscribe(_ => this.RefreshAccessRight()));
        }

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
        public void OnInitializedAsync()
        {
            foreach (var referenceDataLibrary in this.sessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries())
            {
                this.DataSource.AddRange(referenceDataLibrary.ParameterType);
            }
            this.ReferenceDataLibraries = this.sessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries();
            this.UpdateProperties(this.DataSource.Items);
            this.RefreshAccessRight();
        }

        /// <summary>
        /// Adds a new <see cref="ParameterType" />
        /// </summary>
        public void AddNewParameterType(ParameterType parameterType)
        {
            var newRows = new List<ParameterTypeRowViewModel>(this.allRows)
            {
                new(parameterType)
            };

            this.UpdateRows(newRows);
            this.RefreshAccessRight();
        }

        /// <summary>
        /// Updates the <see cref="ParameterType" />
        /// </summary>
        public void UpdateParameterType(ParameterType parameterType)
        {
            var updatedRows = new List<ParameterTypeRowViewModel>(this.allRows);
            var index = updatedRows.FindIndex(x => x.ParameterType.Iid == parameterType.Iid);
            updatedRows[index] = new ParameterTypeRowViewModel(parameterType);
            this.UpdateRows(updatedRows);
            this.RefreshAccessRight();
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="parameterTypes">A collection of <see cref="ParameterType" /></param>
        public void UpdateProperties(IEnumerable<ParameterType> parameterTypes)
        {
            this.allRows = parameterTypes.Select(x => new ParameterTypeRowViewModel(x));
            this.UpdateRows(this.allRows);
        }

        /// <summary>
        /// Refresh the displayed container name for the category rows
        /// </summary>
        /// <param name="rdl">
        /// The updated <see cref="ReferenceDataLibrary" />.
        /// </param>
        private void RefreshContainerName(ReferenceDataLibrary rdl)
        {
            foreach (var parameter in this.Rows.Items)
            {
                if (parameter.ContainerName != rdl.ShortName)
                {
                    parameter.ContainerName = rdl.ShortName;
                }
            }
        }

        /// <summary>
        /// Updates the active user access rights
        /// </summary>
        private void RefreshAccessRight()
        {
            foreach (var row in Rows.Items)
            {
                row.IsAllowedToWrite = this.permissionService.CanWrite(ClassKind.Category, row.ParameterType.Container);
            }
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
