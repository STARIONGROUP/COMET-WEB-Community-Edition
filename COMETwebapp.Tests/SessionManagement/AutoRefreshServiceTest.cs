// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoRefreshServiceTest.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
//
//    Author: Justine Veirier d'aiguebonne, Sam Gerené, Alex Vorobiev, Alexander van Delft
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.SessionManagement
{
    using CDP4Dal;
    using COMETwebapp.SessionManagement;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [TestFixture]
    internal class AutoRefreshServiceTest
    {
        private Mock<ISession> session;
        private ISessionAnchor sessionAnchor;
        private AutoRefreshService autoRefreshService;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.sessionAnchor = new SessionAnchor() { Session = this.session.Object };
            this.autoRefreshService = new AutoRefreshService(this.sessionAnchor);
        }

        [Test]
        public void VerifySetTimer()
        {
            this.autoRefreshService.IsAutoRefreshEnabled = true;
            this.autoRefreshService.SetTimer();
            Assert.That(this.autoRefreshService.Timer, Is.Not.Null);
            Assert.That(this.autoRefreshService.Timer.Enabled, Is.True);

            this.autoRefreshService.IsAutoRefreshEnabled = false;
            this.autoRefreshService.SetTimer();
            Assert.That(this.autoRefreshService.Timer.Enabled, Is.False);


            var beginRefreshReceived = false;
            var endRefreshReceived = false;
            CDPMessageBus.Current.Listen<SessionStateKind>().Where(x => x == SessionStateKind.Refreshing).Subscribe(x =>
            {
                beginRefreshReceived = true;
            });
            CDPMessageBus.Current.Listen<SessionStateKind>().Where(x => x == SessionStateKind.UpToDate).Subscribe(x =>
            {
                endRefreshReceived = true;
            });

            this.autoRefreshService.IsAutoRefreshEnabled = true;
            this.autoRefreshService.AutoRefreshInterval = 3;
            this.autoRefreshService.SetTimer();
            Thread.Sleep(5000);

            Assert.IsTrue(beginRefreshReceived);
            Assert.IsTrue(endRefreshReceived);
        }
    }
}
