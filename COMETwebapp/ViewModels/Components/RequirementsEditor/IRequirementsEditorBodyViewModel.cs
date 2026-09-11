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

    using FluentResults;

    /// <summary>
    /// Interface for the <see cref="RequirementsEditorBodyViewModel" />, driving the Requirements Editor application.
    /// </summary>
    public interface IRequirementsEditorBodyViewModel : ISingleIterationApplicationBaseViewModel
    {
        /// <summary>
        /// Gets the view model driving the requirements changelog view.
        /// </summary>
        IRequirementsChangelogViewModel ChangelogViewModel { get; }

        /// <summary>
        /// Gets or sets the <see cref="RequirementsEditorView" /> currently shown.
        /// </summary>
        RequirementsEditorView ActiveView { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the document is currently shown because of a navigation from the
        /// changelog, in which case the document view offers a "back to changes" affordance.
        /// </summary>
        bool CameFromChangelog { get; set; }

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
        /// Gets or sets a value indicating whether the table-of-contents tree labels specifications and groups by their
        /// short name (true) or their name (false).
        /// </summary>
        bool TreeUsesShortName { get; set; }

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
        /// Gets or sets a value indicating whether the <see cref="SimpleParameterValue" /> columns are shown under each requirement.
        /// </summary>
        bool ShowSimpleParameterValues { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ParametricConstraint" /> trees are shown under each requirement.
        /// </summary>
        bool ShowParametricConstraints { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the relationships to and from each requirement are shown under it.
        /// </summary>
        bool ShowTraceability { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Requirement" /> the document should scroll to on the next render, set by
        /// <see cref="NavigateToRequirement" /> and cleared by the component once the scroll has happened.
        /// </summary>
        Requirement ScrollTarget { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RequirementsGroup" /> the document should scroll to on the next render, set by
        /// <see cref="NavigateToGroup" /> when a table-of-contents entry is clicked and cleared by the component once the
        /// scroll has happened.
        /// </summary>
        RequirementsGroup ScrollTargetGroup { get; set; }

        /// <summary>
        /// Gets the distinct <see cref="ParameterType" />s used by the <see cref="SimpleParameterValue" />s of the
        /// non-deprecated requirements of the selected specification, ordered by short name. The columns are stable
        /// across search and owner/category filtering.
        /// </summary>
        /// <returns>The parameter types, or an empty list when no specification is selected</returns>
        IReadOnlyList<ParameterType> GetSpecificationParameterTypes();

        /// <summary>
        /// Gets or sets the <see cref="ParameterType" /> value columns the user chose to show; an empty selection shows
        /// every parameter type used in the specification.
        /// </summary>
        IEnumerable<ParameterType> SelectedParameterTypeColumns { get; set; }

        /// <summary>
        /// Gets the parameter-type value columns to render: <see cref="GetSpecificationParameterTypes" /> narrowed to
        /// <see cref="SelectedParameterTypeColumns" />, or all of them when the user has not picked any.
        /// </summary>
        /// <returns>The columns to show, in short-name order</returns>
        IReadOnlyList<ParameterType> GetVisibleParameterTypes();

        /// <summary>
        /// Gets the <see cref="SimpleParameterValue" /> of the given <paramref name="requirement" /> for the given
        /// <paramref name="parameterType" />.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <param name="parameterType">The <see cref="ParameterType" /></param>
        /// <returns>The value, or null when the requirement has no value for that parameter type</returns>
        SimpleParameterValue GetSimpleParameterValue(Requirement requirement, ParameterType parameterType);

        /// <summary>
        /// Updates the given <see cref="SimpleParameterValue" /> on the server with the given <paramref name="newValue" />;
        /// the original is never mutated, a clone is sent, and the outcome (including a concurrency conflict) is surfaced
        /// as a toast.
        /// </summary>
        /// <param name="value">The <see cref="SimpleParameterValue" /> to update</param>
        /// <param name="newValue">The new value (one entry per component)</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the write</returns>
        Task<Result> UpdateSimpleParameterValue(SimpleParameterValue value, IEnumerable<string> newValue);

        /// <summary>
        /// Creates a new, empty <see cref="SimpleParameterValue" /> of the given <paramref name="parameterType" /> on the
        /// given <paramref name="requirement" /> so the parameter becomes available to edit.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /> the value is added to</param>
        /// <param name="parameterType">The <see cref="ParameterType" /> of the new value</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the write</returns>
        Task<Result> CreateSimpleParameterValue(Requirement requirement, ParameterType parameterType);

        /// <summary>
        /// Gets the root <see cref="BooleanExpression" />s of the given <paramref name="constraint" />: its
        /// <see cref="ParametricConstraint.TopExpression" /> when set, otherwise the expressions that are not a term
        /// of any other expression.
        /// </summary>
        /// <param name="constraint">The <see cref="ParametricConstraint" /></param>
        /// <returns>The root expressions to render the constraint tree from</returns>
        IEnumerable<BooleanExpression> GetTopExpressions(ParametricConstraint constraint);

        /// <summary>
        /// Gets the child terms of the given <paramref name="expression" />; relational expressions are leaves.
        /// </summary>
        /// <param name="expression">The <see cref="BooleanExpression" /></param>
        /// <returns>The child expressions</returns>
        IReadOnlyList<BooleanExpression> GetTerms(BooleanExpression expression);

        /// <summary>
        /// Gets every <see cref="ParameterOrOverrideBase" /> bound to the given <paramref name="expression" /> through a
        /// <see cref="BinaryRelationship" />.
        /// </summary>
        /// <param name="expression">The <see cref="RelationalExpression" /></param>
        /// <returns>The bound parameters, empty when none are bound</returns>
        IReadOnlyList<ParameterOrOverrideBase> GetBoundParameters(RelationalExpression expression);

        /// <summary>
        /// Gets the <see cref="ParameterOrOverrideBase" /> bound to the given <paramref name="expression" /> through a
        /// <see cref="BinaryRelationship" />.
        /// </summary>
        /// <param name="expression">The <see cref="RelationalExpression" /></param>
        /// <returns>The bound parameter, or null when none is bound</returns>
        ParameterOrOverrideBase GetBoundParameter(RelationalExpression expression);

        /// <summary>
        /// Gets the model code of the <see cref="ParameterOrOverrideBase" /> bound to the given
        /// <paramref name="expression" /> through a <see cref="BinaryRelationship" />.
        /// </summary>
        /// <param name="expression">The <see cref="RelationalExpression" /></param>
        /// <returns>The model code, or null when no parameter is bound</returns>
        string GetBoundParameterModelCode(RelationalExpression expression);

        /// <summary>
        /// Gets the formatted published value of the given <paramref name="parameter" />.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase" /></param>
        /// <returns>The formatted published value, or null when it cannot be shown unambiguously</returns>
        string GetPublishedValue(ParameterOrOverrideBase parameter);

        /// <summary>
        /// Gets the published value of the <see cref="ParameterOrOverrideBase" /> bound to the given
        /// <paramref name="expression" />.
        /// </summary>
        /// <param name="expression">The <see cref="RelationalExpression" /></param>
        /// <returns>The formatted published value, or null when no parameter is bound</returns>
        string GetBoundParameterPublishedValue(RelationalExpression expression);

        /// <summary>
        /// Gets the <see cref="ParameterOrOverrideBase" />s of the element tree that could be linked to the given
        /// <paramref name="expression" />, i.e. the ones sharing its <see cref="ParameterType" />.
        /// </summary>
        /// <param name="expression">The <see cref="RelationalExpression" /></param>
        /// <returns>The candidate parameters, ordered by model code</returns>
        IReadOnlyList<ParameterOrOverrideBase> GetLinkableParameters(RelationalExpression expression);

        /// <summary>
        /// Creates and removes the <see cref="BinaryRelationship" />s so that the given <paramref name="expression" />
        /// ends up bound to exactly the <paramref name="selected" /> parameters.
        /// </summary>
        /// <param name="expression">The <see cref="RelationalExpression" /></param>
        /// <param name="selected">The <see cref="ParameterOrOverrideBase" />s the expression should be bound to</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the operation</returns>
        Task<Result> UpdateParameterLinksAsync(RelationalExpression expression, IReadOnlyCollection<ParameterOrOverrideBase> selected);

        /// <summary>
        /// Gets a one-line human-readable summary of the given <paramref name="expression" /> tree.
        /// </summary>
        /// <param name="expression">The <see cref="BooleanExpression" /></param>
        /// <returns>The summary string</returns>
        string GetExpressionSummary(BooleanExpression expression);

        /// <summary>
        /// Gets whether the constraint expression tree node with the given <paramref name="iid" /> is collapsed.
        /// </summary>
        /// <param name="iid">The identifier of the <see cref="BooleanExpression" /></param>
        /// <returns>true if collapsed</returns>
        bool IsExpressionCollapsed(Guid iid);

        /// <summary>
        /// Toggles the collapsed state of the constraint expression tree node with the given <paramref name="iid" />.
        /// </summary>
        /// <param name="iid">The identifier of the <see cref="BooleanExpression" /></param>
        void ToggleExpression(Guid iid);

        /// <summary>
        /// Gets a display row for every <see cref="BinaryRelationship" /> and <see cref="MultiRelationship" /> of the
        /// iteration the given <paramref name="requirement" /> participates in.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>The traceability rows</returns>
        IReadOnlyList<RequirementRelationshipRow> GetTraceability(Requirement requirement);

        /// <summary>
        /// Navigates the document to the given <paramref name="requirement" />: selects its specification, expands
        /// its ancestor groups and flags it as the <see cref="ScrollTarget" />.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /> to navigate to</param>
        void NavigateToRequirement(Requirement requirement);

        /// <summary>
        /// Navigates the document to the given <paramref name="group" /> from a table-of-contents click: it expands the
        /// group's ancestor document groups and flags it as the <see cref="ScrollTargetGroup" />.
        /// </summary>
        /// <param name="group">The <see cref="RequirementsGroup" /> to navigate to</param>
        void NavigateToGroup(RequirementsGroup group);

        /// <summary>
        /// Navigates the document to the element identified by the given <paramref name="elementId" /> and
        /// <paramref name="elementKind" />, as clicked from a changelog row: it resolves the element (a requirement, a
        /// group, or a specification) in the current iteration and switches the <see cref="ActiveView" /> to
        /// <see cref="RequirementsEditorView.Document" />. Does nothing when the element cannot be resolved (e.g. it was
        /// deleted and is no longer part of the current iteration).
        /// </summary>
        /// <param name="elementId">The <see cref="CDP4Common.CommonData.Thing.Iid" /> of the changed element</param>
        /// <param name="elementKind">The <see cref="RequirementChange.ElementKind" /> of the changed element</param>
        void NavigateToChangelogElement(Guid elementId, string elementKind);

        /// <summary>
        /// Switches the <see cref="ActiveView" /> back to <see cref="RequirementsEditorView.Changelog" />, invoked from
        /// the "Back" affordance shown on the document after a changelog navigation. Requests a scroll to the changelog
        /// row of the element last navigated from, so the user lands back where they were.
        /// </summary>
        void ReturnToChangelog();

        /// <summary>
        /// Gets or sets the <see cref="RequirementsGroup" /> currently being dragged in the table of contents to change
        /// its nesting, or null when no drag is in progress.
        /// </summary>
        RequirementsGroup DraggedGroup { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RequirementsContainer" /> the dragged group is currently hovered over, or null;
        /// used to highlight only the hovered valid drop target.
        /// </summary>
        RequirementsContainer DragOverContainer { get; set; }

        /// <summary>
        /// Determines whether the given <paramref name="group" /> may be dropped onto the given <paramref name="target" />
        /// container (a different container in the same specification that is not the group itself or a descendant).
        /// </summary>
        /// <param name="group">The <see cref="RequirementsGroup" /> being moved.</param>
        /// <param name="target">The target <see cref="RequirementsContainer" />.</param>
        /// <returns>true when the move is allowed</returns>
        bool CanMoveGroup(RequirementsGroup group, RequirementsContainer target);

        /// <summary>
        /// Re-parents the given <paramref name="group" /> under the given <paramref name="target" /> container and
        /// persists the move; does nothing when the move is not allowed.
        /// </summary>
        /// <param name="group">The <see cref="RequirementsGroup" /> to move.</param>
        /// <param name="target">The target <see cref="RequirementsContainer" />.</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the move</returns>
        Task<Result> MoveGroupAsync(RequirementsGroup group, RequirementsContainer target);

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
