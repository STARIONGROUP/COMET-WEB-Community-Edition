// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParticipantRoleDetailsTestFixture.cs" company="RHEA System S.A.">
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

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ParticipantRoleDetailsTestFixture
    {
        private TestContext context;
        private Mock<IParticipantRolesTableViewModel> viewModel;
        private ParticipantRole participantRole;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<IParticipantRolesTableViewModel>();

            this.participantRole = new ParticipantRole()
            {
                Name = "Role 1",
                ShortName = "role1",
                Container = new SiteDirectory(){ ShortName = "siteDir" },
            };

            this.viewModel.Setup(x => x.Thing).Returns(this.participantRole);

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
        public async Task VerifyParticipantRoleForm()
        {
            var wasOnSubmitCallbackInvoked = false;
            var wasOnCancelCallbackInvoked = false;

            var renderer = this.context.RenderComponent<ParticipantRoleDetails>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
            });

            var form = renderer.FindComponent<EditForm>();
            await renderer.InvokeAsync(form.Instance.OnValidSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.CreateOrEditParticipantRole(false), Times.Once);
                Assert.That(wasOnSubmitCallbackInvoked, Is.EqualTo(false));
            });

            renderer.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.OnCancel, new EventCallbackFactory().Create(this, () => { wasOnCancelCallbackInvoked = true; }));
                parameters.Add(p => p.OnSubmit, new EventCallbackFactory().Create(this, () => { wasOnSubmitCallbackInvoked = true; }));
            });

            await renderer.InvokeAsync(form.Instance.OnValidSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.CreateOrEditParticipantRole(false), Times.Once);
                Assert.That(wasOnSubmitCallbackInvoked, Is.EqualTo(true));
            });

            var cancelParticipantRoleButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "cancelParticipantRoleButton");
            await renderer.InvokeAsync(cancelParticipantRoleButton.Instance.Click.InvokeAsync);
            Assert.That(wasOnCancelCallbackInvoked, Is.EqualTo(true));
        }
    }
}
