// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVConstants.cs" company="Starion Group S.A.">
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
    /// The reserved short names and enumeration values of the Verification and Validation (V&amp;V) reference data. The
    /// COMET IME uses exactly the same short names, which is what keeps the two applications compatible: everything V&amp;V
    /// is found by short name, never by identifier. Never write one of these strings as a literal anywhere else, a
    /// mistyped short name compiles and then silently reads and writes nothing.
    /// </summary>
    public static class VandVConstants
    {
        /// <summary>
        /// The short name of the dedicated <see cref="CDP4Common.EngineeringModelData.RequirementsSpecification" /> that holds
        /// the V&amp;V items, their procedure steps and the activities that belong to no report.
        /// </summary>
        public const string VandVSpecificationShortName = "VNV";

        /// <summary>
        /// The name of the dedicated V&amp;V <see cref="CDP4Common.EngineeringModelData.RequirementsSpecification" />.
        /// </summary>
        public const string VandVSpecificationName = "V&V Plan";

        /// <summary>
        /// The prefix of a suggested item short name, VNV_&lt;requirement&gt;_&lt;n&gt;.
        /// </summary>
        public const string ItemShortNamePrefix = "VNV";

        /// <summary>
        /// The requirement stem used in a suggested item short name when the requirement has no short name.
        /// </summary>
        public const string FallbackRequirementStem = "REQ";

        /// <summary>
        /// The prefix of a suggested activity short name (activity number), ACT_&lt;n&gt;.
        /// </summary>
        public const string ActivityShortNamePrefix = "ACT";

        /// <summary>
        /// The category identifying a requirement as a V&amp;V item.
        /// </summary>
        public const string VnVItemCategory = "VnVItem";

        /// <summary>
        /// The sub-category of <see cref="VnVItemCategory" /> identifying a verification item (seeded, never applied).
        /// </summary>
        public const string VerificationItemCategory = "VerificationItem";

        /// <summary>
        /// The sub-category of <see cref="VnVItemCategory" /> identifying a validation item (seeded, never applied).
        /// </summary>
        public const string ValidationItemCategory = "ValidationItem";

        /// <summary>
        /// The category identifying a requirement as a procedure step of a V&amp;V item or activity.
        /// </summary>
        public const string VnVStepCategory = "VnVStep";

        /// <summary>
        /// The category identifying a requirement as a shared V&amp;V activity.
        /// </summary>
        public const string VnVActivityCategory = "VnVActivity";

        /// <summary>
        /// The category identifying a <see cref="CDP4Common.EngineeringModelData.RequirementsSpecification" /> as a V&amp;V
        /// report.
        /// </summary>
        public const string VnVReportCategory = "VnVReport";

        /// <summary>
        /// The category marking a <see cref="CDP4Common.ReportingData.ReviewItemDiscrepancy" /> as a non-conformance report
        /// (seeded, never applied).
        /// </summary>
        public const string NcrCategory = "NCR";

        /// <summary>
        /// The relationship category from a V&amp;V item to the requirement it verifies.
        /// </summary>
        public const string VerifiesCategory = "verifies";

        /// <summary>
        /// The relationship category from a V&amp;V item to the requirement it validates.
        /// </summary>
        public const string ValidatesCategory = "validates";

        /// <summary>
        /// The relationship category from a V&amp;V item to an <see cref="CDP4Common.EngineeringModelData.Option" /> it covers.
        /// </summary>
        public const string CoversOptionCategory = "coversOption";

        /// <summary>
        /// The relationship category from a V&amp;V item to an <see cref="CDP4Common.EngineeringModelData.ActualFiniteState" />
        /// it covers.
        /// </summary>
        public const string CoversStateCategory = "coversState";

        /// <summary>
        /// The relationship category from a V&amp;V item to the parameter or parameter override it measures.
        /// </summary>
        public const string CoversParameterCategory = "coversParameter";

        /// <summary>
        /// The relationship category from a V&amp;V item to the <see cref="CDP4Common.EngineeringModelData.ElementDefinition" />
        /// it is verified on.
        /// </summary>
        public const string VerifiedOnCategory = "verifiedOn";

        /// <summary>
        /// The relationship category from a V&amp;V item or activity to one of its procedure steps.
        /// </summary>
        public const string HasStepCategory = "hasStep";

        /// <summary>
        /// The relationship category from a V&amp;V item to the shared activity that performs it.
        /// </summary>
        public const string PerformedByCategory = "performedBy";

        /// <summary>
        /// The short name of the method attribute (Inspection, Analysis, ...).
        /// </summary>
        public const string Method = "vnv_method";

        /// <summary>
        /// The short name of the stage gate attribute; the order of its enumeration values is the project's review order.
        /// </summary>
        public const string Stage = "vnv_stage";

        /// <summary>
        /// The short name of the integration level attribute.
        /// </summary>
        public const string Level = "vnv_level";

        /// <summary>
        /// The short name of the legacy activity number attribute; an activity's number is now its short name.
        /// </summary>
        public const string ActivityNumber = "vnv_activity_no";

        /// <summary>
        /// The short name of the activity description attribute.
        /// </summary>
        public const string Description = "vnv_description";

        /// <summary>
        /// The short name of the preconditions attribute.
        /// </summary>
        public const string Preconditions = "vnv_preconditions";

        /// <summary>
        /// The short name of the conditions attribute.
        /// </summary>
        public const string Conditions = "vnv_conditions";

        /// <summary>
        /// The short name of the acceptance criteria attribute.
        /// </summary>
        public const string AcceptanceCriteria = "vnv_acceptance";

        /// <summary>
        /// The short name of the facility attribute.
        /// </summary>
        public const string Facility = "vnv_facility";

        /// <summary>
        /// The short name of the external responsible attribute.
        /// </summary>
        public const string ExternalResponsible = "vnv_responsible_ext";

        /// <summary>
        /// The short name of the planned date attribute.
        /// </summary>
        public const string PlannedDate = "vnv_planned_date";

        /// <summary>
        /// The short name of the criticality attribute.
        /// </summary>
        public const string Criticality = "vnv_criticality";

        /// <summary>
        /// The short name of the coverage note attribute.
        /// </summary>
        public const string CoverageNote = "vnv_coverage_note";

        /// <summary>
        /// The short name of the execution status attribute: did the activity happen, and what did it show?
        /// </summary>
        public const string Status = "vnv_status";

        /// <summary>
        /// The short name of the actual execution date attribute.
        /// </summary>
        public const string ActualDate = "vnv_actual_date";

        /// <summary>
        /// The short name of the execution result attribute.
        /// </summary>
        public const string Result = "vnv_result";

        /// <summary>
        /// The short name of the evidence reference attribute.
        /// </summary>
        public const string EvidenceReference = "vnv_evidence_ref";

        /// <summary>
        /// The short name of the compliance attribute: does the design meet the requirement?
        /// </summary>
        public const string Compliance = "vnv_compliance";

        /// <summary>
        /// The short name of the close-out flag attribute: has the work on this item been accepted?
        /// </summary>
        public const string Closed = "vnv_closed";

        /// <summary>
        /// The short name of the requirement closure attribute: is the requirement finished, or is more V&amp;V owed later?
        /// </summary>
        public const string Closure = "vnv_closure";

        /// <summary>
        /// The short name of the close-out reason attribute.
        /// </summary>
        public const string CloseOutReason = "vnv_closeout_reason";

        /// <summary>
        /// The short name of the closed-by attribute.
        /// </summary>
        public const string ClosedBy = "vnv_closed_by";

        /// <summary>
        /// The short name of the closed-on date attribute.
        /// </summary>
        public const string ClosedOn = "vnv_closed_on";

        /// <summary>
        /// The short name of the plan reference attribute.
        /// </summary>
        public const string PlanReference = "vnv_plan_ref";

        /// <summary>
        /// The short name of the procedure reference attribute.
        /// </summary>
        public const string ProcedureReference = "vnv_procedure_ref";

        /// <summary>
        /// The short name of the step number attribute of a procedure step.
        /// </summary>
        public const string StepNumber = "vnv_step_no";

        /// <summary>
        /// The short name of the step action attribute of a procedure step.
        /// </summary>
        public const string StepAction = "vnv_step_action";

        /// <summary>
        /// The short name of the expected result attribute of a procedure step.
        /// </summary>
        public const string StepExpectedResult = "vnv_step_expected";

        /// <summary>
        /// The short name of the actual (as-run) result attribute of a procedure step.
        /// </summary>
        public const string StepActualResult = "vnv_step_actual";

        /// <summary>
        /// The short name of the result attribute of a procedure step.
        /// </summary>
        public const string StepResult = "vnv_step_result";

        /// <summary>
        /// The <see cref="Status" /> value Planned.
        /// </summary>
        public const string StatusPlanned = "Planned";

        /// <summary>
        /// The <see cref="Status" /> value Ready.
        /// </summary>
        public const string StatusReady = "Ready";

        /// <summary>
        /// The <see cref="Status" /> value In Progress.
        /// </summary>
        public const string StatusInProgress = "In Progress";

        /// <summary>
        /// The <see cref="Status" /> value Executed.
        /// </summary>
        public const string StatusExecuted = "Executed";

        /// <summary>
        /// The <see cref="Status" /> value Passed.
        /// </summary>
        public const string StatusPassed = "Passed";

        /// <summary>
        /// The <see cref="Status" /> value Failed.
        /// </summary>
        public const string StatusFailed = "Failed";

        /// <summary>
        /// The <see cref="Status" /> value Waived.
        /// </summary>
        public const string StatusWaived = "Waived";

        /// <summary>
        /// The <see cref="Status" /> value Deviated.
        /// </summary>
        public const string StatusDeviated = "Deviated";

        /// <summary>
        /// The Not Applicable value, shared by <see cref="Status" />, <see cref="Compliance" /> and <see cref="StepResult" />.
        /// </summary>
        public const string NotApplicable = "Not Applicable";

        /// <summary>
        /// The <see cref="Status" /> value Cancelled.
        /// </summary>
        public const string StatusCancelled = "Cancelled";

        /// <summary>
        /// The Not Assessed value, shared by <see cref="Compliance" /> and <see cref="Closure" />, and the value a missing
        /// compliance or closure reads as.
        /// </summary>
        public const string NotAssessed = "Not Assessed";

        /// <summary>
        /// The <see cref="Compliance" /> value Compliant.
        /// </summary>
        public const string ComplianceCompliant = "Compliant";

        /// <summary>
        /// The <see cref="Compliance" /> value Partially Compliant.
        /// </summary>
        public const string CompliancePartiallyCompliant = "Partially Compliant";

        /// <summary>
        /// The <see cref="Compliance" /> value Non-Compliant.
        /// </summary>
        public const string ComplianceNonCompliant = "Non-Compliant";

        /// <summary>
        /// The <see cref="Closure" /> value stating that nothing further is owed for the requirement.
        /// </summary>
        public const string ClosureClosesOut = "Closes Out Requirement";

        /// <summary>
        /// The <see cref="Closure" /> value stating that another item is owed at a later stage gate.
        /// </summary>
        public const string ClosureFurtherRequired = "Further V&V Required";

        /// <summary>
        /// The <see cref="StepResult" /> value Not Run.
        /// </summary>
        public const string StepNotRun = "Not Run";

        /// <summary>
        /// The <see cref="StepResult" /> value Pass.
        /// </summary>
        public const string StepPass = "Pass";

        /// <summary>
        /// The <see cref="StepResult" /> value Fail.
        /// </summary>
        public const string StepFail = "Fail";

        /// <summary>
        /// The <see cref="StepResult" /> value Blocked.
        /// </summary>
        public const string StepBlocked = "Blocked";

        /// <summary>
        /// The values of <see cref="Method" />, the ECSS-E-ST-10-02 verification methods.
        /// </summary>
        public static readonly IReadOnlyList<string> MethodValues = ["Inspection", "Analysis", "Similarity", "Demonstration", "Test", "Review of Design"];

        /// <summary>
        /// The example stage gates offered at set-up time in a typical review order, replaced by the user's own list.
        /// </summary>
        public static readonly IReadOnlyList<string> DefaultStageGates = ["SRR", "PDR", "CDR", "TRR", "FAT", "HAT", "SAT", "ORR", "In-Service"];

        /// <summary>
        /// The values of <see cref="Level" />.
        /// </summary>
        public static readonly IReadOnlyList<string> LevelValues = ["Equipment", "Subsystem", "System", "System-of-Systems", "Operational"];

        /// <summary>
        /// The values of <see cref="Criticality" />.
        /// </summary>
        public static readonly IReadOnlyList<string> CriticalityValues = ["Deployment", "Operation", "Mission-critical"];

        /// <summary>
        /// The values of <see cref="Status" />, Planned first so a new item defaults to it.
        /// </summary>
        public static readonly IReadOnlyList<string> StatusValues =
        [
            StatusPlanned, StatusReady, StatusInProgress, StatusExecuted, StatusPassed, StatusFailed, StatusWaived, StatusDeviated, NotApplicable, StatusCancelled
        ];

        /// <summary>
        /// The values of <see cref="Compliance" />, Not Assessed first so a new item never claims a compliance nobody judged.
        /// </summary>
        public static readonly IReadOnlyList<string> ComplianceValues = [NotAssessed, ComplianceCompliant, CompliancePartiallyCompliant, ComplianceNonCompliant, NotApplicable];

        /// <summary>
        /// The values of <see cref="Closure" />.
        /// </summary>
        public static readonly IReadOnlyList<string> ClosureValues = [NotAssessed, ClosureClosesOut, ClosureFurtherRequired];

        /// <summary>
        /// The values of <see cref="StepResult" />.
        /// </summary>
        public static readonly IReadOnlyList<string> StepResultValues = [StepNotRun, StepPass, StepFail, StepBlocked, NotApplicable];

        /// <summary>
        /// The <see cref="Status" /> values that need no further action: an item in one of them counts as passed.
        /// </summary>
        public static readonly IReadOnlyList<string> ClosedPositiveStatuses = [StatusPassed, StatusWaived, StatusDeviated, NotApplicable];

        /// <summary>
        /// The <see cref="Status" /> values stating that the activity has concluded, so a result is owed.
        /// </summary>
        public static readonly IReadOnlyList<string> ConcludedStatuses = [StatusPassed, StatusFailed, StatusWaived, StatusDeviated, NotApplicable];

        /// <summary>
        /// The <see cref="Compliance" /> values that are a shortfall against the requirement.
        /// </summary>
        public static readonly IReadOnlyList<string> ShortfallCompliances = [CompliancePartiallyCompliant, ComplianceNonCompliant];

        /// <summary>
        /// The attributes an item reads from the activity that performs it when it leaves them empty. Acceptance criteria,
        /// criticality, compliance and every close-out attribute are deliberately absent: they are per requirement.
        /// </summary>
        public static readonly IReadOnlyList<string> DerivedFromActivity = [Method, Stage, Level, Status, PlannedDate, ActualDate, Result, EvidenceReference];

        /// <summary>
        /// The relationship categories that make a requirement count as covered by a V&amp;V item.
        /// </summary>
        public static readonly IReadOnlyList<string> CoverageLinkCategories = [VerifiesCategory, ValidatesCategory];

        /// <summary>
        /// The relationship categories that narrow an item to what it actually measures.
        /// </summary>
        public static readonly IReadOnlyList<string> MeasurementLinkCategories = [CoversParameterCategory, CoversOptionCategory, CoversStateCategory, VerifiedOnCategory];

        /// <summary>
        /// The categories that must exist before a V&amp;V item can be written, because one item write may touch all of them.
        /// </summary>
        public static readonly IReadOnlyList<string> RequiredForItemWrite =
        [
            VnVItemCategory, VerifiesCategory, ValidatesCategory, CoversParameterCategory, CoversOptionCategory, CoversStateCategory,
            VerifiedOnCategory, VnVStepCategory, HasStepCategory, VnVActivityCategory, PerformedByCategory, VnVReportCategory
        ];

        /// <summary>
        /// Normalises an enumeration value for comparison: only letters and digits are kept, lower-cased. The stock
        /// parameter value editor stores a value definition's short name (Non_Compliant, Further_VV_Required) while the
        /// V&amp;V editors store its name (Non-Compliant, Further V&amp;V Required), so both spellings must compare equal.
        /// </summary>
        /// <param name="value">The raw stored or declared value.</param>
        /// <returns>The normalised value, empty for a null or blank input.</returns>
        public static string NormalizeValue(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : string.Concat(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant));
        }

        /// <summary>
        /// Determines whether two enumeration values denote the same value definition (see <see cref="NormalizeValue" />).
        /// Always use this instead of <c>==</c> on an enumeration attribute.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <returns>true when the two values match, including when both are blank.</returns>
        public static bool ValueEquals(string left, string right)
        {
            return NormalizeValue(left) == NormalizeValue(right);
        }

        /// <summary>
        /// Derives a valid short name from a human readable name exactly as the COMET IME does (enumeration value
        /// definitions, item short name stems): spaces and hyphens become an underscore, letters, digits and underscores are
        /// kept, anything else is dropped.
        /// </summary>
        /// <param name="name">The human readable name.</param>
        /// <returns>The derived short name, empty for a null or blank input.</returns>
        public static string ToShortName(string name)
        {
            return string.IsNullOrWhiteSpace(name)
                ? string.Empty
                : string.Concat(name.Where(x => x is ' ' or '-' or '_' || char.IsLetterOrDigit(x)).Select(x => x is ' ' or '-' ? '_' : x));
        }
    }
}
