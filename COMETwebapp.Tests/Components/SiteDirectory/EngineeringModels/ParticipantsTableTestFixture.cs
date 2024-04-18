// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParticipantsTableTestFixture.cs" company="RHEA System S.A.">
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

    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ParticipantsTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ParticipantsTable> renderer;
        private Mock<IParticipantsTableViewModel> viewModel;
        private EngineeringModelSetup model;
        private Participant participant1;
        private Participant participant2;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<IParticipantsTableViewModel>();
            this.model = new EngineeringModelSetup();

            var organization = new Organization() { Name = "org" };
            var participantRole = new ParticipantRole() { Name = "org" };

            this.participant1 = new Participant()
            {
                Person = new Person()
                {
                    GivenName = "person",
                    Surname = "A",
                    Organization = organization,
                },
                Role = participantRole,
                Container = new EngineeringModelSetup(){ ShortName = "model" },
            };

            this.participant2 = new Participant()
            {
                Person = new Person()
                {
                    GivenName = "person",
                    Surname = "B",
                    Organization = organization
                },
                Role = participantRole,
                Container = new EngineeringModelSetup() { ShortName = "model" },
            };

            var rows = new SourceList<ParticipantRowViewModel>();
            rows.Add(new ParticipantRowViewModel(this.participant1));
            rows.Add(new ParticipantRowViewModel(this.participant2));

            this.viewModel.Setup(x => x.Rows).Returns(rows);
            this.viewModel.Setup(x => x.Thing).Returns(new Participant());

            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.RenderComponent<ParticipantsTable>(p =>
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
                Assert.That(this.renderer.Markup, Does.Contain(this.participant1.Person.Name));
                Assert.That(this.renderer.Markup, Does.Contain(this.participant2.Person.Name));
                this.viewModel.Verify(x => x.InitializeViewModel(), Times.Once);
            });

            var details = this.renderer.Find("a");
            details.ClickAsync(new MouseEventArgs());

            Assert.That(this.renderer.Instance.IsOnEditMode, Is.EqualTo(true));
        }

        [Test]
        public async Task VerifyDeleteParticipant()
        {
            var deleteParticipantButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "deleteParticipantButton");
            await this.renderer.InvokeAsync(deleteParticipantButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.OnDeleteButtonClick(It.IsAny<ParticipantRowViewModel>()), Times.Once);
        }

        [Test]
        public async Task VerifyAddingOrEditingParticipant()
        {
            var addParticipantButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addParticipantButton");
            await this.renderer.InvokeAsync(addParticipantButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.EqualTo(true));
                Assert.That(this.viewModel.Object.Thing, Is.InstanceOf(typeof(Participant)));
            });

            var editParticipantButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editParticipantButton");
            await this.renderer.InvokeAsync(editParticipantButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.EqualTo(false));
                Assert.That(this.viewModel.Object.Thing, Is.InstanceOf(typeof(Participant)));
            });

            var saveParticipantsButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "saveParticipantsButton");
            await this.renderer.InvokeAsync(saveParticipantsButton.Instance.Click.InvokeAsync);
            this.viewModel.Verify(x => x.CreateOrEditParticipant(It.IsAny<bool>()), Times.Once);
        }
    }
}
