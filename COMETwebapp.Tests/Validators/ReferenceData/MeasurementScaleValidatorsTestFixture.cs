// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MeasurementScaleValidatorsTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Tests.Validators.ReferenceData
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    using COMETwebapp.Validators.ReferenceData.MeasurementScales;

    using NUnit.Framework;

    [TestFixture]
    public class MeasurementScaleValidatorsTestFixture
    {
        private MeasurementScaleValidator measurementScaleValidator;
        private CyclicRatioScaleValidator cyclicRatioScaleValidator;
        private LogarithmicScaleValidator logarithmicScaleValidator;
        private OrdinalScaleValidator ordinalScaleValidator;

        [SetUp]
        public void SetUp()
        {
            var validationService = new ValidationService();
            this.measurementScaleValidator = new MeasurementScaleValidator(validationService);
            this.cyclicRatioScaleValidator = new CyclicRatioScaleValidator(validationService);
            this.logarithmicScaleValidator = new LogarithmicScaleValidator(validationService);
            this.ordinalScaleValidator = new OrdinalScaleValidator(validationService);
        }

        [Test]
        public void VerifyCyclicRatioScaleValidation()
        {
            var cyclicRatio = new CyclicRatioScale();
            Assert.That(this.cyclicRatioScaleValidator.Validate(cyclicRatio).IsValid, Is.EqualTo(false));

            cyclicRatio = new CyclicRatioScale
            {
                Name = "updated scale",
                ShortName = "updated scale"
            };

            Assert.That(this.cyclicRatioScaleValidator.Validate(cyclicRatio).IsValid, Is.EqualTo(false));
            cyclicRatio.ShortName = "updatedScale";
            Assert.That(this.cyclicRatioScaleValidator.Validate(cyclicRatio).IsValid, Is.EqualTo(false));
            cyclicRatio.Modulus = "modulus";
            Assert.That(this.cyclicRatioScaleValidator.Validate(cyclicRatio).IsValid, Is.EqualTo(true));
        }

        [Test]
        public void VerifyLogarithmicScaleValidation()
        {
            var logarithmicScale = new LogarithmicScale();
            Assert.That(this.logarithmicScaleValidator.Validate(logarithmicScale).IsValid, Is.EqualTo(false));

            logarithmicScale = new LogarithmicScale
            {
                Name = "updated scale",
                ShortName = "updated scale"
            };

            Assert.That(this.logarithmicScaleValidator.Validate(logarithmicScale).IsValid, Is.EqualTo(false));
            logarithmicScale.ShortName = "updatedScale";
            Assert.That(this.logarithmicScaleValidator.Validate(logarithmicScale).IsValid, Is.EqualTo(false));
            logarithmicScale.LogarithmBase = LogarithmBaseKind.NATURAL;
            logarithmicScale.Exponent = "2";
            logarithmicScale.Factor = "3";
            Assert.That(this.logarithmicScaleValidator.Validate(logarithmicScale).IsValid, Is.EqualTo(true));
        }

        [Test]
        public void VerifyMeasurementScaleValidation()
        {
            var intervalScale = new IntervalScale();
            Assert.That(this.measurementScaleValidator.Validate(intervalScale).IsValid, Is.EqualTo(false));

            intervalScale = new IntervalScale
            {
                Name = "updated scale",
                ShortName = "updated scale"
            };

            Assert.That(this.measurementScaleValidator.Validate(intervalScale).IsValid, Is.EqualTo(false));
            intervalScale.ShortName = "updatedScale";
            Assert.That(this.measurementScaleValidator.Validate(intervalScale).IsValid, Is.EqualTo(true));
        }

        [Test]
        public void VerifyOrdinalScaleValidation()
        {
            var ordinalScale = new OrdinalScale();
            Assert.That(this.ordinalScaleValidator.Validate(ordinalScale).IsValid, Is.EqualTo(false));

            ordinalScale = new OrdinalScale
            {
                Name = "updated scale",
                ShortName = "updated scale"
            };

            Assert.That(this.ordinalScaleValidator.Validate(ordinalScale).IsValid, Is.EqualTo(false));
            ordinalScale.ShortName = "updatedScale";
            Assert.That(this.ordinalScaleValidator.Validate(ordinalScale).IsValid, Is.EqualTo(false));
            ordinalScale.UseShortNameValues = true;
            Assert.That(this.ordinalScaleValidator.Validate(ordinalScale).IsValid, Is.EqualTo(true));
        }
    }
}
