// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Utilities.VandV
{
    using System.Globalization;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// An immutable snapshot of the V&amp;V data of one iteration, built once per iteration (re)load. Every relationship map
    /// (item to activity, activity to items, owner to steps, requirement to covering items, item to measurement links) is
    /// resolved in a single pass here, because scanning the iteration's relationships per row is what made the IME's
    /// register stutter on large models. Every V&amp;V view reads this snapshot through <see cref="VandVQueries" />.
    /// </summary>
    public sealed class VandVModel
    {
        /// <summary>
        /// The activity performing each item, keyed by the item's iid.
        /// </summary>
        private readonly Dictionary<Guid, Requirement> activityByItem = [];

        /// <summary>
        /// The items each activity performs, keyed by the activity's iid.
        /// </summary>
        private readonly Dictionary<Guid, List<Requirement>> itemsByActivity = [];

        /// <summary>
        /// The ordered procedure steps of each item or activity, keyed by the owner's iid.
        /// </summary>
        private readonly Dictionary<Guid, List<Requirement>> stepsByOwner = [];

        /// <summary>
        /// The items covering each requirement, keyed by the requirement's iid.
        /// </summary>
        private readonly Dictionary<Guid, List<Requirement>> itemsByRequirement = [];

        /// <summary>
        /// The requirement each item verifies or validates, keyed by the item's iid.
        /// </summary>
        private readonly Dictionary<Guid, Requirement> requirementByItem = [];

        /// <summary>
        /// The measurement links (covers parameter, option, state, verified on) of each item, keyed by the item's iid.
        /// </summary>
        private readonly Dictionary<Guid, List<BinaryRelationship>> measurementLinksByItem = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVModel" /> class.
        /// </summary>
        /// <param name="iteration">The <see cref="CDP4Common.EngineeringModelData.Iteration" />, or null for an empty model.</param>
        private VandVModel(Iteration iteration)
        {
            this.Iteration = iteration;
        }

        /// <summary>
        /// Gets the <see cref="CDP4Common.EngineeringModelData.Iteration" /> the snapshot was built from, or null for an empty
        /// model.
        /// </summary>
        public Iteration Iteration { get; }

        /// <summary>
        /// Gets the dedicated V&amp;V plan specification, or null when it has not been created yet.
        /// </summary>
        public RequirementsSpecification VandVSpecification { get; private set; }

        /// <summary>
        /// Gets the non-deprecated report specifications, ordered by short name.
        /// </summary>
        public IReadOnlyList<RequirementsSpecification> Reports { get; private set; } = [];

        /// <summary>
        /// Gets the non-deprecated V&amp;V items, ordered by short name.
        /// </summary>
        public IReadOnlyList<Requirement> Items { get; private set; } = [];

        /// <summary>
        /// Gets the non-deprecated activities, ordered by short name.
        /// </summary>
        public IReadOnlyList<Requirement> Activities { get; private set; } = [];

        /// <summary>
        /// Gets the requirements subject to V&amp;V coverage, ordered by short name: non-deprecated requirements outside the
        /// V&amp;V plan and the reports that are neither an item, a step nor an activity.
        /// </summary>
        public IReadOnlyList<Requirement> RequirementsToCover { get; private set; } = [];

        /// <summary>
        /// Gets the stage gates in project order: the value definitions of <see cref="VandVConstants.Stage" /> (the defaults
        /// when it is not defined), followed by any stage used by an item that is not defined.
        /// </summary>
        public IReadOnlyList<string> StageGates { get; private set; } = [];

        /// <summary>
        /// Gets the engineering model's <see cref="ModellingAnnotationItem" />s, the review requests among them.
        /// </summary>
        public IReadOnlyList<ModellingAnnotationItem> Annotations { get; private set; } = [];

        /// <summary>
        /// Builds the snapshot of the given iteration.
        /// </summary>
        /// <param name="iteration">The <see cref="CDP4Common.EngineeringModelData.Iteration" />, or null for an empty model.</param>
        /// <param name="referenceDataLibraries">The model's chain of reference data libraries, used for the stage gates.</param>
        /// <returns>The <see cref="VandVModel" />.</returns>
        public static VandVModel Build(Iteration iteration, IEnumerable<ReferenceDataLibrary> referenceDataLibraries)
        {
            var model = new VandVModel(iteration);

            if (iteration == null)
            {
                model.StageGates = VandVConstants.DefaultStageGates;
                return model;
            }

            var specifications = iteration.RequirementsSpecification.Where(x => !x.IsDeprecated).ToList();
            var requirements = specifications.SelectMany(x => x.Requirement).Where(x => !x.IsDeprecated).ToList();

            model.VandVSpecification = specifications.FirstOrDefault(VandVQueries.IsVandVSpecification);
            model.Reports = specifications.Where(VandVQueries.IsReport).OrderBy(x => x.ShortName, StringComparer.Ordinal).ToList();
            model.Items = requirements.Where(x => VandVQueries.IsItem(x) && !VandVQueries.IsStep(x)).OrderBy(x => x.ShortName, StringComparer.Ordinal).ToList();
            model.Activities = requirements.Where(VandVQueries.IsActivity).OrderBy(x => x.ShortName, StringComparer.Ordinal).ToList();
            model.Annotations = iteration.Container is EngineeringModel engineeringModel ? engineeringModel.ModellingAnnotation.ToList() : [];

            model.RequirementsToCover = specifications
                .Where(x => !VandVQueries.IsVandVSpecification(x) && !VandVQueries.IsReport(x))
                .SelectMany(x => x.Requirement)
                .Where(x => !x.IsDeprecated && !VandVQueries.IsItem(x) && !VandVQueries.IsStep(x) && !VandVQueries.IsActivity(x))
                .OrderBy(x => x.ShortName, StringComparer.Ordinal)
                .ToList();

            // a requirement counts only when neither it nor its specification is deprecated, exactly as for Items and Activities
            var liveRequirements = requirements.Select(x => x.Iid).ToHashSet();

            foreach (var relationship in iteration.Relationship.OfType<BinaryRelationship>())
            {
                model.Register(relationship, liveRequirements);
            }

            foreach (var steps in model.stepsByOwner.Values)
            {
                var ordered = steps.OrderBy(GetStepNumber).ThenBy(x => x.ShortName, StringComparer.Ordinal).ToList();
                steps.Clear();
                steps.AddRange(ordered);
            }

            foreach (var items in model.itemsByRequirement.Values.Concat(model.itemsByActivity.Values))
            {
                items.Sort((left, right) => string.CompareOrdinal(left.ShortName, right.ShortName));
            }

            model.StageGates = model.QueryStageGates(referenceDataLibraries);
            return model;
        }

        /// <summary>
        /// Gets the activity performing the given item.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>The first non-deprecated activity it is performed by, or null.</returns>
        public Requirement GetActivity(Requirement item)
        {
            return item == null ? null : this.activityByItem.GetValueOrDefault(item.Iid);
        }

        /// <summary>
        /// Gets the items the given activity performs.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns>The performed items, ordered by short name.</returns>
        public IReadOnlyList<Requirement> GetPerformedItems(Requirement activity)
        {
            return Lookup(this.itemsByActivity, activity);
        }

        /// <summary>
        /// Gets the procedure steps of the given item or activity.
        /// </summary>
        /// <param name="owner">The item or activity.</param>
        /// <returns>The non-deprecated steps, by step number (unparseable last) then short name.</returns>
        public IReadOnlyList<Requirement> GetSteps(Requirement owner)
        {
            return Lookup(this.stepsByOwner, owner);
        }

        /// <summary>
        /// Gets the items covering the given requirement: sources of a verifies or validates link that are non-deprecated
        /// V&amp;V items. A verifies link between two ordinary requirements is traceability, not coverage.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        /// <returns>The covering items, distinct, ordered by short name.</returns>
        public IReadOnlyList<Requirement> GetCoveringItems(Requirement requirement)
        {
            return Lookup(this.itemsByRequirement, requirement);
        }

        /// <summary>
        /// Gets the requirement the given item verifies or validates.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>The target of its first coverage link, or null.</returns>
        public Requirement GetCoveredRequirement(Requirement item)
        {
            return item == null ? null : this.requirementByItem.GetValueOrDefault(item.Iid);
        }

        /// <summary>
        /// Gets the measurement links of the given item (covers parameter, covers option, covers state, verified on).
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>The relationships, in model order.</returns>
        public IReadOnlyList<BinaryRelationship> GetMeasurementLinks(Requirement item)
        {
            return Lookup(this.measurementLinksByItem, item);
        }

        /// <summary>
        /// Gets the list stored for the given thing, or an empty list.
        /// </summary>
        /// <typeparam name="T">The list element type.</typeparam>
        /// <param name="map">The map keyed by iid.</param>
        /// <param name="thing">The key thing.</param>
        /// <returns>The stored list, or an empty one.</returns>
        private static IReadOnlyList<T> Lookup<T>(Dictionary<Guid, List<T>> map, Requirement thing)
        {
            return thing != null && map.TryGetValue(thing.Iid, out var list) ? list : [];
        }

        /// <summary>
        /// Adds the value to the list stored under the given key, once.
        /// </summary>
        /// <param name="map">The map keyed by iid.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The requirement to add.</param>
        private static void AddDistinct(Dictionary<Guid, List<Requirement>> map, Guid key, Requirement value)
        {
            if (!map.TryGetValue(key, out var list))
            {
                list = [];
                map[key] = list;
            }

            if (list.TrueForAll(x => x.Iid != value.Iid))
            {
                list.Add(value);
            }
        }

        /// <summary>
        /// Gets the step number of a procedure step, parsed as an invariant integer; an unparseable number sorts last.
        /// </summary>
        /// <param name="step">The procedure step.</param>
        /// <returns>The step number, or <see cref="int.MaxValue" />.</returns>
        private static int GetStepNumber(Requirement step)
        {
            return int.TryParse(VandVQueries.GetValue(step, VandVConstants.StepNumber), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : int.MaxValue;
        }

        /// <summary>
        /// Registers one relationship in the map of its kind, if any. A relationship whose source is not a live requirement
        /// (deprecated, or in a deprecated specification) is ignored.
        /// </summary>
        /// <param name="relationship">The <see cref="BinaryRelationship" />.</param>
        /// <param name="liveRequirements">The iids of the live requirements.</param>
        private void Register(BinaryRelationship relationship, HashSet<Guid> liveRequirements)
        {
            if (relationship.Source is not Requirement source || !liveRequirements.Contains(source.Iid))
            {
                return;
            }

            var target = relationship.Target as Requirement;

            if (VandVQueries.IsCoverageLink(relationship))
            {
                this.RegisterCoverage(source, target);
            }
            else if (VandVQueries.IsCategorizedAs(relationship, VandVConstants.PerformedByCategory))
            {
                this.RegisterPerformedBy(source, target, liveRequirements);
            }
            else if (VandVQueries.IsCategorizedAs(relationship, VandVConstants.HasStepCategory))
            {
                this.RegisterStep(source, target, liveRequirements);
            }
            else if (VandVConstants.MeasurementLinkCategories.Any(x => VandVQueries.IsCategorizedAs(relationship, x)))
            {
                this.RegisterMeasurement(source, relationship);
            }
        }

        /// <summary>
        /// Registers a verifies or validates link; it counts as coverage only when its source is a V&amp;V item.
        /// </summary>
        /// <param name="source">The live source requirement.</param>
        /// <param name="covered">The target requirement, or null.</param>
        private void RegisterCoverage(Requirement source, Requirement covered)
        {
            if (covered != null && VandVQueries.IsItem(source))
            {
                AddDistinct(this.itemsByRequirement, covered.Iid, source);
                this.requirementByItem.TryAdd(source.Iid, covered);
            }
        }

        /// <summary>
        /// Registers a performed by link from an item to a live activity; the first such link of an item wins.
        /// </summary>
        /// <param name="source">The live source requirement.</param>
        /// <param name="activity">The target requirement, or null.</param>
        /// <param name="liveRequirements">The iids of the live requirements.</param>
        private void RegisterPerformedBy(Requirement source, Requirement activity, HashSet<Guid> liveRequirements)
        {
            if (activity != null && liveRequirements.Contains(activity.Iid) && VandVQueries.IsItem(source) && VandVQueries.IsActivity(activity)
                && this.activityByItem.TryAdd(source.Iid, activity))
            {
                AddDistinct(this.itemsByActivity, activity.Iid, source);
            }
        }

        /// <summary>
        /// Registers a has step link to a live step.
        /// </summary>
        /// <param name="owner">The live item or activity.</param>
        /// <param name="step">The target requirement, or null.</param>
        /// <param name="liveRequirements">The iids of the live requirements.</param>
        private void RegisterStep(Requirement owner, Requirement step, HashSet<Guid> liveRequirements)
        {
            if (step != null && liveRequirements.Contains(step.Iid))
            {
                AddDistinct(this.stepsByOwner, owner.Iid, step);
            }
        }

        /// <summary>
        /// Registers a covers parameter, covers option, covers state or verified on link.
        /// </summary>
        /// <param name="source">The live source requirement.</param>
        /// <param name="relationship">The <see cref="BinaryRelationship" />.</param>
        private void RegisterMeasurement(Requirement source, BinaryRelationship relationship)
        {
            if (!this.measurementLinksByItem.TryGetValue(source.Iid, out var links))
            {
                links = [];
                this.measurementLinksByItem[source.Iid] = links;
            }

            links.Add(relationship);
        }

        /// <summary>
        /// Computes the stage gates, see <see cref="StageGates" />.
        /// </summary>
        /// <param name="referenceDataLibraries">The model's chain of reference data libraries.</param>
        /// <returns>The stage gate names in project order.</returns>
        private List<string> QueryStageGates(IEnumerable<ReferenceDataLibrary> referenceDataLibraries)
        {
            var stageParameterType = (referenceDataLibraries ?? [])
                .SelectMany(x => x.ParameterType)
                .OfType<EnumerationParameterType>()
                .FirstOrDefault(x => x.ShortName == VandVConstants.Stage);

            var stages = stageParameterType?.ValueDefinition.Count > 0
                ? stageParameterType.ValueDefinition.Select(x => x.Name).ToList()
                : VandVConstants.DefaultStageGates.ToList();

            foreach (var used in this.Items.Select(x => VandVQueries.GetEffectiveDisplayValue(this, x, VandVConstants.Stage)).Where(x => x != null))
            {
                if (!stages.Exists(x => VandVConstants.ValueEquals(x, used)))
                {
                    stages.Add(used);
                }
            }

            return stages;
        }
    }
}
