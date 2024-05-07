// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DomainOfExpertiseSelectorViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.ViewModels.Components.Selectors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class DomainOfExpertiseSelectorViewModelTestFixture
    {
        private DomainOfExpertiseSelectorViewModel viewModel;
        private CDPMessageBus messageBus;
        private DomainOfExpertise domain;

        [SetUp]
        public void Setup()
        {
            this.domain = new DomainOfExpertise
            {
                Name = "a name"
            };

            var domain2 = new DomainOfExpertise
            {
                Name = "b name"
            };

            var siteDirectory = new SiteDirectory
            {
                Domain = { this.domain, domain2 }
            };

            var sessionService = new Mock<ISessionService>();
            this.messageBus = new CDPMessageBus();

            sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);
            sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(new DomainOfExpertise());

            this.viewModel = new DomainOfExpertiseSelectorViewModel(sessionService.Object, this.messageBus);
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyDomainChanged()
        {
            var iteration = new Iteration(Guid.NewGuid(), null, null);
            this.messageBus.SendMessage(new DomainChangedEvent(iteration, this.domain));
            Assert.That(this.viewModel.CurrentIterationDomain, Is.Not.EqualTo(iteration));

            this.viewModel.CurrentIteration = iteration;
            this.messageBus.SendMessage(new DomainChangedEvent(iteration, this.domain));
            Assert.That(this.viewModel.CurrentIterationDomain, Is.Not.EqualTo(iteration));
        }

        [Test]
        public async Task VerifyProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableDomainsOfExpertise.Count(), Is.EqualTo(2));
                Assert.That(this.viewModel.AvailableDomainsOfExpertise.First(), Is.EqualTo(this.domain));
                Assert.That(this.viewModel.CurrentIteration, Is.Null);
                Assert.That(this.viewModel.SelectedDomainOfExpertise, Is.Null);
                Assert.That(this.viewModel.CurrentIterationDomain, Is.Null);
            });

            this.viewModel.CurrentIteration = new Iteration();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CurrentIterationDomain, Is.Not.Null);
                Assert.That(this.viewModel.SelectedDomainOfExpertise, Is.Not.Null);
            });

            await this.viewModel.SetSelectedDomainOfExpertiseOrReset(false, new DomainOfExpertise());
            Assert.That(this.viewModel.SelectedDomainOfExpertise, Is.Not.Null);

            var newDomain = new DomainOfExpertise();
            this.viewModel.SelectedDomainOfExpertise = newDomain;
            await this.viewModel.SetSelectedDomainOfExpertiseOrReset(true, newDomain);
        }
    }
}
