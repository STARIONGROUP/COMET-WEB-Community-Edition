// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CreateDomainOfExpertiseValidatorTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.Validators
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Validators;

    using NUnit.Framework;

    [TestFixture]
    public class CreateDomainOfExpertiseValidatorTestFixture
    {
        private CreateDomainOfExpertiseValidator validator;

        [SetUp]
        public void SetUp()
        {
            this.validator = new CreateDomainOfExpertiseValidator();
        }

        [Test]
        public void VerifyValidationScenarios()
        {
            var domain = new DomainOfExpertise();
            Assert.That(this.validator.Validate(domain).IsValid, Is.EqualTo(false));

            domain = new DomainOfExpertise()
            {
                Name = "updated Domain",
                ShortName = "updated Domain"
            };

            Assert.That(this.validator.Validate(domain).IsValid, Is.EqualTo(false));

            domain.ShortName = "updatedDomain";
            Assert.That(this.validator.Validate(domain).IsValid, Is.EqualTo(true));
        }
    }
}
