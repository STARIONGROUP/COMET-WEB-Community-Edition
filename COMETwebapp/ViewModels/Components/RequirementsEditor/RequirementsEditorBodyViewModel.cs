// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsEditorBodyViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.ViewModels.Components.RequirementsEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using ReactiveUI;

    /// <summary>
    /// View model that handles the logic for the Requirements Editor application: it exposes the
    /// <see cref="RequirementsSpecification" />s of the iteration as documents, and the search/filter state used to
    /// render them.
    /// </summary>
    public class RequirementsEditorBodyViewModel : SingleIterationApplicationBaseViewModel, IRequirementsEditorBodyViewModel
    {
        /// <summary>
        /// Backing field for <see cref="SelectedSpecification" />
        /// </summary>
        private RequirementsSpecification selectedSpecification;

        /// <summary>
        /// Backing field for <see cref="SearchText" />
        /// </summary>
        private string searchText;

        /// <summary>
        /// Backing field for <see cref="IsTocCollapsed" />
        /// </summary>
        private bool isTocCollapsed;

        /// <summary>
        /// Backing field for <see cref="ShowOwner" />
        /// </summary>
        private bool showOwner = true;

        /// <summary>
        /// Backing field for <see cref="ShowCategory" />
        /// </summary>
        private bool showCategory = true;

        /// <summary>
        /// The <see cref="Guid" />s of the table-of-contents tree nodes that are currently collapsed.
        /// </summary>
        private readonly HashSet<Guid> collapsedTreeNodes = [];

        /// <summary>
        /// The <see cref="Guid" />s of the document groups that are currently collapsed.
        /// </summary>
        private readonly HashSet<Guid> collapsedDocumentGroups = [];

        /// <summary>
        /// Backing field for <see cref="DisplayMode" />
        /// </summary>
        private RequirementRowDisplayMode displayMode = RequirementRowDisplayMode.ShortNameNameAndDefinition;

        /// <summary>
        /// Backing field for <see cref="SelectedOwners" />
        /// </summary>
        private IEnumerable<DomainOfExpertise> selectedOwners = [];

        /// <summary>
        /// Backing field for <see cref="SelectedCategories" />
        /// </summary>
        private IEnumerable<Category> selectedCategories = [];

        /// <summary>
        /// Creates a new instance of <see cref="RequirementsEditorBodyViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        public RequirementsEditorBodyViewModel(ISessionService sessionService, ICDPMessageBus messageBus) : base(sessionService, messageBus)
        {
        }

        /// <summary>
        /// Gets the non-deprecated <see cref="RequirementsSpecification" />s of the current iteration.
        /// </summary>
        public IEnumerable<RequirementsSpecification> AvailableSpecifications { get; private set; } = [];

        /// <summary>
        /// Gets or sets the <see cref="RequirementsSpecification" /> currently shown as a document.
        /// </summary>
        public RequirementsSpecification SelectedSpecification
        {
            get => this.selectedSpecification;
            set => this.RaiseAndSetIfChanged(ref this.selectedSpecification, value);
        }

        /// <summary>
        /// Gets or sets the keyword used to filter requirements on their definition text.
        /// </summary>
        public string SearchText
        {
            get => this.searchText;
            set => this.RaiseAndSetIfChanged(ref this.searchText, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the left table-of-contents tree is collapsed.
        /// </summary>
        public bool IsTocCollapsed
        {
            get => this.isTocCollapsed;
            set => this.RaiseAndSetIfChanged(ref this.isTocCollapsed, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the owner pill is shown on specifications, groups and requirements.
        /// </summary>
        public bool ShowOwner
        {
            get => this.showOwner;
            set => this.RaiseAndSetIfChanged(ref this.showOwner, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the category pills are shown on specifications, groups and requirements.
        /// </summary>
        public bool ShowCategory
        {
            get => this.showCategory;
            set => this.RaiseAndSetIfChanged(ref this.showCategory, value);
        }

        /// <summary>
        /// Gets or sets the way each requirement is rendered as a row.
        /// </summary>
        public RequirementRowDisplayMode DisplayMode
        {
            get => this.displayMode;
            set => this.RaiseAndSetIfChanged(ref this.displayMode, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="DomainOfExpertise" /> owners; an empty selection means no owner filtering.
        /// </summary>
        public IEnumerable<DomainOfExpertise> SelectedOwners
        {
            get => this.selectedOwners;
            set => this.RaiseAndSetIfChanged(ref this.selectedOwners, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Category" /> values; an empty selection means no category filtering.
        /// </summary>
        public IEnumerable<Category> SelectedCategories
        {
            get => this.selectedCategories;
            set => this.RaiseAndSetIfChanged(ref this.selectedCategories, value);
        }

        /// <summary>
        /// Gets the distinct <see cref="DomainOfExpertise" /> owners available to filter on.
        /// </summary>
        public IReadOnlyList<DomainOfExpertise> AvailableOwners { get; private set; } = [];

        /// <summary>
        /// Gets the distinct <see cref="Category" /> values available to filter on.
        /// </summary>
        public IReadOnlyList<Category> AvailableCategories { get; private set; } = [];

        /// <summary>
        /// Gets a value indicating whether any search or filter is currently narrowing the document.
        /// </summary>
        private bool IsFilterActive => !string.IsNullOrWhiteSpace(this.SearchText) || this.SelectedOwners.Any() || this.SelectedCategories.Any();

        /// <summary>
        /// Gets the visible, non-deprecated requirements located directly under the given <paramref name="container" />.
        /// </summary>
        /// <param name="container">The <see cref="RequirementsContainer" /> (specification or group)</param>
        /// <returns>The requirements to render under that container</returns>
        public IEnumerable<Requirement> GetRequirements(RequirementsContainer container)
        {
            if (this.SelectedSpecification == null)
            {
                return [];
            }

            var group = container as RequirementsGroup;

            return this.SelectedSpecification.Requirement
                .Where(x => !x.IsDeprecated && ReferenceEquals(x.Group, group) && this.PassesFilter(x))
                .OrderBy(x => x.ShortName);
        }

        /// <summary>
        /// Gets the visible child <see cref="RequirementsGroup" />s of the given <paramref name="container" />.
        /// </summary>
        /// <param name="container">The <see cref="RequirementsContainer" /> (specification or group)</param>
        /// <returns>The child groups to render</returns>
        public IEnumerable<RequirementsGroup> GetGroups(RequirementsContainer container)
        {
            return container.Group
                .Where(this.ShouldDisplayGroup)
                .OrderBy(x => x.ShortName);
        }

        /// <summary>
        /// Determines whether the given <paramref name="group" /> should be displayed given the active search and filters.
        /// </summary>
        /// <param name="group">The <see cref="RequirementsGroup" /></param>
        /// <returns>true if no filter is active, or the group (or a descendant) has visible requirements</returns>
        public bool ShouldDisplayGroup(RequirementsGroup group)
        {
            if (!this.IsFilterActive)
            {
                return true;
            }

            return this.GetRequirements(group).Any() || group.Group.Any(this.ShouldDisplayGroup);
        }

        /// <summary>
        /// Gets whether the tree node with the given <paramref name="iid" /> is collapsed in the table-of-contents tree.
        /// </summary>
        /// <param name="iid">The identifier of the specification or group node</param>
        /// <returns>true if collapsed</returns>
        public bool IsTreeNodeCollapsed(Guid iid)
        {
            return this.collapsedTreeNodes.Contains(iid);
        }

        /// <summary>
        /// Toggles the collapsed state of the table-of-contents tree node with the given <paramref name="iid" />.
        /// </summary>
        /// <param name="iid">The identifier of the specification or group node</param>
        public void ToggleTreeNode(Guid iid)
        {
            if (!this.collapsedTreeNodes.Add(iid))
            {
                this.collapsedTreeNodes.Remove(iid);
            }
        }

        /// <summary>
        /// Gets whether the group with the given <paramref name="iid" /> is collapsed in the document panel.
        /// </summary>
        /// <param name="iid">The identifier of the group</param>
        /// <returns>true if collapsed</returns>
        public bool IsDocumentGroupCollapsed(Guid iid)
        {
            return this.collapsedDocumentGroups.Contains(iid);
        }

        /// <summary>
        /// Toggles the collapsed state of the document group with the given <paramref name="iid" />.
        /// </summary>
        /// <param name="iid">The identifier of the group</param>
        public void ToggleDocumentGroup(Guid iid)
        {
            if (!this.collapsedDocumentGroups.Add(iid))
            {
                this.collapsedDocumentGroups.Remove(iid);
            }
        }

        /// <summary>
        /// Handles the refresh of the current session by reloading the specifications while preserving the selection.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnSessionRefreshed()
        {
            var previouslySelected = this.SelectedSpecification;
            await this.OnThingChanged();

            if (previouslySelected != null && this.AvailableSpecifications.Contains(previouslySelected))
            {
                this.SelectedSpecification = previouslySelected;
            }
        }

        /// <summary>
        /// Update this view model properties when the <see cref="Iteration" /> has changed.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnThingChanged()
        {
            await base.OnThingChanged();

            this.IsLoading = true;

            this.AvailableSpecifications = this.CurrentThing.RequirementsSpecification
                .Where(x => !x.IsDeprecated)
                .OrderBy(x => x.ShortName)
                .ToList();

            var allRequirements = this.AvailableSpecifications
                .SelectMany(x => x.Requirement)
                .Where(x => !x.IsDeprecated)
                .ToList();

            this.AvailableOwners = allRequirements
                .Select(x => x.Owner)
                .Where(x => x != null)
                .DistinctBy(x => x.Iid)
                .OrderBy(x => x.ShortName)
                .ToList();

            this.AvailableCategories = allRequirements
                .SelectMany(x => x.Category)
                .DistinctBy(x => x.Iid)
                .OrderBy(x => x.ShortName)
                .ToList();

            this.SelectedSpecification = this.AvailableSpecifications.FirstOrDefault();

            this.IsLoading = false;
        }

        /// <summary>
        /// Determines whether the given <paramref name="requirement" /> passes the active search and filters.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>true if it should be shown</returns>
        private bool PassesFilter(Requirement requirement)
        {
            if (!string.IsNullOrWhiteSpace(this.SearchText)
                && !requirement.Definition.Any(x => x.Content != null && x.Content.Contains(this.SearchText, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            if (this.SelectedOwners.Any() && !this.SelectedOwners.Contains(requirement.Owner))
            {
                return false;
            }

            return !this.SelectedCategories.Any() || requirement.Category.Intersect(this.SelectedCategories).Any();
        }
    }
}
