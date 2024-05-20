// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CategoryValidatorTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    using COMETwebapp.Validators.ReferenceData;

    using NUnit.Framework;

    [TestFixture]
    public class CategoryValidatorTestFixture
    {
        private CategoryValidator categoryValidator;

        [SetUp]
        public void SetUp()
        {
            var validationService = new ValidationService();
            this.categoryValidator = new CategoryValidator(validationService);
        }

        [Test]
        public void VerifyCategoryValidation()
        {
            var category = new Category();

            Assert.That(this.categoryValidator.Validate(category).IsValid, Is.EqualTo(false));

            category = new Category
            {
                Name = "updated Unit",
                ShortName = "updated Unit"
            };

            Assert.That(this.categoryValidator.Validate(category).IsValid, Is.EqualTo(false));

            category.ShortName = "updatedUnit";
            category.PermissibleClass = [ClassKind.ActionItem];
            Assert.That(this.categoryValidator.Validate(category).IsValid, Is.EqualTo(true));
        }
    }
}
