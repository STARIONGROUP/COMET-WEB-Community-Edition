// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVRegisterFields.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.VandV
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Utilities.VandV;

    /// <summary>
    /// Builds the field groups and procedure texts shown on the item rows of the V&amp;V register, reading every attribute
    /// through <see cref="VandVQueries" /> so a value the item leaves to its activity is shown as inherited.
    /// </summary>
    public static class VandVRegisterFields
    {
        /// <summary>
        /// The label of the procedure summary field, which also decides whether the steps are shown.
        /// </summary>
        public const string ProcedureField = "Procedure";

        /// <summary>
        /// The labels of the Planning fields.
        /// </summary>
        private static readonly string[] PlanningFields = ["Method", "Stage gate", "Level", "Criticality", "Activity", "Planned", "Acceptance"];

        /// <summary>
        /// The labels of the Execution fields.
        /// </summary>
        private static readonly string[] ExecutionFields = ["Status", "Actual", "Result", "Evidence"];

        /// <summary>
        /// The labels of the Compliance fields.
        /// </summary>
        private static readonly string[] ComplianceFields = ["Compliance", "Close-out", "Closure"];

        /// <summary>
        /// Gets the labels of every field an item can show, in display order, offered in the field filter.
        /// </summary>
        public static IReadOnlyList<string> AllFields { get; } = [..PlanningFields, ProcedureField, ..ExecutionFields, ..ComplianceFields];

        /// <summary>
        /// Gets the fields a view preset selects; empty selects them all, as an empty field filter does.
        /// </summary>
        /// <param name="preset">The <see cref="VandVViewPreset" />.</param>
        /// <returns>The field labels.</returns>
        public static IReadOnlyList<string> GetPresetFields(VandVViewPreset preset)
        {
            return preset switch
            {
                VandVViewPreset.Planning => PlanningFields,
                VandVViewPreset.Procedure => [ProcedureField],
                VandVViewPreset.Execution => ExecutionFields,
                VandVViewPreset.Compliance => ComplianceFields,
                _ => []
            };
        }

        /// <summary>
        /// Gets the label of a view preset.
        /// </summary>
        /// <param name="preset">The <see cref="VandVViewPreset" />.</param>
        /// <returns>The label.</returns>
        public static string GetPresetLabel(VandVViewPreset preset)
        {
            return preset == VandVViewPreset.Register ? "Register (VCD)" : preset.ToString();
        }

        /// <summary>
        /// Gets the field cells of an item, in display order.
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        /// <param name="item">The V&amp;V item.</param>
        /// <param name="selectedFields">The labels of the fields to show; empty shows them all.</param>
        /// <returns>The fields.</returns>
        public static IReadOnlyList<VandVField> GetFields(VandVModel model, Requirement item, IReadOnlyCollection<string> selectedFields)
        {
            bool IsAnySelected(params string[] labels)
            {
                return selectedFields.Count == 0 || labels.Any(selectedFields.Contains);
            }

            IEnumerable<VandVField> fields =
            [
                ..IsAnySelected(PlanningFields) ? GetPlanningFields(model, item) : [],
                ..IsAnySelected(ProcedureField) ? [new VandVField(ProcedureField, GetProcedureSummary(model, item))] : Array.Empty<VandVField>(),
                ..IsAnySelected(ExecutionFields) ? GetExecutionFields(model, item) : [],
                ..IsAnySelected(ComplianceFields) ? GetComplianceFields(model, item) : []
            ];

            return fields.Where(x => selectedFields.Count == 0 || selectedFields.Contains(x.Label)).ToList();
        }

        /// <summary>
        /// Gets the Planning fields of an item: method, stage gate, level, criticality, activity, planned date and
        /// acceptance criteria.
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>The fields, in display order.</returns>
        public static IReadOnlyList<VandVField> GetPlanningFields(VandVModel model, Requirement item)
        {
            var activity = model.GetActivity(item);

            return
            [
                GetField(model, item, "Method", VandVConstants.Method),
                GetField(model, item, "Stage gate", VandVConstants.Stage),
                GetField(model, item, "Level", VandVConstants.Level),
                GetField(model, item, "Criticality", VandVConstants.Criticality),
                new VandVField("Activity", activity?.ShortName ?? VandVQueries.GetValue(item, VandVConstants.ActivityNumber)),
                GetField(model, item, "Planned", VandVConstants.PlannedDate),
                GetField(model, item, "Acceptance", VandVConstants.AcceptanceCriteria)
            ];
        }

        /// <summary>
        /// Gets the Execution fields of an item: status, actual date, result and evidence.
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>The fields, in display order.</returns>
        public static IReadOnlyList<VandVField> GetExecutionFields(VandVModel model, Requirement item)
        {
            return
            [
                GetField(model, item, "Status", VandVConstants.Status),
                GetField(model, item, "Actual", VandVConstants.ActualDate),
                GetField(model, item, "Result", VandVConstants.Result),
                GetField(model, item, "Evidence", VandVConstants.EvidenceReference)
            ];
        }

        /// <summary>
        /// Gets the Compliance fields of an item: compliance, close-out and requirement closure, none of which derive.
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>The fields, in display order.</returns>
        public static IReadOnlyList<VandVField> GetComplianceFields(VandVModel model, Requirement item)
        {
            var closeOut = VandVQueries.IsClosed(item)
                ? $"Closed: {VandVQueries.GetValue(item, VandVConstants.CloseOutReason) ?? "no reason given"}"
                : "Open";

            return
            [
                GetField(model, item, "Compliance", VandVConstants.Compliance),
                new VandVField("Close-out", closeOut),
                GetField(model, item, "Closure", VandVConstants.Closure)
            ];
        }

        /// <summary>
        /// Gets the procedure steps shown under an item: its own steps, or else the steps of the activity performing it.
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>The steps, and whether they are the activity's.</returns>
        public static (IReadOnlyList<Requirement> Steps, bool IsInherited) GetSteps(VandVModel model, Requirement item)
        {
            var own = model.GetSteps(item);

            if (own.Count > 0)
            {
                return (own, false);
            }

            var inherited = model.GetSteps(model.GetActivity(item));
            return (inherited, inherited.Count > 0);
        }

        /// <summary>
        /// Summarises the procedure of an item: for example 1 of 3 step(s) recorded, 1 failed. A step is recorded once its
        /// result is anything but Not Run; nothing is executed by the tool.
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>The summary.</returns>
        public static string GetProcedureSummary(VandVModel model, Requirement item)
        {
            var (steps, isInherited) = GetSteps(model, item);

            if (steps.Count == 0)
            {
                return "No procedure";
            }

            var results = steps.Select(x => VandVQueries.GetValue(x, VandVConstants.StepResult)).ToList();
            var recorded = results.Count(x => x != null && !VandVConstants.ValueEquals(x, VandVConstants.StepNotRun));
            var failed = results.Count(x => VandVConstants.ValueEquals(x, VandVConstants.StepFail));
            var summary = $"{recorded} of {steps.Count} step(s) recorded";

            if (failed > 0)
            {
                summary = $"{summary}, {failed} failed";
            }

            return isInherited ? $"{summary} (from activity)" : summary;
        }

        /// <summary>
        /// Gets the label of a procedure step: its number and action.
        /// </summary>
        /// <param name="step">The procedure step.</param>
        /// <returns>The label.</returns>
        public static string GetStepLabel(Requirement step)
        {
            var number = VandVQueries.GetValue(step, VandVConstants.StepNumber);
            var action = VandVQueries.GetValue(step, VandVConstants.StepAction) ?? step.Name;
            return number == null ? action : $"{number}. {action}";
        }

        /// <summary>
        /// Gets one field of an item, read through its activity where the attribute derives.
        /// </summary>
        /// <param name="model">The <see cref="VandVModel" />.</param>
        /// <param name="item">The V&amp;V item.</param>
        /// <param name="label">The field label.</param>
        /// <param name="parameterTypeShortName">The vnv_* short name.</param>
        /// <returns>The <see cref="VandVField" />.</returns>
        private static VandVField GetField(VandVModel model, Requirement item, string label, string parameterTypeShortName)
        {
            var source = VandVQueries.GetEffectiveSource(model, item, parameterTypeShortName);
            return new VandVField(label, VandVQueries.GetDisplayValue(source, parameterTypeShortName), source == item ? null : source.ShortName);
        }
    }
}
