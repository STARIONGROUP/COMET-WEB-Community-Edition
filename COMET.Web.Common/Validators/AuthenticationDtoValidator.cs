// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AuthenticationDtoValidator.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Validators
{
    using COMET.Web.Common.Model.DTO;

    using FluentValidation;

    /// <summary>
    /// The <see cref="AuthenticationDtoValidator"/> is an <see cref="AbstractValidator{T}"/> that provides custom validation of <see cref="AuthenticationDto"/>
    /// </summary>
    public class AuthenticationDtoValidator: AbstractValidator<AuthenticationDto>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AuthenticationDtoValidator"/>
        /// </summary>
        public AuthenticationDtoValidator()
        {
            this.RuleFor(x => x.SourceAddress).NotEmpty()
                .WithMessage("The Source Address is required.")
                .When(x => x.ShouldValidateSourceAddress);

            this.RuleFor(x => x.SourceAddress).Must(BeAValidHttpOrHttpsUrl)
                .When(x => !string.IsNullOrEmpty(x.SourceAddress))
                .WithMessage("The Source Address should be a valid HTTP or HTTPS URL.");

            this.RuleFor(x => x.UserName).NotEmpty()
                .WithMessage("The Username is required.")
                .When(x => x.ShouldValidateCredentials);

            this.RuleFor(x => x.Password).NotEmpty()
                .WithMessage("The Password is required.")
                .When(x => x.ShouldValidateCredentials);
        }

        /// <summary>
        /// Asserts that the provided source address is a well-formed absolute URL using the HTTP or HTTPS scheme, the
        /// only schemes a CDP4-COMET web server is reachable on. This mirrors the CDP4-COMET-SDK
        /// <c>UriExtensions.AssertUriIsHttpOrHttpsSchema</c> check enforced by the web data-access layer
        /// </summary>
        /// <param name="sourceAddress">The source address to validate</param>
        /// <returns>True if the address is a valid HTTP or HTTPS URL, otherwise false</returns>
        private static bool BeAValidHttpOrHttpsUrl(string sourceAddress)
        {
            return Uri.TryCreate(sourceAddress, UriKind.Absolute, out var uri)
                   && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
