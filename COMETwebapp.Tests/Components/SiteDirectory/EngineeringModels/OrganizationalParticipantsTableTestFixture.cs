// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrganizationalParticipantsTableTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.SiteDirectory.EngineeringModels
{
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.SiteDirectory.EngineeringModel;
    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class OrganizationalParticipantsTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<OrganizationalParticipantsTable> renderer;
        private Mock<IOrganizationalParticipantsTableViewModel> viewModel;
        private EngineeringModelSetup model;
        private OrganizationalParticipant organizationalParticipant1;
        private OrganizationalParticipant organizationParticipant2;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<IOrganizationalParticipantsTableViewModel>();
            this.model = new EngineeringModelSetup();

            this.organizationalParticipant1 = new OrganizationalParticipant()
            {
                Organization = new Organization()
                {
                    Name = "org A",
                    ShortName = "orgA"
                },
                Container = new EngineeringModelSetup(){ ShortName = "model" },
            };

            this.organizationParticipant2 = new OrganizationalParticipant()
            {
                Organization = new Organization()
                {
                    Name = "org B",
                    ShortName = "orgB"
                },
                Container = new EngineeringModelSetup() { ShortName = "model" },
            };

            var rows = new SourceList<OrganizationalParticipantRowViewModel>();
            rows.Add(new OrganizationalParticipantRowViewModel(this.organizationalParticipant1));
            rows.Add(new OrganizationalParticipantRowViewModel(this.organizationParticipant2));

            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.Thing).Returns(new OrganizationalParticipant());

            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<OrganizationalParticipantsTable>(p =>
            {
                p.Add(parameter => parameter.EngineeringModelSetup, this.model);
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
                Assert.That(this.renderer.Instance.EngineeringModelSetup, Is.EqualTo(this.model));
                Assert.That(this.renderer.Markup, Does.Contain(this.organizationalParticipant1.Organization.Name));
                Assert.That(this.renderer.Markup, Does.Contain(this.organizationParticipant2.Organization.ShortName));
                this.viewModel.Verify(x => x.InitializeViewModel(It.IsAny<EngineeringModelSetup>()), Times.Once);
            });
        }

        [Test]
        public async Task VerifyDeleteOrganizationalParticipant()
        {
            var deleteOrganizationalParticipantButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "deleteOrganizationalParticipantButton");
            await this.renderer.InvokeAsync(deleteOrganizationalParticipantButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OnDeleteButtonClick(It.IsAny<OrganizationalParticipantRowViewModel>()), Times.Once);
        }

        [Test]
        public async Task VerifyEditingOrganizationalParticipant()
        {
            var editOrganizationalParticipantsButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editOrganizationalParticipantsButton");
            await this.renderer.InvokeAsync(editOrganizationalParticipantsButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.EqualTo(true));
                Assert.That(this.viewModel.Object.Thing, Is.InstanceOf(typeof(OrganizationalParticipant)));
            });

            var form = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(form.Instance.EditModelSaving.InvokeAsync);
        }
    }
}
