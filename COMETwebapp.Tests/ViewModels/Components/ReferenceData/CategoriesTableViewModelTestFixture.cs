// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CategoriesTableViewModelTestFixture.cs" company="Starion Group S.A.">
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
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.ViewModels.Components.ReferenceData
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.ReferenceData.Categories;

    using Microsoft.Extensions.Logging;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class CategoriesTableViewModelTestFixture
    {
        private CategoriesTableViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private Mock<IPermissionService> permissionService;
        private CDPMessageBus messageBus;
        private SiteDirectory siteDirectory;
        private SiteReferenceDataLibrary siteReferenceDataLibrary;

        [SetUp]
        public void Setup()
        {
            this.sessionService = new Mock<ISessionService>();
            this.permissionService = new Mock<IPermissionService>();
            this.messageBus = new CDPMessageBus();

            this.siteReferenceDataLibrary = new SiteReferenceDataLibrary
            {
                ShortName = "rdl"
            };

            this.siteDirectory = new SiteDirectory
            {
                ShortName = "siteDirectory"
            };

            this.siteDirectory.SiteReferenceDataLibrary.Add(this.siteReferenceDataLibrary);

            var session = new Mock<ISession>();
            session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.GetSiteDirectory()).Returns(this.siteDirectory);

            this.viewModel = new CategoriesTableViewModel(this.sessionService.Object, new Mock<IShowHideDeprecatedThingsService>().Object, this.messageBus, new Mock<ILogger<CategoriesTableViewModel>>().Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
            this.viewModel.Dispose();
        }

        /// <summary>
        /// Verifies that the create permission of an empty table is evaluated against the
        /// <see cref="ReferenceDataLibrary" /> that a new <see cref="Category" /> would be created in, not against the
        /// <see cref="SiteDirectory" />. The permission used to be computed before the library was known, so the
        /// fallback to the <see cref="SiteDirectory" /> refused creation to users who may write the library.
        /// </summary>
        [Test]
        public void VerifyIsAllowedToCreateInAnEmptyTableUsesTheReferenceDataLibrary()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(false);
            this.permissionService.Setup(x => x.CanWrite(ClassKind.Category, this.siteReferenceDataLibrary)).Returns(true);

            this.viewModel.InitializeViewModel();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Rows.Count, Is.EqualTo(0));
                Assert.That(this.viewModel.IsAllowedToCreate, Is.True);
            });

            this.permissionService.Setup(x => x.CanWrite(ClassKind.Category, this.siteReferenceDataLibrary)).Returns(false);
            this.permissionService.Setup(x => x.CanWrite(ClassKind.Category, this.siteDirectory)).Returns(true);

            this.viewModel.InitializeViewModel();

            Assert.That(this.viewModel.IsAllowedToCreate, Is.False, "the SiteDirectory permission must not grant creation in the library");
        }
    }
}
