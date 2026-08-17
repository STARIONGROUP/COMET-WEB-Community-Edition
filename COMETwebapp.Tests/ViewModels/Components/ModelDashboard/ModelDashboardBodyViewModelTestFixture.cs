// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModelDashboardBodyViewModelTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.Tests.ViewModels.Components.ModelDashboard
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.ModelDashboard;
    using COMETwebapp.ViewModels.Components.ModelDashboard.ParameterValues;

    using DynamicData;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class ModelDashboardBodyViewModelTestFixture
    {
        /// <summary>
        /// The view model under test.
        /// </summary>
        private ModelDashboardBodyViewModel viewModel;

        /// <summary>
        /// The mocked <see cref="ISessionService" />.
        /// </summary>
        private Mock<ISessionService> sessionService;

        /// <summary>
        /// The mocked <see cref="IParameterDashboardViewModel" />.
        /// </summary>
        private Mock<IParameterDashboardViewModel> parameterDashboard;

        /// <summary>
        /// The message bus used by the view model.
        /// </summary>
        private CDPMessageBus messageBus;

        /// <summary>
        /// The iteration providing the test fixture's element graph.
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// Builds the iteration and the view model with mocked dependencies.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.sessionService = new Mock<ISessionService>();
            this.parameterDashboard = new Mock<IParameterDashboardViewModel>();

            var session = new Mock<ISession>();
            this.sessionService.Setup(x => x.Session).Returns(session.Object);

            var domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS" };
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(domain);
            this.sessionService.Setup(x => x.GetModelDomains(It.IsAny<EngineeringModelSetup>())).Returns([domain]);

            var modelSetup = new EngineeringModelSetup { Name = "ModelName", ShortName = "ModelShortName" };

            this.iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup { IterationNumber = 1, Container = modelSetup }
            };

            var openIterations = new SourceList<Iteration>();
            openIterations.Add(this.iteration);
            this.sessionService.Setup(x => x.OpenIterations).Returns(openIterations);

            this.viewModel = new ModelDashboardBodyViewModel(this.sessionService.Object, this.parameterDashboard.Object, this.messageBus)
            {
                CurrentThing = this.iteration
            };
        }

        /// <summary>
        /// Tears down the message bus and disposes of the view model.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
            this.viewModel.Dispose();
        }

        [Test]
        public async Task VerifyExternalParameterChangeIsReflectedAfterEndUpdate()
        {
            var elementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid()
            };

            this.iteration.Element.Add(elementDefinition);

            // Clear the invocation recorded by the initial CurrentThing assignment in SetUp, so that Verify below
            // proves a NEW refresh happened in reaction to EndUpdate, not the one from initialisation.
            this.parameterDashboard.Invocations.Clear();

            var isLoadingValues = new List<bool>();
            this.viewModel.WhenAnyValue(x => x.IsLoading).Subscribe(isLoadingValues.Add);

            this.messageBus.SendObjectChangeEvent(elementDefinition, EventKind.Added);
            this.messageBus.SendMessage(new SessionEvent(this.sessionService.Object.Session, SessionStatus.EndUpdate));

            Assert.That(() => isLoadingValues.Contains(true) && !this.viewModel.IsLoading, Is.True.After(2000, 25),
                "the view model never signalled a re-render in reaction to the external write");

            Assert.Multiple(() =>
            {
                this.parameterDashboard.Verify(x => x.UpdateProperties(It.IsAny<Iteration>(), It.IsAny<IEnumerable<Option>>(),
                        It.IsAny<IEnumerable<ActualFiniteState>>(), It.IsAny<IEnumerable<ParameterType>>(), It.IsAny<DomainOfExpertise>(), It.IsAny<IEnumerable<DomainOfExpertise>>()),
                    Times.AtLeastOnce,
                    "ModelDashboardBodyViewModel has no OnEndUpdate override, so a cross-panel write's EndUpdate never " +
                    "reaches OnSessionRefreshed/UpdateDashboards.");

                Assert.That(isLoadingValues, Has.Some.EqualTo(true),
                    "IsLoading must toggle so the Model Dashboard re-renders.");
            });
        }
    }
}
