// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ScaleValueDefinitionValidatorTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Validators.ReferenceData.MeasurementScales
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    using COMETwebapp.Validators.ReferenceData.MeasurementScales;

    using NUnit.Framework;

    [TestFixture]
    public class ScaleValueDefinitionValidatorTestFixture
    {
        private ScaleValueDefinitionValidator validator;

        [SetUp]
        public void SetUp()
        {
            var validationService = new ValidationService();
            this.validator = new ScaleValueDefinitionValidator(validationService);
        }

        [Test]
        public void VerifyValidationScenarios()
        {
            var scaleValueDefinition = new ScaleValueDefinition();
            Assert.That(this.validator.Validate(scaleValueDefinition).IsValid, Is.EqualTo(false));

            scaleValueDefinition = new ScaleValueDefinition
            {
                Name = "scaleValue1",
                ShortName = "scale value short name",
                Value = "10"
            };

            Assert.That(this.validator.Validate(scaleValueDefinition).IsValid, Is.EqualTo(false));

            scaleValueDefinition.ShortName = "scaleValueShortName";
            Assert.That(this.validator.Validate(scaleValueDefinition).IsValid, Is.EqualTo(true));
        }
    }
}
