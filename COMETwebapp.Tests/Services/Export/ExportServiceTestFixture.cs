// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ExportServiceTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Services.Export
{
    using System.IO;
    using System.Threading.Tasks;

    using COMETwebapp.Services.Export;
    using COMETwebapp.Services.Interoperability;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ExportServiceTestFixture
    {
        private Mock<IJsUtilitiesService> jsUtilitiesService;
        private ExportService exportService;

        [SetUp]
        public void SetUp()
        {
            this.jsUtilitiesService = new Mock<IJsUtilitiesService>();
            this.jsUtilitiesService.Setup(x => x.DownloadFileFromStreamAsync(It.IsAny<Stream>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            this.exportService = new ExportService(this.jsUtilitiesService.Object);
        }

        [Test]
        public async Task VerifyExportAndDownloadAsync()
        {
            var stream = new MemoryStream([1, 2, 3]);
            var exporter = new Mock<IExporter>();
            exporter.Setup(x => x.FileName).Returns("export.bin");
            exporter.Setup(x => x.Export()).Returns(stream);

            await this.exportService.ExportAndDownloadAsync(exporter.Object);

            this.jsUtilitiesService.Verify(x => x.DownloadFileFromStreamAsync(stream, "export.bin"), Times.Once);
        }
    }
}
