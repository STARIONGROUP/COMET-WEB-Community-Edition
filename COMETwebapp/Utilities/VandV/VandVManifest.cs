// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVManifest.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The single declaration of every parameter type, category and rule the V&amp;V capability needs, identical to the COMET
    /// IME's manifest so both applications seed and recognise the same reference data. The check here and the seeder are
    /// plain functions over these lists: adding an attribute is one list entry.
    /// </summary>
    public static class VandVManifest
    {
        /// <summary>
        /// The name by which the model's requirement category is recognised when no short name matches.
        /// </summary>
        public const string RequirementCategoryName = "Requirement";

        /// <summary>
        /// The short names, compared case-insensitively, by which the model's own requirement category is recognised, in
        /// order of preference.
        /// </summary>
        public static readonly IReadOnlyList<string> RequirementCategoryShortNames = ["REQUIREMENT", "REQ"];

        /// <summary>
        /// Every V&amp;V parameter type, in the IME's declaration order.
        /// </summary>
        public static readonly IReadOnlyList<VandVParameterTypeDefinition> ParameterTypes =
        [
            new(VandVConstants.Method, "V&V Method", VandVParameterKind.Enumeration, VandVConstants.MethodValues),
            new(VandVConstants.Stage, "V&V Stage Gate", VandVParameterKind.Enumeration, VandVConstants.DefaultStageGates),
            new(VandVConstants.Level, "V&V Integration Level", VandVParameterKind.Enumeration, VandVConstants.LevelValues),
            new(VandVConstants.ActivityNumber, "V&V Activity Number", VandVParameterKind.Text, []),
            new(VandVConstants.Description, "V&V Activity Description", VandVParameterKind.Text, []),
            new(VandVConstants.Preconditions, "V&V Preconditions", VandVParameterKind.Text, []),
            new(VandVConstants.Conditions, "V&V Conditions", VandVParameterKind.Text, []),
            new(VandVConstants.AcceptanceCriteria, "V&V Acceptance Criteria", VandVParameterKind.Text, []),
            new(VandVConstants.Facility, "V&V Facility", VandVParameterKind.Text, []),
            new(VandVConstants.ExternalResponsible, "V&V External Responsible", VandVParameterKind.Text, []),
            new(VandVConstants.PlannedDate, "V&V Planned Date", VandVParameterKind.Date, []),
            new(VandVConstants.Criticality, "V&V Criticality", VandVParameterKind.Enumeration, VandVConstants.CriticalityValues),
            new(VandVConstants.CoverageNote, "V&V Coverage Note", VandVParameterKind.Text, []),
            new(VandVConstants.Status, "V&V Status", VandVParameterKind.Enumeration, VandVConstants.StatusValues),
            new(VandVConstants.ActualDate, "V&V Actual Date", VandVParameterKind.Date, []),
            new(VandVConstants.Result, "V&V Result", VandVParameterKind.Text, []),
            new(VandVConstants.EvidenceReference, "V&V Evidence Reference", VandVParameterKind.Text, []),
            new(VandVConstants.Compliance, "V&V Compliance Status", VandVParameterKind.Enumeration, VandVConstants.ComplianceValues),
            new(VandVConstants.Closed, "V&V Closed", VandVParameterKind.Boolean, []),
            new(VandVConstants.Closure, "V&V Requirement Closure", VandVParameterKind.Enumeration, VandVConstants.ClosureValues),
            new(VandVConstants.CloseOutReason, "V&V Close-out Reason", VandVParameterKind.Text, []),
            new(VandVConstants.ClosedBy, "V&V Closed By", VandVParameterKind.Text, []),
            new(VandVConstants.ClosedOn, "V&V Closed On", VandVParameterKind.Date, []),
            new(VandVConstants.PlanReference, "V&V Plan Reference", VandVParameterKind.Text, []),
            new(VandVConstants.ProcedureReference, "V&V Procedure Reference", VandVParameterKind.Text, []),
            new(VandVConstants.StepNumber, "V&V Step Number", VandVParameterKind.Text, []),
            new(VandVConstants.StepAction, "V&V Step Action", VandVParameterKind.Text, []),
            new(VandVConstants.StepExpectedResult, "V&V Step Expected Result", VandVParameterKind.Text, []),
            new(VandVConstants.StepActualResult, "V&V Step Actual Result", VandVParameterKind.Text, []),
            new(VandVConstants.StepResult, "V&V Step Result", VandVParameterKind.Enumeration, VandVConstants.StepResultValues)
        ];

        /// <summary>
        /// Every V&amp;V category, in the IME's declaration order.
        /// </summary>
        public static readonly IReadOnlyList<VandVCategoryDefinition> Categories =
        [
            new(VandVConstants.VnVItemCategory, "VnV Item", [ClassKind.Requirement]),
            new(VandVConstants.VerificationItemCategory, "Verification Item", [ClassKind.Requirement], VandVConstants.VnVItemCategory),
            new(VandVConstants.ValidationItemCategory, "Validation Item", [ClassKind.Requirement], VandVConstants.VnVItemCategory),
            new(VandVConstants.NcrCategory, "NCR", [ClassKind.ReviewItemDiscrepancy]),
            new(VandVConstants.VnVStepCategory, "VnV Procedure Step", [ClassKind.Requirement]),
            new(VandVConstants.VnVActivityCategory, "VnV Activity", [ClassKind.Requirement]),
            new(VandVConstants.VnVReportCategory, "VnV Report", [ClassKind.RequirementsSpecification]),
            new(VandVConstants.VerifiesCategory, "verifies", [ClassKind.BinaryRelationship]),
            new(VandVConstants.ValidatesCategory, "validates", [ClassKind.BinaryRelationship]),
            new(VandVConstants.CoversOptionCategory, "covers option", [ClassKind.BinaryRelationship]),
            new(VandVConstants.CoversStateCategory, "covers state", [ClassKind.BinaryRelationship]),
            new(VandVConstants.CoversParameterCategory, "covers parameter", [ClassKind.BinaryRelationship]),
            new(VandVConstants.VerifiedOnCategory, "verified on", [ClassKind.BinaryRelationship]),
            new(VandVConstants.HasStepCategory, "has step", [ClassKind.BinaryRelationship]),
            new(VandVConstants.PerformedByCategory, "performed by", [ClassKind.BinaryRelationship])
        ];

        /// <summary>
        /// One parameterized category rule per V&amp;V requirement kind, each mandating exactly what its creation always
        /// writes, so incomplete things also surface in the stock rule verification.
        /// </summary>
        public static readonly IReadOnlyList<VandVParameterizedCategoryRuleDefinition> ParameterizedCategoryRules =
        [
            new("VnVItemAttributesRule", "V&V Item mandatory attributes", VandVConstants.VnVItemCategory,
                [VandVConstants.Method, VandVConstants.Stage, VandVConstants.AcceptanceCriteria, VandVConstants.Status]),
            new("VnVActivityAttributesRule", "V&V Activity mandatory attributes", VandVConstants.VnVActivityCategory,
                [VandVConstants.Method, VandVConstants.Stage]),
            new("VnVStepAttributesRule", "V&V Procedure Step mandatory attributes", VandVConstants.VnVStepCategory,
                [VandVConstants.StepNumber])
        ];

        /// <summary>
        /// The binary relationship rules governing the coverage links, from a V&amp;V item to the model's requirement category.
        /// </summary>
        public static readonly IReadOnlyList<VandVBinaryRelationshipRuleDefinition> BinaryRelationshipRules =
        [
            new("verifiesRule", "verifies (V&V item verifies a requirement)", VandVConstants.VerifiesCategory, "verifies", "is verified by", VandVConstants.VnVItemCategory),
            new("validatesRule", "validates (V&V item validates a requirement)", VandVConstants.ValidatesCategory, "validates", "is validated by", VandVConstants.VnVItemCategory)
        ];

        /// <summary>
        /// Checks the given chain of reference data libraries against the manifest, by short name only, so anything defined
        /// anywhere in the chain (a site library included) is reused rather than reported missing. As in the IME, a deprecated
        /// thing counts as present: re-seeding its short name would collide, so it must be un-deprecated instead.
        /// </summary>
        /// <param name="referenceDataLibraries">The model's library and every library it requires.</param>
        /// <returns>The <see cref="VandVReferenceDataCheck" />.</returns>
        public static VandVReferenceDataCheck Check(IEnumerable<ReferenceDataLibrary> referenceDataLibraries)
        {
            var libraries = referenceDataLibraries?.ToList() ?? [];
            var parameterTypes = libraries.SelectMany(x => x.ParameterType).Select(x => x.ShortName).ToHashSet(StringComparer.Ordinal);
            var categories = libraries.SelectMany(x => x.DefinedCategory).Select(x => x.ShortName).ToHashSet(StringComparer.Ordinal);
            var rules = libraries.SelectMany(x => x.Rule).Select(x => x.ShortName).ToHashSet(StringComparer.Ordinal);

            return new VandVReferenceDataCheck
            {
                MissingParameterTypes = ParameterTypes.Where(x => !parameterTypes.Contains(x.ShortName)).ToList(),
                MissingCategories = Categories.Where(x => !categories.Contains(x.ShortName)).ToList(),
                MissingParameterizedCategoryRules = ParameterizedCategoryRules.Where(x => !rules.Contains(x.ShortName)).ToList(),
                MissingBinaryRelationshipRules = BinaryRelationshipRules.Where(x => !rules.Contains(x.ShortName)).ToList(),
                RequirementCategory = ResolveRequirementCategory(libraries)
            };
        }

        /// <summary>
        /// Resolves the model's requirement category: a non-deprecated category permissible on requirements whose short name
        /// is REQUIREMENT, else REQ (case-insensitive), else whose name is Requirement.
        /// </summary>
        /// <param name="referenceDataLibraries">The model's library and every library it requires.</param>
        /// <returns>The requirement <see cref="Category" />, or null when none matches.</returns>
        public static Category ResolveRequirementCategory(IEnumerable<ReferenceDataLibrary> referenceDataLibraries)
        {
            var candidates = (referenceDataLibraries ?? [])
                .SelectMany(x => x.DefinedCategory)
                .Where(x => !x.IsDeprecated && x.PermissibleClass.Contains(ClassKind.Requirement))
                .ToList();

            return RequirementCategoryShortNames
                       .Select(shortName => candidates.FirstOrDefault(x => string.Equals(x.ShortName, shortName, StringComparison.OrdinalIgnoreCase)))
                       .FirstOrDefault(x => x != null)
                   ?? candidates.FirstOrDefault(x => string.Equals(x.Name, RequirementCategoryName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
