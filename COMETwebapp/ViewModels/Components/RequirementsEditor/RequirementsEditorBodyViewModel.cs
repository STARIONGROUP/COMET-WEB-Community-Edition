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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Extensions;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.Utilities;

    using FluentResults;

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
        /// Backing field for <see cref="IndentGroups" />
        /// </summary>
        private bool indentGroups = true;

        /// <summary>
        /// The <see cref="Guid" />s of the table-of-contents tree nodes that are currently collapsed.
        /// </summary>
        private readonly HashSet<Guid> collapsedTreeNodes = [];

        /// <summary>
        /// The <see cref="Guid" />s of the document groups that are currently collapsed.
        /// </summary>
        private readonly HashSet<Guid> collapsedDocumentGroups = [];

        /// <summary>
        /// The <see cref="Guid" />s of the constraint expression tree nodes that are currently collapsed.
        /// </summary>
        private readonly HashSet<Guid> collapsedExpressions = [];

        /// <summary>
        /// Backing field for <see cref="SelectedParameterTypeColumns" />
        /// </summary>
        private IEnumerable<ParameterType> selectedParameterTypeColumns = [];

        /// <summary>
        /// Backing field for <see cref="DisplayMode" />
        /// </summary>
        private RequirementRowDisplayMode displayMode = RequirementRowDisplayMode.ShortNameNameAndDefinition;

        /// <summary>
        /// Backing field for <see cref="TreeUsesShortName" />
        /// </summary>
        private bool treeUsesShortName = true;

        /// <summary>
        /// Backing field for <see cref="SelectedOwners" />
        /// </summary>
        private IEnumerable<DomainOfExpertise> selectedOwners = [];

        /// <summary>
        /// Backing field for <see cref="SelectedCategories" />
        /// </summary>
        private IEnumerable<Category> selectedCategories = [];

        /// <summary>
        /// Backing field for <see cref="ShowSimpleParameterValues" />
        /// </summary>
        private bool showSimpleParameterValues;

        /// <summary>
        /// Backing field for <see cref="ShowParametricConstraints" />
        /// </summary>
        private bool showParametricConstraints;

        /// <summary>
        /// Backing field for <see cref="ShowTraceability" />
        /// </summary>
        private bool showTraceability;

        /// <summary>
        /// Backing field for <see cref="ScrollTarget" />
        /// </summary>
        private Requirement scrollTarget;

        /// <summary>
        /// Memoised result of <see cref="GetSpecificationParameterTypes" /> - it is queried once per requirement row,
        /// so caching it keeps document rendering linear in the number of requirements.
        /// </summary>
        private (RequirementsSpecification Specification, bool ShowDeprecated, IReadOnlyList<ParameterType> ParameterTypes) parameterTypesCache;

        /// <summary>
        /// Creates a new instance of <see cref="RequirementsEditorBodyViewModel" />
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService" /></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus" /></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService" /></param>
        public RequirementsEditorBodyViewModel(ISessionService sessionService, ICDPMessageBus messageBus, IShowHideDeprecatedThingsService showHideDeprecatedThingsService) : base(sessionService, messageBus)
        {
            this.ShowHideDeprecatedThingsService = showHideDeprecatedThingsService;
        }

        /// <summary>
        /// Gets the <see cref="IShowHideDeprecatedThingsService" /> that drives whether deprecated things are shown.
        /// </summary>
        public IShowHideDeprecatedThingsService ShowHideDeprecatedThingsService { get; }

        /// <summary>
        /// Gets the <see cref="RequirementsSpecification" />s of the current iteration; deprecated ones are included
        /// only when the global "Show Deprecated Things" toggle is on.
        /// </summary>
        public IEnumerable<RequirementsSpecification> AvailableSpecifications =>
            this.CurrentThing == null
                ? []
                : this.CurrentThing.RequirementsSpecification
                    .Where(x => this.ShowHideDeprecatedThingsService.ShowDeprecatedThings || !x.IsDeprecated)
                    .OrderBy(x => x.ShortName);

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
        /// Gets or sets a value indicating whether groups are indented in the document. When off, groups and
        /// requirements are rendered flush-left regardless of nesting depth.
        /// </summary>
        public bool IndentGroups
        {
            get => this.indentGroups;
            set => this.RaiseAndSetIfChanged(ref this.indentGroups, value);
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
        /// Gets or sets a value indicating whether the table-of-contents tree labels specifications and groups by their
        /// short name (true) or their name (false).
        /// </summary>
        public bool TreeUsesShortName
        {
            get => this.treeUsesShortName;
            set => this.RaiseAndSetIfChanged(ref this.treeUsesShortName, value);
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
        /// Gets or sets a value indicating whether the <see cref="SimpleParameterValue" /> columns are shown under each requirement.
        /// </summary>
        public bool ShowSimpleParameterValues
        {
            get => this.showSimpleParameterValues;
            set => this.RaiseAndSetIfChanged(ref this.showSimpleParameterValues, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ParametricConstraint" /> trees are shown under each requirement.
        /// </summary>
        public bool ShowParametricConstraints
        {
            get => this.showParametricConstraints;
            set => this.RaiseAndSetIfChanged(ref this.showParametricConstraints, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the relationships to and from each requirement are shown under it.
        /// </summary>
        public bool ShowTraceability
        {
            get => this.showTraceability;
            set => this.RaiseAndSetIfChanged(ref this.showTraceability, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Requirement" /> the document should scroll to on the next render, set by
        /// <see cref="NavigateToRequirement" /> and cleared by the component once the scroll has happened.
        /// </summary>
        public Requirement ScrollTarget
        {
            get => this.scrollTarget;
            set => this.RaiseAndSetIfChanged(ref this.scrollTarget, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ParameterType" /> value columns the user chose to show; an empty selection shows
        /// every parameter type used in the specification.
        /// </summary>
        public IEnumerable<ParameterType> SelectedParameterTypeColumns
        {
            get => this.selectedParameterTypeColumns;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameterTypeColumns, value);
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
                .Where(x => (this.ShowHideDeprecatedThingsService.ShowDeprecatedThings || !x.IsDeprecated) && ReferenceEquals(x.Group, group) && this.PassesFilter(x))
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
        /// Gets the distinct <see cref="ParameterType" />s used by the <see cref="SimpleParameterValue" />s of the
        /// non-deprecated requirements of the selected specification, ordered by short name. The columns are stable
        /// across search and owner/category filtering so requirement rows line up; they form the value columns shown
        /// under each requirement.
        /// </summary>
        /// <returns>The parameter types, or an empty list when no specification is selected</returns>
        public IReadOnlyList<ParameterType> GetSpecificationParameterTypes()
        {
            if (this.SelectedSpecification == null)
            {
                return [];
            }

            var showDeprecated = this.ShowHideDeprecatedThingsService.ShowDeprecatedThings;

            if (ReferenceEquals(this.parameterTypesCache.Specification, this.SelectedSpecification) && this.parameterTypesCache.ShowDeprecated == showDeprecated)
            {
                return this.parameterTypesCache.ParameterTypes;
            }

            var parameterTypes = this.SelectedSpecification.Requirement
                .Where(x => showDeprecated || !x.IsDeprecated)
                .SelectMany(x => x.ParameterValue)
                .Select(x => x.ParameterType)
                .Where(x => x != null)
                .DistinctBy(x => x.Iid)
                .OrderBy(x => x.ShortName)
                .ToList();

            this.parameterTypesCache = (this.SelectedSpecification, showDeprecated, parameterTypes);
            return parameterTypes;
        }

        /// <summary>
        /// Gets the parameter-type value columns to render: <see cref="GetSpecificationParameterTypes" /> narrowed to
        /// <see cref="SelectedParameterTypeColumns" />, or all of them when the user has not picked any.
        /// </summary>
        /// <returns>The columns to show, in short-name order</returns>
        public IReadOnlyList<ParameterType> GetVisibleParameterTypes()
        {
            var all = this.GetSpecificationParameterTypes();
            var selectedIids = this.SelectedParameterTypeColumns.Select(x => x.Iid).ToHashSet();

            return selectedIids.Count == 0 ? all : all.Where(x => selectedIids.Contains(x.Iid)).ToList();
        }

        /// <summary>
        /// Gets the <see cref="SimpleParameterValue" /> of the given <paramref name="requirement" /> for the given
        /// <paramref name="parameterType" />.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <param name="parameterType">The <see cref="ParameterType" /></param>
        /// <returns>The value, or null when the requirement has no value for that parameter type</returns>
        public SimpleParameterValue GetSimpleParameterValue(Requirement requirement, ParameterType parameterType)
        {
            return requirement.ParameterValue.FirstOrDefault(x => x.ParameterType?.Iid == parameterType.Iid);
        }

        /// <summary>
        /// Updates the given <see cref="SimpleParameterValue" /> on the server with the given <paramref name="newValue" />;
        /// the original is never mutated, a clone is sent. A <see cref="NotificationDescription" /> surfaces the outcome
        /// (including a concurrency conflict rejected by the server) as a toast; on failure the original value is left
        /// untouched so the document reverts to it on the next render.
        /// </summary>
        /// <param name="value">The <see cref="SimpleParameterValue" /> to update</param>
        /// <param name="newValue">The new value (one entry per component)</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the write</returns>
        public async Task<Result> UpdateSimpleParameterValue(SimpleParameterValue value, IEnumerable<string> newValue)
        {
            var newValueArray = new ValueArray<string>(newValue);

            if (value.Value.SequenceEqual(newValueArray))
            {
                return Result.Ok();
            }

            var clone = value.Clone(false);
            clone.Value = newValueArray;

            return await this.SessionService.CreateOrUpdateThingsWithNotification(value.GetContainerOfType<Iteration>().Clone(false), [clone],
                new NotificationDescription { OnSuccess = "Value updated", OnError = "Failed to update the value (it may have been changed by someone else)" });
        }

        /// <summary>
        /// Creates a new, empty <see cref="SimpleParameterValue" /> of the given <paramref name="parameterType" /> on the
        /// given <paramref name="requirement" /> so the parameter becomes available to edit; the default value is a "-"
        /// per component. The <paramref name="requirement" /> is cloned before the write.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /> the value is added to</param>
        /// <param name="parameterType">The <see cref="ParameterType" /> of the new value</param>
        /// <returns>A <see cref="Task{T}" /> with the <see cref="Result" /> of the write</returns>
        public async Task<Result> CreateSimpleParameterValue(Requirement requirement, ParameterType parameterType)
        {
            var newValue = new SimpleParameterValue
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterType,
                Scale = (parameterType as QuantityKind)?.DefaultScale,
                Value = new ValueArray<string>(Enumerable.Repeat("-", Math.Max(parameterType.NumberOfValues, 1)))
            };

            var requirementClone = requirement.Clone(false);
            requirementClone.ParameterValue.Add(newValue);

            return await this.SessionService.CreateOrUpdateThingsWithNotification(requirement.GetContainerOfType<Iteration>().Clone(false), [requirementClone, newValue],
                new NotificationDescription { OnSuccess = $"Added {parameterType.ShortName}", OnError = $"Failed to add {parameterType.ShortName}" });
        }

        /// <summary>
        /// Gets the root <see cref="BooleanExpression" />s of the given <paramref name="constraint" />: its
        /// <see cref="ParametricConstraint.TopExpression" /> when set, otherwise the expressions that are not a term
        /// of any other expression.
        /// </summary>
        /// <param name="constraint">The <see cref="ParametricConstraint" /></param>
        /// <returns>The root expressions to render the constraint tree from</returns>
        public IEnumerable<BooleanExpression> GetTopExpressions(ParametricConstraint constraint)
        {
            if (constraint.TopExpression != null)
            {
                return [constraint.TopExpression];
            }

            return constraint.Expression.GetTopLevelExpressions();
        }

        /// <summary>
        /// Gets the child terms of the given <paramref name="expression" />; relational expressions are leaves.
        /// </summary>
        /// <param name="expression">The <see cref="BooleanExpression" /></param>
        /// <returns>The child expressions</returns>
        public IReadOnlyList<BooleanExpression> GetTerms(BooleanExpression expression)
        {
            return expression switch
            {
                AndExpression andExpression => andExpression.Term,
                OrExpression orExpression => orExpression.Term,
                ExclusiveOrExpression exclusiveOrExpression => exclusiveOrExpression.Term,
                NotExpression { Term: not null } notExpression => [notExpression.Term],
                _ => []
            };
        }

        /// <summary>
        /// Gets the <see cref="ParameterOrOverrideBase" /> bound to the given <paramref name="expression" /> through a
        /// <see cref="BinaryRelationship" />, the way requirement verification links a parameter to a relational
        /// expression.
        /// </summary>
        /// <param name="expression">The <see cref="RelationalExpression" /></param>
        /// <returns>The bound parameter, or null when none is bound</returns>
        public ParameterOrOverrideBase GetBoundParameter(RelationalExpression expression)
        {
            var binding = this.CurrentThing?.Relationship.OfType<BinaryRelationship>()
                .FirstOrDefault(x => (x.Source == expression && x.Target is ParameterOrOverrideBase) || (x.Target == expression && x.Source is ParameterOrOverrideBase));

            if (binding == null)
            {
                return null;
            }

            return (ParameterOrOverrideBase)(binding.Source == expression ? binding.Target : binding.Source);
        }

        /// <summary>
        /// Gets the model code of the <see cref="ParameterOrOverrideBase" /> bound to the given
        /// <paramref name="expression" />.
        /// </summary>
        /// <param name="expression">The <see cref="RelationalExpression" /></param>
        /// <returns>The model code, or null when no parameter is bound</returns>
        public string GetBoundParameterModelCode(RelationalExpression expression)
        {
            return this.GetBoundParameter(expression)?.ModelCode();
        }

        /// <summary>
        /// Gets the published value of the <see cref="ParameterOrOverrideBase" /> bound to the given
        /// <paramref name="expression" /> — the value the bound element last published, to compare against the
        /// constraint's threshold.
        /// </summary>
        /// <param name="expression">The <see cref="RelationalExpression" /></param>
        /// <returns>The formatted published value, or null when no parameter is bound</returns>
        public string GetBoundParameterPublishedValue(RelationalExpression expression)
        {
            var parameter = this.GetBoundParameter(expression);
            var valueSets = parameter?.ValueSets.OfType<ParameterValueSetBase>().ToList() ?? [];

            // only show a single, unambiguous published value; an option/state-dependent parameter has several and we
            // would otherwise present an arbitrary one as "the" published value
            return valueSets.Count == 1 ? ParameterValueFormatter.Format(valueSets[0].Published, parameter.Scale) : null;
        }

        /// <summary>
        /// Gets a one-line human-readable summary of the given <paramref name="expression" /> subtree (e.g.
        /// <c>NOT (d_r &lt; 500) AND (a &gt; 4)</c>), built the same way the tree renders so it stays consistent with it.
        /// </summary>
        /// <param name="expression">The <see cref="BooleanExpression" /></param>
        /// <returns>The summary string</returns>
        public string GetExpressionSummary(BooleanExpression expression)
        {
            switch (expression)
            {
                case RelationalExpression relational:
                    var scale = relational.Scale == null ? string.Empty : $" {relational.Scale.ShortName}";
                    return $"{relational.ParameterType?.ShortName} {ParameterValueFormatter.RelationalOperatorSymbol(relational.RelationalOperator)} {string.Join(", ", relational.Value)}{scale}";

                case NotExpression { Term: not null } not:
                    return $"NOT ({this.GetExpressionSummary(not.Term)})";

                default:
                    var separator = expression switch
                    {
                        AndExpression => " AND ",
                        OrExpression => " OR ",
                        ExclusiveOrExpression => " XOR ",
                        _ => " "
                    };

                    return string.Join(separator, this.GetTerms(expression).Select(x => $"({this.GetExpressionSummary(x)})"));
            }
        }

        /// <summary>
        /// Gets whether the expression tree node with the given <paramref name="iid" /> is collapsed.
        /// </summary>
        /// <param name="iid">The identifier of the <see cref="BooleanExpression" /></param>
        /// <returns>true if collapsed</returns>
        public bool IsExpressionCollapsed(Guid iid)
        {
            return this.collapsedExpressions.Contains(iid);
        }

        /// <summary>
        /// Toggles the collapsed state of the expression tree node with the given <paramref name="iid" />.
        /// </summary>
        /// <param name="iid">The identifier of the <see cref="BooleanExpression" /></param>
        public void ToggleExpression(Guid iid)
        {
            if (!this.collapsedExpressions.Add(iid))
            {
                this.collapsedExpressions.Remove(iid);
            }
        }

        /// <summary>
        /// Gets a display row for every <see cref="BinaryRelationship" /> and <see cref="MultiRelationship" /> of the
        /// iteration the given <paramref name="requirement" /> participates in.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>The traceability rows</returns>
        public IReadOnlyList<RequirementRelationshipRow> GetTraceability(Requirement requirement)
        {
            if (this.CurrentThing == null)
            {
                return [];
            }

            var rows = new List<RequirementRelationshipRow>();

            // ponytail: linear scan of the iteration's relationships; if this shows up in a profile on large models,
            // requirement.QueryRelationships is the SDK's indexed reverse-lookup.
            foreach (var relationship in this.CurrentThing.Relationship)
            {
                switch (relationship)
                {
                    case BinaryRelationship binary when binary.Source == requirement || binary.Target == requirement:
                        rows.Add(new RequirementRelationshipRow
                        {
                            Relationship = binary,
                            Direction = binary.Source == requirement ? RelationshipDirection.Outgoing : RelationshipDirection.Incoming,
                            RelatedThings = [binary.Source == requirement ? binary.Target : binary.Source],
                            RuleNames = this.GetMatchingRuleNames(binary)
                        });

                        break;

                    case MultiRelationship multi when multi.RelatedThing.Contains(requirement):
                        rows.Add(new RequirementRelationshipRow
                        {
                            Relationship = multi,
                            Direction = RelationshipDirection.Bidirectional,
                            RelatedThings = multi.RelatedThing.Where(x => x != requirement).ToList(),
                            RuleNames = this.GetMatchingRuleNames(multi)
                        });

                        break;
                }
            }

            return rows;
        }

        /// <summary>
        /// Navigates the document to the given <paramref name="requirement" />: clears the active search and filters
        /// (so the target is guaranteed to render), selects its specification, expands its ancestor groups and flags
        /// it as the <see cref="ScrollTarget" />.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /> to navigate to</param>
        public void NavigateToRequirement(Requirement requirement)
        {
            this.SearchText = null;
            this.SelectedOwners = [];
            this.SelectedCategories = [];

            // a link can point at a deprecated requirement; surface deprecated things so the target actually renders
            if (requirement.IsDeprecated)
            {
                this.ShowHideDeprecatedThingsService.ShowDeprecatedThings = true;
            }

            for (var group = requirement.Group; group != null; group = group.Container as RequirementsGroup)
            {
                this.collapsedDocumentGroups.Remove(group.Iid);
            }

            this.SelectedSpecification = requirement.GetContainerOfType<RequirementsSpecification>();
            this.ScrollTarget = requirement;
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
            this.parameterTypesCache = default;

            if (this.CurrentThing == null)
            {
                this.AvailableOwners = [];
                this.AvailableCategories = [];
                this.SelectedSpecification = null;
                this.IsLoading = false;
                return;
            }

            var allRequirements = this.CurrentThing.RequirementsSpecification
                .Where(x => !x.IsDeprecated)
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

        /// <summary>
        /// Gets the names of the <see cref="BinaryRelationshipRule" />s or <see cref="MultiRelationshipRule" />s of the
        /// open reference data libraries whose relationship category is carried by the given <paramref name="relationship" />.
        /// </summary>
        /// <param name="relationship">The <see cref="Relationship" /></param>
        /// <returns>The matching rule names</returns>
        private List<string> GetMatchingRuleNames(Relationship relationship)
        {
            var rules = this.SessionService.Session.OpenReferenceDataLibraries.SelectMany(x => x.Rule);

            var matching = relationship is BinaryRelationship
                ? rules.OfType<BinaryRelationshipRule>().Where(x => RelationshipCarriesCategory(relationship, x.RelationshipCategory)).Select(x => x.Name)
                : rules.OfType<MultiRelationshipRule>().Where(x => RelationshipCarriesCategory(relationship, x.RelationshipCategory)).Select(x => x.Name);

            return matching.ToList();
        }

        /// <summary>
        /// Determines whether the given <paramref name="relationship" /> carries the given <paramref name="ruleCategory" />
        /// directly or through a sub-category, the way a relationship satisfies a rule under ECSS-E-TM-10-25.
        /// </summary>
        /// <param name="relationship">The <see cref="Relationship" /></param>
        /// <param name="ruleCategory">The rule's <see cref="Category" /></param>
        /// <returns>true if the relationship is categorised with the rule's category or a sub-category of it</returns>
        private static bool RelationshipCarriesCategory(Relationship relationship, Category ruleCategory)
        {
            return relationship.Category.Any(x => x == ruleCategory || x.AllSuperCategories().Contains(ruleCategory));
        }
    }
}
