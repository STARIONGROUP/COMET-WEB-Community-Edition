// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsChangelogCalculator.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// Computes the list of <see cref="RequirementChange" />s between two <see cref="Iteration" />s by matching the
    /// requirement-related elements of each iteration by their <see cref="CDP4Common.CommonData.Thing.Iid" />.
    /// </summary>
    public static class RequirementsChangelogCalculator
    {
        /// <summary>
        /// Compares the requirement-related content of two <see cref="Iteration" />s and returns the changes that turn
        /// <paramref name="baseIteration" /> into <paramref name="currentIteration" />.
        /// </summary>
        /// <param name="baseIteration">The older, baseline <see cref="Iteration" /></param>
        /// <param name="currentIteration">The newer <see cref="Iteration" /></param>
        /// <returns>A read-only list of <see cref="RequirementChange" />s</returns>
        public static IReadOnlyList<RequirementChange> Compare(Iteration baseIteration, Iteration currentIteration)
        {
            var changes = new List<RequirementChange>();

            changes.AddRange(CompareContainers(baseIteration.RequirementsSpecification, currentIteration.RequirementsSpecification, "Requirements Specification", x => x.IsDeprecated));
            changes.AddRange(CompareContainers(AllGroups(baseIteration), AllGroups(currentIteration), "Requirements Group", null));
            changes.AddRange(CompareRequirements(AllRequirements(baseIteration), AllRequirements(currentIteration)));
            changes.AddRange(CompareTraceability(baseIteration, currentIteration));

            return changes;
        }

        /// <summary>
        /// Gets every <see cref="Requirement" /> of every <see cref="RequirementsSpecification" /> of the given
        /// <paramref name="iteration" />.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>The requirements of the iteration</returns>
        private static IEnumerable<Requirement> AllRequirements(Iteration iteration)
        {
            return iteration.RequirementsSpecification.SelectMany(x => x.Requirement);
        }

        /// <summary>
        /// Gets every <see cref="RequirementsGroup" /> reachable from every <see cref="RequirementsSpecification" /> of
        /// the given <paramref name="iteration" />, at any nesting depth.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>The groups of the iteration</returns>
        private static IEnumerable<RequirementsGroup> AllGroups(Iteration iteration)
        {
            return iteration.RequirementsSpecification.SelectMany(x => FlattenGroups(x.Group));
        }

        /// <summary>
        /// Recursively flattens the given <paramref name="groups" /> and their descendants into a single sequence.
        /// </summary>
        /// <param name="groups">The top-level <see cref="RequirementsGroup" />s to flatten</param>
        /// <returns>The groups and all of their descendants</returns>
        private static IEnumerable<RequirementsGroup> FlattenGroups(IEnumerable<RequirementsGroup> groups)
        {
            return groups.SelectMany(group => new[] { group }.Concat(FlattenGroups(group.Group)));
        }

        /// <summary>
        /// Splits the given <paramref name="baseItems" /> and <paramref name="currentItems" /> into the ones only
        /// present in <paramref name="currentItems" />, the ones only present in <paramref name="baseItems" />, and the
        /// pairs present in both, matched by <see cref="Thing.Iid" />.
        /// </summary>
        /// <typeparam name="T">The <see cref="Thing" /> subtype being matched</typeparam>
        /// <param name="baseItems">The items of the baseline iteration</param>
        /// <param name="currentItems">The items of the newer iteration</param>
        /// <returns>The created, deleted and matched items</returns>
        private static (List<T> Created, List<T> Deleted, List<(T Base, T Current)> Matched) Diff<T>(IEnumerable<T> baseItems, IEnumerable<T> currentItems) where T : Thing
        {
            var baseMap = baseItems.ToDictionary(x => x.Iid);
            var currentMap = currentItems.ToDictionary(x => x.Iid);

            var created = currentMap.Where(x => !baseMap.ContainsKey(x.Key)).Select(x => x.Value).ToList();
            var deleted = baseMap.Where(x => !currentMap.ContainsKey(x.Key)).Select(x => x.Value).ToList();
            var matched = currentMap.Where(x => baseMap.ContainsKey(x.Key)).Select(x => (baseMap[x.Key], x.Value)).ToList();

            return (created, deleted, matched);
        }

        /// <summary>
        /// Gets the <see cref="RequirementsSpecification" /> that owns the given <paramref name="thing" />: the thing
        /// itself when it is a specification, otherwise the nearest specification found by walking up its containers,
        /// or null when it has none.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /></param>
        /// <returns>The owning <see cref="RequirementsSpecification" />, or null</returns>
        private static RequirementsSpecification GetOwningSpecification(Thing thing)
        {
            return thing as RequirementsSpecification ?? thing?.GetContainerOfType<RequirementsSpecification>();
        }

        /// <summary>
        /// Builds a <see cref="RequirementChange" /> row, filling in the owning <paramref name="specification" />'s
        /// fields, or leaving them empty when it is null.
        /// </summary>
        /// <param name="kind">The <see cref="RequirementChangeKind" /></param>
        /// <param name="elementKind">The human-readable kind of the changed element</param>
        /// <param name="elementId">The <see cref="Thing.Iid" /> of the changed element</param>
        /// <param name="elementName">The name of the changed element</param>
        /// <param name="elementShortName">The short name of the changed element</param>
        /// <param name="author">The name of the element's author</param>
        /// <param name="specification">The owning <see cref="RequirementsSpecification" />, or null when there is none</param>
        /// <param name="field">The name of the changed field, empty when the whole element changed</param>
        /// <param name="oldValue">The value in the baseline iteration, empty when there is none</param>
        /// <param name="newValue">The value in the newer iteration, empty when there is none</param>
        /// <returns>The built <see cref="RequirementChange" /></returns>
        private static RequirementChange NewChange(RequirementChangeKind kind, string elementKind, Guid elementId, string elementName, string elementShortName, string author,
            RequirementsSpecification specification, string field = "", string oldValue = "", string newValue = "")
        {
            return new RequirementChange
            {
                Kind = kind,
                ElementKind = elementKind,
                ElementId = elementId,
                ElementName = elementName,
                ElementShortName = elementShortName,
                Field = field,
                OldValue = oldValue,
                NewValue = newValue,
                Author = author,
                SpecificationId = specification?.Iid ?? Guid.Empty,
                SpecificationName = specification?.Name ?? string.Empty,
                SpecificationShortName = specification?.ShortName ?? string.Empty
            };
        }

        /// <summary>
        /// Compares the <see cref="RequirementsSpecification" />s or <see cref="RequirementsGroup" />s of two
        /// iterations: elements only in one iteration are reported as created or deleted, and elements present in both
        /// are reported as deprecated, restored, or modified on their name, short name or owner.
        /// </summary>
        /// <typeparam name="T">Either <see cref="RequirementsSpecification" /> or <see cref="RequirementsGroup" /></typeparam>
        /// <param name="baseItems">The items of the baseline iteration</param>
        /// <param name="currentItems">The items of the newer iteration</param>
        /// <param name="elementKind">The human-readable kind ("Requirements Specification" or "Requirements Group") used on every reported row</param>
        /// <param name="isDeprecated">
        /// An accessor for the deprecation flag, or null when <typeparamref name="T" /> cannot be deprecated (as is the
        /// case for <see cref="RequirementsGroup" />)
        /// </param>
        /// <returns>The changes detected between the two sets</returns>
        private static IEnumerable<RequirementChange> CompareContainers<T>(IEnumerable<T> baseItems, IEnumerable<T> currentItems, string elementKind, Func<T, bool> isDeprecated) where T : RequirementsContainer
        {
            var changes = new List<RequirementChange>();
            var (created, deleted, matched) = Diff(baseItems, currentItems);

            changes.AddRange(created.Select(x => NewChange(RequirementChangeKind.Created, elementKind, x.Iid, x.Name, x.ShortName, x.Actor?.Name ?? string.Empty, GetOwningSpecification(x), newValue: x.Name)));
            changes.AddRange(deleted.Select(x => NewChange(RequirementChangeKind.Deleted, elementKind, x.Iid, x.Name, x.ShortName, x.Actor?.Name ?? string.Empty, GetOwningSpecification(x), oldValue: x.Name)));

            foreach (var (baseItem, currentItem) in matched)
            {
                var specification = GetOwningSpecification(currentItem);

                if (isDeprecated != null && TryCompareDeprecation(baseItem, currentItem, elementKind, isDeprecated, specification, out var deprecationChange))
                {
                    changes.Add(deprecationChange);
                    continue;
                }

                changes.AddRange(CompareNameShortNameOwner(baseItem, currentItem, elementKind, specification));
            }

            return changes;
        }

        /// <summary>
        /// Compares the requirements of two iterations, matched by <see cref="Thing.Iid" />.
        /// </summary>
        /// <param name="baseRequirements">The requirements of the baseline iteration</param>
        /// <param name="currentRequirements">The requirements of the newer iteration</param>
        /// <returns>The changes detected between the two sets</returns>
        private static IEnumerable<RequirementChange> CompareRequirements(IEnumerable<Requirement> baseRequirements, IEnumerable<Requirement> currentRequirements)
        {
            const string elementKind = "Requirement";
            var changes = new List<RequirementChange>();
            var (created, deleted, matched) = Diff(baseRequirements, currentRequirements);

            changes.AddRange(created.Select(x => NewChange(RequirementChangeKind.Created, elementKind, x.Iid, x.Name, x.ShortName, x.Actor?.Name ?? string.Empty, GetOwningSpecification(x), newValue: x.Name)));
            changes.AddRange(deleted.Select(x => NewChange(RequirementChangeKind.Deleted, elementKind, x.Iid, x.Name, x.ShortName, x.Actor?.Name ?? string.Empty, GetOwningSpecification(x), oldValue: x.Name)));

            foreach (var (baseRequirement, currentRequirement) in matched)
            {
                var specification = GetOwningSpecification(currentRequirement);

                if (TryCompareDeprecation(baseRequirement, currentRequirement, elementKind, x => x.IsDeprecated, specification, out var deprecationChange))
                {
                    changes.Add(deprecationChange);
                    continue;
                }

                changes.AddRange(CompareNameShortNameOwner(baseRequirement, currentRequirement, elementKind, specification));
                changes.AddRange(CompareRequirementFields(baseRequirement, currentRequirement, specification));
            }

            return changes;
        }

        /// <summary>
        /// Detects whether the deprecation state of a matched pair flipped, producing a single
        /// <see cref="RequirementChangeKind.Deprecated" /> or <see cref="RequirementChangeKind.Restored" /> row when it
        /// did.
        /// </summary>
        /// <typeparam name="T">The <see cref="Thing" /> subtype being compared</typeparam>
        /// <param name="baseItem">The element as it was in the baseline iteration</param>
        /// <param name="currentItem">The element as it is in the newer iteration</param>
        /// <param name="elementKind">The human-readable kind used on the reported row</param>
        /// <param name="isDeprecated">An accessor for the deprecation flag</param>
        /// <param name="specification">The element's owning <see cref="RequirementsSpecification" />, or null</param>
        /// <param name="change">The resulting <see cref="RequirementChange" /> when the state flipped</param>
        /// <returns>true when the deprecation state flipped and <paramref name="change" /> was produced</returns>
        private static bool TryCompareDeprecation<T>(T baseItem, T currentItem, string elementKind, Func<T, bool> isDeprecated, RequirementsSpecification specification, out RequirementChange change) where T : Thing, INamedThing, IShortNamedThing
        {
            var wasDeprecated = isDeprecated(baseItem);
            var isNowDeprecated = isDeprecated(currentItem);

            if (wasDeprecated == isNowDeprecated)
            {
                change = null;
                return false;
            }

            change = NewChange(isNowDeprecated ? RequirementChangeKind.Deprecated : RequirementChangeKind.Restored, elementKind, currentItem.Iid, currentItem.Name, currentItem.ShortName,
                currentItem.Actor?.Name ?? string.Empty, specification);

            return true;
        }

        /// <summary>
        /// Compares the name, short name and owner shared by every requirement-related element, and reports one
        /// <see cref="RequirementChangeKind.Modified" /> row per changed field.
        /// </summary>
        /// <typeparam name="T">The <see cref="Thing" /> subtype being compared</typeparam>
        /// <param name="baseItem">The element as it was in the baseline iteration</param>
        /// <param name="currentItem">The element as it is in the newer iteration</param>
        /// <param name="elementKind">The human-readable kind used on every reported row</param>
        /// <param name="specification">The element's owning <see cref="RequirementsSpecification" />, or null</param>
        /// <returns>The changes detected on the shared fields</returns>
        private static IEnumerable<RequirementChange> CompareNameShortNameOwner<T>(T baseItem, T currentItem, string elementKind, RequirementsSpecification specification) where T : Thing, INamedThing, IShortNamedThing, IOwnedThing
        {
            var changes = new List<RequirementChange>();
            var author = currentItem.Actor?.Name ?? string.Empty;
            var name = currentItem.Name;
            var shortName = currentItem.ShortName;

            if (baseItem.Name != currentItem.Name)
            {
                changes.Add(NewChange(RequirementChangeKind.Modified, elementKind, currentItem.Iid, name, shortName, author, specification, field: "Name", oldValue: baseItem.Name, newValue: currentItem.Name));
            }

            if (baseItem.ShortName != currentItem.ShortName)
            {
                changes.Add(NewChange(RequirementChangeKind.Modified, elementKind, currentItem.Iid, name, shortName, author, specification, field: "Short name", oldValue: baseItem.ShortName, newValue: currentItem.ShortName));
            }

            if (baseItem.Owner?.Iid != currentItem.Owner?.Iid)
            {
                changes.Add(NewChange(RequirementChangeKind.Modified, elementKind, currentItem.Iid, name, shortName, author, specification,
                    field: "Owner", oldValue: baseItem.Owner?.ShortName ?? string.Empty, newValue: currentItem.Owner?.ShortName ?? string.Empty));
            }

            return changes;
        }

        /// <summary>
        /// Compares the fields specific to a <see cref="Requirement" />: its definition, categories, parent group,
        /// simple parameter values and parametric constraints.
        /// </summary>
        /// <param name="baseRequirement">The requirement as it was in the baseline iteration</param>
        /// <param name="currentRequirement">The requirement as it is in the newer iteration</param>
        /// <param name="specification">The requirement's owning <see cref="RequirementsSpecification" />, or null</param>
        /// <returns>The changes detected on the requirement-specific fields</returns>
        private static IEnumerable<RequirementChange> CompareRequirementFields(Requirement baseRequirement, Requirement currentRequirement, RequirementsSpecification specification)
        {
            const string elementKind = "Requirement";
            var changes = new List<RequirementChange>();
            var author = currentRequirement.Actor?.Name ?? string.Empty;
            var name = currentRequirement.Name;
            var shortName = currentRequirement.ShortName;
            var elementId = currentRequirement.Iid;

            var baseDefinition = DefinitionText(baseRequirement);
            var currentDefinition = DefinitionText(currentRequirement);

            if (baseDefinition != currentDefinition)
            {
                changes.Add(NewChange(RequirementChangeKind.Modified, elementKind, elementId, name, shortName, author, specification, field: "Definition", oldValue: baseDefinition, newValue: currentDefinition));
            }

            var baseCategories = CategoryText(baseRequirement);
            var currentCategories = CategoryText(currentRequirement);

            if (baseCategories != currentCategories)
            {
                changes.Add(NewChange(RequirementChangeKind.Modified, elementKind, elementId, name, shortName, author, specification, field: "Category", oldValue: baseCategories, newValue: currentCategories));
            }

            if (baseRequirement.Group?.Iid != currentRequirement.Group?.Iid)
            {
                changes.Add(NewChange(RequirementChangeKind.Modified, elementKind, elementId, name, shortName, author, specification,
                    field: "Group", oldValue: baseRequirement.Group?.ShortName ?? string.Empty, newValue: currentRequirement.Group?.ShortName ?? string.Empty));
            }

            changes.AddRange(CompareSimpleParameterValues(baseRequirement, currentRequirement, name, shortName, author, specification));
            changes.AddRange(CompareParametricConstraints(baseRequirement, currentRequirement, name, shortName, author, specification));

            return changes;
        }

        /// <summary>
        /// Computes a single-language-agnostic definition string for the given <paramref name="requirement" />, so it
        /// can be compared as one value.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>The joined definition content, ordered by language code</returns>
        private static string DefinitionText(Requirement requirement)
        {
            return string.Join(" | ", requirement.Definition.OrderBy(x => x.LanguageCode).Select(x => x.Content));
        }

        /// <summary>
        /// Computes a comparable string for the <see cref="Category" /> set of the given <paramref name="requirement" />.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>The categories' short names, ordered and comma-separated</returns>
        private static string CategoryText(Requirement requirement)
        {
            return string.Join(", ", requirement.Category.OrderBy(x => x.ShortName).Select(x => x.ShortName));
        }

        /// <summary>
        /// Compares the <see cref="SimpleParameterValue" />s of two matched requirements, one <see cref="ParameterType" />
        /// at a time, and reports one row per parameter type whose value was added, removed or changed: added is
        /// reported as <see cref="RequirementChangeKind.Created" />, removed as <see cref="RequirementChangeKind.Deleted" />,
        /// and changed as <see cref="RequirementChangeKind.Modified" />.
        /// </summary>
        /// <param name="baseRequirement">The requirement as it was in the baseline iteration</param>
        /// <param name="currentRequirement">The requirement as it is in the newer iteration</param>
        /// <param name="name">The requirement's current name, used on every reported row</param>
        /// <param name="shortName">The requirement's current short name, used on every reported row</param>
        /// <param name="author">The requirement's current author, used on every reported row</param>
        /// <param name="specification">The requirement's owning <see cref="RequirementsSpecification" />, or null</param>
        /// <returns>The changes detected on the requirement's simple parameter values</returns>
        private static IEnumerable<RequirementChange> CompareSimpleParameterValues(Requirement baseRequirement, Requirement currentRequirement, string name, string shortName, string author, RequirementsSpecification specification)
        {
            var baseValues = baseRequirement.ParameterValue.Where(x => x.ParameterType != null).ToDictionary(x => x.ParameterType.Iid);
            var currentValues = currentRequirement.ParameterValue.Where(x => x.ParameterType != null).ToDictionary(x => x.ParameterType.Iid);
            var changes = new List<RequirementChange>();

            foreach (var parameterTypeIid in baseValues.Keys.Union(currentValues.Keys))
            {
                var hasBase = baseValues.TryGetValue(parameterTypeIid, out var baseValue);
                var hasCurrent = currentValues.TryGetValue(parameterTypeIid, out var currentValue);
                var oldText = hasBase ? string.Join(", ", baseValue.Value) : string.Empty;
                var newText = hasCurrent ? string.Join(", ", currentValue.Value) : string.Empty;

                if (oldText == newText)
                {
                    continue;
                }

                var parameterType = hasCurrent ? currentValue.ParameterType : baseValue.ParameterType;
                var kind = !hasBase ? RequirementChangeKind.Created : !hasCurrent ? RequirementChangeKind.Deleted : RequirementChangeKind.Modified;

                changes.Add(NewChange(kind, "Requirement", currentRequirement.Iid, name, shortName, author, specification,
                    field: $"Value: {parameterType.ShortName}",
                    oldValue: kind == RequirementChangeKind.Created ? string.Empty : oldText,
                    newValue: kind == RequirementChangeKind.Deleted ? string.Empty : newText));
            }

            return changes;
        }

        /// <summary>
        /// Compares the <see cref="ParametricConstraint" />s of two matched requirements, by <see cref="Thing.Iid" />,
        /// and reports one row per constraint added, removed, or whose expression summary changed: added is reported as
        /// <see cref="RequirementChangeKind.Created" />, removed as <see cref="RequirementChangeKind.Deleted" />, and a
        /// changed summary as <see cref="RequirementChangeKind.Modified" />.
        /// </summary>
        /// <param name="baseRequirement">The requirement as it was in the baseline iteration</param>
        /// <param name="currentRequirement">The requirement as it is in the newer iteration</param>
        /// <param name="name">The requirement's current name, used on every reported row</param>
        /// <param name="shortName">The requirement's current short name, used on every reported row</param>
        /// <param name="author">The requirement's current author, used on every reported row</param>
        /// <param name="specification">The requirement's owning <see cref="RequirementsSpecification" />, or null</param>
        /// <returns>The changes detected on the requirement's parametric constraints</returns>
        private static IEnumerable<RequirementChange> CompareParametricConstraints(Requirement baseRequirement, Requirement currentRequirement, string name, string shortName, string author, RequirementsSpecification specification)
        {
            const string elementKind = "Requirement";
            const string field = "Constraint";
            var changes = new List<RequirementChange>();
            var (created, deleted, matched) = Diff(baseRequirement.ParametricConstraint, currentRequirement.ParametricConstraint);

            changes.AddRange(created.Select(x => NewChange(RequirementChangeKind.Created, elementKind, currentRequirement.Iid, name, shortName, author, specification, field: field, newValue: SummariseConstraint(x))));
            changes.AddRange(deleted.Select(x => NewChange(RequirementChangeKind.Deleted, elementKind, currentRequirement.Iid, name, shortName, author, specification, field: field, oldValue: SummariseConstraint(x))));

            changes.AddRange(matched
                .Where(x => SummariseConstraint(x.Base) != SummariseConstraint(x.Current))
                .Select(x => NewChange(RequirementChangeKind.Modified, elementKind, currentRequirement.Iid, name, shortName, author, specification,
                    field: field, oldValue: SummariseConstraint(x.Base), newValue: SummariseConstraint(x.Current))));

            return changes;
        }

        /// <summary>
        /// Builds a short, one-line human-readable summary of the given <paramref name="constraint" />, joining the
        /// summaries of its top-level expressions, the same way <c>RequirementsEditorBodyViewModel.GetExpressionSummary</c>
        /// summarises the constraint tree the requirements editor renders.
        /// </summary>
        /// <param name="constraint">The <see cref="ParametricConstraint" /></param>
        /// <returns>The summary, empty when the constraint has no expression</returns>
        private static string SummariseConstraint(ParametricConstraint constraint)
        {
            return string.Join("; ", constraint.Expression.GetTopLevelExpressions().Select(SummariseExpression));
        }

        /// <summary>
        /// Builds a short, one-line human-readable summary of the given <paramref name="expression" /> subtree (e.g.
        /// <c>NOT (d_r &lt; 500) AND (a &gt; 4)</c>).
        /// </summary>
        /// <param name="expression">The <see cref="BooleanExpression" /></param>
        /// <returns>The summary string</returns>
        private static string SummariseExpression(BooleanExpression expression)
        {
            switch (expression)
            {
                case RelationalExpression relational:
                    var scale = relational.Scale == null ? string.Empty : $" {relational.Scale.ShortName}";
                    return $"{relational.ParameterType?.ShortName} {relational.RelationalOperator.ToScientificNotationString()} {string.Join(", ", relational.Value)}{scale}";

                case NotExpression { Term: not null } not:
                    return $"NOT ({SummariseExpression(not.Term)})";

                default:
                    var separator = expression switch
                    {
                        AndExpression => " AND ",
                        OrExpression => " OR ",
                        ExclusiveOrExpression => " XOR ",
                        _ => " "
                    };

                    return string.Join(separator, GetTerms(expression).Select(x => $"({SummariseExpression(x)})"));
            }
        }

        /// <summary>
        /// Gets the child terms of the given <paramref name="expression" />; relational expressions are leaves.
        /// </summary>
        /// <param name="expression">The <see cref="BooleanExpression" /></param>
        /// <returns>The child expressions</returns>
        private static IReadOnlyList<BooleanExpression> GetTerms(BooleanExpression expression)
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
        /// Compares the traceability <see cref="BinaryRelationship" />s of two iterations: only relationships whose
        /// source or target is, or is contained in, a <see cref="Requirement" />, <see cref="RequirementsSpecification" />
        /// or <see cref="RequirementsGroup" /> are considered, matched by <see cref="Thing.Iid" />.
        /// </summary>
        /// <param name="baseIteration">The baseline <see cref="Iteration" /></param>
        /// <param name="currentIteration">The newer <see cref="Iteration" /></param>
        /// <returns>The created and deleted traceability rows</returns>
        private static IEnumerable<RequirementChange> CompareTraceability(Iteration baseIteration, Iteration currentIteration)
        {
            var (created, deleted, _) = Diff(RequirementRelatedRelationships(baseIteration), RequirementRelatedRelationships(currentIteration));
            var changes = new List<RequirementChange>();

            changes.AddRange(created.Select(x => BuildTraceabilityChange(x, RequirementChangeKind.Created)));
            changes.AddRange(deleted.Select(x => BuildTraceabilityChange(x, RequirementChangeKind.Deleted)));

            return changes;
        }

        /// <summary>
        /// Gets the <see cref="BinaryRelationship" />s of the given <paramref name="iteration" /> whose source or
        /// target is requirement-related.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <returns>The requirement-related relationships</returns>
        private static IEnumerable<BinaryRelationship> RequirementRelatedRelationships(Iteration iteration)
        {
            return iteration.Relationship.OfType<BinaryRelationship>().Where(x => IsRequirementRelated(x.Source) || IsRequirementRelated(x.Target));
        }

        /// <summary>
        /// Determines whether the given <paramref name="thing" /> is, or is contained in, a requirement, specification
        /// or group, so a link on e.g. a requirement's parametric constraint parameter is also requirement-related.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /></param>
        /// <returns>true when the thing is requirement-related</returns>
        private static bool IsRequirementRelated(Thing thing)
        {
            return thing is Requirement or RequirementsSpecification or RequirementsGroup
                   || thing?.GetContainerOfType<Requirement>() != null
                   || thing?.GetContainerOfType<RequirementsSpecification>() != null;
        }

        /// <summary>
        /// Builds the traceability row for the given <paramref name="relationship" />, labelled with its source and
        /// target's short names and carrying its categories in the changed value.
        /// </summary>
        /// <param name="relationship">The <see cref="BinaryRelationship" /></param>
        /// <param name="kind">The <see cref="RequirementChangeKind" />, either created or deleted</param>
        /// <returns>The traceability <see cref="RequirementChange" /></returns>
        private static RequirementChange BuildTraceabilityChange(BinaryRelationship relationship, RequirementChangeKind kind)
        {
            var label = $"{DescribeEndpoint(relationship.Source)} to {DescribeEndpoint(relationship.Target)}";
            var categories = string.Join(", ", relationship.Category.Select(x => x.ShortName));
            var specification = GetOwningSpecification(relationship.Source) ?? GetOwningSpecification(relationship.Target);

            return NewChange(kind, "Binary Relationship", relationship.Iid, label, label, relationship.Actor?.Name ?? string.Empty, specification,
                field: "Relationship",
                oldValue: kind == RequirementChangeKind.Deleted ? categories : string.Empty,
                newValue: kind == RequirementChangeKind.Created ? categories : string.Empty);
        }

        /// <summary>
        /// Builds a readable label for one end of a traceability relationship. A <see cref="RelationalExpression" /> is
        /// described by its owning requirement and its expression summary (its <c>UserFriendlyShortName</c> is not
        /// meaningful), a parameter by its parameter type, and any other <see cref="Thing" /> by its
        /// <c>UserFriendlyShortName</c>.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> at one end of the relationship</param>
        /// <returns>The endpoint label, empty when <paramref name="thing" /> is null</returns>
        private static string DescribeEndpoint(Thing thing)
        {
            switch (thing)
            {
                case null:
                    return string.Empty;

                case RelationalExpression relational:
                    var requirement = relational.GetContainerOfType<Requirement>();
                    var summary = SummariseExpression(relational);
                    return requirement == null ? summary : $"{requirement.ShortName} ({summary})";

                case ParameterOrOverrideBase parameter:
                    return parameter.ParameterType?.ShortName ?? parameter.UserFriendlyShortName;

                default:
                    return thing.UserFriendlyShortName;
            }
        }
    }
}
