// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SubscriptionDashboardBodyViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.SubscriptionDashboard
{
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.SubscriptionDashboard;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionDashboardBodyViewModelTestFixture
    {
        private SubscriptionDashboardBodyViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<ISubscribedTableViewModel> subscribedTableViewModel;
        private ICDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.subscribedTableViewModel = new Mock<ISubscribedTableViewModel>();
            this.messageBus = new CDPMessageBus();
            this.viewModel = new SubscriptionDashboardBodyViewModel(this.sessionService.Object, this.subscribedTableViewModel.Object, this.messageBus);
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyOnIterationChanged()
        {
            Assert.That(() => this.viewModel.CurrentThing = null,Throws.Nothing);
            var domain = new DomainOfExpertise();
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(domain);
            this.viewModel.CurrentThing = new Iteration();

            this.subscribedTableViewModel.Verify(x => x.UpdateProperties(It.IsAny<IEnumerable<ParameterSubscription>>(), 
                this.viewModel.CurrentThing.Option, this.viewModel.CurrentThing), Times.Once);
        }

        [Test]
        public void VerifyOnDomainChanged()
        {
            this.messageBus.SendMessage(new DomainChangedEvent(new Iteration(), new DomainOfExpertise()));

            this.subscribedTableViewModel.Verify(x => x.UpdateProperties(It.IsAny<IEnumerable<ParameterSubscription>>(),
                It.IsAny<IEnumerable<Option>>(), null), Times.Once);
        }

        [Test]
        public void VerifySessionRefresh()
        {
            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);

            this.subscribedTableViewModel.Verify(x => x.UpdateProperties(It.IsAny<IEnumerable<ParameterSubscription>>(),
                It.IsAny<IEnumerable<Option>>(), null), Times.Once);
        }
    }
}
