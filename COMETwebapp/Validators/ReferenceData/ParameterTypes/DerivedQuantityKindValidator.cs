// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DerivedQuantityKindValidator.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Validators.ReferenceData.ParameterTypes
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    using COMET.Web.Common.Extensions;

    using COMETwebapp.Validators.ReferenceData.ParameterTypes.GenericValidators;

    using FluentValidation;

    /// <summary>
    /// A class to validate the <see cref="DerivedQuantityKind" />
    /// </summary>
    public class DerivedQuantityKindValidator : AbstractValidator<DerivedQuantityKind>
    {
        /// <summary>
        /// Instantiates a new <see cref="DerivedQuantityKindValidator" />
        /// </summary>
        /// <param name="validationService">The <see cref="IValidationService"/></param>
        public DerivedQuantityKindValidator(IValidationService validationService)
        {
            this.Include(new ParameterQuantityKindValidator(validationService));
            this.RuleForEach(x => x.QuantityKindFactor).SetValidator(new QuantityKindFactorValidator(validationService));

            this.RuleFor(x => x.QuantityKindFactor)
                .Must(x => x.Count > 0)
                .WithMessage("At least one quantity kind factor must exist")
                .Validate(validationService, nameof(DerivedQuantityKind.QuantityKindFactor));
        }
    }
}
