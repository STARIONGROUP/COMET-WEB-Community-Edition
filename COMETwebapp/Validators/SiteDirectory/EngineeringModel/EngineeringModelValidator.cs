// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EngineeringModelValidator.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
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
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Validators.SiteDirectory.EngineeringModel
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    using COMET.Web.Common.Extensions;

    using FluentValidation;

    /// <summary>
    /// A class to validate the <see cref="EngineeringModelSetup"/>
    /// </summary>
    public class EngineeringModelValidator : AbstractValidator<EngineeringModelSetup>
    {
        /// <summary>
        /// Instantiates a new <see cref="EngineeringModelValidator"/>
        /// </summary>
        public EngineeringModelValidator(IValidationService validationService) : base()
        {
            this.RuleFor(x => x.Name).Validate(validationService, nameof(EngineeringModelSetup.Name));
            this.RuleFor(x => x.ShortName).Validate(validationService, nameof(EngineeringModelSetup.ShortName));
            this.RuleFor(x => x.Kind).Validate(validationService, nameof(EngineeringModelSetup.Kind));
            this.RuleFor(x => x.StudyPhase).Validate(validationService, nameof(EngineeringModelSetup.StudyPhase));

            this.RuleFor(x => x.SourceEngineeringModelSetupIid)
                .NotEmpty()
                .WithMessage("The Source Model must be selected if no Site RDL is set")
                .Validate(validationService, nameof(EngineeringModelSetup.SourceEngineeringModelSetupIid))
                .Unless(x => x.RequiredRdl.Count > 0);

            this.RuleFor(x => x.DefaultOrganizationalParticipant)
                .NotNull()
                .WithMessage("The Model Admin Organization must not be empty if any organization was selected")
                .Validate(validationService, nameof(EngineeringModelSetup.DefaultOrganizationalParticipant))
                .Unless(x => x.OrganizationalParticipant.Count == 0);

            this.RuleFor(x => x.ActiveDomain)
                .Must(x => x.Count > 0)
                .WithMessage("One or more active domains must be selected")
                .Validate(validationService, nameof(EngineeringModelSetup.ActiveDomain))
                .Unless(x => x.SourceEngineeringModelSetupIid != null);

            this.RuleFor(x => x.RequiredRdl)
                .Must(x => x.Count > 0)
                .WithMessage("The Site RDL must be selected if no Source Model is set")
                .Validate(validationService, nameof(EngineeringModelSetup.RequiredRdl))
                .Unless(x => x.SourceEngineeringModelSetupIid != null);
        }
    }
}
