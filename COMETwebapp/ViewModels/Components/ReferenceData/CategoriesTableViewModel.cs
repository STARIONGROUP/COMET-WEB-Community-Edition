// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoriesTableViewModel.cs" company="RHEA System S.A.">
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
    using COMETwebapp.Utilities.DisposableObject;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;

    using DynamicData;

    using ReactiveUI;

    /// <summary>
    /// View model used to manage <see cref="Category" />
    /// </summary>
    public class CategoriesTableViewModel : DisposableObject, ICategoriesTableViewModel
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
        /// A collection of all <see cref="CategoryRowViewModel" />
        /// </summary>
        private IEnumerable<CategoryRowViewModel> allRows = new List<CategoryRowViewModel>();

        /// <summary>
        /// Backing field for <see cref="IsAllowedToWrite" />
        /// </summary>
        private bool isAllowedToWrite;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoriesTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        public CategoriesTableViewModel(ISessionService sessionService)
        {
            this.sessionService = sessionService;
            this.permissionService = sessionService.Session.PermissionService;

            this.Disposables.Add(CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Category))
                .Where(objectChange => objectChange.EventKind == EventKind.Added &&
                                       objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                .Select(x => x.ChangedThing as Category)
                .Subscribe(this.AddNewCategory));

            this.Disposables.Add(CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Category))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                       objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                .Select(x => x.ChangedThing as Category)
                .Subscribe(this.UpdateCategory));

            this.Disposables.Add(CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ReferenceDataLibrary))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                       objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                .Select(x => x.ChangedThing as ReferenceDataLibrary)
                .Subscribe(this.RefreshContainerName));

            this.Disposables.Add(CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(PersonRole))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                       objectChange.ChangedThing.Cache == this.sessionService.Session.Assembler.Cache)
                .Select(x => x.ChangedThing as PersonRole)
                .Subscribe(_=> this.RefreshAccessRight()));
        }

        /// <summary>
        /// A reactive collection of <see cref="CategoryRowViewModel" />
        /// </summary>
        public SourceList<CategoryRowViewModel> Rows { get; } = new();

        /// <summary>
        /// Gets or sets the data source for the grid control.
        /// </summary>
        public SourceList<Category> DataSource { get; } = new();

        /// <summary>
        /// Value indicating if the <see cref="ParameterType" /> is deprecated
        /// </summary>
        public bool IsAllowedToWrite
        {
            get => this.isAllowedToWrite;
            set => this.RaiseAndSetIfChanged(ref this.isAllowedToWrite, value);
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// Override this method if you will perform an asynchronous operation and
        /// want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        public void OnInitializedAsync()
        {
            foreach (var siteReferenceDataLibrary in this.sessionService.Session.RetrieveSiteDirectory().SiteReferenceDataLibrary)
            {
                this.DataSource.AddRange(siteReferenceDataLibrary.DefinedCategory);
            }

            this.UpdateProperties(this.DataSource.Items);
            this.RefreshAccessRight();
        }

        /// <summary>
        /// Adds a new <see cref="Category" />
        /// </summary>
        public void AddNewCategory(Category category)
        {
            var newRows = new List<CategoryRowViewModel>(this.allRows)
            {
                new(category)
            };

            this.UpdateRows(newRows);
        }

        /// <summary>
        /// Updates the <see cref="Category" />
        /// </summary>
        public void UpdateCategory(Category category)
        {
            var updatedRows = new List<CategoryRowViewModel>(this.allRows);
            var index = updatedRows.FindIndex(x => x.Category.Iid == category.Iid);
            updatedRows[index] = new CategoryRowViewModel(category);
            this.UpdateRows(updatedRows);
        }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        /// <param name="categories">A collection of <see cref="Category" /></param>
        public void UpdateProperties(IEnumerable<Category> categories)
        {
            this.allRows = categories.Select(x => new CategoryRowViewModel(x));
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
            foreach (var category in this.Rows.Items)
            {
                if (category.ContainerName != rdl.ShortName)
                {
                    category.ContainerName = rdl.ShortName;
                }
            }
        }

        /// <summary>
        /// Updates the active user access rights
        /// </summary>
        private void RefreshAccessRight()
        {
            this.IsAllowedToWrite = this.sessionService.Session.RetrieveSiteDirectory().SiteReferenceDataLibrary.All(s => this.permissionService.CanWrite(ClassKind.Category, s));
        }

        /// <summary>
        /// Update the <see cref="Rows" /> collection based on a collection of
        /// <see cref="CategoryRowViewModel" /> to display.
        /// </summary>
        /// <param name="rowsToDisplay">A collection of <see cref="CategoryRowViewModel" /></param>
        private void UpdateRows(IEnumerable<CategoryRowViewModel> rowsToDisplay)
        {
            rowsToDisplay = rowsToDisplay.ToList();

            var deletedRows = this.Rows.Items.Where(x => rowsToDisplay.All(r => r.Category.Iid != x.Category.Iid)).ToList();
            var addedRows = rowsToDisplay.Where(x => this.Rows.Items.All(r => r.Category.Iid != x.Category.Iid)).ToList();
            var existingRows = rowsToDisplay.Where(x => this.Rows.Items.Any(r => r.Category.Iid == x.Category.Iid)).ToList();

            this.Rows.RemoveMany(deletedRows);
            this.Rows.AddRange(addedRows);

            foreach (var existingRow in existingRows)
            {
                this.Rows.Items.First(x => x.Category.Iid == existingRow.Category.Iid).UpdateProperties(existingRow);
            }
        }
    }
}