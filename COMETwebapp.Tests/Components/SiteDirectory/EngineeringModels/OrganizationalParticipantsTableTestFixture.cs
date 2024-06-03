// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OrganizationalParticipantsTableTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.SiteDirectory.EngineeringModels
{
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
        private OrganizationalParticipant organizationalParticipant2;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<IOrganizationalParticipantsTableViewModel>();

            this.model = new EngineeringModelSetup
            {
                ShortName = "model"
            };

            this.organizationalParticipant1 = new OrganizationalParticipant
            {
                Organization = new Organization
                {
                    Name = "org A",
                    ShortName = "orgA"
                }
            };

            this.organizationalParticipant2 = new OrganizationalParticipant
            {
                Organization = new Organization
                {
                    Name = "org B",
                    ShortName = "orgB"
                }
            };

            this.model.OrganizationalParticipant.AddRange([this.organizationalParticipant1, this.organizationalParticipant2]);

            var rows = new SourceList<OrganizationalParticipantRowViewModel>();
            rows.Add(new OrganizationalParticipantRowViewModel(this.organizationalParticipant1));
            rows.Add(new OrganizationalParticipantRowViewModel(this.organizationalParticipant2));

            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.CurrentThing).Returns(new OrganizationalParticipant());
            this.viewModel.Setup(x => x.CurrentModel).Returns(this.model);

            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<OrganizationalParticipantsTable>(p => { p.Add(parameter => parameter.ViewModel, this.viewModel.Object); });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public async Task VerifyEditingOrganizationalParticipant()
        {
            var editOrganizationalParticipantsClickableItem = this.renderer.FindComponent<DxToolbarItem>();
            await this.renderer.InvokeAsync(editOrganizationalParticipantsClickableItem.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.IsOnEditMode, Is.EqualTo(true));
                Assert.That(this.viewModel.Object.CurrentThing, Is.InstanceOf(typeof(OrganizationalParticipant)));
            });

            var form = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(form.Instance.EditModelSaving.InvokeAsync);
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.ViewModel, Is.Not.Null);
                Assert.That(this.renderer.Markup, Does.Contain(this.organizationalParticipant1.Organization.Name));
                Assert.That(this.renderer.Markup, Does.Contain(this.organizationalParticipant2.Organization.ShortName));
            });
        }
    }
}
