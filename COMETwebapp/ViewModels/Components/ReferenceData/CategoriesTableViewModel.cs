// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoriesTableViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
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

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities.DisposableObject;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;
    using COMETwebapp.Wrappers;

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
        /// Injected property to get access to <see cref="IShowHideDeprecatedThingsService" />
        /// </summary>
        public IShowHideDeprecatedThingsService ShowHideDeprecatedThingsService { get; }

        ///     The <see cref="Category" /> to create or edit
        /// </summary>
        public Category Category { get; set; } = new();
        
        /// <summary>
        /// A collection of all <see cref="CategoryRowViewModel" />
        /// </summary>
        private IEnumerable<CategoryRowViewModel> allRows = new List<CategoryRowViewModel>();

        /// <summary>
        /// Backing field for <see cref="IsOnDeprecationMode" />
        /// </summary>
        private bool isOnDeprecationMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoriesTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        public CategoriesTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService)
        {
            this.sessionService = sessionService;
            this.permissionService = sessionService.Session.PermissionService;
            this.ShowHideDeprecatedThingsService = showHideDeprecatedThingsService;

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
                .Subscribe(_ => this.RefreshAccessRight()));
        }

        /// <summary>The
        /// <see cref="Category" />
        /// to create or edit
        /// </summary>
        public Category Category { get; set; } = new();

        /// <summary>
        /// A reactive collection of <see cref="CategoryRowViewModel" />
        /// </summary>
        public SourceList<CategoryRowViewModel> Rows { get; } = new();

        /// <summary>
        /// Gets or sets the data source for the grid control.
        /// </summary>
        public SourceList<Category> DataSource { get; } = new();

        /// <summary>
        /// Available <see cref="ReferenceDataLibrary" />s
        /// </summary>
        public IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; set; }

        /// <summary>
        /// Available <see cref="ClassKind" />s
        /// </summary>
        public IEnumerable<ClassKindWrapper> PermissibleClasses { get; set; } = Enum.GetValues<ClassKind>().Select(x => new ClassKindWrapper(x));

        /// <summary>
        /// Selected <see cref="ClassKind" />s
        /// </summary>
        public IEnumerable<ClassKindWrapper> SelectedPermissibleClasses { get; set; } = new List<ClassKindWrapper>();

        /// <summary>
        /// Selected super <see cref="Category" />
        /// </summary>
        public IEnumerable<Category> SelectedSuperCategories { get; set; } = new List<Category>();

        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        public bool IsOnDeprecationMode
        {
            get => this.isOnDeprecationMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnDeprecationMode, value);
        }

        /// <summary>
        /// popum message dialog
        /// </summary>
        public string ConfirmationMessageDialog { get; set; }

        /// <summary>
        /// selected container
        /// </summary>
        public ReferenceDataLibrary SelectedReferenceDataLibrary { get; set; }

        /// <summary>
        /// Method invoked when canceling the deprecation/un-deprecation of a <see cref="Category" />
        /// </summary>
        public void OnCancelButtonClick()
        {
            this.IsOnDeprecationMode = false;
        }

        /// <summary>
        /// Method invoked when confirming the deprecation/un-deprecation of a <see cref="Category" />
        /// </summary>
        public async void OnConfirmButtonClick()
        {
            if (this.Category.IsDeprecated)
            {
                await this.UnDeprecatingCategory();
            }
            else
            {
                await this.DeprecatingCategory();
            }

            this.IsOnDeprecationMode = false;
        }

        /// <summary>
        /// Action invoked when the deprecate or undeprecate button is clicked
        /// </summary>
        /// <param name="categoryRow"> The <see cref="CategoryRowViewModel" /> to deprecate or undeprecate </param>
        public void OnDeprecateUnDeprecateButtonClick(CategoryRowViewModel categoryRow)
        {
            this.Category = new Category();
            this.Category = categoryRow.Category;
            this.ConfirmationMessageDialog = this.Category.IsDeprecated ? "You are about to un-deprecate the category: " + categoryRow.Name : "You are about to deprecate the category: " + categoryRow.Name;
            this.IsOnDeprecationMode = true;
        }

        /// <summary>
        /// Tries to create a new <see cref="Category" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task AddingCategory()
        {
            var thingsToCreate = new List<Thing>();

            if (this.SelectedSuperCategories.Any())
            {
                this.Category.SuperCategory = this.SelectedSuperCategories.ToList();
            }

            if (this.SelectedPermissibleClasses.Any())
            {
                this.Category.PermissibleClass = this.SelectedPermissibleClasses.Select(x => x.ClassKind).ToList();
            }

            this.Category.Container = this.SelectedReferenceDataLibrary;
            thingsToCreate.Add(this.Category);
            var clonedRDL = this.SelectedReferenceDataLibrary.Clone(false);
            clonedRDL.DefinedCategory.Add(this.Category);

            try
            {
                await this.sessionService.CreateThings(clonedRDL, thingsToCreate);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }
        }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// Override this method if you will perform an asynchronous operation and
        /// want the component to refresh when that operation is completed.
        /// </summary>
        public void OnInitialized()
        {
            foreach (var referenceDataLibrary in this.sessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries())
            {
                this.DataSource.AddRange(referenceDataLibrary.DefinedCategory);
            }

            this.ReferenceDataLibraries = this.sessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries();
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
            this.RefreshAccessRight();
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
            this.RefreshAccessRight();
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
        /// Tries to undeprecate a <see cref="Category" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task UnDeprecatingCategory()
        {
            var cateoryToUnDeprecate = new List<Category>();
            var clonedCateory = this.Category.Clone(false);
            clonedCateory.IsDeprecated = false;
            cateoryToUnDeprecate.Add(clonedCateory);

            try
            {
                await this.sessionService.UpdateThings(this.sessionService.GetSiteDirectory(), cateoryToUnDeprecate);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }

            this.IsOnDeprecationMode = false;
        }

        /// <summary>
        /// Tries to deprecate a <see cref="Category" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task DeprecatingCategory()
        {
            var categoryToDeprecate = new List<Category>();
            var clonedCategory = this.Category.Clone(false);
            clonedCategory.IsDeprecated = true;
            categoryToDeprecate.Add(clonedCategory);

            try
            {
                await this.sessionService.UpdateThings(this.sessionService.GetSiteDirectory(), categoryToDeprecate);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }

            this.IsOnDeprecationMode = false;
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
            foreach (var row in this.Rows.Items)
            {
                row.IsAllowedToWrite = this.permissionService.CanWrite(ClassKind.Category, row.Category.Container);
            }
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
