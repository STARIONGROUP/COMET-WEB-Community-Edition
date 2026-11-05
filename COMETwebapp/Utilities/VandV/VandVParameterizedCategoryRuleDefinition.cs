// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVParameterizedCategoryRuleDefinition.cs" company="Starion Group S.A.">
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
    /// The declaration of one V&amp;V <see cref="CDP4Common.SiteDirectoryData.ParameterizedCategoryRule" />, which makes the
    /// given attributes mandatory on everything carrying the given category.
    /// </summary>
    /// <param name="ShortName">The reserved short name the rule is found by.</param>
    /// <param name="Name">The human readable name.</param>
    /// <param name="CategoryShortName">The short name of the category the rule applies to.</param>
    /// <param name="ParameterTypeShortNames">The short names of the mandatory parameter types.</param>
    public sealed record VandVParameterizedCategoryRuleDefinition(string ShortName, string Name, string CategoryShortName, IReadOnlyList<string> ParameterTypeShortNames);
}
