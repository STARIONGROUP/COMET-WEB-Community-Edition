// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ArchiveFileServiceTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMET.Web.Common.Tests.Services.SessionManagement
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using COMET.Web.Common.Services.SessionManagement;

    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ArchiveFileServiceTestFixture
    {
        private ArchiveFileService archiveFileService;
        private Mock<ILogger<ArchiveFileService>> logger;

        [SetUp]
        public void Setup()
        {
            this.logger = new Mock<ILogger<ArchiveFileService>>();
            this.archiveFileService = new ArchiveFileService(this.logger.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.archiveFileService.Remove();
        }

        [Test]
        public async Task VerifyPersistAsync()
        {
            var nullFileResult = await this.archiveFileService.PersistAsync(null);

            Assert.Multiple(() =>
            {
                Assert.That(nullFileResult.IsFailed, Is.True);
                Assert.That(this.archiveFileService.ArchivePath, Is.Null);
            });

            var oversizedFile = CreateMockedFile(ArchiveFileService.MaximumArchiveSize + 1, []);
            var oversizedResult = await this.archiveFileService.PersistAsync(oversizedFile.Object);

            Assert.Multiple(() =>
            {
                Assert.That(oversizedResult.IsFailed, Is.True);
                Assert.That(this.archiveFileService.ArchivePath, Is.Null);
            });

            var content = "content"u8.ToArray();
            var file = CreateMockedFile(content.Length, content);
            var result = await this.archiveFileService.PersistAsync(file.Object);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(this.archiveFileService.ArchivePath, Is.EqualTo(result.Value));
                Assert.That(File.Exists(result.Value), Is.True);
                Assert.That(File.ReadAllBytes(result.Value), Is.EqualTo(content));
            });

            var firstArchivePath = result.Value;
            var secondContent = "other"u8.ToArray();
            var secondFile = CreateMockedFile(secondContent.Length, secondContent);
            var secondResult = await this.archiveFileService.PersistAsync(secondFile.Object);

            Assert.Multiple(() =>
            {
                Assert.That(secondResult.IsSuccess, Is.True);
                Assert.That(File.Exists(firstArchivePath), Is.False);
                Assert.That(File.Exists(secondResult.Value), Is.True);
                Assert.That(File.ReadAllBytes(secondResult.Value), Is.EqualTo(secondContent));
            });
        }

        [Test]
        public async Task VerifyRemove()
        {
            var content = "content"u8.ToArray();
            var file = CreateMockedFile(content.Length, content);
            var result = await this.archiveFileService.PersistAsync(file.Object);
            var archivePath = result.Value;

            this.archiveFileService.Remove();

            Assert.Multiple(() =>
            {
                Assert.That(this.archiveFileService.ArchivePath, Is.Null);
                Assert.That(File.Exists(archivePath), Is.False);
            });

            Assert.That(() => this.archiveFileService.Remove(), Throws.Nothing);
        }

        [Test]
        public async Task VerifyDisposeAsync()
        {
            var content = "content"u8.ToArray();
            var file = CreateMockedFile(content.Length, content);
            var result = await this.archiveFileService.PersistAsync(file.Object);
            var archivePath = result.Value;

            await this.archiveFileService.DisposeAsync();

            Assert.That(File.Exists(archivePath), Is.False);
        }

        [Test]
        public void VerifyRemoveOrphanedArchives()
        {
            var archiveDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "comet-web-archives"));
            var orphanedArchive = Path.Combine(archiveDirectory.FullName, $"{Guid.NewGuid()}.zip");
            var freshArchive = Path.Combine(archiveDirectory.FullName, $"{Guid.NewGuid()}.zip");

            try
            {
                File.WriteAllText(orphanedArchive, "orphaned");
                File.SetLastWriteTimeUtc(orphanedArchive, DateTime.UtcNow - TimeSpan.FromDays(1));

                File.WriteAllText(freshArchive, "fresh");
                File.SetLastWriteTimeUtc(freshArchive, DateTime.UtcNow);

                var removalLogger = new Mock<ILogger>();
                ArchiveFileService.RemoveOrphanedArchives(TimeSpan.FromHours(1), removalLogger.Object);

                Assert.Multiple(() =>
                {
                    Assert.That(File.Exists(orphanedArchive), Is.False);
                    Assert.That(File.Exists(freshArchive), Is.True);
                });
            }
            finally
            {
                if (File.Exists(orphanedArchive))
                {
                    File.Delete(orphanedArchive);
                }

                if (File.Exists(freshArchive))
                {
                    File.Delete(freshArchive);
                }
            }
        }

        /// <summary>
        /// Creates a mocked <see cref="IBrowserFile" /> with the provided size and content
        /// </summary>
        /// <param name="size">The reported size, in bytes, of the file</param>
        /// <param name="content">The bytes returned by <see cref="IBrowserFile.OpenReadStream" /></param>
        /// <returns>The configured <see cref="Mock{T}" /></returns>
        private static Mock<IBrowserFile> CreateMockedFile(long size, byte[] content)
        {
            var file = new Mock<IBrowserFile>();
            file.Setup(x => x.Size).Returns(size);
            file.Setup(x => x.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(new MemoryStream(content));
            return file;
        }
    }
}
