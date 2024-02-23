// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleEngineeringModelApplicationTemplateTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

namespace COMET.Web.Common.Tests.Components.Applications
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Extensions;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Components.Applications;
    using COMET.Web.Common.Components.Selectors;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using DynamicData;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class SingleEngineeringModelApplicationTemplateTestFixture
    {
        private Mock<ISingleEngineeringModelApplicationTemplateViewModel> viewModel;
        private List<EngineeringModel> openEngineeringModels;
        private TestContext context;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<ISingleEngineeringModelApplicationTemplateViewModel>();

            this.openEngineeringModels = [];
            var sessionService = new Mock<ISessionService>();
            sessionService.Setup(x => x.OpenEngineeringModels).Returns(this.openEngineeringModels);
            sessionService.Setup(x => x.OpenIterations).Returns(new SourceList<Iteration>());
            var session = new Mock<ISession>();
            session.Setup(x => x.DataSourceUri).Returns("http://localhost:5000");
            sessionService.Setup(x => x.Session).Returns(session.Object);
            this.viewModel.Setup(x => x.SessionService).Returns(sessionService.Object);
            var mockConfigurationService = new Mock<IConfigurationService>();
            mockConfigurationService.Setup(x => x.ServerConfiguration).Returns(new ServerConfiguration());
            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.Services.AddSingleton<IOpenModelViewModel, OpenModelViewModel>();
            this.context.Services.AddSingleton(mockConfigurationService.Object);
            this.context.Services.AddSingleton(new Mock<IStringTableService>().Object);
            this.context.Services.AddSingleton(sessionService.Object);
            this.context.ConfigureDevExpressBlazor();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyWithEngineeringModelIdParameter()
        {
            this.openEngineeringModels.Add(new EngineeringModel
            {
                Iid = Guid.NewGuid(),
                EngineeringModelSetup = new EngineeringModelSetup()
                {
                    Iid = Guid.NewGuid()
                }
            });

            this.viewModel.Setup(x => x.OnThingSelect(It.IsAny<EngineeringModel>())).Callback((EngineeringModel engineeringModel) => this.viewModel.Setup(x => x.SelectedThing).Returns(engineeringModel));
            var renderer = this.context.RenderComponent<SingleEngineeringModelApplicationTemplate>(parameters => { parameters.Add(p => p.EngineeringModelId, Guid.NewGuid()); });

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.EngineeringModelId, Is.EqualTo(this.openEngineeringModels[0].Iid));
                this.viewModel.Verify(x => x.OnThingSelect(this.openEngineeringModels[0]), Times.Once);
            });

            this.viewModel.Setup(x => x.SelectedThing).Returns((EngineeringModel)null);
            _ = this.context.RenderComponent<SingleEngineeringModelApplicationTemplate>(parameters => { parameters.Add(p => p.EngineeringModelId, this.openEngineeringModels[0].Iid); });

            this.viewModel.Verify(x => x.OnThingSelect(this.openEngineeringModels[0]), Times.Exactly(2));

            this.viewModel.Setup(x => x.SelectedThing).Returns(new EngineeringModel
            {
                Iid = Guid.NewGuid(),
                EngineeringModelSetup = new EngineeringModelSetup()
                {
                    Iid = Guid.NewGuid()
                }
            });

            renderer = this.context.RenderComponent<SingleEngineeringModelApplicationTemplate>(parameters => { parameters.Add(p => p.EngineeringModelId, this.openEngineeringModels[0].Iid); });

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.EngineeringModelId, Is.EqualTo(this.viewModel.Object.SelectedThing.Iid));
                this.viewModel.Verify(x => x.OnThingSelect(this.openEngineeringModels[0]), Times.Exactly(2));
            });
        }

        [Test]
        public void VerifyWithoutEngineeringModelIdParameter()
        {
            this.openEngineeringModels.Add(new EngineeringModel()
            {
                Iid = Guid.NewGuid(), 
                EngineeringModelSetup = new EngineeringModelSetup()
                {
                    Iid = Guid.NewGuid()
                }
            });

            var renderer = this.context.RenderComponent<SingleEngineeringModelApplicationTemplate>(parameters =>
            {
                parameters.Add(p => p.Body, builder =>
                {
                    builder.OpenElement(0, "p");
                    builder.AddContent(1, "body");
                    builder.CloseComponent();
                });
            });

            var navigationManager = this.context.Services.GetService<NavigationManager>();

            Assert.Multiple(() =>
            {
                Assert.That(navigationManager.Uri, Is.EqualTo("http://localhost/"));
                this.viewModel.Verify(x => x.OnThingSelect(this.openEngineeringModels[0]), Times.Exactly(2));
            });

            this.viewModel.Setup(x => x.SelectedThing).Returns(this.openEngineeringModels[0]);
            renderer.Instance.SetCorrectUrl();
            var engineeringModel = this.viewModel.Object.SelectedThing;

            Assert.Multiple(() =>
            {
                Assert.That(navigationManager.Uri, Does.Contain("localhost%3A5000"));
                Assert.That(navigationManager.Uri, Does.Contain(engineeringModel.EngineeringModelSetup.Iid.ToShortGuid()));
            });

            renderer.Render();

            var pElement = renderer.Find("p");
            Assert.That(pElement.TextContent, Is.EqualTo("body"));

            this.openEngineeringModels.Add(new EngineeringModel());

            renderer = this.context.RenderComponent<SingleEngineeringModelApplicationTemplate>(parameters =>
            {
                parameters.Add(p => p.Body, builder =>
                {
                    builder.OpenElement(0, "p");
                    builder.AddContent(1, "body");
                    builder.CloseComponent();
                });
            });

            Assert.That(() => renderer.FindComponent<EngineeringModelSelector>(), Throws.Exception);
            this.viewModel.Verify(x => x.AskToSelectThing(), Times.Once);
            this.viewModel.Setup(x => x.IsOnSelectionMode).Returns(true);
            this.viewModel.Setup(x => x.EngineeringModelSelectorViewModel).Returns(new EngineeringModelSelectorViewModel());
            renderer.Render();
            Assert.That(() => renderer.FindComponent<EngineeringModelSelector>(), Throws.Nothing);
            this.openEngineeringModels.Clear();
            this.viewModel.Setup(x => x.SelectedThing).Returns((EngineeringModel)null);
            renderer.Instance.SetCorrectUrl();

            renderer.Render();

            Assert.Multiple(() =>
            {
                Assert.That(navigationManager.Uri, Is.EqualTo("http://localhost/"));
                Assert.That(() => renderer.FindComponent<OpenModel>(), Throws.Nothing);
            });
        }
    }
}
