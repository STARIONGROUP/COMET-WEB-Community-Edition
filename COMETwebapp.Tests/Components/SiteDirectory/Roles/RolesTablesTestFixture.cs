// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RolesTablesTestFixture.cs" company="RHEA System S.A.">
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
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.SiteDirectory.Roles;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Roles;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class RolesTablesTestFixture
    {
        private TestContext context;
        private IRenderedComponent<RolesTables> renderer;
        private Mock<IParticipantRolesTableViewModel> participantRolesViewModel;
        private Mock<IPersonRolesTableViewModel> personRolesViewModel;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();

            this.participantRolesViewModel = new Mock<IParticipantRolesTableViewModel>();
            this.participantRolesViewModel.Setup(x => x.Rows).Returns(new SourceList<ParticipantRoleRowViewModel>());

            this.personRolesViewModel = new Mock<IPersonRolesTableViewModel>();
            this.personRolesViewModel.Setup(x => x.Rows).Returns(new SourceList<PersonRoleRowViewModel>());

            this.context.Services.AddSingleton(this.participantRolesViewModel.Object);
            this.context.Services.AddSingleton(this.personRolesViewModel.Object);
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<RolesTables>();
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
                Assert.That(this.renderer.Instance.ParticipantRolesViewModel, Is.Not.Null);
                Assert.That(this.renderer.Instance.PersonRolesViewModel, Is.Not.Null);
                this.participantRolesViewModel.Verify(x => x.InitializeViewModel(), Times.Once);
            });
        }

        [Test]
        public async Task VerifyRoleSelection()
        {
            Assert.That(this.renderer.Instance.SelectedComponent, Is.Null);

            var participantRoles = this.renderer.FindComponent<ParticipantRolesTable>();
            await this.renderer.InvokeAsync(() => participantRoles.Instance.OnRoleSelected.InvokeAsync(new ParticipantRole()));
            Assert.That(this.renderer.Instance.SelectedComponent, Is.Not.Null);

            var detailsComponent = this.renderer.FindComponent<ParticipantRoleDetails>();
            Assert.That(detailsComponent.Instance.OnCancel.HasDelegate, Is.EqualTo(true));

            await this.renderer.InvokeAsync(detailsComponent.Instance.OnCancel.InvokeAsync);
            Assert.That(this.renderer.Instance.SelectedComponent, Is.Null);
        }
    }
}
