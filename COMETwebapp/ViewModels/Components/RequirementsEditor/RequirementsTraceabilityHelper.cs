// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsTraceabilityHelper.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// Resolves the <see cref="Relationship" />s a <see cref="Requirement" /> participates in against the
    /// <see cref="BinaryRelationshipRule" />s and <see cref="MultiRelationshipRule" />s of the open reference data
    /// libraries, and builds the traceability rows and relationship details rendered for it.
    /// </summary>
    public static class RequirementsTraceabilityHelper
    {
        /// <summary>
        /// Gets a display row for every <see cref="BinaryRelationship" /> and <see cref="MultiRelationship" /> of the
        /// given <paramref name="iteration" /> the given <paramref name="requirement" /> participates in.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <param name="iteration">The <see cref="Iteration" /> whose relationships are scanned</param>
        /// <param name="rules">The <see cref="Rule" />s of the open reference data libraries</param>
        /// <returns>The traceability rows</returns>
        public static IReadOnlyList<RequirementRelationshipRow> GetTraceability(Requirement requirement, Iteration iteration, IReadOnlyCollection<Rule> rules)
        {
            var rows = new List<RequirementRelationshipRow>();

            foreach (var relationship in iteration.Relationship)
            {
                switch (relationship)
                {
                    case BinaryRelationship binary when binary.Source == requirement || binary.Target == requirement:
                        rows.Add(new RequirementRelationshipRow
                        {
                            Relationship = binary,
                            Direction = binary.Source == requirement ? RelationshipDirection.Outgoing : RelationshipDirection.Incoming,
                            RelatedThings = [binary.Source == requirement ? binary.Target : binary.Source],
                            RuleNames = GetMatchingRuleNames(binary, rules)
                        });

                        break;

                    case MultiRelationship multi when multi.RelatedThing.Contains(requirement):
                        rows.Add(new RequirementRelationshipRow
                        {
                            Relationship = multi,
                            Direction = RelationshipDirection.Bidirectional,
                            RelatedThings = multi.RelatedThing.Where(x => x != requirement).ToList(),
                            RuleNames = GetMatchingRuleNames(multi, rules)
                        });

                        break;
                }
            }

            return rows;
        }

        /// <summary>
        /// Gets a relationship detail for every <see cref="BinaryRelationship" /> and <see cref="MultiRelationship" /> of
        /// the given <paramref name="iteration" /> the given <paramref name="requirement" /> participates in, resolved
        /// to its matched rules so an export can lay out a column per rule and direction. A relationship that matches
        /// several rules yields one detail per matched rule; one that matches no rule yields a single detail with a null
        /// rule.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <param name="iteration">The <see cref="Iteration" /> whose relationships are scanned</param>
        /// <param name="rules">The <see cref="Rule" />s of the open reference data libraries</param>
        /// <returns>The relationship details</returns>
        public static IReadOnlyList<RequirementRelationshipDetail> GetRelationshipDetails(Requirement requirement, Iteration iteration, IReadOnlyCollection<Rule> rules)
        {
            var details = new List<RequirementRelationshipDetail>();

            foreach (var relationship in iteration.Relationship)
            {
                IReadOnlyList<Thing> relatedThings;
                RelationshipDirection direction;

                switch (relationship)
                {
                    case BinaryRelationship binary when binary.Source == requirement || binary.Target == requirement:
                        direction = binary.Source == requirement ? RelationshipDirection.Outgoing : RelationshipDirection.Incoming;
                        relatedThings = [binary.Source == requirement ? binary.Target : binary.Source];
                        break;

                    case MultiRelationship multi when multi.RelatedThing.Contains(requirement):
                        direction = RelationshipDirection.Bidirectional;
                        relatedThings = multi.RelatedThing.Where(x => x != requirement).ToList();
                        break;

                    default:
                        continue;
                }

                var categories = relationship.Category.ToList();
                var references = GetMatchingRules(relationship, rules).Select(ToRuleReference).ToList();

                if (references.Count == 0)
                {
                    details.Add(new RequirementRelationshipDetail { RelatedThings = relatedThings, Direction = direction, Rule = null, Categories = categories });
                    continue;
                }

                details.AddRange(references.Select(reference => new RequirementRelationshipDetail { RelatedThings = relatedThings, Direction = direction, Rule = reference, Categories = categories }));
            }

            return details;
        }

        /// <summary>
        /// Gets the names of the <see cref="BinaryRelationshipRule" />s or <see cref="MultiRelationshipRule" />s among
        /// <paramref name="rules" /> whose relationship category is carried by the given <paramref name="relationship" />.
        /// </summary>
        /// <param name="relationship">The <see cref="Relationship" /></param>
        /// <param name="rules">The <see cref="Rule" />s of the open reference data libraries</param>
        /// <returns>The matching rule names</returns>
        public static List<string> GetMatchingRuleNames(Relationship relationship, IReadOnlyCollection<Rule> rules)
        {
            return GetMatchingRules(relationship, rules).Select(rule => rule.Name).ToList();
        }

        /// <summary>
        /// Gets the <see cref="BinaryRelationshipRule" />s or <see cref="MultiRelationshipRule" />s among
        /// <paramref name="rules" /> whose relationship category is carried by the given <paramref name="relationship" />.
        /// </summary>
        /// <param name="relationship">The <see cref="Relationship" /></param>
        /// <param name="rules">The <see cref="Rule" />s of the open reference data libraries</param>
        /// <returns>The matching rules</returns>
        public static List<Rule> GetMatchingRules(Relationship relationship, IReadOnlyCollection<Rule> rules)
        {
            return relationship is BinaryRelationship
                ? rules.OfType<BinaryRelationshipRule>().Where(x => RelationshipCarriesCategory(relationship, x.RelationshipCategory)).Cast<Rule>().ToList()
                : rules.OfType<MultiRelationshipRule>().Where(x => RelationshipCarriesCategory(relationship, x.RelationshipCategory)).Cast<Rule>().ToList();
        }

        /// <summary>
        /// Builds a <see cref="RelationshipRuleReference" /> from the given <paramref name="rule" />: a binary rule keeps
        /// its forward and inverse names (it is directional), a multi rule keeps only its name.
        /// </summary>
        /// <param name="rule">The <see cref="Rule" /></param>
        /// <returns>The reference</returns>
        private static RelationshipRuleReference ToRuleReference(Rule rule)
        {
            return rule is BinaryRelationshipRule binaryRule
                ? new RelationshipRuleReference { Iid = rule.Iid, Name = rule.Name, ForwardName = binaryRule.ForwardRelationshipName, InverseName = binaryRule.InverseRelationshipName }
                : new RelationshipRuleReference { Iid = rule.Iid, Name = rule.Name };
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
