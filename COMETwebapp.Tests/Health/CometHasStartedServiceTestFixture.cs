// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CometHasStartedServiceTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Health
{
    using System;

    using COMETwebapp.Health;

    using NUnit.Framework;

    [TestFixture]
    public class CometHasStartedServiceTestFixture
    {
        private CometHasStartedService service;

        [SetUp]
        public void SetUp()
        {
            this.service = new CometHasStartedService();
        }

        [Test]
        public void VerifyInitialState()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.service.HasStarted, Is.False);
                Assert.That(this.service.StartedAt, Is.EqualTo(DateTime.MinValue));
            });
        }

        [Test]
        public void VerifyMarkStartedFlipsTheFlag()
        {
            var before = DateTime.UtcNow;
            this.service.MarkStarted();
            var after = DateTime.UtcNow;

            Assert.Multiple(() =>
            {
                Assert.That(this.service.HasStarted, Is.True);
                Assert.That(this.service.StartedAt, Is.InRange(before, after));
                Assert.That(this.service.StartedAt.Kind, Is.EqualTo(DateTimeKind.Utc));
            });
        }

        [Test]
        public void VerifyMarkStartedIsIdempotent()
        {
            this.service.MarkStarted();
            var firstTimestamp = this.service.StartedAt;

            System.Threading.Thread.Sleep(5);
            this.service.MarkStarted();

            Assert.Multiple(() =>
            {
                Assert.That(this.service.HasStarted, Is.True);
                Assert.That(this.service.StartedAt, Is.EqualTo(firstTimestamp));
            });
        }
    }
}
