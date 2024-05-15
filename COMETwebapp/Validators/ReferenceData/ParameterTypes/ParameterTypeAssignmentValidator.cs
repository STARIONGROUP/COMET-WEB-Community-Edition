// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeAssignmentValidator.cs" company="Starion Group S.A.">
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

    using FluentValidation;

    /// <summary>
    /// A class to validate the <see cref="IParameterTypeAssignment" />
    /// </summary>
    public class ParameterTypeAssignmentValidator : AbstractValidator<IParameterTypeAssignment>
    {
        /// <summary>
        /// Instantiates a new <see cref="ParameterTypeAssignmentValidator" />
        /// </summary>
        /// <param name="validationService">The <see cref="IValidationService" /></param>
        public ParameterTypeAssignmentValidator(IValidationService validationService)
        {
            this.RuleFor(x => x.ParameterType).NotEmpty().Validate(validationService, nameof(IParameterTypeAssignment.ParameterType));
            this.RuleFor(x => x.MeasurementScale).Validate(validationService, nameof(IParameterTypeAssignment.MeasurementScale));
        }
    }
}
