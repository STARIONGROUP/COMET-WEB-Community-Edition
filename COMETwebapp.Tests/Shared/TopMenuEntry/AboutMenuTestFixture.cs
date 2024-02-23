// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AboutMenuTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.Shared.TopMenuEntry
{
    using Bunit;

    using COMET.Web.Common.Services.VersionService;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Shared;
    using COMETwebapp.Shared.TopMenuEntry;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class AboutMenuTestFixture
    {
        private TestContext context;
        private Mock<IVersionService> versionService;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.versionService = new Mock<IVersionService>();
            this.versionService.Setup(x => x.GetVersion()).Returns("1.1.2");
            this.context.Services.AddSingleton(this.versionService.Object);
            this.context.Services.AddSingleton(new Mock<IHttpClientFactory>().Object);
            this.context.ConfigureDevExpressBlazor();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyAboutEntry()
        {
            var renderer = this.context.RenderComponent<DxMenu>(parameters =>
            {
                parameters.Add(p => p.Items, builder =>
                {
                    builder.OpenComponent(0, typeof(AboutMenu));
                    builder.CloseComponent();
                });
            });
            
            var aboutMenu = renderer.FindComponent<AboutMenu>();
            var popup = aboutMenu.FindComponent<DxPopup>();
            Assert.That(popup.Instance.Visible, Is.False);
            var aboutMenuEntry = renderer.FindComponent<DxMenuItem>();
            await renderer.InvokeAsync(aboutMenuEntry.Instance.Click.InvokeAsync);
            Assert.That(popup.Instance.Visible, Is.True);

            var aboutComponent = popup.FindComponent<About>();
            var boldedText = aboutComponent.Find("b");
            Assert.That(boldedText.TextContent, Is.EqualTo(this.versionService.Object.GetVersion()));

            var closeButton = popup.FindComponent<DxButton>();
            await renderer.InvokeAsync(closeButton.Instance.Click.InvokeAsync);
            Assert.That(popup.Instance.Visible, Is.False);
        }
    }
}
