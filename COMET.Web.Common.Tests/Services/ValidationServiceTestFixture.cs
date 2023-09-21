// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ValidationServiceTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMET.Web.Common.Tests.Services
{
    using COMET.Web.Common.Services;

    using NUnit.Framework;

    [TestFixture]
    public class ValidationServiceTestFixture
    {
        [Test]
        public void VerifyValidateProperty()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ValidationService.ValidateProperty("SelectedSource", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("SelectedSource", "valid source"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("ShortName", "invalid ShortName"), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("ShortName", "validShortName"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("RDLShortName", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("RDLShortName", "validRDL"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("RDLName", " invalidRDLName"), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("RDLName", "validRDLName"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("NativeName", "1nativeName1"), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("NativeName", "nativeName"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("FileRevisionName", " invalid FileRevisionName"), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("FileRevisionName", "FileRevisionName"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Name", "invalidName)"), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Name", "validName"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("EmailAddress", "invalidemail.com"), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("EmailAddress", "valid@example.com"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("TelephoneNumber", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("TelephoneNumber", "000 000 000"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("UserPreference", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("UserPreference", "userPref"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("LanguageCode", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("LanguageCode", "language code"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Content", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Content", "content"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Password", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Password", "password"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("ForwardRelationshipName", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("ForwardRelationshipName", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("InverseRelationshipName", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("InverseRelationshipName", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Exponent", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Exponent", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Symbol", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Symbol", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("ScaleValueDefinition", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("ScaleValueDefinition", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("ScaleReferenceQuantityValue", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("ScaleReferenceQuantityValue", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Factor", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Factor", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Modulus", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Modulus", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Extension", "invalidExtension"), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Extension", "valid1"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("FileTypeName", "invented"), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("FileTypeName", "audio/1"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Value", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Value", "valid"), Is.Null.Or.Empty); 
                Assert.That(ValidationService.ValidateProperty("ModelSetupShortName", "invalid!"), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("ModelSetupShortName", "valid"), Is.Null.Or.Empty); 
                Assert.That(ValidationService.ValidateProperty("PersonShortName", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("PersonShortName", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("PersonGivenName", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("PersonGivenName", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("PersonSurname", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("PersonSurname", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("RequirementShortName", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("RequirementShortName", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("ModelSetupName", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("ModelSetupName", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("ConversionFactor", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("ConversionFactor", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Description", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Description", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Title", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("Title", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("EnumerationValueDefinitionShortName", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("EnumerationValueDefinitionShortName", "valid"), Is.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("EnumerationValueDefinitionName", ""), Is.Not.Null.Or.Empty);
                Assert.That(ValidationService.ValidateProperty("EnumerationValueDefinitionName", "valid"), Is.Null.Or.Empty);

            });
        }
    }
}

