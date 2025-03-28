﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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
    using COMET.Web.Common.Services.Cache;
    using COMET.Web.Common.Services.ConfigurationService;
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
        private OpenModelViewModel viewModel;
        private TestContext context;
        private Mock<ISessionService> sessionService;
        private Mock<IConfigurationService> configurationService;
        private Mock<ICacheService> cacheService;
        private List<EngineeringModelSetup> engineeringModels;

        [SetUp]
        public void Setup()
        {
            this.engineeringModels =
            [
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
                            FrozenOn = DateTimeOffset.Now.AddDays(-1).DateTime
                        },
                        new IterationSetup
                        {
                            Iid = Guid.NewGuid(),
                            IterationNumber = 2
                        }
                    }
                }
            ];

            this.context = new TestContext();
            this.sessionService = new Mock<ISessionService>();
            this.configurationService = new Mock<IConfigurationService>();
            this.cacheService = new Mock<ICacheService>();
            this.viewModel = new OpenModelViewModel(this.sessionService.Object, this.configurationService.Object, this.cacheService.Object);
            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddSingleton<IOpenModelViewModel>(this.viewModel);

            var stringTableService = new Mock<IStringTableService>();
            this.context.Services.AddSingleton(stringTableService.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyOpenModel()
        {
            this.sessionService.Setup(x => x.OpenIterations).Returns(new SourceList<Iteration>());
            this.sessionService.Setup(x => x.GetParticipantModels()).Returns(this.engineeringModels);
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

            var result = await this.viewModel.OpenSession();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedDomainOfExpertise, Is.Null);
                Assert.That(this.viewModel.SelectedIterationSetup, Is.Null);
                Assert.That(this.viewModel.AvailableIterationSetups, Is.Empty);
                Assert.That(this.viewModel.AvailablesDomainOfExpertises, Is.Empty);
                Assert.That(result.IsFailed, Is.True);
                Assert.That(result.Errors[0].Message, Is.EqualTo("The selected iteration and the domain of expertise should not be null"));
            });
        }

        [Test]
        public async Task VerifyOpenModelFails()
        {
            this.sessionService.Setup(x => x.OpenIterations).Returns(new SourceList<Iteration>());
            this.sessionService.Setup(x => x.GetParticipantModels()).Returns(this.engineeringModels);
            this.context.RenderComponent<OpenModel>();

            this.sessionService.Setup(x => x.GetModelDomains(It.IsAny<EngineeringModelSetup>()))
                .Returns(new List<DomainOfExpertise> { new() { Name = "Thermodynamic" } });

            this.viewModel.SelectedEngineeringModel = this.viewModel.AvailableEngineeringModelSetups.First();
            this.viewModel.SelectedDomainOfExpertise = this.viewModel.AvailablesDomainOfExpertises.First();
            this.viewModel.SelectedIterationSetup = this.viewModel.AvailableIterationSetups.First();

            var iteration = new Iteration(Guid.NewGuid(), null, null)
            {
                IterationSetup = this.engineeringModels.SelectMany(x => x.IterationSetup).Single(x => x.Iid == this.viewModel.SelectedIterationSetup.IterationSetupId)
            };

            var openIterations = new SourceList<Iteration>();
            openIterations.Add(iteration);

            this.sessionService.Setup(x => x.OpenIterations).Returns(openIterations);

            var result = await this.viewModel.OpenSession();

            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailed, Is.True);
                Assert.That(result.Errors[0].Message, Is.EqualTo("The selected iteration is already openened"));
            });
        }
    }
}
