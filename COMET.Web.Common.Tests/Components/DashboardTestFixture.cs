﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DashboardTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Tests.Components
{
    using AngleSharp.Html.Dom;

    using Bunit;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.RegistrationService;
    using COMET.Web.Common.Services.StringTableService;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class DashboardTestFixture
    {
        private List<Application> applications;
        private Mock<IRegistrationService> registrationService;
        private Mock<IStringTableService> configurationService;
        private TestContext context;

        [SetUp]
        public void Setup()
        {
            this.applications = new List<Application>()
            {
                new()
                {
                    Color = "#123456",
                    Description = "A description",
                    Icon = "eye",
                    Name = "Application 1",
                    Url = "/Application1"
                },
                new()
                {
                    Color = "#123456",
                    Description = "A disabled description",
                    Icon = "file",
                    Name = "Application 2",
                    Url = "/Application2", 
                    IsDisabled = true
                }
			};

            this.registrationService = new Mock<IRegistrationService>();

            this.registrationService.Setup(x => x.RegisteredApplications)
                .Returns(this.applications);

            this.configurationService = new Mock<IStringTableService>();
            this.configurationService.Setup(x => x.GetText(TextConfigurationKind.LandingPageTitle)).Returns(string.Empty);

            this.context = new TestContext();
            this.context.Services.AddSingleton(this.registrationService.Object);
            this.context.Services.AddSingleton(this.configurationService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            this.context.Dispose();
        }

        [Test]
        public void VerifyDashboard()
        {
            var renderer = this.context.RenderComponent<Dashboard>();
            var applicationCards = renderer.FindComponents<ApplicationCard>();
            Assert.That(applicationCards, Has.Count.EqualTo(2));

            var applicationCard = applicationCards[0];
            Assert.That(applicationCard.Instance.CurrentApplication, Is.EqualTo(this.applications[0]));
            var navigationManager = this.context.Services.GetService<NavigationManager>()!;
            var link = (IHtmlAnchorElement)applicationCard.Find("a");
            navigationManager.NavigateTo(link.Href);

            Assert.That(navigationManager.Uri, Does.EndWith(this.applications[0].Url));

            applicationCard = applicationCards[1];
            Assert.That(() => applicationCard.Find(".disabled"), Throws.Nothing);
        }
	}
}
