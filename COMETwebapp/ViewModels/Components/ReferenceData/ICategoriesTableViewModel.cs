// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICategoriesTableViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;
    using COMETwebapp.Wrappers;
    using DevExpress.Blazor;
    using DynamicData;

    /// <summary>
    /// View model used to manage <see cref="Category" />
    /// </summary>
    public interface ICategoriesTableViewModel : ISingleIterationApplicationBaseViewModel
    {
        /// <summary>
        /// The <see cref="Category" /> to create or edit
        /// </summary>
        Category Category { get; set; }

        /// <summary>
        /// Gets or sets the data source for the grid control.
        /// </summary>
        SourceList<Category> DataSource { get; }

        /// <summary>
        /// A reactive collection of <see cref="CategoryRowViewModel" />
        /// </summary>
        SourceList<CategoryRowViewModel> Rows { get; }

        /// <summary>
        /// Injected property to get access to <see cref="IShowHideDeprecatedThingsService" />
        /// </summary>
        IShowHideDeprecatedThingsService ShowHideDeprecatedThingsService { get; }

        /// <summary>
        ///    Available <see cref="ReferenceDataLibrary"/>s
        /// </summary>
        IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; set; }

        /// <summary>
        /// Available <see cref="ClassKind" />s
        /// </summary>
        IEnumerable<ClassKindWrapper> PermissibleClasses { get; set; }

        /// <summary>
        /// Selected <see cref="ClassKind" />s
        /// </summary>
        IEnumerable<ClassKindWrapper> SelectedPermissibleClasses { get; set; }

        /// <summary>
        /// Selected super <see cref="Category" />
        /// </summary>
        IEnumerable<Category> SelectedSuperCategories { get; set; }

        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        bool IsOnDeprecationMode { get; set; }

        /// <summary>
        /// popum message dialog
        /// </summary>
        string ConfirmationMessageDialog { get; set; }

        /// <summary>
        /// selected container
        /// </summary>
        ReferenceDataLibrary SelectedReferenceDataLibrary { get; set; }

        /// <summary>
        /// The <see cref="ICategoryHierarchyDiagramViewModel" />
        /// </summary>
        ICategoryHierarchyDiagramViewModel CategoryHierarchyDiagramViewModel { get; }

        /// <summary>
        /// Action invoked when the deprecate or undeprecate button is clicked
        /// </summary>
        /// <param name="categoryRow"> The <see cref="CategoryRowViewModel" /> to deprecate or undeprecate </param>
        void OnDeprecateUnDeprecateButtonClick(CategoryRowViewModel categoryRow);

        /// <summary>
        /// Method invoked when confirming the deprecation/un-deprecation of a <see cref="Category" />
        /// </summary>
        void OnConfirmButtonClick();

        /// <summary>
        /// Method invoked when canceling the deprecation/un-deprecation of a <see cref="Category" />
        /// </summary>
        void OnCancelButtonClick();

        /// <summary>
        /// Tries to create a new <see cref="Category" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task AddingCategory();

        /// <summary>
        ///     Method invoked when the component is ready to start, having received its
        ///     initial parameters from its parent in the render tree.
        ///     Override this method if you will perform an asynchronous operation and
        ///     want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task OnInitializedAsync();

        /// <summary>
        /// set the selected <see cref="CategoryRowViewModel" />
        /// </summary>
        /// <param name="selectedCategory">The selected <see cref="CategoryRowViewModel" /></param>
        void SelectCategory(CategoryRowViewModel selectedCategory);
    }
}
