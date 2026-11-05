// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVBinaryRelationshipRuleDefinition.cs" company="Starion Group S.A.">
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
    /// <summary>
    /// The declaration of one V&amp;V <see cref="CDP4Common.SiteDirectoryData.BinaryRelationshipRule" />. Its target category
    /// is not declared: it is the model's own requirement category, resolved at set-up time.
    /// </summary>
    /// <param name="ShortName">The reserved short name the rule is found by.</param>
    /// <param name="Name">The human readable name.</param>
    /// <param name="RelationshipCategoryShortName">The short name of the category the governed relationships carry.</param>
    /// <param name="ForwardRelationshipName">The forward relationship name.</param>
    /// <param name="InverseRelationshipName">The inverse relationship name.</param>
    /// <param name="SourceCategoryShortName">The short name of the required source category.</param>
    public sealed record VandVBinaryRelationshipRuleDefinition(string ShortName, string Name, string RelationshipCategoryShortName, string ForwardRelationshipName, string InverseRelationshipName, string SourceCategoryShortName);
}
