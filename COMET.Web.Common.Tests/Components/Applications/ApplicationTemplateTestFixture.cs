// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ApplicationTemplateTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMET.Web.Common.Tests.Components.Applications
{
    using System.Drawing;

    using CDP4Dal;

    using COMET.Web.Common.Components.Applications;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components.Applications;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ApplicationTemplateTestFixture
    {
        private TestContext context;
        private Mock<IApplicationTemplateViewModel> viewModel;
        private Mock<ISessionService> sessionService;
        private ServerConfiguration serverConfiguration;
        private IConfiguration configuration;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.viewModel = new Mock<IApplicationTemplateViewModel>();
            this.sessionService = new Mock<ISessionService>();
            this.serverConfiguration = new ServerConfiguration();
            this.configuration = new ConfigurationBuilder().AddJsonFile("Data/server_configuration_tests.json").Build();
            this.serverConfiguration.ServerAddress = "abc";
            this.configuration[ConfigurationKeys.ServerConfigurationKey] = this.serverConfiguration.ServerAddress;
            this.viewModel.Setup(x => x.SessionService).Returns(this.sessionService.Object);
            var session = new Mock<ISession>();
            session.Setup(x => x.DataSourceUri).Returns("abc");
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.context.Services.AddSingleton(this.configuration);
            this.context.Services.AddSingleton(this.viewModel.Object);
        }

        [Test]
        public void VerifyApplicationTemplateWithoutDefinedAddress()
        {
            this.context.RenderComponent<ApplicationTemplate>();
            var navigationManager = this.context.Services.GetService<NavigationManager>();
            Assert.That(navigationManager.Uri, Does.Contain($"{QueryKeys.ServerKey}=abc"));
        }

        [Test]
        public void VerifyApplicationTemplateWithDefinedAddress()
        {
            this.serverConfiguration.ServerAddress = "abc";
            this.configuration[ConfigurationKeys.ServerConfigurationKey] = this.serverConfiguration.ServerAddress;
            this.context.RenderComponent<ApplicationTemplate>();
            var navigationManager = this.context.Services.GetService<NavigationManager>();
            Assert.That(navigationManager.Uri, Does.Not.Contain($"{QueryKeys.ServerKey}=abc"));
        }
    }
}
