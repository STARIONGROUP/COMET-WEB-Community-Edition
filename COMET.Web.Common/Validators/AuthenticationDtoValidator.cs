// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="AuthenticationDtoValidator.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2025 Starion Group S.A.
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
            this.RuleFor(x => x.SourceAddress).Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrEmpty(x.SourceAddress))
                .WithMessage("The Source Address should be a valid URL.");

            this.RuleFor(x => x.UserName).NotEmpty()
                .When(x => x.ShouldValidateCredentials);
            
            this.RuleFor(x => x.Password).NotEmpty()
                .When(x => x.ShouldValidateCredentials);
        }
    }
}
