// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVQueries.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The single source of truth for reading V&amp;V data: what is an item, a step, an activity or a report, how an
    /// attribute is read (through the performing activity where it derives), how an item rolls up, and which review
    /// requests concern it. The register, the matrix, the checks and the export all go through these functions so they
    /// cannot disagree, and they match the COMET IME's rules.
    /// </summary>
    public static class VandVQueries
    {
        /// <summary>
        /// The placeholder the stock editors write for an unset attribute.
        /// </summary>
        private const string UnsetValue = "-";

        /// <summary>
        /// The values a boolean attribute reads as true, compared case-insensitively.
        /// </summary>
        private static readonly IReadOnlyList<string> TrueValues = ["true", "yes", "1", "-1"];

        /// <summary>
        /// Determines whether the given thing carries the category with the given short name, directly or through a super
        /// category.
        /// </summary>
        /// <param name="thing">The <see cref="ICategorizableThing" />.</param>
        /// <param name="categoryShortName">The category short name.</param>
        /// <returns>true when the thing carries the category or a sub-category of it.</returns>
        public static bool IsCategorizedAs(ICategorizableThing thing, string categoryShortName)
        {
            return thing != null && thing.Category.Any(x => x.ShortName == categoryShortName || x.AllSuperCategories().Any(super => super.ShortName == categoryShortName));
        }

        /// <summary>
        /// Determines whether the given requirement is a V&amp;V item.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" />.</param>
        /// <returns>true when it is categorized <see cref="VandVConstants.VnVItemCategory" />.</returns>
        public static bool IsItem(Requirement requirement)
        {
            return IsCategorizedAs(requirement, VandVConstants.VnVItemCategory);
        }

        /// <summary>
        /// Determines whether the given requirement is a procedure step.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" />.</param>
        /// <returns>true when it is categorized <see cref="VandVConstants.VnVStepCategory" />.</returns>
        public static bool IsStep(Requirement requirement)
        {
            return IsCategorizedAs(requirement, VandVConstants.VnVStepCategory);
        }

        /// <summary>
        /// Determines whether the given requirement is a shared activity.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" />.</param>
        /// <returns>true when it is categorized <see cref="VandVConstants.VnVActivityCategory" />.</returns>
        public static bool IsActivity(Requirement requirement)
        {
            return IsCategorizedAs(requirement, VandVConstants.VnVActivityCategory);
        }

        /// <summary>
        /// Determines whether the given specification is a V&amp;V report.
        /// </summary>
        /// <param name="specification">The <see cref="RequirementsSpecification" />.</param>
        /// <returns>true when it is categorized <see cref="VandVConstants.VnVReportCategory" />.</returns>
        public static bool IsReport(RequirementsSpecification specification)
        {
            return IsCategorizedAs(specification, VandVConstants.VnVReportCategory);
        }

        /// <summary>
        /// Determines whether the given specification is the dedicated V&amp;V plan.
        /// </summary>
        /// <param name="specification">The <see cref="RequirementsSpecification" />.</param>
        /// <returns>true when its short name is <see cref="VandVConstants.VandVSpecificationShortName" />.</returns>
        public static bool IsVandVSpecification(RequirementsSpecification specification)
        {
            return specification?.ShortName == VandVConstants.VandVSpecificationShortName;
        }

        /// <summary>
        /// Determines whether the given relationship is a coverage link (verifies or validates). Whether it really counts as
        /// coverage also depends on its source being a V&amp;V item, see <see cref="VandVModel.GetCoveringItems" />.
        /// </summary>
        /// <param name="relationship">The <see cref="BinaryRelationship" />.</param>
        /// <returns>true when it carries a coverage link category.</returns>
        public static bool IsCoverageLink(BinaryRelationship relationship)
        {
            return VandVConstants.CoverageLinkCategories.Any(x => IsCategorizedAs(relationship, x));
        }

        /// <summary>
        /// Determines whether the given thing is V&amp;V bookkeeping that the Requirements view hides by default, by short name
        /// only so it also works on a model whose V&amp;V reference data was seeded by the IME:
        /// the V&amp;V plan and the reports; items, steps and activities, and any requirement inside a V&amp;V specification;
        /// a relationship with a V&amp;V-only category or with a V&amp;V thing at either end (an ordinary verifies link between
        /// two requirements stays visible); and anything contained in one of those (groups, constraints, values).
        /// </summary>
        /// <param name="thing">The <see cref="Thing" />.</param>
        /// <returns>true for V&amp;V bookkeeping.</returns>
        public static bool IsVandVThing(Thing thing)
        {
            return thing switch
            {
                null => false,
                RequirementsSpecification specification => IsVandVSpecification(specification) || IsReport(specification),
                Requirement requirement => IsItem(requirement) || IsStep(requirement) || IsActivity(requirement) || IsVandVThing(requirement.Container),
                BinaryRelationship relationship => HasVandVOnlyCategory(relationship) || IsVandVEnd(relationship.Source) || IsVandVEnd(relationship.Target),
                MultiRelationship relationship => HasVandVOnlyCategory(relationship) || relationship.RelatedThing.Any(IsVandVEnd),
                _ => IsVandVThing(thing.Container)
            };
        }

        /// <summary>
        /// Gets the V&amp;V coverage of the given requirement.
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        /// <param name="requirement">The requirement.</param>
        /// <returns>The roll-up of the items covering it, or null when no item covers it.</returns>
        public static VandVRollUp? GetCoverage(VandVModel model, Requirement requirement)
        {
            var items = model.GetCoveringItems(requirement);
            return items.Count == 0 ? null : GetRollUp(model, items);
        }

        /// <summary>
        /// Gets the value of the given requirement whose parameter type carries the given short name.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" />.</param>
        /// <param name="parameterTypeShortName">The vnv_* short name.</param>
        /// <returns>The <see cref="SimpleParameterValue" />, or null when the attribute was never written.</returns>
        public static SimpleParameterValue GetSimpleParameterValue(Requirement requirement, string parameterTypeShortName)
        {
            return requirement?.ParameterValue.FirstOrDefault(x => x.ParameterType?.ShortName == parameterTypeShortName);
        }

        /// <summary>
        /// Gets the stored value of the given attribute, reading a blank value and the "-" placeholder as unset.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" />.</param>
        /// <param name="parameterTypeShortName">The vnv_* short name.</param>
        /// <returns>The stored value, or null when unset.</returns>
        public static string GetValue(Requirement requirement, string parameterTypeShortName)
        {
            var value = GetSimpleParameterValue(requirement, parameterTypeShortName)?.Value.FirstOrDefault();
            return string.IsNullOrWhiteSpace(value) || value == UnsetValue ? null : value;
        }

        /// <summary>
        /// Gets the display value of the given attribute: an enumeration value is shown by its value definition's name,
        /// whether the stored text is that name or its short name; a value no longer defined is shown as stored.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" />.</param>
        /// <param name="parameterTypeShortName">The vnv_* short name.</param>
        /// <returns>The display value, or null when unset.</returns>
        public static string GetDisplayValue(Requirement requirement, string parameterTypeShortName)
        {
            var value = GetValue(requirement, parameterTypeShortName);

            if (value == null || GetSimpleParameterValue(requirement, parameterTypeShortName).ParameterType is not EnumerationParameterType enumeration)
            {
                return value;
            }

            return enumeration.ValueDefinition.FirstOrDefault(x => VandVConstants.ValueEquals(x.Name, value) || VandVConstants.ValueEquals(x.ShortName, value))?.Name ?? value;
        }

        /// <summary>
        /// Gets the boolean value of the given attribute.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" />.</param>
        /// <param name="parameterTypeShortName">The vnv_* short name.</param>
        /// <returns>true when the stored value is true, yes, 1 or -1; false when unset or anything else.</returns>
        public static bool GetBooleanValue(Requirement requirement, string parameterTypeShortName)
        {
            var value = GetValue(requirement, parameterTypeShortName);
            return value != null && TrueValues.Any(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the requirement that supplies the effective value of the given attribute of an item: the item itself when it
        /// states the value or the attribute never derives, otherwise the activity performing it when that states the value.
        /// A different return value than <paramref name="item" /> means the value is inherited.
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        /// <param name="item">The V&amp;V item.</param>
        /// <param name="parameterTypeShortName">The vnv_* short name.</param>
        /// <returns>The item or its activity.</returns>
        public static Requirement GetEffectiveSource(VandVModel model, Requirement item, string parameterTypeShortName)
        {
            if (GetValue(item, parameterTypeShortName) != null || !VandVConstants.DerivedFromActivity.Contains(parameterTypeShortName))
            {
                return item;
            }

            var activity = model.GetActivity(item);
            return GetValue(activity, parameterTypeShortName) != null ? activity : item;
        }

        /// <summary>
        /// Gets the effective stored value of the given attribute of an item, read through its activity where it derives
        /// (see <see cref="GetEffectiveSource" />).
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        /// <param name="item">The V&amp;V item.</param>
        /// <param name="parameterTypeShortName">The vnv_* short name.</param>
        /// <returns>The effective value, or null when unset on both.</returns>
        public static string GetEffectiveValue(VandVModel model, Requirement item, string parameterTypeShortName)
        {
            return GetValue(GetEffectiveSource(model, item, parameterTypeShortName), parameterTypeShortName);
        }

        /// <summary>
        /// Gets the effective display value of the given attribute of an item (see <see cref="GetEffectiveSource" /> and
        /// <see cref="GetDisplayValue" />).
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        /// <param name="item">The V&amp;V item.</param>
        /// <param name="parameterTypeShortName">The vnv_* short name.</param>
        /// <returns>The effective display value, or null when unset on both.</returns>
        public static string GetEffectiveDisplayValue(VandVModel model, Requirement item, string parameterTypeShortName)
        {
            return GetDisplayValue(GetEffectiveSource(model, item, parameterTypeShortName), parameterTypeShortName);
        }

        /// <summary>
        /// Gets the compliance of an item; it never derives, and a missing value reads as Not Assessed.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>The stored compliance, or <see cref="VandVConstants.NotAssessed" />.</returns>
        public static string GetCompliance(Requirement item)
        {
            return GetValue(item, VandVConstants.Compliance) ?? VandVConstants.NotAssessed;
        }

        /// <summary>
        /// Determines whether the given compliance is a shortfall (Partially Compliant or Non-Compliant).
        /// </summary>
        /// <param name="compliance">The stored compliance, in either spelling.</param>
        /// <returns>true for a shortfall.</returns>
        public static bool IsShortfall(string compliance)
        {
            return VandVConstants.ShortfallCompliances.Any(x => VandVConstants.ValueEquals(x, compliance));
        }

        /// <summary>
        /// Determines whether an item is closed out.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>true when <see cref="VandVConstants.Closed" /> reads as true.</returns>
        public static bool IsClosed(Requirement item)
        {
            return GetBooleanValue(item, VandVConstants.Closed);
        }

        /// <summary>
        /// Determines whether the given annotation references the given thing, as its primary annotated thing or through a
        /// related thing reference.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation" />.</param>
        /// <param name="thing">The annotated <see cref="Thing" />.</param>
        /// <returns>true when the annotation references the thing.</returns>
        public static bool References(EngineeringModelDataAnnotation annotation, Thing thing)
        {
            if (annotation == null || thing == null)
            {
                return false;
            }

            return annotation.PrimaryAnnotatedThing?.ReferencedThing?.Iid == thing.Iid || annotation.RelatedThing.Any(x => x.ReferencedThing?.Iid == thing.Iid);
        }

        /// <summary>
        /// Determines whether the given annotation is a review request: a review item discrepancy, a request for deviation or
        /// a request for waiver. A change request and a note are not concessions against a requirement.
        /// </summary>
        /// <param name="annotation">The <see cref="ModellingAnnotationItem" />.</param>
        /// <returns>true for a review request.</returns>
        public static bool IsReviewRequest(ModellingAnnotationItem annotation)
        {
            return annotation is ReviewItemDiscrepancy or RequestForDeviation or RequestForWaiver;
        }

        /// <summary>
        /// Determines whether a review request against the given thing is still open.
        /// </summary>
        /// <param name="annotations">The engineering model's <see cref="ModellingAnnotationItem" />s.</param>
        /// <param name="thing">The annotated <see cref="Thing" />.</param>
        /// <returns>true when an open review request references the thing.</returns>
        public static bool HasOpenReviewRequest(IEnumerable<ModellingAnnotationItem> annotations, Thing thing)
        {
            return (annotations ?? []).Any(x => IsReviewRequest(x) && x.Status == AnnotationStatusKind.OPEN && References(x, thing));
        }

        /// <summary>
        /// Determines whether a waiver or deviation conceding a shortfall of the given item has been accepted, that is a
        /// request for waiver or deviation referencing it is no longer open.
        /// </summary>
        /// <param name="annotations">The engineering model's <see cref="ModellingAnnotationItem" />s.</param>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>true when an accepted concession references the item.</returns>
        public static bool HasAcceptedConcession(IEnumerable<ModellingAnnotationItem> annotations, Thing item)
        {
            return (annotations ?? []).Any(x => (x is RequestForWaiver or RequestForDeviation) && x.Status != AnnotationStatusKind.OPEN && References(x, item));
        }

        /// <summary>
        /// Rolls up one item. A closed item passes, unless it falls short of the requirement without an accepted concession.
        /// An item still open fails when its effective status is Failed or its compliance is a shortfall, passes when its
        /// effective status needs no further action (Passed, Waived, Deviated, Not Applicable), and is open otherwise.
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>The <see cref="VandVItemOutcome" />.</returns>
        public static VandVItemOutcome GetItemOutcome(VandVModel model, Requirement item)
        {
            var isShortfall = IsShortfall(GetCompliance(item));

            if (IsClosed(item))
            {
                return isShortfall && !HasAcceptedConcession(model.Annotations, item) ? VandVItemOutcome.Failed : VandVItemOutcome.Passed;
            }

            var status = GetEffectiveValue(model, item, VandVConstants.Status);

            if (isShortfall || VandVConstants.ValueEquals(status, VandVConstants.StatusFailed))
            {
                return VandVItemOutcome.Failed;
            }

            return VandVConstants.ClosedPositiveStatuses.Any(x => VandVConstants.ValueEquals(x, status)) ? VandVItemOutcome.Passed : VandVItemOutcome.Open;
        }

        /// <summary>
        /// Rolls up the given items.
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        /// <param name="items">The V&amp;V items.</param>
        /// <returns>The <see cref="VandVRollUp" />.</returns>
        public static VandVRollUp GetRollUp(VandVModel model, IEnumerable<Requirement> items)
        {
            var outcomes = items.Select(x => GetItemOutcome(model, x)).ToList();

            return new VandVRollUp(
                outcomes.Count(x => x == VandVItemOutcome.Passed),
                outcomes.Count(x => x == VandVItemOutcome.Failed),
                outcomes.Count(x => x == VandVItemOutcome.Open));
        }

        /// <summary>
        /// Formats the coverage of one requirement: Not covered, or for example 2 item(s): 1 passed, 1 open.
        /// </summary>
        /// <param name="rollUp">The roll-up of the items covering the requirement.</param>
        /// <returns>The coverage text.</returns>
        public static string FormatRequirementCoverage(VandVRollUp rollUp)
        {
            if (rollUp.Total == 0)
            {
                return "Not covered";
            }

            string[] parts =
            [
                rollUp.Passed > 0 ? $"{rollUp.Passed} passed" : null,
                rollUp.Failed > 0 ? $"{rollUp.Failed} failed" : null,
                rollUp.Open > 0 ? $"{rollUp.Open} open" : null
            ];

            return $"{rollUp.Total} item(s): {string.Join(", ", parts.Where(x => x != null))}";
        }

        /// <summary>
        /// Formats the coverage of a specification or group: for example 7/12 verified, 1 failed, where the failed count is
        /// summed per requirement as the IME does (an item covering two requirements below counts twice); empty when there
        /// is no requirement below.
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        /// <param name="requirements">The requirements to be covered below the specification or group.</param>
        /// <returns>The coverage text.</returns>
        public static string FormatContainerCoverage(VandVModel model, IEnumerable<Requirement> requirements)
        {
            var rollUps = requirements.Select(x => GetRollUp(model, model.GetCoveringItems(x))).ToList();

            if (rollUps.Count == 0)
            {
                return string.Empty;
            }

            var failed = rollUps.Sum(x => x.Failed);
            var text = $"{rollUps.Count(x => x.IsVerified)}/{rollUps.Count} verified";
            return failed > 0 ? $"{text}, {failed} failed" : text;
        }

        /// <summary>
        /// Suggests a short name for a new item covering the given requirement, VNV_&lt;requirement&gt;_&lt;n&gt; with the first
        /// n not used by any requirement of the iteration.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" />.</param>
        /// <param name="requirement">The requirement the new item covers.</param>
        /// <returns>The suggested short name.</returns>
        public static string SuggestItemShortName(Iteration iteration, Requirement requirement)
        {
            var requirementStem = string.IsNullOrWhiteSpace(requirement.ShortName) ? VandVConstants.FallbackRequirementStem : requirement.ShortName;
            var stem = $"{VandVConstants.ItemShortNamePrefix}_{VandVConstants.ToShortName(requirementStem)}";
            return SuggestFreeShortName(iteration, stem);
        }

        /// <summary>
        /// Suggests the short name (activity number) of a new activity, ACT_&lt;n&gt; with the first n not used by any
        /// requirement of the iteration.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" />.</param>
        /// <returns>The suggested short name.</returns>
        public static string SuggestActivityShortName(Iteration iteration)
        {
            return SuggestFreeShortName(iteration, VandVConstants.ActivityShortNamePrefix);
        }

        /// <summary>
        /// Determines whether the given relationship carries a category only V&amp;V authors.
        /// </summary>
        /// <param name="relationship">The <see cref="Relationship" />.</param>
        /// <returns>true when it carries a V&amp;V-only category.</returns>
        private static bool HasVandVOnlyCategory(Relationship relationship)
        {
            return VandVConstants.VandVOnlyLinkCategories.Any(x => IsCategorizedAs(relationship, x));
        }

        /// <summary>
        /// Determines whether an end of a relationship is V&amp;V bookkeeping. An end that is itself a relationship is not
        /// followed: two relationships pointing at each other would otherwise recurse until the process dies.
        /// </summary>
        /// <param name="end">The related thing.</param>
        /// <returns>true when the end is V&amp;V bookkeeping.</returns>
        private static bool IsVandVEnd(Thing end)
        {
            return end is not Relationship && IsVandVThing(end);
        }

        /// <summary>
        /// Gets &lt;stem&gt;_&lt;n&gt; with the first n from 1 not used as a short name by any requirement of the iteration.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" />.</param>
        /// <param name="stem">The short name stem.</param>
        /// <returns>The free short name.</returns>
        private static string SuggestFreeShortName(Iteration iteration, string stem)
        {
            var used = iteration.RequirementsSpecification
                .SelectMany(x => x.Requirement)
                .Select(x => x.ShortName)
                .ToHashSet(StringComparer.Ordinal);

            var index = 1;

            while (used.Contains($"{stem}_{index}"))
            {
                index++;
            }

            return $"{stem}_{index}";
        }
    }
}
