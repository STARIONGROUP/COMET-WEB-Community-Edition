// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DefinitionFormTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.Common
{
    using Bunit;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Common;

    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture for the <see cref="DefinitionForm" /> component.
    /// </summary>
    [TestFixture]
    public class DefinitionFormTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<DefinitionForm> renderer;
        private Definition definition;
        private Mock<ISessionService> sessionService;
        private bool isSaved;
        private bool isCanceled;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(this.sessionService.Object);

            // The container matters: a Definition without one is a Definition being created, which the form deliberately
            // lets through without a permission check. An existing Definition always has its container set.
            this.definition = new Definition
            {
                Iid = Guid.NewGuid(),
                Content = "Sample content",
                LanguageCode = "en-GB",
                Container = new Category { Iid = Guid.NewGuid() }
            };

            this.isSaved = false;
            this.isCanceled = false;

            var availableLanguages = new List<NaturalLanguage>
            {
                new() { LanguageCode = "en-GB", Name = "English", NativeName = "English" },
                new() { LanguageCode = "fr", Name = "French", NativeName = "Français" }
            };

            this.renderer = this.context.Render<DefinitionForm>(parameters => parameters
                .Add(p => p.Item, this.definition)
                .Add(p => p.AvailableLanguages, availableLanguages)
                .Add(p => p.OnSaved, () => Task.FromResult(this.isSaved = true))
                .Add(p => p.OnCanceled, () => Task.FromResult(this.isCanceled = true)));
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public void VerifyDefinitionFormRendering()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.renderer.Instance.Item, Is.SameAs(this.definition));
                Assert.That(this.renderer.Instance.AvailableLanguages.ToList(), Has.Count.EqualTo(2));
                Assert.That(this.renderer.Markup, Does.Contain("Sample content"));
            }
        }

        [Test]
        public async Task VerifyFormSubmissionAndCancellation()
        {
            var editForm = this.renderer.FindComponent<EditForm>();
            await this.renderer.InvokeAsync(editForm.Instance.OnValidSubmit.InvokeAsync);

            Assert.That(this.isSaved, Is.True);

            var formButtons = this.renderer.FindComponent<FormButtons>();
            await this.renderer.InvokeAsync(formButtons.Instance.OnCancel.InvokeAsync);

            Assert.That(this.isCanceled, Is.True);
        }

        [Test]
        public void VerifyIsSaveButtonEnabledHonoursWritePermission()
        {
            var permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(this.definition)).Returns(false);
            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(permissionService.Object);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);

            this.renderer.Render();

            Assert.That(this.renderer.FindComponent<FormButtons>().Instance.SaveButtonEnabled, Is.False,
                "editing a Definition the active user is not allowed to write must disable the Save button");

            permissionService.Setup(x => x.CanWrite(this.definition)).Returns(true);
            this.renderer.Render();

            Assert.That(this.renderer.FindComponent<FormButtons>().Instance.SaveButtonEnabled, Is.True,
                "editing a Definition the active user is allowed to write must enable the Save button");
        }
    }
}
