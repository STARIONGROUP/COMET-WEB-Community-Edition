// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PageIntroBoxTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.Shared.PageIntroBox
{
    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Shared.PageIntroBox;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class PageIntroBoxTestFixture
    {
        private BunitContext context;
        private Mock<IStringTableService> stringTableService;
        private Mock<ISessionService> sessionService;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.stringTableService = new Mock<IStringTableService>();
            this.sessionService = new Mock<ISessionService>();

            var person = new Person();
            var siteDirectory = new SiteDirectory();
            var session = new Mock<ISession>();
            session.Setup(x => x.ActivePerson).Returns(person);
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);

            this.context.Services.AddSingleton(this.stringTableService.Object);
            this.context.Services.AddSingleton(this.sessionService.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.Dispose();
        }

        [Test]
        public async Task VerifyDismissAsync()
        {
            var application = new Application
            {
                Name = "Test App",
                Url = "test-url",
                PageIntroSummary = "Summary"
            };

            var pageIntroBox = this.context.Render<PageIntroBox>(parameters => parameters
                .Add(p => p.Application, application));

            var dismissButton = pageIntroBox.FindComponents<DxButton>().FirstOrDefault(x => x.Instance.CssClass == "page-intro-box__dismiss");
            Assert.That(dismissButton, Is.Not.Null);
            await pageIntroBox.InvokeAsync(() => dismissButton.Instance.Click.InvokeAsync());

            using (Assert.EnterMultipleScope())
            {
                this.sessionService.Verify(x => x.CreateOrUpdateThings(
                    It.IsAny<SiteDirectory>(),
                    It.Is<IReadOnlyCollection<Thing>>(things => things.Count == 2 && things.OfType<UserPreference>().Any(up => up.ShortName == application.GetPageIntroUserPreferenceKey() && up.Value == "true"))), Times.Once);

                Assert.That(pageIntroBox.Markup, Is.Empty);
            }
        }

        [Test]
        public void VerifyOnParametersSetAsync()
        {
            var application = new Application
            {
                Name = "Test App",
                Url = "test-url",
                PageIntroSummary = "Default Summary",
                PageIntroPoints = ["Default Point 1"]
            };

            this.stringTableService.Setup(x => x.GetText("test-url.PageIntro.Summary")).Returns("Overridden Summary");
            this.stringTableService.Setup(x => x.GetText("test-url.PageIntro.Points")).Returns("Overridden Point 1|Overridden Point 2");

            var pageIntroBox = this.context.Render<PageIntroBox>(parameters => parameters
                .Add(p => p.Application, application));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(pageIntroBox.Markup, Does.Contain("Overridden Summary"));
                Assert.That(pageIntroBox.Markup, Does.Contain("Overridden Point 1"));
                Assert.That(pageIntroBox.Markup, Does.Contain("Overridden Point 2"));
                Assert.That(pageIntroBox.Markup, Does.Not.Contain("Default Summary"));
                Assert.That(pageIntroBox.Markup, Does.Not.Contain("Default Point 1"));
            }

            this.stringTableService.Setup(x => x.GetText("test-url.PageIntro.Summary")).Returns((string)null);
            this.stringTableService.Setup(x => x.GetText("test-url.PageIntro.Points")).Returns((string)null);

            pageIntroBox.Render(parameters => parameters
                .Add(p => p.Application, application));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(pageIntroBox.Markup, Does.Contain("Default Summary"));
                Assert.That(pageIntroBox.Markup, Does.Contain("Default Point 1"));
                Assert.That(pageIntroBox.Markup, Does.Not.Contain("Overridden Summary"));
            }
        }

        [Test]
        public void VerifyRender()
        {
            var application = new Application
            {
                Name = "Test App",
                Url = "test-url",
                PageIntroSummary = "Summary",
                PageIntroPoints = ["Point 1", "Point 2"]
            };

            var pageIntroBox = this.context.Render<PageIntroBox>(parameters => parameters
                .Add(p => p.Application, application));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(pageIntroBox.Markup, Does.Contain("Test App"));
                Assert.That(pageIntroBox.Markup, Does.Contain("Summary"));
                Assert.That(pageIntroBox.Markup, Does.Contain("Point 1"));
                Assert.That(pageIntroBox.Markup, Does.Contain("Point 2"));
            }

            pageIntroBox.Render(parameters => parameters
                .Add(p => p.Application, new Application { Name = "Empty App", Url = "empty", PageIntroSummary = string.Empty, PageIntroPoints = [] }));

            Assert.That(pageIntroBox.Markup, Is.Empty);

            var person = this.sessionService.Object.Session.ActivePerson;
            person.UserPreference.Add(new UserPreference { ShortName = application.GetPageIntroUserPreferenceKey(), Value = "true" });

            pageIntroBox.Render(parameters => parameters
                .Add(p => p.Application, application));

            Assert.That(pageIntroBox.Markup, Is.Empty);

            pageIntroBox.Render(parameters => parameters
                .Add(p => p.IsPageIntroductionVisible, true));

            Assert.That(pageIntroBox.Markup, Does.Contain("Test App"));

            pageIntroBox.Render(parameters => parameters
                .Add(p => p.Application, null));

            Assert.That(pageIntroBox.Markup, Is.Empty);
        }
    }
}
