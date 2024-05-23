// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="NotificationServiceTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.Services.NotificationService
{
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.NotificationService;

    using DynamicData;

    using FluentResults;

    using NUnit.Framework;

    [TestFixture]
    public class NotificationServiceTestFixture
    {
        private NotificationService notificationService;

        [SetUp]
        public void Setup()
        {
            this.notificationService = new NotificationService();
        }

        [Test]
        public void VerifyNotificationService()
        {
            Assert.That(this.notificationService.NotificationCount, Is.EqualTo(0));

            this.notificationService.AddNotifications(-2);
            Assert.That(this.notificationService.NotificationCount, Is.EqualTo(0));

            this.notificationService.AddNotifications(4);
            Assert.That(this.notificationService.NotificationCount, Is.EqualTo(4));

            this.notificationService.RemoveNotifications(1);
            Assert.That(this.notificationService.NotificationCount, Is.EqualTo(3));

            this.notificationService.RemoveNotifications(-1);
            Assert.That(this.notificationService.NotificationCount, Is.EqualTo(3));

            this.notificationService.RemoveNotifications(5);
            Assert.That(this.notificationService.NotificationCount, Is.EqualTo(0));
        }

        [Test]
        public void VerifyResultsList()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.notificationService.Results, Is.Not.Null);
                Assert.That(this.notificationService.Results, Has.Count.EqualTo(0));
            });

            this.notificationService.Results.Add(new ResultNotification(new Result(), new NotificationDescription()));
            Assert.That(this.notificationService.Results, Has.Count.EqualTo(1));
        }
    }
}
