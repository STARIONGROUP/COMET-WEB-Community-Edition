// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="LogicalOperatorKind.cs" company="Starion Group S.A.">
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
    /// <summary>
    /// The logical operator of a composite boolean expression group in the parametric constraint editor. Maps to the
    /// SDK <see cref="CDP4Common.EngineeringModelData.AndExpression" />, <see cref="CDP4Common.EngineeringModelData.OrExpression" />
    /// and <see cref="CDP4Common.EngineeringModelData.ExclusiveOrExpression" /> types.
    /// </summary>
    public enum LogicalOperatorKind
    {
        /// <summary>
        /// A conjunction (all terms must hold).
        /// </summary>
        And,

        /// <summary>
        /// A disjunction (at least one term must hold).
        /// </summary>
        Or,

        /// <summary>
        /// An exclusive disjunction (exactly one term must hold).
        /// </summary>
        Xor
    }
}
