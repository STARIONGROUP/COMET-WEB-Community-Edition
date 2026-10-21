// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DataItemDetailsComponentTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.Common
{
    using System.Linq;

    using Bunit;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Common;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class DataItemDetailsComponentTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<DataItemDetailsComponent> renderer;
        private Mock<ISessionService> sessionService;
        private int clickCount;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.IsReadOnly).Returns(false);
            this.context.Services.AddSingleton(this.sessionService.Object);

            this.clickCount = 0;

            this.renderer = this.context.Render<DataItemDetailsComponent>(parameters => parameters
                .Add(p => p.IsSelected, false)
                .Add(p => p.OnButtonClick, () => this.clickCount++));
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyIsAllowedToCreateDefaultsToFalse()
        {
            // Fail-closed: a host that renders an add button but forgets to bind IsAllowedToCreate from its own
            // permission check must get a disabled button, never one that silently ignores the user's actual
            // write permission.
            var button = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "dataItemDetailsButton");

            Assert.That(button.Instance.Enabled, Is.False, "the default must not grant create access to a host that forgot to bind it");

            this.renderer.Render(parameters => parameters
                .Add(p => p.IsSelected, false)
                .Add(p => p.OnButtonClick, () => this.clickCount++)
                .Add(p => p.IsAllowedToCreate, true));

            button = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "dataItemDetailsButton");

            Assert.That(button.Instance.Enabled, Is.True, "a host that explicitly grants create access gets an enabled button");
        }
    }
}
