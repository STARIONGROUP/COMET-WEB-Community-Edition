// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="VandVRegisterScenario.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.VandV
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Tests.Utilities.VandV;
    using COMETwebapp.Utilities.VandV;

    /// <summary>
    /// A small model for the V&amp;V register fixtures:
    /// SPEC holds R1 (covered by I1, passed at CDR, and I2, open at PDR), group G1 with R2 (covered by I3, performed by
    /// ACT_1 which has two steps, one failed) and sub-group G11 with the uncovered R3; SPEC2 holds the uncovered R4.
    /// </summary>
    internal sealed class VandVRegisterScenario
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVRegisterScenario" /> class.
        /// </summary>
        public VandVRegisterScenario()
        {
            const string requirementCategory = VandVTestData.RequirementCategoryShortName;

            this.Specification = this.Data.AddSpecification("SPEC");
            this.Group = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "G1", Name = "Group one" };
            this.SubGroup = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "G11", Name = "Group one one" };
            this.EmptyGroup = new RequirementsGroup { Iid = Guid.NewGuid(), ShortName = "G2", Name = "Empty group" };
            this.Group.Group.Add(this.SubGroup);
            this.Specification.Group.AddRange([this.Group, this.EmptyGroup]);

            this.FirstRequirement = this.Data.AddRequirement(this.Specification, "R1", requirementCategory);
            this.SecondRequirement = this.Data.AddRequirement(this.Specification, "R2", requirementCategory);
            this.SecondRequirement.Group = this.Group;
            this.ThirdRequirement = this.Data.AddRequirement(this.Specification, "R3", requirementCategory);
            this.ThirdRequirement.Group = this.SubGroup;
            this.DeprecatedRequirement = this.Data.AddRequirement(this.Specification, "R0", requirementCategory);
            this.DeprecatedRequirement.IsDeprecated = true;

            this.SecondSpecification = this.Data.AddSpecification("SPEC2");
            this.FourthRequirement = this.Data.AddRequirement(this.SecondSpecification, "R4", requirementCategory);

            var plan = this.Data.AddSpecification(VandVConstants.VandVSpecificationShortName);
            this.PassedItem = this.Data.AddRequirement(plan, "VNV_R1_1", VandVConstants.VnVItemCategory);
            this.OpenItem = this.Data.AddRequirement(plan, "VNV_R1_2", VandVConstants.VnVItemCategory);
            this.PerformedItem = this.Data.AddRequirement(plan, "VNV_R2_1", VandVConstants.VnVItemCategory);

            var report = this.Data.AddSpecification("FAT", VandVConstants.VnVReportCategory);
            this.Activity = this.Data.AddRequirement(report, "ACT_1", VandVConstants.VnVActivityCategory);
            this.PassedStep = this.Data.AddRequirement(report, "ACT_1_S01", VandVConstants.VnVStepCategory);
            this.FailedStep = this.Data.AddRequirement(report, "ACT_1_S02", VandVConstants.VnVStepCategory);

            this.Data.Link(this.PassedItem, this.FirstRequirement, VandVConstants.VerifiesCategory);
            this.Data.Link(this.OpenItem, this.FirstRequirement, VandVConstants.ValidatesCategory);
            this.Data.Link(this.PerformedItem, this.SecondRequirement, VandVConstants.VerifiesCategory);
            this.Data.Link(this.PerformedItem, this.Activity, VandVConstants.PerformedByCategory);
            this.Data.Link(this.Activity, this.PassedStep, VandVConstants.HasStepCategory);
            this.Data.Link(this.Activity, this.FailedStep, VandVConstants.HasStepCategory);

            this.Data.SetValue(this.PassedItem, VandVConstants.Status, "Passed");
            this.Data.SetValue(this.PassedItem, VandVConstants.Stage, "CDR");
            this.Data.SetValue(this.PassedItem, VandVConstants.Closed, "true");
            this.Data.SetValue(this.PassedItem, VandVConstants.CloseOutReason, "Accepted at CDR");
            this.Data.SetValue(this.OpenItem, VandVConstants.Stage, "PDR");
            this.Data.SetValue(this.Activity, VandVConstants.Method, "Test");
            this.Data.SetValue(this.Activity, VandVConstants.Stage, "CDR");
            this.Data.SetValue(this.PassedStep, VandVConstants.StepNumber, "1");
            this.Data.SetValue(this.PassedStep, VandVConstants.StepAction, "Weigh the unit");
            this.Data.SetValue(this.PassedStep, VandVConstants.StepResult, "Pass");
            this.Data.SetValue(this.FailedStep, VandVConstants.StepNumber, "2");
            this.Data.SetValue(this.FailedStep, VandVConstants.StepAction, "Compare with the budget");
            this.Data.SetValue(this.FailedStep, VandVConstants.StepResult, "Fail");
        }

        /// <summary>
        /// Gets the builder holding the scenario.
        /// </summary>
        public VandVTestData Data { get; } = new();

        /// <summary>
        /// Gets SPEC.
        /// </summary>
        public RequirementsSpecification Specification { get; }

        /// <summary>
        /// Gets SPEC2.
        /// </summary>
        public RequirementsSpecification SecondSpecification { get; }

        /// <summary>
        /// Gets G1, in SPEC.
        /// </summary>
        public RequirementsGroup Group { get; }

        /// <summary>
        /// Gets G11, in G1.
        /// </summary>
        public RequirementsGroup SubGroup { get; }

        /// <summary>
        /// Gets G2, in SPEC, without requirements.
        /// </summary>
        public RequirementsGroup EmptyGroup { get; }

        /// <summary>
        /// Gets R1, filed directly in SPEC.
        /// </summary>
        public Requirement FirstRequirement { get; }

        /// <summary>
        /// Gets R2, filed in G1.
        /// </summary>
        public Requirement SecondRequirement { get; }

        /// <summary>
        /// Gets R3, uncovered and filed in G11.
        /// </summary>
        public Requirement ThirdRequirement { get; }

        /// <summary>
        /// Gets R4, uncovered and filed directly in SPEC2.
        /// </summary>
        public Requirement FourthRequirement { get; }

        /// <summary>
        /// Gets the deprecated R0, which is not to be covered.
        /// </summary>
        public Requirement DeprecatedRequirement { get; }

        /// <summary>
        /// Gets the item covering R1 that passed at CDR.
        /// </summary>
        public Requirement PassedItem { get; }

        /// <summary>
        /// Gets the item covering R1 that is open at PDR.
        /// </summary>
        public Requirement OpenItem { get; }

        /// <summary>
        /// Gets the item covering R2, performed by the activity.
        /// </summary>
        public Requirement PerformedItem { get; }

        /// <summary>
        /// Gets ACT_1.
        /// </summary>
        public Requirement Activity { get; }

        /// <summary>
        /// Gets the passed step of the activity.
        /// </summary>
        public Requirement PassedStep { get; }

        /// <summary>
        /// Gets the failed step of the activity.
        /// </summary>
        public Requirement FailedStep { get; }

        /// <summary>
        /// Builds the snapshot of the scenario.
        /// </summary>
        /// <returns>The <see cref="VandVModel" />.</returns>
        public VandVModel BuildModel()
        {
            return this.Data.BuildModel();
        }
    }
}
