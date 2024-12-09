// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IndexComponentTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.Components
{
    using System.Collections.ObjectModel;
    using System.Reflection;

    using Bunit;
    using Bunit.TestDoubles;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.Cache;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.RegistrationService;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.StringTableService;
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
        private IIndexViewModel viewModel;
        private TestContext context;
        private Mock<ICacheService> cacheService;
        private Mock<IVersionService> versionService;
        private Mock<ISessionService> sessionService;
        private Mock<IConfigurationService> serverConnectionService;
        private Mock<IAuthenticationService> authenticationService;
        private TestAuthorizationContext authorization;
        private SourceList<Iteration> sourceList;
        private Mock<IRegistrationService> registrationService;
        private readonly Guid modelId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.versionService = new Mock<IVersionService>();
            this.sessionService = new Mock<ISessionService>();
            this.serverConnectionService = new Mock<IConfigurationService>();
            this.cacheService = new Mock<ICacheService>();
            var serverConfiguration = new ServerConfiguration { FullTrustConfiguration = new FullTrustConfiguration() };
            this.serverConnectionService.Setup(x => x.ServerConfiguration).Returns(serverConfiguration);
            this.sourceList = new SourceList<Iteration>();
            this.sessionService.Setup(x => x.OpenIterations).Returns(this.sourceList);
            this.sessionService.Setup(x => x.OpenEngineeringModels).Returns([]);

            this.authenticationService = new Mock<IAuthenticationService>();

            this.viewModel = new IndexViewModel(this.versionService.Object, this.sessionService.Object, this.authenticationService.Object);
            this.registrationService = new Mock<IRegistrationService>();
            this.registrationService.Setup(x => x.RegisteredAssemblies).Returns(new List<Assembly>());
            this.registrationService.Setup(x => x.RegisteredApplications).Returns(new List<Application>());
            this.context.Services.AddSingleton(this.viewModel);
            this.context.Services.AddSingleton(this.authenticationService.Object);
            this.context.Services.AddSingleton(this.sessionService.Object);
            this.context.Services.AddSingleton(this.versionService.Object);
            this.context.Services.AddSingleton(this.serverConnectionService.Object);
            this.context.Services.AddSingleton<ILoginViewModel, LoginViewModel>();
            this.context.Services.AddSingleton<IOpenModelViewModel, OpenModelViewModel>();
            this.context.Services.AddSingleton(this.registrationService.Object);
            this.context.Services.AddSingleton(this.cacheService.Object);
            this.context.ConfigureDevExpressBlazor();
            this.authorization = this.context.AddTestAuthorization();

            var configurationService = new Mock<IStringTableService>();
            configurationService.Setup(x => x.GetText(It.IsAny<string>())).Returns("something");
            this.context.Services.AddSingleton(configurationService.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

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
                Iid = this.modelId,
                IterationSetup =
                {
                    new IterationSetup
                    {
                        IterationIid = Guid.NewGuid()
                    }
                }
            };

            var engineeringModel = new EngineeringModel
            {
                Iid = this.modelId,
                EngineeringModelSetup = engineeringModelSetup
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
            this.sessionService.Setup(x => x.OpenEngineeringModels).Returns(new ReadOnlyCollection<EngineeringModel>([engineeringModel]));

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

            var renderer = this.context.RenderComponent<IndexComponent>(parameters => { parameters.Add(p => p.Redirect, url); });

            var login = renderer.FindComponent<Login>();
            Assert.That(login.Instance.ViewModel.AuthenticationDto.SourceAddress, Is.EqualTo(targetServer));
        }
    }
}
