// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementValidationRules.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Validators.EngineeringModel
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Holds the name and short-name validation rules shared by the <see cref="Requirement" />,
    /// <see cref="RequirementsGroup" /> and <see cref="RequirementsSpecification" /> validators. Unlike the default
    /// ECSS-E-TM-10-25 name rule, these allow the first character to be a digit, so a requirement, group or specification
    /// can be numbered to control its reading order. This mirrors the desktop IME's <c>ImeValidationService</c>.
    /// </summary>
    public static class RequirementValidationRules
    {
        /// <summary>
        /// The regular expression a <see cref="DefinedThing.Name" /> must match: it may start with a letter or a digit,
        /// and must not contain parentheses or a trailing space. Same intent as the shared "Name" rule, but the first
        /// character may also be a digit (identical to the desktop IME's <c>RequirementName</c> rule).
        /// </summary>
        public const string NameRule = @"^([\p{L}\d]|[\p{L}\d][^()]*[^()\s])$";

        /// <summary>
        /// The error text shown when a name does not match <see cref="NameRule" />.
        /// </summary>
        public const string NameRuleErrorText = "The Name must start with a letter or a digit and not contain any parentheses or trailing spaces.";

        /// <summary>
        /// The key of the CDP4-COMET-SDK validation rule to use for a requirement short name. The SDK's
        /// <c>RequirementShortName</c> rule already allows a leading digit (alphanumeric characters and dashes), unlike
        /// the generic "ShortName" rule which forbids one.
        /// </summary>
        public const string ShortNameRuleKey = "RequirementShortName";
    }
}
