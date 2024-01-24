// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SessionMenuViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Tests.ViewModels.Shared.TopMenuEntry
{
    using CDP4Dal;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Services.NotificationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SessionMenuViewModelTestFixture
    {
        private SessionMenuViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IAutoRefreshService> autoRefreshService;
        private Mock<INotificationService> notificationService;
        private ICDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.autoRefreshService = new Mock<IAutoRefreshService>();
            this.notificationService = new Mock<INotificationService>();
            this.messageBus = new CDPMessageBus();

            this.viewModel = new SessionMenuViewModel(this.sessionService.Object, this.autoRefreshService.Object, this.notificationService.Object, this.messageBus);
        }

        [TearDown]
        public void Teardown()
        {
            this.viewModel.Dispose();
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AutoRefreshService, Is.EqualTo(this.autoRefreshService.Object));
                Assert.That(this.viewModel.SessionService, Is.EqualTo(this.sessionService.Object));
                Assert.That(this.viewModel.NotificationService, Is.EqualTo(this.notificationService.Object));
                Assert.That(this.viewModel.IsRefreshing, Is.False);
            });
        }

        [Test]
        public async Task VerifyRefreshSession()
        {
            await this.viewModel.RefreshSession();
            this.sessionService.Verify(x => x.RefreshSession(), Times.Once);
        }

        [Test]
        public void VerifyMessageBusSubscriptions()
        {
            this.messageBus.SendMessage(SessionStateKind.Refreshing);
            Assert.That(this.viewModel.IsRefreshing, Is.True);
            this.messageBus.SendMessage(SessionStateKind.RefreshEnded);
            Assert.That(this.viewModel.IsRefreshing, Is.False);

            Assert.Multiple(() =>
            {
                Assert.That(() => this.messageBus.SendMessage(SessionStateKind.IterationClosed), Throws.Nothing);
                Assert.That(() => this.messageBus.SendMessage(SessionStateKind.IterationOpened), Throws.Nothing);
                Assert.That(() => this.messageBus.SendMessage((SessionStateKind)10), Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
            });
        }
    }
}
