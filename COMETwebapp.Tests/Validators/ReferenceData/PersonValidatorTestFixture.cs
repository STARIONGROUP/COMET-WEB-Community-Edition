﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PersonValidatorTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Validators.ReferenceData
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    using COMETwebapp.Validators.SiteDirectory;

    using NUnit.Framework;

    [TestFixture]
    public class PersonValidatorTestFixture
    {
        private PersonValidator personValidator;

        [SetUp]
        public void SetUp()
        {
            var validationService = new ValidationService();
            this.personValidator = new PersonValidator(validationService);
        }

        [Test]
        public void VerifyPersonValidation()
        {
            var person = new Person();

            Assert.That(this.personValidator.Validate(person).IsValid, Is.EqualTo(false));

            person = new Person
            {
                GivenName = "first",
                Surname = "last",
                ShortName = "user name"
            };

            Assert.That(this.personValidator.Validate(person).IsValid, Is.EqualTo(false));

            person.ShortName = "userName";
            Assert.That(this.personValidator.Validate(person).IsValid, Is.EqualTo(true));
        }
    }
}
