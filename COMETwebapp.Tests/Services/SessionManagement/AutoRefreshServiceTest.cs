// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoRefreshServiceTest.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
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

namespace COMETwebapp.Tests.Services.SessionManagement
{
    using CDP4Dal;
    using COMETwebapp.SessionManagement;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Reactive.Linq;
    using System.Threading;

    using COMETwebapp.Services.SessionManagement;

    [TestFixture]
    internal class AutoRefreshServiceTest
    {
        private Mock<ISessionService> sessionAnchor;
        private AutoRefreshService autoRefreshService;

        [SetUp]
        public void Setup()
        {

            sessionAnchor = new Mock<ISessionService>();

            autoRefreshService = new AutoRefreshService(sessionAnchor.Object);
        }

        [Test]
        public void VerifySetTimer()
        {
            autoRefreshService.IsAutoRefreshEnabled = true;
            autoRefreshService.AutoRefreshInterval = 1;
            autoRefreshService.SetTimer();
            Thread.Sleep(5000);
            sessionAnchor.Verify(x => x.RefreshSession(), Times.AtLeastOnce);

            sessionAnchor.Invocations.Clear();

            autoRefreshService.IsAutoRefreshEnabled = false;
            autoRefreshService.SetTimer();
            Thread.Sleep(5000);
            sessionAnchor.Verify(x => x.RefreshSession(), Times.Never);

            autoRefreshService.Dispose();
        }
    }
}
