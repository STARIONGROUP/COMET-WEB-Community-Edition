// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ArchiveLoginTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.IntegrationTests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using COMETwebapp.Tests.IntegrationTests.PageModels;

    using Microsoft.Playwright;

    using NUnit.Framework;

    using static Microsoft.Playwright.Assertions;

    /// <summary>
    /// End-to-end tests covering opening an ECSS-E-TM-10-25 Annex C3 archive read-only from the landing page. Each test
    /// uses its own fresh browser (these tests need an unauthenticated starting state).
    /// </summary>
    /// <remarks>
    /// Unlike the other end-to-end fixtures these tests do not need a COMET Web Services server: the archive is the data
    /// source. <see cref="VerifyUserCanOpenAnArchive" /> does need an archive to upload and goes inconclusive without one.
    /// </remarks>
    [TestFixture]
    public class ArchiveLoginTestFixture : E2ETestBase
    {
        /// <summary>
        /// Gets the full path of the Annex C3 archive to open (env <c>COMET_E2E_ARCHIVE</c>). No archive is committed to
        /// the repository, so the test that uploads one is skipped unless this points at a readable archive.
        /// </summary>
        private static string ArchivePath => Environment.GetEnvironmentVariable("COMET_E2E_ARCHIVE");

        /// <summary>
        /// Gets the password that the archive referenced by <see cref="ArchivePath" /> is encrypted with
        /// (env <c>COMET_E2E_ARCHIVE_PASSWORD</c>, defaults to the shared end-to-end password).
        /// </summary>
        private static string ArchivePassword => Environment.GetEnvironmentVariable("COMET_E2E_ARCHIVE_PASSWORD") ?? Password;

        /// <summary>
        /// Gets the <see cref="ArchiveLoginPageModel" /> page object for the current test.
        /// </summary>
        private ArchiveLoginPageModel Archive { get; set; }

        [SetUp]
        public async Task SetUp()
        {
            await this.StartBrowserAsync();
            this.Archive = new ArchiveLoginPageModel(this.Page);
        }

        [TearDown]
        public Task TearDown()
        {
            return this.StopBrowserAsync();
        }

        [Test]
        public async Task VerifyServerLoginIsOfferedByDefault()
        {
            await this.Login.NavigateAsync(AppUrl);

            // #login-form is present whether this server configuration needs the single-step or the multi-step
            // (source-address-then-credentials) flow, so it proves the server form rendered without depending on which
            // step is showing - unlike #connectbtn, which is only there once past a "Next" step some configurations
            // require.
            await Expect(this.Archive.ConnectionKindSelector).ToBeVisibleAsync();
            await Expect(this.Page.Locator("#login-form")).ToBeAttachedAsync();
            await Expect(this.Archive.Form).ToBeHiddenAsync();
        }

        [Test]
        public async Task VerifyArchiveFormIsOfferedOnTheLandingPage()
        {
            await this.Login.NavigateAsync(AppUrl);
            await this.Archive.SelectArchiveConnectionAsync();

            // The drop zone and its submit button render with the form, but on a cold Blazor circuit that first render
            // can outlast the shorter default used for steady-state assertions.
            var coldCircuit = new LocatorAssertionsToBeVisibleOptions { Timeout = ServerRoundTripTimeoutMilliseconds };

            await Expect(this.Archive.Form).ToBeVisibleAsync(coldCircuit);
            await Expect(this.Archive.FileInput).ToBeVisibleAsync(coldCircuit);
            await Expect(this.Archive.OpenArchiveButton).ToBeVisibleAsync(coldCircuit);
        }

        [Test]
        public async Task VerifyRequiredFieldValidationIsShown()
        {
            await this.Login.NavigateAsync(AppUrl);
            await this.Archive.SelectArchiveConnectionAsync();

            await this.Archive.OpenArchiveButton.ClickAsync();

            await Expect(this.Archive.ValidationErrors).ToContainTextAsync("The Username is required.");
            await Expect(this.Archive.ValidationErrors).ToContainTextAsync("The Password is required.");
            await Expect(this.Login.UnauthorizedNotice).ToBeVisibleAsync();
        }

        [Test]
        public async Task VerifyArchiveCanBeDroppedOnTheDropZone()
        {
            await this.Login.NavigateAsync(AppUrl);
            await this.Archive.SelectArchiveConnectionAsync();

            // Dropping a file onto a native <input type="file"> is what makes drag-and-drop work here, so the input has
            // to actually cover the drop zone. A file picker rendered at its default size next to the zone still passes
            // a SetInputFiles-based test while a real drop lands on the document and the browser opens the file.
            var zone = await this.Archive.DropZone.BoundingBoxAsync();
            var input = await this.Archive.FileInput.BoundingBoxAsync();

            Assert.That(zone, Is.Not.Null, "the drop zone should be laid out");
            Assert.That(input, Is.Not.Null, "the file input should be laid out");

            // The input is absolutely positioned inside the zone, so it fills the padding box and is inset by the zone's
            // 2px dashed border on each side. Allow for that rather than demanding an exact match.
            const int borderTolerance = 6;

            Assert.Multiple(() =>
            {
                Assert.That(input.Width, Is.EqualTo(zone.Width).Within(borderTolerance), "the file input should span the drop zone's width");
                Assert.That(input.Height, Is.EqualTo(zone.Height).Within(borderTolerance), "the file input should span the drop zone's height");
                Assert.That(input.X, Is.EqualTo(zone.X).Within(borderTolerance), "the file input should be aligned with the drop zone");
                Assert.That(input.Y, Is.EqualTo(zone.Y).Within(borderTolerance), "the file input should be aligned with the drop zone");

                // A file input left at its intrinsic size is roughly 250x20, so this is what actually distinguishes a
                // working drop zone from the unstyled control that silently let drops fall through to the document.
                Assert.That(input.Height, Is.GreaterThan(60), "the file input should be tall enough to catch a drop");
            });
        }

        [Test]
        public async Task VerifyMissingArchiveIsReported()
        {
            await this.Login.NavigateAsync(AppUrl);
            await this.Archive.SelectArchiveConnectionAsync();

            await this.Archive.EnterCredentialsAsync(Username, ArchivePassword);
            await this.Archive.OpenArchiveButton.ClickAsync();

            await Expect(this.Archive.Errors).ToContainTextAsync("Select an Annex C3 archive to open");
            await Expect(this.Login.UnauthorizedNotice).ToBeVisibleAsync();
        }

        [Test]
        public async Task VerifyUserCanOpenAnArchive()
        {
            Assume.That(ArchivePath, Is.Not.Null.And.Not.Empty, "Set COMET_E2E_ARCHIVE to the path of an Annex C3 archive to run this test.");
            Assume.That(File.Exists(ArchivePath), Is.True, $"The archive '{ArchivePath}' referenced by COMET_E2E_ARCHIVE does not exist.");

            await this.Login.NavigateAsync(AppUrl);
            await this.Archive.SelectArchiveConnectionAsync();
            await this.Archive.OpenArchiveAsync(ArchivePath, Username, ArchivePassword);

            await Expect(this.Home.SessionSidebar).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = ServerRoundTripTimeoutMilliseconds });
            await Expect(this.Archive.ReadOnlyBanner).ToBeVisibleAsync();
            await Expect(this.Tabs.BlazorError).ToBeHiddenAsync();
        }
    }
}
