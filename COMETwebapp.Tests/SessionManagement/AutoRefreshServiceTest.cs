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

namespace COMETwebapp.Tests.SessionManagement
{
    using CDP4Dal;
    using COMETwebapp.SessionManagement;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Reactive.Linq;
    using System.Threading;

    [TestFixture]
    internal class AutoRefreshServiceTest
    {
        private Mock<ISessionAnchor> sessionAnchor;
        private AutoRefreshService autoRefreshService;

        [SetUp]
        public void Setup()
        {
            
            this.sessionAnchor = new Mock<ISessionAnchor>();
            
            this.autoRefreshService = new AutoRefreshService(this.sessionAnchor.Object);
        }

        [Test]
        public void VerifySetTimer()
        {
            this.autoRefreshService.IsAutoRefreshEnabled = true;
            this.autoRefreshService.AutoRefreshInterval = 1;
            this.autoRefreshService.SetTimer();
            Thread.Sleep(5000);
            this.sessionAnchor.Verify(x => x.RefreshSession(), Times.AtLeastOnce);

            this.sessionAnchor.Invocations.Clear();

            this.autoRefreshService.IsAutoRefreshEnabled = false;
            this.autoRefreshService.SetTimer();
            Thread.Sleep(5000);
            this.sessionAnchor.Verify(x => x.RefreshSession(), Times.Never);

            this.autoRefreshService.Dispose();
        }
    }
}
