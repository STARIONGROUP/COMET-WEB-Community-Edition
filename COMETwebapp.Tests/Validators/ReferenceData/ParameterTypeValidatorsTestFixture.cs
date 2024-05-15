// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeValidatorsTestFixture.cs" company="Starion Group S.A.">
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

    using COMETwebapp.Validators.ReferenceData.ParameterTypes;
    using COMETwebapp.Validators.ReferenceData.ParameterTypes.GenericValidators;

    using NUnit.Framework;

    [TestFixture]
    public class ParameterTypeValidatorsTestFixture
    {
        private ParameterTypeValidator parameterTypeValidator;
        private ParameterQuantityKindValidator quantityKindValidator;
        private CompoundParameterTypeValidator compoundParameterTypeValidator;
        private DerivedQuantityKindValidator derivedQuantityKindValidator;
        private EnumerationParameterTypeValidator enumerationParameterTypeValidator;
        private SampledFunctionParameterTypeValidator sampledFunctionParameterTypeValidator;
        private SpecializedQuantityKindValidator specializedQuantityKindValidator;

        [SetUp]
        public void SetUp()
        {
            var validationService = new ValidationService();
            this.parameterTypeValidator = new ParameterTypeValidator(validationService);
            this.quantityKindValidator = new ParameterQuantityKindValidator(validationService);
            this.compoundParameterTypeValidator = new CompoundParameterTypeValidator(validationService);
            this.derivedQuantityKindValidator = new DerivedQuantityKindValidator(validationService);
            this.enumerationParameterTypeValidator = new EnumerationParameterTypeValidator(validationService);
            this.sampledFunctionParameterTypeValidator = new SampledFunctionParameterTypeValidator(validationService);
            this.specializedQuantityKindValidator = new SpecializedQuantityKindValidator(validationService);
        }

        [Test]
        public void VerifCompoundParameterTypeValidation()
        {
            var parameterType = new CompoundParameterType();
            Assert.That(this.compoundParameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(false));

            parameterType = new CompoundParameterType
            {
                Name = "updated boolean",
                ShortName = "updatedBoolean",
                Symbol = "symb"
            };

            Assert.That(this.compoundParameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(false));
            parameterType.Component.Add(new ParameterTypeComponent());

            Assert.That(this.compoundParameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(false));
            parameterType.Component.Clear();

            parameterType.Component.Add(new ParameterTypeComponent
            {
                ShortName = "component",
                ParameterType = new TextParameterType()
            });

            Assert.That(this.compoundParameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(true));
        }

        [Test]
        public void VerifyDerivedQuantityKindValidation()
        {
            var parameterType = new DerivedQuantityKind();
            Assert.That(this.derivedQuantityKindValidator.Validate(parameterType).IsValid, Is.EqualTo(false));

            parameterType = new DerivedQuantityKind
            {
                Name = "updated boolean",
                ShortName = "updatedBoolean",
                Symbol = "symb",
                DefaultScale = new OrdinalScale(),
                QuantityDimensionSymbol = "1",
                PossibleScale = [new OrdinalScale()]
            };

            Assert.That(this.derivedQuantityKindValidator.Validate(parameterType).IsValid, Is.EqualTo(false));
            parameterType.QuantityKindFactor.Add(new QuantityKindFactor());

            Assert.That(this.derivedQuantityKindValidator.Validate(parameterType).IsValid, Is.EqualTo(false));
            parameterType.QuantityKindFactor.Clear();

            parameterType.QuantityKindFactor.Add(new QuantityKindFactor
            {
                Exponent = "component",
                QuantityKind = new SimpleQuantityKind()
            });

            Assert.That(this.derivedQuantityKindValidator.Validate(parameterType).IsValid, Is.EqualTo(true));
        }

        [Test]
        public void VerifyEnumerationParameterTypeValidation()
        {
            var parameterType = new EnumerationParameterType();
            Assert.That(this.enumerationParameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(false));

            parameterType = new EnumerationParameterType
            {
                Name = "updated boolean",
                ShortName = "updatedBoolean",
                Symbol = "symb"
            };

            Assert.That(this.enumerationParameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(false));
            parameterType.ValueDefinition.Add(new EnumerationValueDefinition());

            Assert.That(this.enumerationParameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(false));
            parameterType.ValueDefinition.Clear();

            parameterType.ValueDefinition.Add(new EnumerationValueDefinition
            {
                Name = "name",
                ShortName = "shortName"
            });

            Assert.That(this.enumerationParameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(true));
        }

        [Test]
        public void VerifyParameterTypeValidation()
        {
            var parameterType = new BooleanParameterType();
            Assert.That(this.parameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(false));

            parameterType = new BooleanParameterType
            {
                Name = "updated boolean",
                ShortName = "updated boolean"
            };

            Assert.That(this.parameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(false));
            parameterType.ShortName = "updatedBoolean";
            parameterType.Symbol = "symb";
            Assert.That(this.parameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(true));
        }

        [Test]
        public void VerifyQuantityKindValidation()
        {
            var parameterType = new SimpleQuantityKind();
            Assert.That(this.quantityKindValidator.Validate(parameterType).IsValid, Is.EqualTo(false));

            parameterType = new SimpleQuantityKind
            {
                Name = "updated boolean",
                ShortName = "updatedBoolean",
                Symbol = "symb"
            };

            Assert.That(this.quantityKindValidator.Validate(parameterType).IsValid, Is.EqualTo(false));
            parameterType.DefaultScale = new OrdinalScale();
            parameterType.QuantityDimensionSymbol = "1";
            parameterType.PossibleScale = [parameterType.DefaultScale];
            Assert.That(this.quantityKindValidator.Validate(parameterType).IsValid, Is.EqualTo(true));
        }

        [Test]
        public void VerifySampledFunctionParameterTypeValidation()
        {
            var parameterType = new SampledFunctionParameterType();
            Assert.That(this.sampledFunctionParameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(false));

            parameterType = new SampledFunctionParameterType
            {
                Name = "updated boolean",
                ShortName = "updatedBoolean",
                Symbol = "symb",
                DegreeOfInterpolation = 1
            };

            Assert.That(this.sampledFunctionParameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(false));
            parameterType.DependentParameterType.Add(new DependentParameterTypeAssignment());
            parameterType.IndependentParameterType.Add(new IndependentParameterTypeAssignment());

            Assert.That(this.sampledFunctionParameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(false));
            parameterType.DependentParameterType.Clear();
            parameterType.IndependentParameterType.Clear();

            parameterType.DependentParameterType.Add(new DependentParameterTypeAssignment
            {
                ParameterType = new TextParameterType(),
                MeasurementScale = new OrdinalScale()
            });

            Assert.That(this.sampledFunctionParameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(false));

            parameterType.IndependentParameterType.Add(new IndependentParameterTypeAssignment
            {
                ParameterType = new TextParameterType(),
                MeasurementScale = new OrdinalScale()
            });

            Assert.That(this.sampledFunctionParameterTypeValidator.Validate(parameterType).IsValid, Is.EqualTo(true));
        }

        [Test]
        public void VerifySpecializedQuantityKindValidation()
        {
            var parameterType = new SpecializedQuantityKind();
            Assert.That(this.specializedQuantityKindValidator.Validate(parameterType).IsValid, Is.EqualTo(false));

            parameterType = new SpecializedQuantityKind
            {
                Name = "updated boolean",
                ShortName = "updatedBoolean",
                Symbol = "symb",
                DefaultScale = new OrdinalScale(),
                QuantityDimensionSymbol = "1",
                PossibleScale = [new OrdinalScale()]
            };

            Assert.That(this.specializedQuantityKindValidator.Validate(parameterType).IsValid, Is.EqualTo(false));
            parameterType.General = new SimpleQuantityKind();
            Assert.That(this.specializedQuantityKindValidator.Validate(parameterType).IsValid, Is.EqualTo(true));
        }
    }
}
