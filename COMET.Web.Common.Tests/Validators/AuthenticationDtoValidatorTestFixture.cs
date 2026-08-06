// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AuthenticationDtoValidatorTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Validators
{
    using COMET.Web.Common.Model.DTO;
    using COMET.Web.Common.Validators;

    using NUnit.Framework;

    [TestFixture]
    public class AuthenticationDtoValidatorTestFixture
    {
        private AuthenticationDtoValidator validator;

        [SetUp]
        public void Setup()
        {
            this.validator = new AuthenticationDtoValidator();
        }

        [Test]
        public void VerifySourceAddressValidation()
        {
            var dto = new AuthenticationDto
            {
                ShouldValidateSourceAddress = true,
                ShouldValidateCredentials = false
            };

            var emptyErrors = this.validator.Validate(dto).Errors;

            dto.SourceAddress = "not a url";
            var invalidErrors = this.validator.Validate(dto).Errors;

            dto.SourceAddress = "ftp://localhost:5000";
            var wrongSchemeErrors = this.validator.Validate(dto).Errors;

            dto.SourceAddress = "http://localhost:5000";
            var validErrors = this.validator.Validate(dto).Errors;

            dto.SourceAddress = "https://localhost:5000";
            var validHttpsErrors = this.validator.Validate(dto).Errors;

            var notRequired = new AuthenticationDto
            {
                ShouldValidateSourceAddress = false,
                ShouldValidateCredentials = false
            };

            Assert.Multiple(() =>
            {
                Assert.That(emptyErrors.Select(x => x.ErrorMessage), Does.Contain("The Source Address is required."));
                Assert.That(invalidErrors.Select(x => x.ErrorMessage), Does.Contain("The Source Address should be a valid HTTP or HTTPS URL."));
                Assert.That(wrongSchemeErrors.Select(x => x.ErrorMessage), Does.Contain("The Source Address should be a valid HTTP or HTTPS URL."));
                Assert.That(validErrors, Is.Empty);
                Assert.That(validHttpsErrors, Is.Empty);
                Assert.That(this.validator.Validate(notRequired).Errors, Is.Empty);
            });
        }

        [Test]
        public void VerifyCredentialsValidationMessagesMatchLabels()
        {
            var dto = new AuthenticationDto
            {
                ShouldValidateCredentials = true,
                ShouldValidateSourceAddress = false
            };

            var errors = this.validator.Validate(dto).Errors.Select(x => x.ErrorMessage).ToList();

            dto.UserName = "user";
            dto.Password = "pass";
            var noErrors = this.validator.Validate(dto).Errors;

            Assert.Multiple(() =>
            {
                Assert.That(errors, Does.Contain("The Username is required."));
                Assert.That(errors, Does.Contain("The Password is required."));
                Assert.That(noErrors, Is.Empty);
            });
        }

        [Test]
        public void VerifyCredentialsAreNotValidatedWhenNotRequired()
        {
            var dto = new AuthenticationDto
            {
                ShouldValidateCredentials = false,
                ShouldValidateSourceAddress = false
            };

            Assert.That(this.validator.Validate(dto).Errors, Is.Empty);
        }
    }
}
