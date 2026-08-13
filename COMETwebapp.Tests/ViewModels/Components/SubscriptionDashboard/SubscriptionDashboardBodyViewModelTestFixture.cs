// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SubscriptionDashboardBodyViewModelTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Tests.ViewModels.Components.SubscriptionDashboard
{
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using DynamicData;

    using CDP4Web.Enumerations;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.SubscriptionDashboard;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class SubscriptionDashboardBodyViewModelTestFixture
    {
        private SubscriptionDashboardBodyViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<ISubscribedTableViewModel> subscribedTableViewModel;
        private CDPMessageBus messageBus;
        private SourceList<Iteration> openIterations;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.openIterations = new SourceList<Iteration>();
            this.sessionService.Setup(x => x.OpenIterations).Returns(this.openIterations);
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
            var iteration = new Iteration();
            this.openIterations.Add(iteration);
            this.viewModel.CurrentThing = iteration;

            this.subscribedTableViewModel.Verify(x => x.UpdateProperties(It.IsAny<IEnumerable<ParameterSubscription>>(), 
                this.viewModel.CurrentThing.Option, this.viewModel.CurrentThing), Times.Once);
        }

        [Test]
        public async Task VerifyOnDomainChanged()
        {
            var domain = new DomainOfExpertise();
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(domain);
            var iteration = new Iteration();
            this.openIterations.Add(iteration);
            this.viewModel.CurrentThing = iteration;
            this.messageBus.SendMessage(new DomainChangedEvent(iteration, domain));
            await Task.Delay(50);

            this.subscribedTableViewModel.Verify(x => x.UpdateProperties(It.IsAny<IEnumerable<ParameterSubscription>>(),
                It.IsAny<IEnumerable<Option>>(), iteration), Times.AtLeastOnce);
        }

        [Test]
        public async Task VerifySessionRefresh()
        {
            var domain = new DomainOfExpertise();
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(domain);
            var iteration = new Iteration();
            this.openIterations.Add(iteration);
            this.viewModel.CurrentThing = iteration;
            this.messageBus.SendMessage(SessionServiceEvent.SessionRefreshed, this.sessionService.Object.Session);
            await Task.Delay(50);

            this.subscribedTableViewModel.Verify(x => x.UpdateProperties(It.IsAny<IEnumerable<ParameterSubscription>>(),
                It.IsAny<IEnumerable<Option>>(), iteration), Times.AtLeastOnce);
        }
        
        [Test]
        public void VerifyExternalParameterChangeIsReflectedAfterEndUpdate()
        {
            var domain = new DomainOfExpertise();
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(domain);
            var iteration = new Iteration();
            this.openIterations.Add(iteration);
            this.viewModel.CurrentThing = iteration;

            var elementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid()
            };

            this.viewModel.CurrentThing.Element.Add(elementDefinition);

            // Clear the invocation recorded by the initial CurrentThing assignment above, so that Verify below
            // proves a NEW refresh happened in reaction to EndUpdate, not the one from initialisation.
            this.subscribedTableViewModel.Invocations.Clear();

            var isLoadingValues = new List<bool>();
            this.viewModel.WhenAnyValue(x => x.IsLoading).Subscribe(isLoadingValues.Add);

            this.messageBus.SendObjectChangeEvent(elementDefinition, EventKind.Added);
            this.messageBus.SendMessage(new SessionEvent(this.sessionService.Object.Session, SessionStatus.EndUpdate));

            Assert.That(() => isLoadingValues.Contains(true) && !this.viewModel.IsLoading, Is.True.After(2000, 25),
                "the view model never signalled a re-render in reaction to the external write");

            Assert.Multiple(() =>
            {
                this.subscribedTableViewModel.Verify(x => x.UpdateProperties(It.IsAny<IEnumerable<ParameterSubscription>>(),
                        It.IsAny<IEnumerable<Option>>(), It.IsAny<Iteration>()), Times.AtLeastOnce,
                    "SubscriptionDashboardBodyViewModel has no OnEndUpdate override, so a cross-panel write's EndUpdate " +
                    "never reaches OnSessionRefreshed/UpdateTables.");

                Assert.That(isLoadingValues, Has.Some.EqualTo(true),
                    "IsLoading must toggle so the Subscription Dashboard re-renders.");
            });
        }
    }
}
