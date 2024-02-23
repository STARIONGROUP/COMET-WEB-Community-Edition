// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleIterationApplicationBaseViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Tests.ViewModels.Components.Applications
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SingleIterationApplicationBaseViewModelTestFixture
    {
        private SingleIterationApplicationViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private ICDPMessageBus messageBus;

        private class SingleIterationApplicationViewModel : SingleIterationApplicationBaseViewModel
        {
            public int OnSessionRefreshCount;

            /// <summary>
            /// Initializes a new instance of the <see cref="SingleIterationApplicationBaseViewModel" /> class.
            /// </summary>
            /// <param name="sessionService">The <see cref="ISessionService" /></param>
            /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
            public SingleIterationApplicationViewModel(ISessionService sessionService, ICDPMessageBus messageBus) : base(sessionService, messageBus)
            {
            }

            public void Initialize(IEnumerable<Type> types)
            {
                this.InitializeSubscriptions(types);
            }

            /// <summary>
            /// Handles the refresh of the current <see cref="ISession" />
            /// </summary>
            /// <returns>A <see cref="Task" /></returns>
            protected override Task OnSessionRefreshed()
            {
                this.OnSessionRefreshCount++;
                return Task.CompletedTask;
            }
        }

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.messageBus = new CDPMessageBus();
            this.viewModel = new SingleIterationApplicationViewModel(this.sessionService.Object, this.messageBus);
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyObjectChangeSubscriptions()
        {
            this.viewModel.Initialize(new List<Type> { typeof(ElementBase) });
            var elementDefinition = new ElementDefinition { Container = new Iteration() };

            this.messageBus.SendObjectChangeEvent(elementDefinition, EventKind.Added);
            Assert.That(this.viewModel.GetAddedThings, Is.Empty);

            this.viewModel.CurrentThing = new Iteration { Iid = Guid.NewGuid() };

            this.messageBus.SendObjectChangeEvent(elementDefinition, EventKind.Added);
            Assert.That(this.viewModel.GetAddedThings, Is.Empty);

            this.viewModel.CurrentThing.Element.Add(elementDefinition);

            this.messageBus.SendObjectChangeEvent(elementDefinition, EventKind.Added);
            Assert.That(this.viewModel.GetAddedThings, Has.Count.EqualTo(1));
        }

        [Test]
        public void VerifyOnIterationOrDomainChanged()
        {
            var iteration = new Iteration();
            var domain = new DomainOfExpertise();
            this.sessionService.Setup(x => x.GetDomainOfExpertise(iteration)).Returns(domain);
            Assert.That(this.viewModel.CurrentDomain, Is.Null);
            this.viewModel.CurrentThing = iteration;
            this.viewModel.HasSetInitialValuesOnce = true;
            Assert.That(this.viewModel.CurrentDomain, Is.EqualTo(domain));
            var newDomain = new DomainOfExpertise();
            this.sessionService.Setup(x => x.GetDomainOfExpertise(iteration)).Returns(newDomain);
            this.messageBus.SendMessage(new DomainChangedEvent(iteration, newDomain));
            Assert.That(this.viewModel.CurrentDomain, Is.EqualTo(newDomain));
            this.viewModel.CurrentThing = null;
            Assert.That(this.viewModel.CurrentDomain, Is.Null);
        }

        [Test]
        public void VerifySessionSubscription()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.OnSessionRefreshCount, Is.EqualTo(0));
                Assert.That(this.viewModel.IsRefreshing, Is.False);
            });
            
            this.messageBus.SendMessage(SessionStateKind.Refreshing);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.OnSessionRefreshCount, Is.EqualTo(0));
                Assert.That(this.viewModel.IsRefreshing, Is.True);
            });

            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.OnSessionRefreshCount, Is.EqualTo(1));
                Assert.That(this.viewModel.IsRefreshing, Is.False);
            });

            Assert.Multiple(() =>
            {
                Assert.That(() => this.messageBus.SendMessage(SessionStateKind.IterationClosed), Throws.Nothing);
                Assert.That(() => this.messageBus.SendMessage(SessionStateKind.IterationOpened), Throws.Nothing);
                Assert.That(() => this.messageBus.SendMessage((SessionStateKind)10), Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
            });
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CurrentDomain, Is.Null);
                Assert.That(this.viewModel.IsLoading, Is.False);
                Assert.That(this.viewModel.HasSetInitialValuesOnce, Is.False);
                Assert.That(this.viewModel.CurrentThing, Is.Null);
            });
        }
    }
}
