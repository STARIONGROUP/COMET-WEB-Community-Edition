// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CategoriesTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.ReferenceData.Categories
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable;
    using COMETwebapp.ViewModels.Components.ReferenceData.Rows;
    using COMETwebapp.Wrappers;

    /// <summary>
    /// View model used to manage <see cref="Category" />
    /// </summary>
    public class CategoriesTableViewModel : DeprecatableDataItemTableViewModel<Category, CategoryRowViewModel>, ICategoriesTableViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoriesTableViewModel" /> class.
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        public CategoriesTableViewModel(ISessionService sessionService, IShowHideDeprecatedThingsService showHideDeprecatedThingsService, ICDPMessageBus messageBus, ILogger<CategoriesTableViewModel> logger)
            : base(sessionService, messageBus, showHideDeprecatedThingsService, logger)
        {
            this.InitializeSubscriptions([typeof(ReferenceDataLibrary)]);
        }

        /// <summary>
        /// The <see cref="ICategoryHierarchyDiagramViewModel" />
        /// </summary>
        public ICategoryHierarchyDiagramViewModel CategoryHierarchyDiagramViewModel { get; } = new CategoryHierarchyDiagramViewModel();

        /// <summary>
        /// Available <see cref="ReferenceDataLibrary" />s
        /// </summary>
        public IEnumerable<ReferenceDataLibrary> ReferenceDataLibraries { get; set; } = [];

        /// <summary>
        /// Available <see cref="ClassKind" />s
        /// </summary>
        public IEnumerable<ClassKindWrapper> PermissibleClasses { get; set; } = Enum.GetValues<ClassKind>().Select(x => new ClassKindWrapper(x));

        /// <summary>
        /// selected container
        /// </summary>
        public ReferenceDataLibrary SelectedReferenceDataLibrary { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// Override this method if you will perform an asynchronous operation and
        /// want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        public override void InitializeViewModel()
        {
            base.InitializeViewModel();
            this.ReferenceDataLibraries = this.SessionService.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries();
        }

        /// <summary>
        /// Tries to create a new <see cref="Category" />
        /// </summary>
        /// <param name="shouldCreate">The value to check if a new <see cref="Category" /> should be created</param>
        /// <returns>A <see cref="Task" /></returns>
        public async Task CreateCategory(bool shouldCreate)
        {
            try
            {
                this.IsLoading = true;

                var hasRdlChanged = this.SelectedReferenceDataLibrary != this.CurrentThing.Container;
                var rdlClone = this.SelectedReferenceDataLibrary.Clone(false);
                var thingsToCreate = new List<Thing>();

                if (shouldCreate || hasRdlChanged)
                {
                    rdlClone.DefinedCategory.Add(this.CurrentThing);
                    thingsToCreate.Add(rdlClone);
                }

                thingsToCreate.Add(this.CurrentThing);
                await this.SessionService.CreateOrUpdateThingsWithNotification(rdlClone, thingsToCreate, this.GetNotificationDescription(shouldCreate));

                if (this.CurrentThing.Original is not null)
                {
                    this.CurrentThing = (Category)this.CurrentThing.Original.Clone(true);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Create or Update Category failed");
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Update this view model properties when the <see cref="SingleThingApplicationBaseViewModel{TThing}.CurrentThing" /> has
        /// changed
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            await base.OnThingChanged();
            this.SelectedReferenceDataLibrary = (ReferenceDataLibrary)this.CurrentThing.Container ?? this.ReferenceDataLibraries.FirstOrDefault();

            if (this.CurrentThing.Iid == Guid.Empty)
            {
                return;
            }

            this.CategoryHierarchyDiagramViewModel.SelectedCategory = this.CurrentThing;
            this.CategoryHierarchyDiagramViewModel.Rows = this.CurrentThing.SuperCategory;
            this.CategoryHierarchyDiagramViewModel.SubCategories = ((Category)this.CurrentThing.Original).AllDerivedCategories();
            this.CategoryHierarchyDiagramViewModel.SetupDiagram();
        }

        /// <summary>
        /// Queries a list of things of the current type
        /// </summary>
        /// <returns>A list of things</returns>
        protected override List<Category> QueryListOfThings()
        {
            return this.SessionService.GetSiteDirectory().AvailableReferenceDataLibraries().SelectMany(x => x.DefinedCategory).ToList();
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
        protected override async Task OnSessionRefreshed()
        {
            var updatedRdls = this.UpdatedThings.OfType<ReferenceDataLibrary>().ToList();
            var updatedCategories = this.UpdatedThings.OfType<Category>().ToList();
            await base.OnSessionRefreshed();

            if (updatedCategories.Count != 0)
            {
                this.CategoryHierarchyDiagramViewModel.SetupDiagram();
            }

            foreach (var rdl in updatedRdls)
            {
                this.RefreshContainerName(rdl);
            }
        }
    }
}
