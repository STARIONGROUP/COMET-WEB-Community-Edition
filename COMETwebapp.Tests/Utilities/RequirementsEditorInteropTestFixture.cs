// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsEditorInteropTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Utilities
{
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Utilities;

    using Microsoft.Extensions.Logging;
    using Microsoft.JSInterop;
    using Microsoft.JSInterop.Infrastructure;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RequirementsEditorInteropTestFixture
    {
        private readonly ILogger logger = new Mock<ILogger>().Object;

        [Test]
        public async Task VerifyInitialiseTocResizerAsync()
        {
            var jsRuntime = new Mock<IJSRuntime>();
            jsRuntime.Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object[]>())).ReturnsAsync((IJSVoidResult)null);

            await RequirementsEditorInterop.InitialiseTocResizerAsync(jsRuntime.Object, this.logger, "resizer", "toc");

            jsRuntime.Verify(x => x.InvokeAsync<IJSVoidResult>("cometResizer.init", It.Is<object[]>(a => a[0].Equals("resizer") && a[1].Equals("toc") && a[2].Equals(150))), Times.Once);

            jsRuntime.Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object[]>())).ThrowsAsync(new InvalidOperationException("prerendering"));

            Assert.That(() => RequirementsEditorInterop.InitialiseTocResizerAsync(jsRuntime.Object, this.logger, "resizer", "toc"), Throws.Nothing, "the handle is cosmetic");
        }

        [Test]
        public async Task VerifyScrollIntoViewAsync()
        {
            var domDataService = new Mock<IDomDataService>();

            await RequirementsEditorInterop.ScrollIntoViewAsync(domDataService.Object, this.logger, "anchor");

            domDataService.Verify(x => x.ScrollElementIntoView("anchor"), Times.Once);

            domDataService.Setup(x => x.ScrollElementIntoView(It.IsAny<string>())).ThrowsAsync(new JSException("stale script"));
            Assert.That(() => RequirementsEditorInterop.ScrollIntoViewAsync(domDataService.Object, this.logger, "anchor"), Throws.Nothing);

            domDataService.Setup(x => x.ScrollElementIntoView(It.IsAny<string>())).ThrowsAsync(new JSDisconnectedException("gone"));
            Assert.That(() => RequirementsEditorInterop.ScrollIntoViewAsync(domDataService.Object, this.logger, "anchor"), Throws.Nothing);

            domDataService.Setup(x => x.ScrollElementIntoView(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("a real bug"));
            Assert.That(() => RequirementsEditorInterop.ScrollIntoViewAsync(domDataService.Object, this.logger, "anchor"), Throws.InvalidOperationException, "only the harmless interop errors are swallowed");
        }
    }
}
