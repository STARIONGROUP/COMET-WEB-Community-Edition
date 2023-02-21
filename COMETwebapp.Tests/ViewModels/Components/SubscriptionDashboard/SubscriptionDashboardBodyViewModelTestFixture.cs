// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SubscriptionDashboardBodyViewModelTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.ViewModels.Components.SubscriptionDashboard;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionDashboardBodyViewModelTestFixture
    {
        private SubscriptionDashboardBodyViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<ISubscribedTableViewModel> subscribedTableViewModel;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.subscribedTableViewModel = new Mock<ISubscribedTableViewModel>();
            this.viewModel = new SubscriptionDashboardBodyViewModel(this.sessionService.Object, this.subscribedTableViewModel.Object);
        }

        [Test]
        public void VerifyOnIterationChanged()
        {
            Assert.That(() => this.viewModel.CurrentIteration = null,Throws.Nothing);
            var domain = new DomainOfExpertise();
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(domain);
            this.viewModel.CurrentIteration = new Iteration();

            this.subscribedTableViewModel.Verify(x => x.UpdateProperties(It.IsAny<IEnumerable<ParameterSubscription>>(), 
                this.viewModel.CurrentIteration.Option, this.viewModel.CurrentIteration), Times.Once);
        }

        [Test]
        public void VerifyOnDomainChanged()
        {
            CDPMessageBus.Current.SendMessage(new DomainChangedEvent(new Iteration(), new DomainOfExpertise()));

            this.subscribedTableViewModel.Verify(x => x.UpdateProperties(It.IsAny<IEnumerable<ParameterSubscription>>(),
                It.IsAny<IEnumerable<Option>>(), null), Times.Once);
        }

        [Test]
        public void VerifySessionRefresh()
        {
            CDPMessageBus.Current.SendMessage(new SessionEvent(null, SessionStatus.EndUpdate));

            this.subscribedTableViewModel.Verify(x => x.UpdateProperties(It.IsAny<IEnumerable<ParameterSubscription>>(),
                It.IsAny<IEnumerable<Option>>(), null), Times.Once);
        }
    }
}
