// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonRolesTableTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Components.SiteDirectory.Roles
{
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.SiteDirectory.Roles;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Roles;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class PersonRolesTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<PersonRolesTable> renderer;
        private Mock<IPersonRolesTableViewModel> viewModel;
        private PersonRole personRole;
        private bool wasRoleSelected;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<IPersonRolesTableViewModel>();

            this.personRole = new PersonRole()
            {
                Name = "Role 1",
                ShortName = "role1",
                Container = new SiteDirectory(){ ShortName = "siteDir" },
            };

            var rows = new SourceList<PersonRoleRowViewModel>();
            rows.Add(new PersonRoleRowViewModel(this.personRole));

            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.Thing).Returns(this.personRole);
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<PersonRolesTable>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);

                parameters.Add(p => p.OnRoleSelected, new EventCallbackFactory().Create<PersonRole>(this, () =>
                {
                    this.wasRoleSelected = true;
                }));
            });
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
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.ViewModel, Is.Not.Null);
                Assert.That(this.renderer.Markup, Does.Contain(this.personRole.Name));
                this.viewModel.Verify(x => x.InitializeViewModel(), Times.Once);
            });
        }

        [Test]
        public async Task VerifyAddPersonRoleClick()
        {
            var addPersonRoleButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addPersonRoleButton");
            await this.renderer.InvokeAsync(addPersonRoleButton.Instance.Click.InvokeAsync);

            var grid = this.renderer.FindComponent<DxGrid>();
            Assert.That(grid.Instance.IsEditing(), Is.EqualTo(true));

            await this.renderer.InvokeAsync(grid.Instance.EditModelSaving.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditPersonRole(true), Times.Once);
        }

        [Test]
        public async Task VerifyRowClick()
        {
            Assert.That(this.wasRoleSelected, Is.EqualTo(false));

            var firstRow = this.viewModel.Object.Rows.Items.First();
            var grid = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(async () => await grid.Instance.SelectedDataItemChanged.InvokeAsync(firstRow));

            Assert.That(this.wasRoleSelected, Is.EqualTo(true));
        }
    }
}