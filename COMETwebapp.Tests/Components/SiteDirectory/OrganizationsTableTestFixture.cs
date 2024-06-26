// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrganizationsTableTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

    using Microsoft.AspNetCore.Components.Forms;
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
            this.viewModel.Setup(x => x.CurrentThing).Returns(new Organization());

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
        public async Task VerifyAddingOrEditingOrganization()
        {
            var renderer = this.context.RenderComponent<OrganizationsTable>();

            var addOrganizationButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "dataItemDetailsButton");
            await renderer.InvokeAsync(addOrganizationButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.IsOnEditMode, Is.EqualTo(true));
                Assert.That(this.viewModel.Object.CurrentThing, Is.InstanceOf(typeof(Organization)));
            });

            var organizationsGrid = renderer.FindComponent<DxGrid>();
            await renderer.InvokeAsync(() => organizationsGrid.Instance.SelectedDataItemChanged.InvokeAsync(new OrganizationRowViewModel(this.organization1)));
            Assert.That(renderer.Instance.IsOnEditMode, Is.EqualTo(true));

            var organizationsForm = renderer.FindComponent<OrganizationsForm>();
            var organizationsEditForm = organizationsForm.FindComponent<EditForm>();
            await organizationsForm.InvokeAsync(organizationsEditForm.Instance.OnValidSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.CreateOrEditOrganization(false), Times.Once);
                Assert.That(this.viewModel.Object.CurrentThing, Is.InstanceOf(typeof(Organization)));
            });

            var form = renderer.FindComponent<DxGrid>();
            await renderer.InvokeAsync(form.Instance.EditModelSaving.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditOrganization(false), Times.Once);
        }
    }
}
