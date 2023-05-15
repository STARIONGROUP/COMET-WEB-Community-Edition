// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IndexComponentTestFixture.cs" company="RHEA System S.A.">
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
    using System.Reflection;

    using Bunit;
    using Bunit.TestDoubles;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.RegistrationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.VersionService;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components;

    using DynamicData;

    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class IndexComponentTestFixture
    {
        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.versionService = new Mock<IVersionService>();
            this.sessionService = new Mock<ISessionService>();
            this.sourceList = new SourceList<Iteration>();
            this.sessionService.Setup(x => x.OpenIterations).Returns(this.sourceList);

            this.authenticationService = new Mock<IAuthenticationService>();

            this.viewModel = new IndexViewModel(this.versionService.Object, this.sessionService.Object, this.authenticationService.Object);
            this.registrationService = new Mock<IRegistrationService>();
            this.registrationService.Setup(x => x.RegisteredAssemblies).Returns(new List<Assembly>());
            this.registrationService.Setup(x => x.RegisteredApplications).Returns(new List<Application>());
            this.context.Services.AddSingleton(this.viewModel);
            this.context.Services.AddSingleton(this.authenticationService.Object);
            this.context.Services.AddSingleton(this.sessionService.Object);
            this.context.Services.AddSingleton(this.versionService.Object);
            this.context.Services.AddSingleton<ILoginViewModel, LoginViewModel>();
            this.context.Services.AddSingleton<IOpenModelViewModel, OpenModelViewModel>();
            this.context.Services.AddSingleton(this.registrationService.Object);
            this.context.ConfigureDevExpressBlazor();
            this.authorization = this.context.AddTestAuthorization();
            
            var configurationService = new Mock<IConfigurationService>();
            this.context.Services.AddSingleton(configurationService.Object);
}

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        private IIndexViewModel viewModel;
        private TestContext context;
        private Mock<IVersionService> versionService;
        private Mock<ISessionService> sessionService;
        private Mock<IAuthenticationService> authenticationService;
        private TestAuthorizationContext authorization;
        private SourceList<Iteration> sourceList;
        private Mock<IRegistrationService> registrationService;

        [Test]
        public void VerifyIndexPageAuthorized()
        {
            this.authorization.SetAuthorized("User");
            this.sessionService.Setup(x => x.IsSessionOpen).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.DataSourceUri).Returns("http://localhost");
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            var renderer = this.context.RenderComponent<IndexComponent>();
            Assert.That(() => renderer.FindComponent<OpenModel>(), Throws.Nothing);
            this.sourceList.Add(new Iteration());
            Assert.That(() => renderer.FindComponent<Dashboard>(), Throws.Nothing);
        }

        [Test]
        public void VerifyIndexPageNotAuthorized()
        {
            var renderer = this.context.RenderComponent<IndexComponent>();
            Assert.That(() => renderer.FindComponent<Login>(), Throws.Nothing);
        }

        [Test]
        public void VerifyIndexPageWithRedirectionAuthorized()
        {
            this.authorization.SetAuthorized("User");
            this.sessionService.Setup(x => x.IsSessionOpen).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.DataSourceUri).Returns("http://localhost");
            this.sessionService.Setup(x => x.Session).Returns(session.Object);

            var iteration = new Iteration
            {
                Iid = Guid.NewGuid()
            };

            var domain = new DomainOfExpertise
            {
                Iid = Guid.NewGuid()
            };

            var openIterations = new SourceList<Iteration>();

            this.sessionService.Setup(x => x.OpenIterations).Returns(openIterations);

            var engineeringModelSetup = new EngineeringModelSetup
            {
                Iid = Guid.NewGuid(),
                IterationSetup =
                {
                    new IterationSetup
                    {
                        IterationIid = Guid.NewGuid()
                    }
                }
            };

            var queries = new Dictionary<string, string>
            {
                [QueryKeys.DomainKey] = domain.Iid.ToShortGuid(),
                [QueryKeys.IterationKey] = iteration.Iid.ToShortGuid(),
                [QueryKeys.ModelKey] = engineeringModelSetup.Iid.ToShortGuid()
            };

            var url = QueryHelpers.AddQueryString("ModelDashboard", queries);

            var renderer = this.context.RenderComponent<IndexComponent>(parameters =>
                parameters.Add(p => p.Redirect, url));

            var openModel = renderer.FindComponent<OpenModel>();
            Assert.That(openModel.Instance.ViewModel.SelectedEngineeringModel, Is.Null);

            this.sessionService.Setup(x => x.GetParticipantModels()).Returns(new List<EngineeringModelSetup> { engineeringModelSetup });

            renderer = this.context.RenderComponent<IndexComponent>(parameters =>
                parameters.Add(p => p.Redirect, url));

            openModel = renderer.FindComponent<OpenModel>();
            Assert.That(openModel.Instance.ViewModel.SelectedEngineeringModel, Is.Not.Null);

            engineeringModelSetup.IterationSetup.Add(new IterationSetup
            {
                IterationIid = iteration.Iid
            });

            renderer = this.context.RenderComponent<IndexComponent>(parameters =>
                parameters.Add(p => p.Redirect, url));

            openModel = renderer.FindComponent<OpenModel>();
            Assert.That(openModel.Instance.ViewModel.SelectedIterationSetup, Is.Not.Null);

            this.sessionService.Setup(x => x.GetModelDomains(engineeringModelSetup)).Returns(new List<DomainOfExpertise>
            {
                domain
            });

            renderer = this.context.RenderComponent<IndexComponent>(parameters =>
                parameters.Add(p => p.Redirect, url));

            openModel = renderer.FindComponent<OpenModel>();
            Assert.That(openModel.Instance.ViewModel.SelectedDomainOfExpertise, Is.Not.Null);
        }

        [Test]
        public void VerifyIndexPageWithRedirectionNotAuthorized()
        {
            const string targetServer = "http://localhost:5000";
            var url = QueryHelpers.AddQueryString("ModelDashboard", QueryKeys.ServerKey, targetServer);

            var renderer = this.context.RenderComponent<IndexComponent>(parameters =>
                parameters.Add(p => p.Redirect, url));

            var login = renderer.FindComponent<Login>();
            Assert.That(login.Instance.ViewModel.AuthenticationDto.SourceAddress, Is.EqualTo(targetServer));
        }
    }
}
