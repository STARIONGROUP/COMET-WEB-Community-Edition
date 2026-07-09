// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IRequirementsEditorBodyViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;

    /// <summary>
    /// Interface for the <see cref="RequirementsEditorBodyViewModel" />, driving the Requirements Editor application.
    /// </summary>
    public interface IRequirementsEditorBodyViewModel : ISingleIterationApplicationBaseViewModel
    {
        /// <summary>
        /// Gets the non-deprecated <see cref="RequirementsSpecification" />s of the current iteration.
        /// </summary>
        IEnumerable<RequirementsSpecification> AvailableSpecifications { get; }

        /// <summary>
        /// Gets or sets the <see cref="RequirementsSpecification" /> currently shown as a document.
        /// </summary>
        RequirementsSpecification SelectedSpecification { get; set; }

        /// <summary>
        /// Gets or sets the keyword used to filter requirements on their definition text.
        /// </summary>
        string SearchText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the left table-of-contents tree is collapsed.
        /// </summary>
        bool IsTocCollapsed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the owner pill is shown on specifications, groups and requirements.
        /// </summary>
        bool ShowOwner { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the category pills are shown on specifications, groups and requirements.
        /// </summary>
        bool ShowCategory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether groups are indented in the document.
        /// </summary>
        bool IndentGroups { get; set; }

        /// <summary>
        /// Gets the <see cref="IShowHideDeprecatedThingsService" /> that drives whether deprecated things are shown.
        /// </summary>
        IShowHideDeprecatedThingsService ShowHideDeprecatedThingsService { get; }

        /// <summary>
        /// Gets or sets the way each requirement is rendered as a row.
        /// </summary>
        RequirementRowDisplayMode DisplayMode { get; set; }

        /// <summary>
        /// Gets the distinct <see cref="DomainOfExpertise" /> owners available to filter on.
        /// </summary>
        IReadOnlyList<DomainOfExpertise> AvailableOwners { get; }

        /// <summary>
        /// Gets the distinct <see cref="Category" /> values available to filter on.
        /// </summary>
        IReadOnlyList<Category> AvailableCategories { get; }

        /// <summary>
        /// Gets or sets the selected <see cref="DomainOfExpertise" /> owners; an empty selection means no owner filtering.
        /// </summary>
        IEnumerable<DomainOfExpertise> SelectedOwners { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="Category" /> values; an empty selection means no category filtering.
        /// </summary>
        IEnumerable<Category> SelectedCategories { get; set; }

        /// <summary>
        /// Gets the visible, non-deprecated requirements located directly under the given <paramref name="container" />
        /// (the selected specification or one of its groups), after applying search and filters.
        /// </summary>
        /// <param name="container">The <see cref="RequirementsContainer" /> (specification or group)</param>
        /// <returns>The requirements to render under that container</returns>
        IEnumerable<Requirement> GetRequirements(RequirementsContainer container);

        /// <summary>
        /// Gets the visible child <see cref="RequirementsGroup" />s of the given <paramref name="container" />.
        /// </summary>
        /// <param name="container">The <see cref="RequirementsContainer" /> (specification or group)</param>
        /// <returns>The child groups to render</returns>
        IEnumerable<RequirementsGroup> GetGroups(RequirementsContainer container);

        /// <summary>
        /// Determines whether the given <paramref name="group" /> should be displayed given the active search and filters.
        /// </summary>
        /// <param name="group">The <see cref="RequirementsGroup" /></param>
        /// <returns>true if the group (or one of its descendants) has visible requirements, or no filter is active</returns>
        bool ShouldDisplayGroup(RequirementsGroup group);

        /// <summary>
        /// Gets whether the tree node with the given <paramref name="iid" /> is collapsed in the table-of-contents tree.
        /// </summary>
        /// <param name="iid">The <see cref="CDP4Common.CommonData.Thing.Iid" /> of the specification or group node</param>
        /// <returns>true if collapsed</returns>
        bool IsTreeNodeCollapsed(Guid iid);

        /// <summary>
        /// Toggles the collapsed state of the table-of-contents tree node with the given <paramref name="iid" />.
        /// </summary>
        /// <param name="iid">The <see cref="CDP4Common.CommonData.Thing.Iid" /> of the specification or group node</param>
        void ToggleTreeNode(Guid iid);

        /// <summary>
        /// Gets whether the group with the given <paramref name="iid" /> is collapsed in the document panel.
        /// </summary>
        /// <param name="iid">The <see cref="CDP4Common.CommonData.Thing.Iid" /> of the group</param>
        /// <returns>true if collapsed</returns>
        bool IsDocumentGroupCollapsed(Guid iid);

        /// <summary>
        /// Toggles the collapsed state of the document group with the given <paramref name="iid" />.
        /// </summary>
        /// <param name="iid">The <see cref="CDP4Common.CommonData.Thing.Iid" /> of the group</param>
        void ToggleDocumentGroup(Guid iid);

        /// <summary>
        /// Gets the view model driving the confirm dialog used for deprecate, restore and delete actions.
        /// </summary>
        IConfirmCancelPopupViewModel ConfirmCancelPopupViewModel { get; }

        /// <summary>
        /// Gets the view model driving the create/edit form.
        /// </summary>
        IEditRequirementThingViewModel EditViewModel { get; }

        /// <summary>
        /// Gets the header text shown on the create/edit popup.
        /// </summary>
        string EditPopupHeader { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the create/edit popup is open.
        /// </summary>
        bool IsOnEditMode { get; set; }

        /// <summary>
        /// Opens the create form for a new <see cref="RequirementsSpecification" />.
        /// </summary>
        void OpenCreateSpecification();

        /// <summary>
        /// Opens the create form for a new <see cref="RequirementsGroup" /> under the given <paramref name="parent" />.
        /// </summary>
        /// <param name="parent">The specification or group the new group is placed under.</param>
        void OpenCreateGroup(RequirementsContainer parent);

        /// <summary>
        /// Opens the create form for a new <see cref="Requirement" /> under the given <paramref name="parent" />.
        /// </summary>
        /// <param name="parent">The specification or group the new requirement is filed under.</param>
        void OpenCreateRequirement(RequirementsContainer parent);

        /// <summary>
        /// Opens the edit form for the given <paramref name="thing" />.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> to edit.</param>
        void OpenEdit(Thing thing);

        /// <summary>
        /// Opens the confirm dialog to toggle the deprecation of the given deprecatable <paramref name="thing" />.
        /// </summary>
        /// <param name="thing">The deprecatable <see cref="Thing" />.</param>
        void ConfirmDeprecation(Thing thing);

        /// <summary>
        /// Opens the confirm dialog to permanently delete the given <paramref name="thing" />.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> to delete.</param>
        void ConfirmDeletion(Thing thing);

        /// <summary>
        /// Persists an inline edit of the first definition of the given <paramref name="requirement" />.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /> whose definition changed.</param>
        /// <param name="content">The new definition content.</param>
        /// <returns>A <see cref="Task" /></returns>
        Task SaveInlineDefinitionAsync(Requirement requirement, string content);
    }
}
