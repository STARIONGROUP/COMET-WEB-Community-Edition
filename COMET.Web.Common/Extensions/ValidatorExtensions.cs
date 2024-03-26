// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidatorExtensions.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Extensions
{
    using CDP4Common.Validation;

    using FluentValidation;

    /// <summary>
    /// Static extension methods for <see cref="AbstractValidator{T}"/>s
    /// </summary>
    public static class ValidatorExtensions
    {
        /// <summary>
        /// The custom validator that uses a <see cref="IValidationService"/> to validate data
        /// </summary>
        /// <typeparam name="T">The root object type</typeparam>
        /// <typeparam name="TElement">The value type</typeparam>
        /// <param name="ruleBuilder">The <see cref="IRuleBuilder{T,TProperty}" /> for which data will be validated</param>
        /// <param name="validationService">The <see cref="IValidationService" /> to validate data using Comet rules</param>
        /// <param name="propertyName">The property name to be validated</param>
        /// <returns>A <see cref="IRuleBuilderOptions{T,TProperty}"/> with the validation message</returns>
        public static IRuleBuilderOptions<T, TElement> Validate<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder, IValidationService validationService, string propertyName)
        {
             return ruleBuilder.Must((_, element, context) =>
             {
                 var validationError = validationService.ValidateProperty(propertyName, element);
                 context.MessageFormatter.AppendArgument("ValidationError", validationError);
                 return string.IsNullOrWhiteSpace(validationError);
             }).WithMessage("{ValidationError}");
        }
    }
}
