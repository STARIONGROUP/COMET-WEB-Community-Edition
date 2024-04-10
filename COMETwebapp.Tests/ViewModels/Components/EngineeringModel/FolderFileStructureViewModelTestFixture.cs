// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="FolderFileStructureViewModelTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.ViewModels.Components.EngineeringModel
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.EngineeringModel.FolderFileStructure;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class FolderFileStructureViewModelTestFixture
    {
        private FolderFileStructureViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private CommonFileStore commonFileStore;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();

            var siteDirectory = new SiteDirectory()
            {
                Domain =
                {
                    new DomainOfExpertise()
                    {
                        ShortName = "doe",
                        Name = "Domain Of Expertise"
                    }
                }
            };

            var folder1 = new Folder()
            {
                Name = "folder 1"
            };

            var folder2 = new Folder()
            {
                Name = "folder 2",
                ContainingFolder = folder1
            };

            var file = new File() { CurrentContainingFolder = folder1 };
            file.FileRevision.Add(new FileRevision());

            this.commonFileStore = new CommonFileStore()
            {
                Name = "CFS",
                Folder = { folder1 },
                File = { file },
                Owner = siteDirectory.Domain.First()
            };

            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(siteDirectory);
            this.viewModel = new FolderFileStructureViewModel(this.sessionService.Object);
        }

        [Test]
        public void VerifyInitializeViewModel()
        {
            this.viewModel.InitializeViewModel(this.commonFileStore);

            /*
             * Here the folder-file structure is:
             * - Root
             *      - Folder 1
             *          - Folder 2
             *          - File
             */
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.DomainsOfExpertise.Count(), Is.GreaterThan(0));
                Assert.That(this.viewModel.Structure, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.Structure.First().Content, Has.Count.EqualTo(2));
            });
        }
    }
}
