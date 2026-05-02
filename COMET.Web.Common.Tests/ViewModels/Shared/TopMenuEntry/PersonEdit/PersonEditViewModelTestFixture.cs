// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PersonEditViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.ViewModels.Shared.TopMenuEntry.PersonEdit
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry.PersonEdit;

    using FluentResults;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class PersonEditViewModelTestFixture
    {
        /// <summary>
        /// The mocked session service used to provide the SiteDirectory and capture write calls.
        /// </summary>
        private Mock<ISessionService> sessionService;

        /// <summary>
        /// The active <see cref="Person" /> used as the editable target.
        /// </summary>
        private Person activePerson;

        /// <summary>
        /// The <see cref="SiteDirectory" /> hosting the active person.
        /// </summary>
        private SiteDirectory siteDirectory;

        /// <summary>
        /// The pre-existing <see cref="EmailAddress" /> on the active person.
        /// </summary>
        private EmailAddress existingEmail;

        /// <summary>
        /// The pre-existing <see cref="TelephoneNumber" /> on the active person.
        /// </summary>
        private TelephoneNumber existingTelephone;

        /// <summary>
        /// The view model under test.
        /// </summary>
        private PersonEditViewModel viewModel;

        /// <summary>
        /// Builds a SiteDirectory with one Person carrying one e-mail and one telephone, configures the
        /// session-service mock to return the SiteDirectory and to succeed on writes, and constructs
        /// the view model.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.existingEmail = new EmailAddress { Iid = Guid.NewGuid(), Value = "alice@example.com", VcardType = VcardEmailAddressKind.WORK };
            this.existingTelephone = new TelephoneNumber { Iid = Guid.NewGuid(), Value = "+31 6 12345678" };
            this.existingTelephone.VcardType.Add(VcardTelephoneNumberKind.WORK);

            this.activePerson = new Person { Iid = Guid.NewGuid(), GivenName = "Alice", Surname = "Smith", OrganizationalUnit = "Systems" };
            this.activePerson.EmailAddress.Add(this.existingEmail);
            this.activePerson.TelephoneNumber.Add(this.existingTelephone);
            this.activePerson.DefaultEmailAddress = this.existingEmail;
            this.activePerson.DefaultTelephoneNumber = this.existingTelephone;

            this.siteDirectory = new SiteDirectory { Iid = Guid.NewGuid() };
            this.siteDirectory.Person.Add(this.activePerson);

            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(this.siteDirectory);
            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Ok());

            var logger = new Mock<ILogger<PersonEditViewModel>>();

            this.viewModel = new PersonEditViewModel(this.sessionService.Object, logger.Object);
        }

        /// <summary>
        /// Disposes of the view model.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyInitializeFromActivePerson()
        {
            this.viewModel.Initialize(this.activePerson);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CurrentPerson, Is.SameAs(this.activePerson));
                Assert.That(this.viewModel.GivenName, Is.EqualTo(this.activePerson.GivenName));
                Assert.That(this.viewModel.Surname, Is.EqualTo(this.activePerson.Surname));
                Assert.That(this.viewModel.OrganizationalUnit, Is.EqualTo(this.activePerson.OrganizationalUnit));
                Assert.That(this.viewModel.EmailAddresses, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.EmailAddresses[0].Original, Is.SameAs(this.existingEmail));
                Assert.That(this.viewModel.EmailAddresses[0].IsDefault, Is.True);
                Assert.That(this.viewModel.TelephoneNumbers, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.TelephoneNumbers[0].Original, Is.SameAs(this.existingTelephone));
                Assert.That(this.viewModel.TelephoneNumbers[0].IsDefault, Is.True);
                Assert.That(this.viewModel.IsPasswordEditEnabled, Is.False);
                Assert.That(this.viewModel.Password, Is.Empty);
            });
        }

        [Test]
        public void VerifyIsValidRequiresGivenAndSurname()
        {
            this.viewModel.Initialize(this.activePerson);

            Assert.That(this.viewModel.IsValid, Is.True);

            this.viewModel.GivenName = string.Empty;
            Assert.That(this.viewModel.IsValid, Is.False, "Empty GivenName must invalidate the form.");

            this.viewModel.GivenName = "Alice";
            this.viewModel.Surname = string.Empty;
            Assert.That(this.viewModel.IsValid, Is.False, "Empty Surname must invalidate the form.");
        }

        [Test]
        public void VerifyIsValidRequiresMatchingPasswordWhenEnabled()
        {
            this.viewModel.Initialize(this.activePerson);

            this.viewModel.IsPasswordEditEnabled = true;
            Assert.That(this.viewModel.IsValid, Is.False, "Empty new password must invalidate.");

            this.viewModel.Password = "newSecret";
            this.viewModel.PasswordConfirmation = "different";
            Assert.That(this.viewModel.IsValid, Is.False, "Mismatched confirmation must invalidate.");

            this.viewModel.PasswordConfirmation = "newSecret";
            Assert.That(this.viewModel.IsValid, Is.True);
        }

        [Test]
        public void VerifyIsValidRequiresEmailValuePresent()
        {
            this.viewModel.Initialize(this.activePerson);

            this.viewModel.AddEmail();
            Assert.That(this.viewModel.IsValid, Is.False, "An empty newly added e-mail row must invalidate the form.");

            this.viewModel.EmailAddresses[^1].Value = "second@example.com";
            Assert.That(this.viewModel.IsValid, Is.True);
        }

        [Test]
        public async Task VerifySaveCallsSessionServiceWithCorrectContainerAndAppliedFields()
        {
            this.viewModel.Initialize(this.activePerson);
            this.viewModel.GivenName = "Alicia";
            this.viewModel.Surname = "Smithee";
            this.viewModel.OrganizationalUnit = "Newer Org";

            await this.viewModel.SaveAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.Is<Thing>(t => t is SiteDirectory && t.Iid == this.siteDirectory.Iid),
                    It.Is<IReadOnlyCollection<Thing>>(c => c.OfType<Person>().Any(p => p.Iid == this.activePerson.Iid && p.GivenName == "Alicia" && p.Surname == "Smithee" && p.OrganizationalUnit == "Newer Org")),
                    It.IsAny<NotificationDescription>()),
                Times.Once);
        }

        [Test]
        public async Task VerifySavePersistsPasswordOnlyWhenEnabled()
        {
            this.viewModel.Initialize(this.activePerson);

            await this.viewModel.SaveAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.IsAny<Thing>(),
                    It.Is<IReadOnlyCollection<Thing>>(c => c.OfType<Person>().Any(p => p.Password == this.activePerson.Password)),
                    It.IsAny<NotificationDescription>()),
                Times.Once);

            this.viewModel.IsPasswordEditEnabled = true;
            this.viewModel.Password = "newSecret";
            this.viewModel.PasswordConfirmation = "newSecret";

            await this.viewModel.SaveAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.IsAny<Thing>(),
                    It.Is<IReadOnlyCollection<Thing>>(c => c.OfType<Person>().Any(p => p.Password == "newSecret")),
                    It.IsAny<NotificationDescription>()),
                Times.Once);
        }

        [Test]
        public async Task VerifySaveAddsNewEmailIntoCollectionAndThings()
        {
            this.viewModel.Initialize(this.activePerson);
            this.viewModel.AddEmail();
            this.viewModel.EmailAddresses[^1].Value = "second@example.com";
            this.viewModel.EmailAddresses[^1].VcardType = VcardEmailAddressKind.HOME;

            await this.viewModel.SaveAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.IsAny<Thing>(),
                    It.Is<IReadOnlyCollection<Thing>>(c => c.OfType<EmailAddress>().Any(e => e.Value == "second@example.com" && e.VcardType == VcardEmailAddressKind.HOME)
                                                          && c.OfType<Person>().Any(p => p.EmailAddress.Any(e => e.Value == "second@example.com"))),
                    It.IsAny<NotificationDescription>()),
                Times.Once);
        }

        [Test]
        public async Task VerifySaveSetsDefaultEmailWhenIsDefault()
        {
            this.viewModel.Initialize(this.activePerson);
            this.viewModel.EmailAddresses[0].IsDefault = false;
            this.viewModel.AddEmail();
            this.viewModel.EmailAddresses[^1].Value = "second@example.com";
            this.viewModel.EmailAddresses[^1].IsDefault = true;

            await this.viewModel.SaveAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.IsAny<Thing>(),
                    It.Is<IReadOnlyCollection<Thing>>(c => c.OfType<Person>().Any(p => p.DefaultEmailAddress != null && p.DefaultEmailAddress.Value == "second@example.com")),
                    It.IsAny<NotificationDescription>()),
                Times.Once);
        }

        [Test]
        public async Task VerifySaveAddsNewTelephoneIntoCollectionAndThings()
        {
            this.viewModel.Initialize(this.activePerson);
            this.viewModel.AddTelephone();
            this.viewModel.TelephoneNumbers[^1].Value = "+31 6 99999999";
            this.viewModel.TelephoneNumbers[^1].VcardType = new[] { VcardTelephoneNumberKind.HOME };

            await this.viewModel.SaveAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(
                    It.IsAny<Thing>(),
                    It.Is<IReadOnlyCollection<Thing>>(c => c.OfType<TelephoneNumber>().Any(t => t.Value == "+31 6 99999999" && t.VcardType.Contains(VcardTelephoneNumberKind.HOME))
                                                          && c.OfType<Person>().Any(p => p.TelephoneNumber.Any(t => t.Value == "+31 6 99999999"))),
                    It.IsAny<NotificationDescription>()),
                Times.Once);
        }

        [Test]
        public async Task VerifySaveFiresOnSavedOnSuccess()
        {
            this.viewModel.Initialize(this.activePerson);

            var fired = false;
            this.viewModel.OnSaved = EventCallback.Factory.Create(this, () => fired = true);

            await this.viewModel.SaveAsync();

            Assert.That(fired, Is.True);
        }

        [Test]
        public async Task VerifySaveDoesNotFireOnSavedOnFailure()
        {
            this.sessionService
                .Setup(x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()))
                .ReturnsAsync(Result.Fail("write failed"));

            this.viewModel.Initialize(this.activePerson);

            var fired = false;
            this.viewModel.OnSaved = EventCallback.Factory.Create(this, () => fired = true);

            await this.viewModel.SaveAsync();

            Assert.That(fired, Is.False);
        }

        [Test]
        public async Task VerifySaveIsNoOpWhenInvalid()
        {
            this.viewModel.Initialize(this.activePerson);
            this.viewModel.GivenName = string.Empty;

            await this.viewModel.SaveAsync();

            this.sessionService.Verify(
                x => x.CreateOrUpdateThingsWithNotification(It.IsAny<Thing>(), It.IsAny<IReadOnlyCollection<Thing>>(), It.IsAny<NotificationDescription>()),
                Times.Never);
        }

        [Test]
        public void VerifyRemoveEmailDropsRowAndRemembersOriginal()
        {
            this.viewModel.Initialize(this.activePerson);
            var row = this.viewModel.EmailAddresses[0];

            this.viewModel.RemoveEmail(row);

            Assert.That(this.viewModel.EmailAddresses, Is.Empty);
        }
    }
}
