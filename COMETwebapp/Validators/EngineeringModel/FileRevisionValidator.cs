// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FileRevisionValidator.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Validators.EngineeringModel
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Validation;

    using COMET.Web.Common.Extensions;

    using FluentValidation;

    /// <summary>
    /// A class to validate the <see cref="FileRevisionValidator"/>
    /// </summary>
    public class FileRevisionValidator : AbstractValidator<FileRevision>
    {
        /// <summary>
        /// Instantiates a new <see cref="FileRevisionValidator"/>
        /// </summary>
        public FileRevisionValidator(IValidationService validationService) : base()
        {
            this.RuleFor(x => x.Name).Validate(validationService, nameof(FileRevision.Name));
            this.RuleFor(x => x.ContainingFolder).Validate(validationService, nameof(FileRevision.ContainingFolder));
            this.RuleFor(x => x.FileType).NotEmpty().Validate(validationService, nameof(FileRevision.FileType));
            this.RuleFor(x => x.LocalPath).NotEmpty().Validate(validationService, nameof(FileRevision.LocalPath));
            this.RuleFor(x => x.Path).NotEmpty().Validate(validationService, nameof(FileRevision.Path));
        }
    }
}
