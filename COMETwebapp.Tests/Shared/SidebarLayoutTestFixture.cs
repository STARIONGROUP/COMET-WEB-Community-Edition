// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SidebarLayoutTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Shared
{
    using Bunit;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Shared.TopMenuEntry;

    using COMETwebapp.Shared;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SidebarLayoutTestFixture
    {
        private BunitContext context;
        private Mock<ISessionService> sessionService;

        [SetUp]
        public void Setup()
        {
            this.context = new BunitContext();
            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.IsSessionOpen).Returns(false);

            var sessionMenuViewModel = new Mock<ISessionMenuViewModel>();
            sessionMenuViewModel.Setup(x => x.SessionService).Returns(this.sessionService.Object);

            this.context.Services.AddSingleton(sessionMenuViewModel.Object);
            this.context.ConfigureDevExpressBlazor();
            this.context.JSInterop.SetupVoid("cometKeyboard.init").SetVoidResult();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyLayoutExposesSkipLinksAndMainLandmark()
        {
            var renderer = this.context.Render<SidebarLayout>(parameters => parameters.Add(p => p.Body, "<span>content</span>"));

            var skipLinks = renderer.FindAll(".skip-link");
            var main = renderer.Find("#main-content");

            Assert.Multiple(() =>
            {
                Assert.That(skipLinks, Has.Count.EqualTo(2));
                Assert.That(skipLinks[0].GetAttribute("href"), Is.EqualTo("#main-side-bar"));
                Assert.That(skipLinks[1].GetAttribute("href"), Is.EqualTo("#main-content"));
                Assert.That(main.TagName, Is.EqualTo("MAIN"));
                Assert.That(main.TextContent, Does.Contain("content"));
            });
        }

        [Test]
        public void VerifyKeyboardHelpersAreInitializedOnRender()
        {
            this.context.Render<SidebarLayout>(parameters => parameters.Add(p => p.Body, "<span>content</span>"));

            this.context.JSInterop.VerifyInvoke("cometKeyboard.init");
        }
    }
}
