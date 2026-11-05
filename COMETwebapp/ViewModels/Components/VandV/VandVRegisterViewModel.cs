// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVRegisterViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.VandV
{
    using System.Runtime.CompilerServices;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Utilities.DisposableObject;

    using COMETwebapp.Utilities.VandV;
    using COMETwebapp.ViewModels.Components.RequirementsEditor;

    using ReactiveUI;

    /// <summary>
    /// View model of the V&amp;V register, see <see cref="IVandVRegisterViewModel" />. The register is a flat list of rows
    /// rebuilt, on the next read, whenever the snapshot, the scope, a filter or the expansion state changes. A closed tab
    /// reads nothing, so a reload costs it nothing.
    /// ponytail: every row renders (no virtualisation), which holds for hundreds of requirements; virtualise the rows if
    /// models with thousands of items feel slow.
    /// </summary>
    public class VandVRegisterViewModel : DisposableObject, IVandVRegisterViewModel
    {
        /// <summary>
        /// The <see cref="ILogger" /> used to record rows that could not be built.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The iids of the rows whose expansion differs from the default: items start collapsed, everything else expanded.
        /// </summary>
        private readonly HashSet<Guid> toggledRows = [];

        /// <summary>
        /// The iids of the collapsed nodes of the table of contents.
        /// </summary>
        private readonly HashSet<Guid> collapsedTreeNodes = [];

        /// <summary>
        /// The relationships already looked up for this snapshot, keyed by the iid of the requirement or item: each lookup
        /// scans every relationship of the iteration, and a rebuild runs on every keystroke of a search.
        /// </summary>
        private readonly Dictionary<Guid, IReadOnlyList<RequirementRelationshipRow>> traceabilityByThing = [];

        /// <summary>
        /// Whether the first specification has been selected, which happens once per iteration so "all specifications" survives a
        /// reload.
        /// </summary>
        private bool isScopeInitialized;

        /// <summary>
        /// The requirements to be covered, keyed by the iid of the group they are filed in, or of their specification when in
        /// no group.
        /// </summary>
        private Dictionary<Guid, List<Requirement>> requirementsByContainer = [];

        /// <summary>
        /// The iids of the requirements to be covered in the current snapshot.
        /// </summary>
        private HashSet<Guid> requirementsToCover = [];

        /// <summary>
        /// The iids of the groups holding a requirement the rows show, at any depth, offered in the table of contents.
        /// </summary>
        private HashSet<Guid> shownGroups = [];

        /// <summary>
        /// Whether <see cref="rows" /> no longer match the state they are built from.
        /// </summary>
        private bool areRowsStale = true;

        /// <summary>
        /// Backing field for <see cref="Rows" />
        /// </summary>
        private IReadOnlyList<VandVRegisterRow> rows = [];

        /// <summary>
        /// Backing field for <see cref="SelectedSpecification" />
        /// </summary>
        private RequirementsSpecification selectedSpecification;

        /// <summary>
        /// Backing field for <see cref="SearchText" />
        /// </summary>
        private string searchText;

        /// <summary>
        /// Backing field for <see cref="SelectedStages" />
        /// </summary>
        private IEnumerable<string> selectedStages = [];

        /// <summary>
        /// Backing field for <see cref="SelectedFields" />
        /// </summary>
        private IEnumerable<string> selectedFields = [];

        /// <summary>
        /// Backing field for <see cref="IsTocCollapsed" />
        /// </summary>
        private bool isTocCollapsed;

        /// <summary>
        /// Backing field for <see cref="DisplayMode" />
        /// </summary>
        private RequirementRowDisplayMode displayMode = RequirementRowDisplayMode.ShortNameNameAndDefinition;

        /// <summary>
        /// Backing field for <see cref="TreeUsesShortName" />
        /// </summary>
        private bool treeUsesShortName = true;

        /// <summary>
        /// Backing field for <see cref="IndentGroups" />
        /// </summary>
        private bool indentGroups = true;

        /// <summary>
        /// Backing field for <see cref="ShowOwner" />
        /// </summary>
        private bool showOwner = true;

        /// <summary>
        /// Backing field for <see cref="ShowCategory" />
        /// </summary>
        private bool showCategory = true;

        /// <summary>
        /// Backing field for <see cref="ShowTraceability" />
        /// </summary>
        private bool showTraceability;

        /// <summary>
        /// Backing field for <see cref="ScrollTarget" />
        /// </summary>
        private Guid? scrollTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVRegisterViewModel" /> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger" /> used to record rows that could not be built.</param>
        public VandVRegisterViewModel(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Gets the V&amp;V snapshot the register is built from.
        /// </summary>
        public VandVModel Model { get; private set; } = VandVModel.Build(null, null);

        /// <summary>
        /// Gets the rows to render, in order.
        /// </summary>
        public IReadOnlyList<VandVRegisterRow> Rows
        {
            get
            {
                this.EnsureRows();
                return this.rows;
            }
        }

        /// <summary>
        /// Gets the message shown when the rows could not be built, or null.
        /// </summary>
        public string BuildError { get; private set; }

        /// <summary>
        /// Gets the specifications holding requirements to be covered.
        /// </summary>
        public IEnumerable<RequirementsSpecification> AvailableSpecifications { get; private set; } = [];

        /// <summary>
        /// Gets or sets the specification the register is scoped to, or null for all; selecting it again shows them all.
        /// </summary>
        public RequirementsSpecification SelectedSpecification
        {
            get => this.selectedSpecification;
            set => this.SetAndInvalidateRows(ref this.selectedSpecification, value == this.selectedSpecification ? null : value);
        }

        /// <summary>
        /// Gets or sets the text the requirements and items are searched for, by short name and name.
        /// </summary>
        public string SearchText
        {
            get => this.searchText;
            set => this.SetAndInvalidateRows(ref this.searchText, value);
        }

        /// <summary>
        /// Gets or sets the stage gates the items are filtered on; empty means no filter.
        /// </summary>
        public IEnumerable<string> SelectedStages
        {
            get => this.selectedStages;
            set => this.SetAndInvalidateRows(ref this.selectedStages, value);
        }

        /// <summary>
        /// Gets or sets the labels of the fields shown on an item; empty shows them all.
        /// </summary>
        public IEnumerable<string> SelectedFields
        {
            get => this.selectedFields;
            set => this.SetAndInvalidateRows(ref this.selectedFields, value);
        }

        /// <summary>
        /// Gets the preset the selected fields currently match, or null for a custom combination.
        /// </summary>
        public VandVViewPreset? ActivePreset => Enum.GetValues<VandVViewPreset>()
            .Select(x => (VandVViewPreset?)x)
            .FirstOrDefault(x => VandVRegisterFields.GetPresetFields(x.Value).ToHashSet().SetEquals(this.SelectedFields));

        /// <summary>
        /// Gets or sets a value indicating whether the table of contents is collapsed.
        /// </summary>
        public bool IsTocCollapsed
        {
            get => this.isTocCollapsed;
            set => this.RaiseAndSetIfChanged(ref this.isTocCollapsed, value);
        }

        /// <summary>
        /// Gets or sets the way the rows are labelled: by short name, by name or by both.
        /// </summary>
        public RequirementRowDisplayMode DisplayMode
        {
            get => this.displayMode;
            set => this.RaiseAndSetIfChanged(ref this.displayMode, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the table of contents and the traceability links use short names (true)
        /// or names (false).
        /// </summary>
        public bool TreeUsesShortName
        {
            get => this.treeUsesShortName;
            set => this.RaiseAndSetIfChanged(ref this.treeUsesShortName, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether specifications, groups and requirements are indented by their depth.
        /// </summary>
        public bool IndentGroups
        {
            get => this.indentGroups;
            set => this.RaiseAndSetIfChanged(ref this.indentGroups, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the owner pill is shown.
        /// </summary>
        public bool ShowOwner
        {
            get => this.showOwner;
            set => this.RaiseAndSetIfChanged(ref this.showOwner, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the category pills are shown.
        /// </summary>
        public bool ShowCategory
        {
            get => this.showCategory;
            set => this.RaiseAndSetIfChanged(ref this.showCategory, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the relationships of each requirement and item are shown.
        /// </summary>
        public bool ShowTraceability
        {
            get => this.showTraceability;
            set => this.SetAndInvalidateRows(ref this.showTraceability, value);
        }

        /// <summary>
        /// Gets the iid of the row to scroll to on the next render, or null.
        /// </summary>
        public Guid? ScrollTarget
        {
            get => this.scrollTarget;
            private set => this.RaiseAndSetIfChanged(ref this.scrollTarget, value);
        }

        /// <summary>
        /// Gets the groups below a specification or group that hold a requirement the rows show, so the table of contents
        /// follows the search and filters as on the Requirements tab, and never offers a group without a row to scroll to.
        /// </summary>
        /// <param name="container">The specification or group.</param>
        /// <returns>The groups, ordered by short name.</returns>
        public IEnumerable<RequirementsGroup> GetGroups(RequirementsContainer container)
        {
            this.EnsureRows();
            return container.Group.Where(x => this.shownGroups.Contains(x.Iid)).OrderBy(x => x.ShortName, StringComparer.Ordinal);
        }

        /// <summary>
        /// Determines whether the node of the table of contents is collapsed.
        /// </summary>
        /// <param name="iid">The iid of the specification or group.</param>
        /// <returns>true when collapsed.</returns>
        public bool IsTreeNodeCollapsed(Guid iid)
        {
            return this.collapsedTreeNodes.Contains(iid);
        }

        /// <summary>
        /// Collapses or expands the node of the table of contents.
        /// </summary>
        /// <param name="iid">The iid of the specification or group.</param>
        public void ToggleTreeNode(Guid iid)
        {
            if (!this.collapsedTreeNodes.Remove(iid))
            {
                this.collapsedTreeNodes.Add(iid);
            }
        }

        /// <summary>
        /// Scrolls the register to a group clicked in the table of contents, expanding it and its ancestors.
        /// </summary>
        /// <param name="group">The group.</param>
        public void NavigateToGroup(RequirementsGroup group)
        {
            this.ExpandWithAncestors(group);
            this.InvalidateRows();
            this.ScrollTarget = group.Iid;
        }

        /// <summary>
        /// Selects the fields of the given view.
        /// </summary>
        /// <param name="preset">The <see cref="VandVViewPreset" />.</param>
        public void ApplyPreset(VandVViewPreset preset)
        {
            this.SelectedFields = VandVRegisterFields.GetPresetFields(preset);
        }

        /// <summary>
        /// Expands or collapses the given row.
        /// </summary>
        /// <param name="row">The <see cref="VandVRegisterRow" />.</param>
        public void Toggle(VandVRegisterRow row)
        {
            if (!this.toggledRows.Remove(row.Thing.Iid))
            {
                this.toggledRows.Add(row.Thing.Iid);
            }

            this.InvalidateRows();
        }

        /// <summary>
        /// Clears the search and filters, scopes the register to the requirement's specification, expands the requirement
        /// and its items, and flags it as the <see cref="ScrollTarget" />. A requirement that is not in the register (for
        /// example in a deprecated specification) just shows the whole register.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        public void Focus(Requirement requirement)
        {
            var isInRegister = requirement != null && this.requirementsToCover.Contains(requirement.Iid);

            // through the fields: the setter of the specification would show all specifications if it is selected already
            this.RaiseAndSetIfChanged(ref this.searchText, null, nameof(this.SearchText));
            this.RaiseAndSetIfChanged(ref this.selectedStages, [], nameof(this.SelectedStages));
            this.RaiseAndSetIfChanged(ref this.selectedSpecification, isInRegister ? (RequirementsSpecification)requirement.Container : null, nameof(this.SelectedSpecification));

            if (isInRegister)
            {
                this.toggledRows.Remove(requirement.Iid);
                this.ExpandWithAncestors((RequirementsContainer)GetOwnGroup(requirement) ?? this.selectedSpecification);
                this.toggledRows.UnionWith(this.Model.GetCoveringItems(requirement).Select(x => x.Iid));
            }

            this.InvalidateRows();
            this.ScrollTarget = isInRegister ? requirement.Iid : null;
        }

        /// <summary>
        /// Follows a traceability link: focuses the requirement, or a requirement an item covers, preferring the one the link
        /// was clicked on. A link to anything else, such as an activity, has no row to go to and does nothing.
        /// </summary>
        /// <param name="requirement">The linked requirement.</param>
        /// <param name="origin">The requirement or item whose row holds the link.</param>
        public void NavigateToRequirement(Requirement requirement, Requirement origin)
        {
            if (this.requirementsToCover.Contains(requirement.Iid))
            {
                this.Focus(requirement);
                return;
            }

            var covered = this.Model.RequirementsToCover.Where(x => this.Model.GetCoveringItems(x).Contains(requirement)).ToList();
            var target = covered.Find(x => x == origin || this.Model.GetCoveringItems(x).Contains(origin)) ?? covered.FirstOrDefault();

            if (target != null)
            {
                this.Focus(target);
            }
        }

        /// <summary>
        /// Clears the <see cref="ScrollTarget" /> once the row was scrolled to.
        /// </summary>
        public void ClearScrollTarget()
        {
            this.ScrollTarget = null;
        }

        /// <summary>
        /// Rebuilds the register from a new snapshot, keeping the scope, filters and expansion state as far as they still
        /// exist. The first snapshot with requirements of an iteration selects the first specification, as the Requirements tab does.
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        public void SetModel(VandVModel model)
        {
            if (model.Iteration?.Iid != this.Model.Iteration?.Iid)
            {
                this.isScopeInitialized = false;
                this.toggledRows.Clear();
                this.collapsedTreeNodes.Clear();
            }

            this.Model = model;
            this.traceabilityByThing.Clear();
            this.requirementsToCover = model.RequirementsToCover.Select(x => x.Iid).ToHashSet();
            this.requirementsByContainer = model.RequirementsToCover.GroupBy(x => GetOwnGroup(x)?.Iid ?? x.Container.Iid).ToDictionary(x => x.Key, x => x.ToList());

            this.AvailableSpecifications = model.RequirementsToCover
                .Select(x => x.Container)
                .OfType<RequirementsSpecification>()
                .Distinct()
                .OrderBy(x => x.ShortName, StringComparer.Ordinal)
                .ToList();

            this.DropWhatNoLongerExists();

            if (!this.isScopeInitialized && this.AvailableSpecifications.Any())
            {
                this.isScopeInitialized = true;
                this.RaiseAndSetIfChanged(ref this.selectedSpecification, this.AvailableSpecifications.First(), nameof(this.SelectedSpecification));
            }

            this.InvalidateRows();
        }

        /// <summary>
        /// Gets the group a requirement is filed in. A group of another specification is ignored: the web editor never
        /// creates that, but other clients can, and the requirement must not vanish from its own specification.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        /// <returns>The group, or null when it is filed directly in its specification.</returns>
        private static RequirementsGroup GetOwnGroup(Requirement requirement)
        {
            return requirement.Group?.GetContainerOfType<RequirementsSpecification>() == requirement.Container ? requirement.Group : null;
        }

        /// <summary>
        /// Gets the group a requirement is filed in and that group's ancestors.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        /// <returns>The groups, innermost first.</returns>
        private static IEnumerable<RequirementsGroup> GetOwnGroupChain(Requirement requirement)
        {
            for (var group = GetOwnGroup(requirement); group != null; group = group.Container as RequirementsGroup)
            {
                yield return group;
            }
        }

        /// <summary>
        /// Determines whether the thing's short name or name contains the search text.
        /// </summary>
        /// <param name="shortName">The short name.</param>
        /// <param name="name">The name.</param>
        /// <param name="text">The search text.</param>
        /// <returns>true on a match.</returns>
        private static bool Matches(string shortName, string name, string text)
        {
            return (shortName?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false) || (name?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        /// <summary>
        /// Gets every requirement of the given lookup below a specification or group, at any depth.
        /// </summary>
        /// <param name="byContainer">The requirements, keyed by the iid of the group or specification they are filed in.</param>
        /// <param name="container">The specification or group.</param>
        /// <returns>The requirements.</returns>
        private static IEnumerable<Requirement> GetBelow(Dictionary<Guid, List<Requirement>> byContainer, RequirementsContainer container)
        {
            var direct = byContainer.GetValueOrDefault(container.Iid) ?? [];
            return direct.Concat(container.Group.SelectMany(x => GetBelow(byContainer, x)));
        }

        /// <summary>
        /// Removes the collapsed state of a specification or group and of everything above it.
        /// </summary>
        /// <param name="container">The specification or group.</param>
        private void ExpandWithAncestors(RequirementsContainer container)
        {
            for (var current = container; current != null; current = current.Container as RequirementsContainer)
            {
                this.toggledRows.Remove(current.Iid);
            }
        }

        /// <summary>
        /// Clears a scope or stage filter that a reload removed, otherwise the register stays empty with nothing to reselect.
        /// </summary>
        private void DropWhatNoLongerExists()
        {
            if (this.selectedSpecification != null && !this.AvailableSpecifications.Contains(this.selectedSpecification))
            {
                this.RaiseAndSetIfChanged(ref this.selectedSpecification, null, nameof(this.SelectedSpecification));
            }

            var existingStages = this.selectedStages.Where(stage => this.Model.StageGates.Any(gate => VandVConstants.ValueEquals(gate, stage))).ToList();

            if (existingStages.Count != this.selectedStages.Count())
            {
                this.RaiseAndSetIfChanged(ref this.selectedStages, existingStages, nameof(this.SelectedStages));
            }
        }

        /// <summary>
        /// Sets a property the rows depend on and marks them as stale.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="field">The backing field.</param>
        /// <param name="value">The new value.</param>
        /// <param name="propertyName">The property name.</param>
        private void SetAndInvalidateRows<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            this.RaiseAndSetIfChanged(ref field, value, propertyName);
            this.InvalidateRows();
        }

        /// <summary>
        /// Determines whether a row is expanded: items start collapsed, every other row expanded, and a toggle flips it.
        /// </summary>
        /// <param name="kind">The row kind.</param>
        /// <param name="iid">The iid of the row's thing.</param>
        /// <returns>true when expanded.</returns>
        private bool IsExpanded(VandVRegisterRowKind kind, Guid iid)
        {
            return (kind != VandVRegisterRowKind.Item) ^ this.toggledRows.Contains(iid);
        }

        /// <summary>
        /// Determines whether an item is at one of the selected stage gates, comparing its effective value, so a stage
        /// inherited from its activity counts.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>true when no stage gate is selected or the item is at one of them.</returns>
        private bool IsAtSelectedStage(Requirement item)
        {
            if (!this.SelectedStages.Any())
            {
                return true;
            }

            var stage = VandVQueries.GetEffectiveDisplayValue(this.Model, item, VandVConstants.Stage);
            return this.SelectedStages.Any(x => VandVConstants.ValueEquals(stage, x));
        }

        /// <summary>
        /// Gets the covering items of a requirement that pass the stage filter and the search.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        /// <returns>The items to show.</returns>
        private List<Requirement> GetShownItems(Requirement requirement)
        {
            var requirementMatches = string.IsNullOrWhiteSpace(this.SearchText) || Matches(requirement.ShortName, requirement.Name, this.SearchText);

            return this.Model.GetCoveringItems(requirement)
                .Where(this.IsAtSelectedStage)
                .Where(x => requirementMatches || Matches(x.ShortName, x.Name, this.SearchText))
                .ToList();
        }

        /// <summary>
        /// Determines whether a requirement is shown: it is in scope, matches the search itself or through an item, and
        /// under a stage filter still has an item at one of those gates.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        /// <returns>true when shown.</returns>
        private bool IsShown(Requirement requirement)
        {
            if (this.SelectedSpecification != null && requirement.Container != this.SelectedSpecification)
            {
                return false;
            }

            var hasSearch = !string.IsNullOrWhiteSpace(this.SearchText);

            if (!this.SelectedStages.Any() && (!hasSearch || Matches(requirement.ShortName, requirement.Name, this.SearchText)))
            {
                return true;
            }

            return this.GetShownItems(requirement).Count > 0;
        }

        /// <summary>
        /// Gets the relationships of a requirement or item, while traceability is shown.
        /// </summary>
        /// <param name="requirement">The requirement or item.</param>
        /// <returns>The rows, or null when traceability is off.</returns>
        private IReadOnlyList<RequirementRelationshipRow> GetTraceability(Requirement requirement)
        {
            if (!this.ShowTraceability)
            {
                return null;
            }

            if (!this.traceabilityByThing.TryGetValue(requirement.Iid, out var relationships))
            {
                relationships = RequirementsTraceabilityHelper.GetTraceability(requirement, this.Model.Iteration, this.Model.Rules, _ => true);
                this.traceabilityByThing[requirement.Iid] = relationships;
            }

            return relationships;
        }

        /// <summary>
        /// Marks the rows as stale and tells who shows them. <see cref="Rows" /> is built when read, so there is no field to set.
        /// </summary>
        private void InvalidateRows()
        {
            this.areRowsStale = true;
            this.RaisePropertyChanged(nameof(this.Rows));
        }

        /// <summary>
        /// Builds <see cref="rows" /> and <see cref="shownGroups" /> when they are stale.
        /// </summary>
        private void EnsureRows()
        {
            if (!this.areRowsStale)
            {
                return;
            }

            try
            {
                this.BuildRows();
                this.BuildError = null;
            }
            catch (Exception exception)
            {
                // the build runs while something reads the rows, most often a render, where an exception would end the circuit
                this.logger.LogError(exception, "The rows of the V&V register could not be built");
                this.rows = [];
                this.shownGroups = [];
                this.BuildError = "The V&V register could not be built. See the log for details.";
            }

            this.areRowsStale = false;
        }

        /// <summary>
        /// Builds <see cref="rows" /> and <see cref="shownGroups" />.
        /// </summary>
        private void BuildRows()
        {
            var shownByContainer = this.requirementsByContainer.ToDictionary(x => x.Key, x => x.Value.Where(this.IsShown).ToList());
            var context = new RebuildContext(shownByContainer);

            foreach (var specification in this.SelectedSpecification == null ? this.AvailableSpecifications : [this.SelectedSpecification])
            {
                this.AddContainer(context, VandVRegisterRowKind.Specification, specification, 0);
            }

            this.shownGroups = shownByContainer.Values.SelectMany(x => x).SelectMany(GetOwnGroupChain).Select(x => x.Iid).ToHashSet();
            this.rows = context.Rows;
        }

        /// <summary>
        /// Gets the roll-up of all the items covering a requirement, whatever the filters show, so a filtered-out failed item
        /// never makes a requirement read as verified. Computed once per rebuild, as every ancestor row needs it too.
        /// </summary>
        /// <param name="context">The <see cref="RebuildContext" />.</param>
        /// <param name="requirement">The requirement.</param>
        /// <returns>The <see cref="VandVRollUp" />.</returns>
        private VandVRollUp GetRollUp(RebuildContext context, Requirement requirement)
        {
            if (!context.RollUps.TryGetValue(requirement.Iid, out var rollUp))
            {
                rollUp = VandVQueries.GetRollUp(this.Model, this.Model.GetCoveringItems(requirement));
                context.RollUps[requirement.Iid] = rollUp;
            }

            return rollUp;
        }

        /// <summary>
        /// Adds the row of a specification or group holding shown requirements and, when expanded, its content: the
        /// requirements filed directly in it, then its groups. Its coverage counts every requirement below it, so a filter
        /// that hides a failed requirement never makes the specification read as verified.
        /// </summary>
        /// <param name="context">The <see cref="RebuildContext" />.</param>
        /// <param name="kind">The row kind.</param>
        /// <param name="container">The specification or group.</param>
        /// <param name="depth">The row's depth.</param>
        private void AddContainer(RebuildContext context, VandVRegisterRowKind kind, RequirementsContainer container, int depth)
        {
            if (!GetBelow(context.ShownByContainer, container).Any())
            {
                return;
            }

            var rollUps = GetBelow(this.requirementsByContainer, container).Select(x => this.GetRollUp(context, x)).ToList();
            var isExpanded = this.IsExpanded(kind, container.Iid);
            context.Rows.Add(new VandVRegisterRow(kind, container, depth, true, isExpanded, VandVQueries.FormatContainerCoverage(rollUps)));

            if (!isExpanded)
            {
                return;
            }

            foreach (var requirement in (context.ShownByContainer.GetValueOrDefault(container.Iid) ?? []).OrderBy(x => x.ShortName, StringComparer.Ordinal))
            {
                this.AddRequirement(context, requirement, depth + 1);
            }

            foreach (var group in container.Group.OrderBy(x => x.ShortName, StringComparer.Ordinal))
            {
                this.AddContainer(context, VandVRegisterRowKind.Group, group, depth + 1);
            }
        }

        /// <summary>
        /// Adds the row of a requirement and, when expanded, its items and their steps.
        /// </summary>
        /// <param name="context">The <see cref="RebuildContext" />.</param>
        /// <param name="requirement">The requirement.</param>
        /// <param name="depth">The requirement's depth.</param>
        private void AddRequirement(RebuildContext context, Requirement requirement, int depth)
        {
            var items = this.GetShownItems(requirement);
            var rollUp = this.GetRollUp(context, requirement);
            var isExpanded = this.IsExpanded(VandVRegisterRowKind.Requirement, requirement.Iid);
            context.Rows.Add(new VandVRegisterRow(VandVRegisterRowKind.Requirement, requirement, depth, items.Count > 0, isExpanded, VandVQueries.FormatRequirementCoverage(rollUp), rollUp, Traceability: this.GetTraceability(requirement)));

            if (!isExpanded)
            {
                return;
            }

            foreach (var item in items)
            {
                this.AddItem(context, item, depth + 1);
            }
        }

        /// <summary>
        /// Adds the row of an item with its field cells and, when expanded, its procedure steps.
        /// </summary>
        /// <param name="context">The <see cref="RebuildContext" />.</param>
        /// <param name="item">The V&amp;V item.</param>
        /// <param name="depth">The item's depth.</param>
        private void AddItem(RebuildContext context, Requirement item, int depth)
        {
            var fields = VandVRegisterFields.GetFields(this.Model, item, this.SelectedFields.ToList());
            IReadOnlyList<Requirement> steps = [];
            var isInherited = false;

            if (fields.Any(x => x.Label == VandVRegisterFields.ProcedureField))
            {
                (steps, isInherited) = VandVRegisterFields.GetSteps(this.Model, item);
            }

            var isExpanded = this.IsExpanded(VandVRegisterRowKind.Item, item.Iid);
            context.Rows.Add(new VandVRegisterRow(VandVRegisterRowKind.Item, item, depth, steps.Count > 0, isExpanded, string.Empty, Outcome: VandVQueries.GetItemOutcome(this.Model, item), Fields: fields, Traceability: this.GetTraceability(item)));

            if (isExpanded)
            {
                context.Rows.AddRange(steps.Select(x => new VandVRegisterRow(VandVRegisterRowKind.Step, x, depth + 1, false, false, string.Empty, IsInherited: isInherited)));
            }
        }

        /// <summary>
        /// The working state of one rebuild.
        /// </summary>
        /// <param name="ShownByContainer">
        /// The shown requirements, keyed by the iid of the group they are filed in, or of their specification when in no group.
        /// </param>
        private sealed record RebuildContext(Dictionary<Guid, List<Requirement>> ShownByContainer)
        {
            /// <summary>
            /// Gets the rows built so far.
            /// </summary>
            public List<VandVRegisterRow> Rows { get; } = [];

            /// <summary>
            /// Gets the roll-ups computed so far, keyed by the iid of the requirement.
            /// </summary>
            public Dictionary<Guid, VandVRollUp> RollUps { get; } = [];
        }
    }
}
