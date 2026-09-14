// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BooleanExpressionHelper.cs" company="Starion Group S.A.">
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
    using CDP4Common.Extensions;

    /// <summary>
    /// Renders a <see cref="ParametricConstraint" />'s <see cref="BooleanExpression" /> tree into the one-line
    /// human-readable summaries and root/child expressions used to draw the constraint tree.
    /// </summary>
    public static class BooleanExpressionHelper
    {
        /// <summary>
        /// Gets the root <see cref="BooleanExpression" />s of the given <paramref name="constraint" />: its
        /// <see cref="ParametricConstraint.TopExpression" /> when set, otherwise the expressions that are not a term
        /// of any other expression.
        /// </summary>
        /// <param name="constraint">The <see cref="ParametricConstraint" /></param>
        /// <returns>The root expressions to render the constraint tree from</returns>
        public static IEnumerable<BooleanExpression> GetTopExpressions(ParametricConstraint constraint)
        {
            if (constraint.TopExpression != null)
            {
                return [constraint.TopExpression];
            }

            return constraint.Expression.GetTopLevelExpressions();
        }

        /// <summary>
        /// Gets the child terms of the given <paramref name="expression" />; relational expressions are leaves. Uses the
        /// SDK's <see cref="BooleanExpression.GetMyExpressions" />, which returns the direct children of any
        /// <see cref="BooleanExpression" /> (the terms of an and/or/xor, the single term of a not, none for a leaf).
        /// </summary>
        /// <param name="expression">The <see cref="BooleanExpression" /></param>
        /// <returns>The child expressions</returns>
        public static IReadOnlyList<BooleanExpression> GetTerms(BooleanExpression expression)
        {
            return expression.GetMyExpressions();
        }

        /// <summary>
        /// Gets a one-line human-readable summary of the given <paramref name="expression" /> subtree (e.g.
        /// <c>NOT (d_r &lt; 500) AND (a &gt; 4)</c>), built the same way the tree renders so it stays consistent with it.
        /// </summary>
        /// <param name="expression">The <see cref="BooleanExpression" /></param>
        /// <returns>The summary string</returns>
        public static string GetExpressionSummary(BooleanExpression expression)
        {
            switch (expression)
            {
                case RelationalExpression relational:
                    var scale = relational.Scale == null ? string.Empty : $" {relational.Scale.ShortName}";
                    return $"{relational.ParameterType?.ShortName} {relational.RelationalOperator.ToScientificNotationString()} {string.Join(", ", relational.Value)}{scale}";

                case NotExpression { Term: not null } not:
                    return $"NOT ({GetExpressionSummary(not.Term)})";

                default:
                    var separator = expression switch
                    {
                        AndExpression => " AND ",
                        OrExpression => " OR ",
                        ExclusiveOrExpression => " XOR ",
                        _ => " "
                    };

                    return string.Join(separator, GetTerms(expression).Select(x => $"({GetExpressionSummary(x)})"));
            }
        }
    }
}
