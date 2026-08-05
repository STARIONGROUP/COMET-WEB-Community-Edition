// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ReconnectStateBannerTestFixture.cs" company="Starion Group S.A.">
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
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.Shared.Reconnection
{
    using Bunit;

    using COMETwebapp.Components.Shared.Reconnection;

    using NUnit.Framework;

    [TestFixture]
    public class ReconnectStateBannerTestFixture
    {
        private BunitContext context;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
        }

        [TearDown]
        public void TearDown()
        {
            this.context.Dispose();
        }

        [Test]
        public void VerifyReconnectStateBanner()
        {
            var component = this.context.Render<ReconnectStateBanner>(parameters => parameters
                .Add(p => p.StateClass, "components-reconnect-show")
                .Add(p => p.BadgeType, "warning")
                .Add(p => p.Title, "Connection Lost")
                .Add(p => p.Text, "Network connection lost.")
                .Add(p => p.IsSpinner, true)
                .Add(p => p.ActionText, "Retry Now")
                .Add(p => p.ActionType, ReconnectActionType.Reconnect));

            var container = component.Find(".comet-reconnect-state");
            var titleNode = component.Find("strong");
            var actionButton = component.Find("button");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(container.ClassList.Contains("components-reconnect-show"), Is.True);
                Assert.That(titleNode.TextContent, Is.EqualTo("Connection Lost"));
                Assert.That(actionButton.GetAttribute("onclick"), Is.EqualTo("Blazor.reconnect()"));
            }
        }
    }
}
