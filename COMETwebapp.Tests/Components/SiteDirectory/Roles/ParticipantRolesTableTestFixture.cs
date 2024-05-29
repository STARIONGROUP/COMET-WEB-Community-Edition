// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParticipantRolesTableTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
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

namespace COMETwebapp.Tests.Components.SiteDirectory.Roles
{
    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.SiteDirectory.Roles;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Roles;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ParticipantRolesTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ParticipantRolesTable> renderer;
        private Mock<IParticipantRolesTableViewModel> viewModel;
        private ParticipantRole participantRole1;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<IParticipantRolesTableViewModel>();

            this.participantRole1 = new ParticipantRole
            {
                Name = "Role 1",
                ShortName = "role1",
                Container = new SiteDirectory { ShortName = "siteDir" }
            };

            var rows = new SourceList<ParticipantRoleRowViewModel>();
            rows.Add(new ParticipantRoleRowViewModel(this.participantRole1));

            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.CurrentThing).Returns(this.participantRole1);

            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<ParticipantRolesTable>();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public async Task VerifyAddingOrEditingParticipantRole()
        {
            var addParticipantRoleButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "dataItemDetailsButton");
            await this.renderer.InvokeAsync(addParticipantRoleButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.EqualTo(true));
                Assert.That(this.viewModel.Object.CurrentThing, Is.InstanceOf(typeof(ParticipantRole)));
            });

            var participantRolesGrid = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(() => participantRolesGrid.Instance.SelectedDataItemChanged.InvokeAsync(new ParticipantRoleRowViewModel(this.participantRole1)));
            Assert.That(this.renderer.Instance.IsOnEditMode, Is.EqualTo(true));

            var participantRoleForm = this.renderer.FindComponent<ParticipantRoleForm>();
            var participantRoleEditForm = participantRoleForm.FindComponent<EditForm>();
            await participantRoleForm.InvokeAsync(participantRoleEditForm.Instance.OnValidSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.CreateOrEditParticipantRole(false), Times.Once);
                Assert.That(this.viewModel.Object.CurrentThing, Is.InstanceOf(typeof(ParticipantRole)));
            });

            var form = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(form.Instance.EditModelSaving.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditParticipantRole(false), Times.Once);
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.ViewModel, Is.Not.Null);
                Assert.That(this.renderer.Markup, Does.Contain(this.participantRole1.Name));
                this.viewModel.Verify(x => x.InitializeViewModel(), Times.Once);
            });
        }
    }
}
