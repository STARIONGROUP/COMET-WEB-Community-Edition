// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrganizationsTableTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.SiteDirectory
{
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.SiteDirectory;
    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Organizations;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class OrganizationsTableTestFixture
    {
        private TestContext context;
        private Mock<IOrganizationsTableViewModel> viewModel;
        private Mock<IShowHideDeprecatedThingsService> showHideService;
        private Organization organization1;
        private Organization organization2;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();

            this.viewModel = new Mock<IOrganizationsTableViewModel>();
            this.showHideService = new Mock<IShowHideDeprecatedThingsService>();
            this.showHideService.Setup(x => x.ShowDeprecatedThings).Returns(true);

            this.organization1 = new Organization()
            {
                Name = "A name",
                ShortName = "AName",
                Container = new SiteDirectory(){ ShortName = "siteDir" },
            };

            this.organization2 = new Organization()
            {
                Name = "B name",
                ShortName = "BName",
                Container = new SiteDirectory() { ShortName = "siteDir" },
                IsDeprecated = true
            };

            var rows = new SourceList<OrganizationRowViewModel>();
            rows.Add(new OrganizationRowViewModel(this.organization1));
            rows.Add(new OrganizationRowViewModel(this.organization2));

            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.ShowHideDeprecatedThingsService).Returns(this.showHideService.Object);
            this.viewModel.Setup(x => x.IsOnDeprecationMode).Returns(true);
            this.viewModel.Setup(x => x.Thing).Returns(new Organization());

            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.ConfigureDevExpressBlazor();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public void VerifyOnInitialized()
        {
            var renderer = this.context.RenderComponent<OrganizationsTable>();

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.ShouldCreateThing, Is.EqualTo(false));
                Assert.That(renderer.Instance.ViewModel, Is.Not.Null);
                Assert.That(renderer.Markup, Does.Contain(this.organization1.Name));
                Assert.That(renderer.Markup, Does.Contain(this.organization2.Name));
                this.viewModel.Verify(x => x.InitializeViewModel(), Times.Once);
            });
        }

        [Test]
        public async Task VerifyDeprecatingAndUndeprecatingOrganization()
        {
            var renderer = this.context.RenderComponent<OrganizationsTable>();

            var deprecateButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "deprecateButton");
            await renderer.InvokeAsync(deprecateButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OnDeprecateUnDeprecateButtonClick(It.IsAny<OrganizationRowViewModel>()), Times.Once);

            var unDeprecateButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "undeprecateButton");
            await renderer.InvokeAsync(unDeprecateButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OnDeprecateUnDeprecateButtonClick(It.IsAny<OrganizationRowViewModel>()), Times.Exactly(2));
        }

        [Test]
        public async Task VerifyAddingOrEditingOrganization()
        {
            var renderer = this.context.RenderComponent<OrganizationsTable>();

            var addOrganizationButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addOrganizationButton");
            await renderer.InvokeAsync(addOrganizationButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.ShouldCreateThing, Is.EqualTo(true));
                Assert.That(this.viewModel.Object.Thing, Is.InstanceOf(typeof(Organization)));
            });
            
            var editORganizationButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editOrganizationButton");
            await renderer.InvokeAsync(editORganizationButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.ShouldCreateThing, Is.EqualTo(false));
                Assert.That(this.viewModel.Object.Thing, Is.InstanceOf(typeof(Organization)));
            });
        }
    }
}
