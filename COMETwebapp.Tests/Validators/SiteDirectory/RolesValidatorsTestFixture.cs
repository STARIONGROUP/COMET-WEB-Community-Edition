﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RolesValidatorsTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Validators.SiteDirectory
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    using COMETwebapp.Validators.SiteDirectory.Roles;

    using NUnit.Framework;

    [TestFixture]
    public class RolesValidatorsTestFixture
    {
        private ParticipantRoleValidator participantRoleValidator;
        private PersonRoleValidator personRoleValidator;

        [SetUp]
        public void SetUp()
        {
            var validationService = new ValidationService();
            this.participantRoleValidator = new ParticipantRoleValidator(validationService);
            this.personRoleValidator = new PersonRoleValidator(validationService);
        }

        [Test]
        public void VerifyParticipantRoleValidation()
        {
            var participantRole = new ParticipantRole();
            Assert.That(this.participantRoleValidator.Validate(participantRole).IsValid, Is.EqualTo(false));

            participantRole = new ParticipantRole()
            {
                Name = "updated Participant Role",
                ShortName = "updatedPartRole"
            };

            Assert.That(this.participantRoleValidator.Validate(participantRole).IsValid, Is.EqualTo(true));
        }

        [Test]
        public void VerifyPersonRoleValidation()
        {
            var personRole = new PersonRole();
            Assert.That(this.personRoleValidator.Validate(personRole).IsValid, Is.EqualTo(false));

            personRole = new PersonRole()
            {
                Name = "updated Person Role",
                ShortName = "updatedPersonRole"
            };

            Assert.That(this.personRoleValidator.Validate(personRole).IsValid, Is.EqualTo(true));
        }
    }
}
