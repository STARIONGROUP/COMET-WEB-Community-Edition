// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EngineeringModelValidatorTestFixture.cs" company="Starion Group S.A.">
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

    using COMETwebapp.Validators.SiteDirectory.EngineeringModel;

    using NUnit.Framework;

    [TestFixture]
    public class EngineeringModelValidatorTestFixture
    {
        private EngineeringModelValidator validator;

        [SetUp]
        public void SetUp()
        {
            var validationService = new ValidationService();
            this.validator = new EngineeringModelValidator(validationService);
        }

        [Test]
        public void VerifyValidationScenarios()
        {
            var model = new EngineeringModelSetup();
            Assert.That(this.validator.Validate(model).IsValid, Is.EqualTo(false));

            model = new EngineeringModelSetup()
            {
                Name = "name",
                ShortName = "short"
            };

            Assert.That(this.validator.Validate(model).IsValid, Is.EqualTo(false));

            model.SourceEngineeringModelSetupIid = Guid.NewGuid();
            Assert.That(this.validator.Validate(model).IsValid, Is.EqualTo(true));

            // Verifies the inputs for when the site rdl is set
            model.SourceEngineeringModelSetupIid = null;
            model.RequiredRdl.Add(new ModelReferenceDataLibrary());
            Assert.That(this.validator.Validate(model).IsValid, Is.EqualTo(false));

            model.ActiveDomain = [new DomainOfExpertise()];
            Assert.That(this.validator.Validate(model).IsValid, Is.EqualTo(true));

            // Verifies the organization input validation
            model.OrganizationalParticipant.Add(new OrganizationalParticipant());
            Assert.That(this.validator.Validate(model).IsValid, Is.EqualTo(false));

            model.DefaultOrganizationalParticipant = model.OrganizationalParticipant.First();
            Assert.That(this.validator.Validate(model).IsValid, Is.EqualTo(true));
        }
    }
}
