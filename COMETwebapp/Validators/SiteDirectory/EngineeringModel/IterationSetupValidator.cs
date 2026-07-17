// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IterationSetupValidator.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Validators.SiteDirectory.EngineeringModel
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    using COMET.Web.Common.Extensions;

    using FluentValidation;

    /// <summary>
    /// A class to validate the <see cref="IterationSetup" />
    /// </summary>
    public class IterationSetupValidator : AbstractValidator<IterationSetup>
    {
        /// <summary>
        /// Instantiates a new <see cref="IterationSetupValidator" />
        /// </summary>
        /// <param name="validationService">The <see cref="IValidationService" /></param>
        public IterationSetupValidator(IValidationService validationService)
        {
            // When the iteration number is 0, i.e., we are creating a new iteration setup, the source iteration setup is mandatory. Otherwise, it can be empty.
            this.RuleFor(x => x.SourceIterationSetup)
                .NotEmpty()
                .WithMessage("Source is mandatory")
                .When(x => x.IterationNumber == 0)
                .Validate(validationService, nameof(IterationSetup.SourceIterationSetup));

            this.RuleFor(x => x.Description)
                .Validate(validationService, nameof(IterationSetup.Description));
        }
    }
}
