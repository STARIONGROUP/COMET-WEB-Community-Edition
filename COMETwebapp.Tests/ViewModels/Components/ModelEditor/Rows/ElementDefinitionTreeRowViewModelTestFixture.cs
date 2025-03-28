﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDefinitionTreeRowViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.ModelEditor.Rows
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.ViewModels.Components.ModelEditor.Rows;

    using NUnit.Framework;

    [TestFixture]
    public class ElementDefinitionTreeRowViewModelTestFixture
    {
        private ElementDefinition elementDefinition;
        private ElementUsage elementUsage;
        private string elementDefinitionName;
        private DomainOfExpertise owner;
        private string ownerShortName;
        private string elementUsageName;
        private Iteration iteration;

        [SetUp]
        public void Setup()
        {
            elementDefinitionName = "TestElementDefinition";
            elementUsageName = "TestElementUsage";
            ownerShortName = "TestOwner";

            iteration = new Iteration();

            owner = new DomainOfExpertise(Guid.NewGuid(), null, null)
            {
                ShortName = ownerShortName
            };

            elementUsage = new ElementUsage(Guid.NewGuid(), null, null)
            {
                Name = elementUsageName,
                Owner = owner
            };

            elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                Name = elementDefinitionName,
                Owner = owner,
                ContainedElement = { elementUsage }
            };

            iteration.Element.Add(elementDefinition);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void VerifyCreation()
        {
            var testVM = new ElementDefinitionTreeRowViewModel(elementDefinition);

            Assert.Multiple(() =>
            {
                Assert.That(testVM.ElementBase, Is.EqualTo(elementDefinition));
                Assert.That(testVM.ElementName, Is.EqualTo(elementDefinitionName));
                Assert.That(testVM.OwnerShortName, Is.EqualTo(ownerShortName));
                Assert.That(testVM.IsTopElement, Is.False);
                Assert.That(testVM.Rows.Count, Is.EqualTo(1));
            });
        }

        [Test]
        public void VerifyCreationNullElement()
        {
            Assert.That(() => new ElementDefinitionTreeRowViewModel(null), Throws.ArgumentNullException);
        }

        [Test]
        public void VerifyCreationAndUpdateProperties()
        {
            var testVM = new ElementDefinitionTreeRowViewModel();

            Assert.Multiple(() =>
            {
                Assert.That(testVM.ElementBase, Is.Null);
                Assert.That(testVM.ElementName, Is.Null);
                Assert.That(testVM.OwnerShortName, Is.Null);
                Assert.That(testVM.IsTopElement, Is.False);
                Assert.That(testVM.Rows.Count, Is.EqualTo(0));
            });

            iteration.TopElement = elementDefinition;

            testVM.UpdateProperties(new ElementDefinitionTreeRowViewModel(elementDefinition));

            Assert.Multiple(() =>
            {
                Assert.That(testVM.ElementBase, Is.EqualTo(elementDefinition));
                Assert.That(testVM.ElementName, Is.EqualTo(elementDefinitionName));
                Assert.That(testVM.OwnerShortName, Is.EqualTo(ownerShortName));
                Assert.That(testVM.IsTopElement, Is.True);
                Assert.That(testVM.Rows.Count, Is.EqualTo(1));
            });

            elementDefinition.ContainedElement.Clear();

            testVM.UpdateProperties(new ElementDefinitionTreeRowViewModel(elementDefinition));

            Assert.Multiple(() =>
            {
                Assert.That(testVM.ElementBase, Is.EqualTo(elementDefinition));
                Assert.That(testVM.ElementName, Is.EqualTo(elementDefinitionName));
                Assert.That(testVM.OwnerShortName, Is.EqualTo(ownerShortName));
                Assert.That(testVM.IsTopElement, Is.True);
                Assert.That(testVM.Rows.Count, Is.EqualTo(0));
            });
        }

        [Test]
        public void VerifyUpdatePropertiesNullElement()
        {
            Assert.That(() => new ElementDefinitionTreeRowViewModel(elementDefinition).UpdateProperties(null), Throws.ArgumentNullException);
            Assert.That(() => new ElementDefinitionTreeRowViewModel().UpdateProperties(null), Throws.ArgumentNullException);
        }
    }
}
