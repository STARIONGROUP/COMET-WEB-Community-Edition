// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CommonFileStoreValidatorTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Validators.EngineeringModel
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    using COMETwebapp.Validators.EngineeringModel;

    using NUnit.Framework;

    [TestFixture]
    public class CommonFileStoreValidatorTestFixture
    {
        private CommonFileStoreValidator validator;

        [SetUp]
        public void SetUp()
        {
            var validationService = new ValidationService();
            this.validator = new CommonFileStoreValidator(validationService);
        }

        [Test]
        public void VerifyValidationScenarios()
        {
            var commonFileStore = new CommonFileStore();
            Assert.That(this.validator.Validate(commonFileStore).IsValid, Is.EqualTo(false));

            commonFileStore = new CommonFileStore()
            {
                Name = "name1",
                Owner = new DomainOfExpertise()
            };

            Assert.That(this.validator.Validate(commonFileStore).IsValid, Is.EqualTo(true));
        }
    }
}
