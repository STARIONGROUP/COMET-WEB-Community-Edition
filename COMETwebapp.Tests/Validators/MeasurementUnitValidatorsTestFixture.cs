// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MeasurementUnitValidatorsTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Validators
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;
    
    using COMETwebapp.Validators.ReferenceData.MeasurementUnits;

    using NUnit.Framework;

    [TestFixture]
    public class MeasurementUnitValidatorsTestFixture
    {
        private MeasurementUnitValidator measurementUnitValidator;
        private LinearConversionUnitValidator linearConversionUnitValidator;
        private PrefixedUnitValidator prefixedUnitValidator;
        private UnitFactorValidator unitFactorValidator;

        [SetUp]
        public void SetUp()
        {
            var validationService = new ValidationService();
            this.measurementUnitValidator = new MeasurementUnitValidator(validationService);
            this.linearConversionUnitValidator = new LinearConversionUnitValidator(validationService);
            this.prefixedUnitValidator = new PrefixedUnitValidator(validationService);
            this.unitFactorValidator = new UnitFactorValidator(validationService);
        }

        [Test]
        public void VerifyMeasurementUnitValidation()
        {
            var simpleUnit = new SimpleUnit();

            Assert.That(this.measurementUnitValidator.Validate(simpleUnit).IsValid, Is.EqualTo(false));

            simpleUnit = new SimpleUnit()
            {
                Name = "updated Unit",
                ShortName = "updated Unit"
            };

            Assert.That(this.measurementUnitValidator.Validate(simpleUnit).IsValid, Is.EqualTo(false));

            simpleUnit.ShortName = "updatedUnit";
            Assert.That(this.measurementUnitValidator.Validate(simpleUnit).IsValid, Is.EqualTo(true));
        }

        [Test]
        public void VerifyLinearConversionUnitValidation()
        {
            var linearConversionUnit = new LinearConversionUnit();

            Assert.That(this.linearConversionUnitValidator.Validate(linearConversionUnit).IsValid, Is.EqualTo(false));

            linearConversionUnit = new LinearConversionUnit()
            {
                Name = "updated Unit",
                ShortName = "updated Unit"
            };

            Assert.That(this.linearConversionUnitValidator.Validate(linearConversionUnit).IsValid, Is.EqualTo(false));

            linearConversionUnit.ShortName = "updatedUnit";
            Assert.That(this.linearConversionUnitValidator.Validate(linearConversionUnit).IsValid, Is.EqualTo(false));

            linearConversionUnit.ReferenceUnit = new SimpleUnit();
            linearConversionUnit.ConversionFactor = "1.5";
            Assert.That(this.linearConversionUnitValidator.Validate(linearConversionUnit).IsValid, Is.EqualTo(true));
        }

        [Test]
        public void VerifyPrefixedUnitValidation()
        {
            var prefixedUnit = new PrefixedUnit();

            Assert.That(this.prefixedUnitValidator.Validate(prefixedUnit).IsValid, Is.EqualTo(false));

            prefixedUnit = new PrefixedUnit()
            {
                Prefix = new UnitPrefix(){ ShortName = "pre" },
                ReferenceUnit = new SimpleUnit(){ ShortName = "ref" }
            };

            Assert.Multiple(() =>
            {
                Assert.That(this.prefixedUnitValidator.Validate(prefixedUnit).IsValid, Is.EqualTo(true));
                Assert.That(prefixedUnit.ShortName, Is.EqualTo("preref"));
            });
        }

        [Test]
        public void VerifyUnitFactorValidation()
        {
            var unitFactor = new UnitFactor();

            Assert.That(this.unitFactorValidator.Validate(unitFactor).IsValid, Is.EqualTo(false));

            unitFactor = new UnitFactor()
            {
                Unit = new SimpleUnit(),
                Exponent = "x"
            };

            Assert.That(this.unitFactorValidator.Validate(unitFactor).IsValid, Is.EqualTo(true));
        }
    }
}
