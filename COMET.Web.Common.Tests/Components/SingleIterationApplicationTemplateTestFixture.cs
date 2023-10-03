// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleIterationApplicationTemplateTestFixture.cs" company="RHEA System S.A.">
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

    using CDP4Dal;
    using CDP4Dal.Events;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Components.Selectors;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using DynamicData;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class SingleIterationApplicationTemplateTestFixture
    {
        private Mock<ISingleIterationApplicationTemplateViewModel> viewModel;
        private SourceList<Iteration> openIterations;
        private TestContext context;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.openIterations = new SourceList<Iteration>();
            this.viewModel = new Mock<ISingleIterationApplicationTemplateViewModel>();
            var sessionService = new Mock<ISessionService>();
            sessionService.Setup(x => x.OpenIterations).Returns(this.openIterations);
            var session = new Mock<ISession>();
            session.Setup(x => x.DataSourceUri).Returns("http://localhost:5000");
            sessionService.Setup(x => x.Session).Returns(session.Object);
            sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(new DomainOfExpertise(){Iid = Guid.NewGuid()});
            this.viewModel.Setup(x => x.SessionService).Returns(sessionService.Object);
            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.Services.AddSingleton<IOpenModelViewModel, OpenModelViewModel>();
            this.context.Services.AddSingleton(new Mock<IConfigurationService>().Object);
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
        public void VerifyWithoutIterationIdParameter()
        {
            this.openIterations.Add(new Iteration()
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup()
                {
                    Container = new EngineeringModelSetup()
                    {
                        Iid = Guid.NewGuid()
                    }
                }
            });

            var renderer = this.context.RenderComponent<SingleIterationApplicationTemplate>(parameters =>
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
                this.viewModel.Verify(x => x.SelectIteration(this.openIterations.Items.First()), Times.Once);
            });

            this.viewModel.Setup(x => x.SelectedIteration).Returns(this.openIterations.Items.First());
            renderer.Instance.SetCorrectUrl();
            var iteration = this.viewModel.Object.SelectedIteration;

            Assert.Multiple(() =>
            {
                Assert.That(navigationManager.Uri, Does.Contain("localhost%3A5000"));
                Assert.That(navigationManager.Uri, Does.Contain(iteration.Iid.ToShortGuid()));
                Assert.That(navigationManager.Uri, Does.Contain(this.viewModel.Object.SessionService.GetDomainOfExpertise(iteration).Iid.ToShortGuid()));
                Assert.That(navigationManager.Uri, Does.Contain(iteration.IterationSetup.Container.Iid.ToShortGuid()));
            });

            renderer.Render();

            var pElement = renderer.Find("p");
            Assert.That(pElement.TextContent, Is.EqualTo("body"));

            this.openIterations.Add(new Iteration());

            renderer = this.context.RenderComponent<SingleIterationApplicationTemplate>(parameters =>
            {
                parameters.Add(p => p.Body, builder =>
                {
                    builder.OpenElement(0, "p");
                    builder.AddContent(1, "body");
                    builder.CloseComponent();
                });
            });

            Assert.That(() => renderer.FindComponent<IterationSelector>(), Throws.Exception);
            this.viewModel.Verify(x => x.AskToSelectIteration(), Times.Once);
            this.viewModel.Setup(x => x.IsOnIterationSelectionMode).Returns(true);
            this.viewModel.Setup(x => x.IterationSelectorViewModel).Returns(new IterationSelectorViewModel());
            renderer.Render();
            Assert.That(() => renderer.FindComponent<IterationSelector>(), Throws.Nothing);
            this.openIterations.Clear();
            this.viewModel.Setup(x => x.SelectedIteration).Returns((Iteration)null);
            renderer.Instance.SetCorrectUrl();

            Assert.Multiple(() =>
            {
                Assert.That(navigationManager.Uri, Is.EqualTo("http://localhost/"));
                Assert.That(() => renderer.FindComponent<OpenModel>(), Throws.Nothing);
                Assert.That(() => CDPMessageBus.Current.SendMessage(new DomainChangedEvent(null, null)), Throws.Nothing);
            });
        }

        [Test]
        public void VerifyWithIterationIdParameter()
        {
            this.openIterations.Add(new Iteration()
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup()
                {
                    Container = new EngineeringModelSetup()
                    {
                        Iid = Guid.NewGuid()
                    }
                }
            });

            var renderer = this.context.RenderComponent<SingleIterationApplicationTemplate>(parameters =>
            {
                parameters.Add(p => p.IterationId, Guid.NewGuid());
            });

            Assert.That(renderer.Instance.IterationId, Is.EqualTo(Guid.Empty));

            _ = this.context.RenderComponent<SingleIterationApplicationTemplate>(parameters =>
            {
                parameters.Add(p => p.IterationId, this.openIterations.Items.First().Iid);
            });

            this.viewModel.Verify(x => x.SelectIteration(this.openIterations.Items.First()), Times.Once);

            this.viewModel.Setup(x => x.SelectedIteration).Returns(new Iteration()
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup()
                {
                    Container = new EngineeringModelSetup()
                    {
                        Iid = Guid.NewGuid()
                    }
                }
            });

            renderer = this.context.RenderComponent<SingleIterationApplicationTemplate>(parameters =>
            {
                parameters.Add(p => p.IterationId, this.openIterations.Items.First().Iid);
            });

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.IterationId, Is.EqualTo(this.openIterations.Items.First().Iid));
                this.viewModel.Verify(x => x.SelectIteration(this.openIterations.Items.First()), Times.Once);
            });
        }
    }
}
