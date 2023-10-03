// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Components
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components;

    using DevExpress.Blazor;

    using DynamicData;

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

            var configurationService = new Mock<IStringTableService>();
            this.context.Services.AddSingleton(configurationService.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyOpenModel()
        {
            var engineeringModels = new List<EngineeringModelSetup>
            {
                new()
                {
                    Iid = Guid.NewGuid(),
                    Name = "LOFT",
                    IterationSetup =
                    {
                        new IterationSetup
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
                        new IterationSetup
                        {
                            Iid = Guid.NewGuid(),
                            IterationNumber = 1,
                            FrozenOn = DateTime.Now - TimeSpan.FromDays(1)
                        },
                        new IterationSetup
                        {
                            Iid = Guid.NewGuid(),
                            IterationNumber = 2
                        }
                    }
                }
            };

            this.sessionService.Setup(x => x.OpenIterations).Returns(new SourceList<Iteration>());
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
            await renderer.InvokeAsync(() => button.ClickAsync(new MouseEventArgs()));

            this.sessionService.Verify(x => x.ReadIteration(It.IsAny<IterationSetup>(),
                It.IsAny<DomainOfExpertise>()), Times.Once);

            this.viewModel.SelectedEngineeringModel = null;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedDomainOfExpertise, Is.Null);
                Assert.That(this.viewModel.SelectedIterationSetup, Is.Null);
                Assert.That(this.viewModel.AvailableIterationSetups, Is.Empty);
                Assert.That(this.viewModel.AvailablesDomainOfExpertises, Is.Empty);
                Assert.That(async () => await this.viewModel.OpenSession(), Throws.Nothing);
            });
        }
    }
}
