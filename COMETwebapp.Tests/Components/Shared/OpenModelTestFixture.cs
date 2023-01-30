// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenModelTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.Components.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.Shared;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.ViewModels.Components.Shared;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class OpenModelTestFixture
    {
        private IOpenModelViewModel viewModel;
        private TestContext context;
        private Mock<ISessionService> sessionService;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.sessionService = new Mock<ISessionService>();
            this.viewModel = new OpenModelViewModel(this.sessionService.Object);
            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddSingleton(this.viewModel);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyOpenModel()
        {
            var engineeringModels = new List<EngineeringModelSetup>()
            {
                new()
                {
                    Iid = Guid.NewGuid(),
                    Name = "LOFT",
                    IterationSetup =
                    {
                        new IterationSetup()
                        {
                            Iid = Guid.NewGuid(),
                            IterationNumber = 1
                        }
                    }
                },
                new()
                {
                    Iid = Guid.NewGuid(),
                    Name = "EnVision",
                    IterationSetup =
                    {
                        new IterationSetup()
                        {
                            Iid = Guid.NewGuid(),
                            IterationNumber = 1,
                            FrozenOn = DateTime.Now - TimeSpan.FromDays(1)
                        },
                        new IterationSetup()
                        {
                            Iid = Guid.NewGuid(),
                            IterationNumber = 2
                        }
                    }
                }
            };

            this.sessionService.Setup(x => x.GetParticipantModels()).Returns(engineeringModels);
            var renderer = this.context.RenderComponent<OpenModel>();
            var layoutItems = renderer.FindComponents<DxFormLayoutItem>();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableEngineeringModelSetups.ToList(), Has.Count.EqualTo(2));
                Assert.That(layoutItems, Has.Count.EqualTo(1));
            });

            this.sessionService.Setup(x => x.GetModelDomains(It.IsAny<EngineeringModelSetup>()))
                .Returns(new List<DomainOfExpertise> { new() { Name = "Thermodynamic" } });

            this.viewModel.SelectedEngineeringModel = this.viewModel.AvailableEngineeringModelSetups.First();

            layoutItems = renderer.FindComponents<DxFormLayoutItem>();

            Assert.Multiple(() =>
            {
                Assert.That(layoutItems, Has.Count.EqualTo(3));
                Assert.That(this.viewModel.AvailableIterationSetups.ToList(), Has.Count.EqualTo(2));
                Assert.That(this.viewModel.AvailablesDomainOfExpertises.ToList(), Has.Count.EqualTo(1));
                Assert.That(renderer.Instance.ButtonEnabled, Is.False);
            });

            this.viewModel.SelectedDomainOfExpertise = this.viewModel.AvailablesDomainOfExpertises.First();
            this.viewModel.SelectedIterationSetup = this.viewModel.AvailableIterationSetups.First();
            Assert.That(renderer.Instance.ButtonEnabled, Is.True);

            var button = renderer.Find("#openmodel__button");
            await renderer.InvokeAsync(() => button.ClickAsync((new MouseEventArgs())));

            this.sessionService.Verify(x => x.ReadIteration(It.IsAny<IterationSetup>(), 
                It.IsAny<DomainOfExpertise>()), Times.Once);

            this.viewModel.SelectedEngineeringModel = null;
            
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedDomainOfExpertise, Is.Null);
                Assert.That(this.viewModel.SelectedIterationSetup, Is.Null);
                Assert.That(this.viewModel.AvailableIterationSetups, Is.Empty);
                Assert.That(this.viewModel.AvailablesDomainOfExpertises, Is.Empty);
                Assert.That(async () => await this.viewModel.OpenSession(),Throws.Nothing);
            });
        }
    }
}
