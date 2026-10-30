// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVReferenceDataCheck.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The outcome of checking a chain of reference data libraries against the <see cref="VandVManifest" />: what is absent,
    /// and the requirement category the relationship rules target.
    /// </summary>
    public sealed class VandVReferenceDataCheck
    {
        /// <summary>
        /// Gets the parameter types absent from the chain.
        /// </summary>
        public IReadOnlyList<VandVParameterTypeDefinition> MissingParameterTypes { get; init; } = [];

        /// <summary>
        /// Gets the categories absent from the chain.
        /// </summary>
        public IReadOnlyList<VandVCategoryDefinition> MissingCategories { get; init; } = [];

        /// <summary>
        /// Gets the parameterized category rules absent from the chain.
        /// </summary>
        public IReadOnlyList<VandVParameterizedCategoryRuleDefinition> MissingParameterizedCategoryRules { get; init; } = [];

        /// <summary>
        /// Gets the binary relationship rules absent from the chain.
        /// </summary>
        public IReadOnlyList<VandVBinaryRelationshipRuleDefinition> MissingBinaryRelationshipRules { get; init; } = [];

        /// <summary>
        /// Gets the model's requirement <see cref="Category" /> the binary relationship rules target, or null when none
        /// could be resolved. Those rules still appear in <see cref="MissingBinaryRelationshipRules" /> but cannot be created,
        /// so <see cref="IsComplete" /> ignores them; check this property to tell the user why.
        /// </summary>
        public Category RequirementCategory { get; init; }

        /// <summary>
        /// Gets a value indicating whether nothing that can be created is missing.
        /// </summary>
        public bool IsComplete => this.MissingParameterTypes.Count == 0
                                  && this.MissingCategories.Count == 0
                                  && this.MissingParameterizedCategoryRules.Count == 0
                                  && (this.RequirementCategory == null || this.MissingBinaryRelationshipRules.Count == 0);

        /// <summary>
        /// Gets a value indicating whether V&amp;V items, activities and steps can be written: every parameter type and every
        /// category in <see cref="VandVConstants.RequiredForItemWrite" /> exists. Writing an attribute whose parameter type
        /// cannot be resolved would silently drop what the user typed, so writes are refused until this holds.
        /// </summary>
        public bool CanWriteItems => this.MissingParameterTypes.Count == 0
                                     && !this.MissingCategories.Any(x => VandVConstants.RequiredForItemWrite.Contains(x.ShortName));
    }
}
