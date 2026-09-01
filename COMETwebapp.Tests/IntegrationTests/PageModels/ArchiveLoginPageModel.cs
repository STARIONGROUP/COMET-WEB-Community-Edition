// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ArchiveLoginPageModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.IntegrationTests.PageModels
{
    using System.Threading.Tasks;

    using Microsoft.Playwright;

    using static Microsoft.Playwright.Assertions;

    /// <summary>
    /// Page object for the "Or open an archive" form on the landing page, which opens an ECSS-E-TM-10-25 Annex C3
    /// archive read-only. Encapsulates the selectors and steps so a selector change is a one-line fix here rather than
    /// a change scattered across every fixture.
    /// </summary>
    public class ArchiveLoginPageModel
    {
        /// <summary>
        /// The <see cref="IPage" /> this page object drives.
        /// </summary>
        private readonly IPage page;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveLoginPageModel" /> class.
        /// </summary>
        /// <param name="page">The Playwright <see cref="IPage" />.</param>
        public ArchiveLoginPageModel(IPage page)
        {
            this.page = page;
        }

        /// <summary>
        /// Gets the combo box that chooses between connecting to a server and opening an archive.
        /// </summary>
        public ILocator ConnectionKindSelector => this.page.Locator("#connection-kind");

        /// <summary>
        /// Gets the archive login form.
        /// </summary>
        public ILocator Form => this.page.Locator("#archive-login-form");

        /// <summary>
        /// Gets the zone that an archive can be dropped onto.
        /// </summary>
        public ILocator DropZone => this.page.Locator("#archive-drop-zone");

        /// <summary>
        /// Gets the file input that the archive is uploaded through.
        /// </summary>
        public ILocator FileInput => this.page.Locator("#archive-file");

        /// <summary>
        /// Gets the submit button that opens the uploaded archive.
        /// </summary>
        public ILocator OpenArchiveButton => this.page.Locator("#archive-connectbtn");

        /// <summary>
        /// Gets the list of errors reported by the archive login form.
        /// </summary>
        public ILocator Errors => this.page.Locator("#archive-login-errors");

        /// <summary>
        /// Gets the client-side validation messages shown on the archive login form.
        /// </summary>
        public ILocator ValidationErrors => this.page.Locator("#archive-login-validation");

        /// <summary>
        /// Gets the banner shown once a model is opened from an archive, warning that the model is read-only.
        /// </summary>
        public ILocator ReadOnlyBanner => this.page.Locator("#read-only-banner");

        /// <summary>
        /// Chooses the archive option in the connection-kind combo box, which swaps the server login form for the
        /// archive login form. The server login is offered by default, so every archive test starts with this.
        /// </summary>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task SelectArchiveConnectionAsync()
        {
            // Wait for the server login form's application-owned data-app-ready marker first. The selector is rendered
            // by the server before the Blazor circuit attaches its change handler, so selecting too early changes the
            // DOM element but never reaches the server-side component, and the archive form is never swapped in.
            await Expect(this.page.Locator("#login-form[data-app-ready]"))
                .ToBeAttachedAsync(new LocatorAssertionsToBeAttachedOptions { Timeout = E2ETestBase.ServerRoundTripTimeoutMilliseconds });

            await this.ConnectionKindSelector.SelectOptionAsync("archive");

            await Expect(this.Form).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = E2ETestBase.ServerRoundTripTimeoutMilliseconds });
        }

        /// <summary>
        /// Fills in the username and archive password, flushing the last value through the Blazor circuit.
        /// </summary>
        /// <param name="userName">The short name of the person, contained by the archive, to open the session as.</param>
        /// <param name="password">The password that the archive is encrypted with.</param>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task EnterCredentialsAsync(string userName, string password)
        {
            await this.TypeAsync("archive-username", userName);
            await this.TypeAsync("archive-password", password);

            // BindValueMode debounces before pushing the value through the Blazor circuit; blur the last field so its
            // final value reaches the server-side DTO before the submit.
            await this.TextInput("archive-password").BlurAsync();
        }

        /// <summary>
        /// Performs the full archive login flow and waits until the session is authenticated.
        /// </summary>
        /// <param name="archivePath">The full path of the Annex C3 archive to upload.</param>
        /// <param name="userName">The short name of the person, contained by the archive, to open the session as.</param>
        /// <param name="password">The password that the archive is encrypted with.</param>
        /// <returns>A <see cref="Task" />.</returns>
        public async Task OpenArchiveAsync(string archivePath, string userName, string password)
        {
            await this.FileInput.SetInputFilesAsync(archivePath);

            await this.EnterCredentialsAsync(userName, password);

            await this.OpenArchiveButton.ClickAsync();

            // Opening the archive reads and deserialises the whole model server-side, which can outlast the action default.
            await Expect(this.page.Locator("#unauthorized-notice")).ToBeHiddenAsync(new LocatorAssertionsToBeHiddenOptions { Timeout = E2ETestBase.ServerRoundTripTimeoutMilliseconds });
        }

        /// <summary>
        /// Types a value into an archive login text box deterministically. It first waits for the form's
        /// application-owned <c>data-app-ready</c> marker, because on a cold Blazor circuit the field re-renders as it
        /// wires up and wipes anything typed too early. It then types at a cadence the binding can keep up with and
        /// confirms the field holds the full value.
        /// </summary>
        /// <param name="id">The text box component id.</param>
        /// <param name="value">The value to type.</param>
        /// <returns>A <see cref="Task" />.</returns>
        private async Task TypeAsync(string id, string value)
        {
            await Expect(this.page.Locator("#archive-login-form[data-app-ready]"))
                .ToBeAttachedAsync(new LocatorAssertionsToBeAttachedOptions { Timeout = E2ETestBase.ServerRoundTripTimeoutMilliseconds });

            var input = this.TextInput(id);
            await input.PressSequentiallyAsync(value, TypingCadence);
            await Expect(input).ToHaveValueAsync(value);
        }

        /// <summary>
        /// The per-keystroke typing cadence used for the DevExpress archive login fields, so each keystroke's binding
        /// commits through the Blazor circuit in order.
        /// </summary>
        private static LocatorPressSequentiallyOptions TypingCadence => new() { Delay = 30 };

        /// <summary>
        /// Gets the native input element rendered inside the DevExpress text box with the provided component id.
        /// </summary>
        /// <param name="id">The text box component id.</param>
        /// <returns>The <see cref="ILocator" /> of the native input.</returns>
        private ILocator TextInput(string id)
        {
            return this.page.Locator($"#{id} input");
        }
    }
}
