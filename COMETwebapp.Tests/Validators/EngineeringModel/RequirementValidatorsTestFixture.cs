// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementValidatorsTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Validators.EngineeringModel
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Validation;

    using COMETwebapp.Validators.EngineeringModel;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementValidatorsTestFixture
    {
        private ValidationService validationService;

        [SetUp]
        public void SetUp()
        {
            this.validationService = new ValidationService();
        }

        [Test]
        public void VerifyRequirementValidation()
        {
            var validator = new RequirementValidator(this.validationService);

            Assert.Multiple(() =>
            {
                Assert.That(validator.Validate(new Requirement()).IsValid, Is.False, "An empty requirement is invalid.");
                Assert.That(validator.Validate(new Requirement { Name = "First", ShortName = "1REQ" }).IsValid, Is.False, "A short name starting with a digit is invalid.");
                Assert.That(validator.Validate(new Requirement { Name = "First", ShortName = "REQ1" }).IsValid, Is.True);
            });
        }

        [Test]
        public void VerifyRequirementsGroupValidation()
        {
            var validator = new RequirementsGroupValidator(this.validationService);

            Assert.Multiple(() =>
            {
                Assert.That(validator.Validate(new RequirementsGroup()).IsValid, Is.False);
                Assert.That(validator.Validate(new RequirementsGroup { Name = "Group", ShortName = "1GRP" }).IsValid, Is.False);
                Assert.That(validator.Validate(new RequirementsGroup { Name = "Group", ShortName = "GRP" }).IsValid, Is.True);
            });
        }

        [Test]
        public void VerifyRequirementsSpecificationValidation()
        {
            var validator = new RequirementsSpecificationValidator(this.validationService);

            Assert.Multiple(() =>
            {
                Assert.That(validator.Validate(new RequirementsSpecification()).IsValid, Is.False);
                Assert.That(validator.Validate(new RequirementsSpecification { Name = "Spec", ShortName = "1SPEC" }).IsValid, Is.False);
                Assert.That(validator.Validate(new RequirementsSpecification { Name = "Spec", ShortName = "SPEC" }).IsValid, Is.True);
            });
        }
    }
}
