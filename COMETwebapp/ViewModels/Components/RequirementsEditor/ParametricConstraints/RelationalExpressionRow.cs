// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RelationalExpressionRow.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.RequirementsEditor.ParametricConstraints
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// A leaf of the parametric-constraint tree: a relational expression comparing a <see cref="ParameterType" /> against
    /// a value with a <see cref="RelationalOperatorKind" /> (and an optional <see cref="MeasurementScale" />). Maps to the
    /// SDK <see cref="RelationalExpression" />.
    /// </summary>
    public class RelationalExpressionRow : BooleanExpressionRow
    {
        /// <summary>
        /// Gets or sets the compared <see cref="ParameterType" />.
        /// </summary>
        public ParameterType ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the relational operator.
        /// </summary>
        public RelationalOperatorKind Operator { get; set; } = RelationalOperatorKind.EQ;

        /// <summary>
        /// Gets or sets the compared value.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional <see cref="MeasurementScale" /> of the value.
        /// </summary>
        public MeasurementScale Scale { get; set; }

        /// <summary>
        /// Gets a value indicating whether the relational expression has a parameter type and a value.
        /// </summary>
        public override bool IsValid => this.ParameterType != null && !string.IsNullOrWhiteSpace(this.Value);
    }
}
