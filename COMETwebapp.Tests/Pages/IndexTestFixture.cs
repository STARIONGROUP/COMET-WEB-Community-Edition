// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IndexTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Pages
{
    using Bunit;
    using Bunit.TestDoubles;

    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using COMETwebapp.Components.Shared;
    using COMETwebapp.Pages;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Services.VersionService;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.ViewModels.Components.Shared;
    using COMETwebapp.ViewModels.Pages;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class IndexTestFixture
    {
        private IIndexViewModel viewModel;
        private TestContext context;
        private Mock<IVersionService> versionService;
        private Mock<ISessionService> sessionService;
        private Mock<IAuthenticationService> authenticationService;
        private TestAuthorizationContext authorization;
        private SourceList<Iteration> sourceList;

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

            this.context.Services.AddSingleton(this.viewModel);
            this.context.Services.AddSingleton(this.authenticationService.Object);
            this.context.Services.AddSingleton(this.sessionService.Object);
            this.context.Services.AddSingleton<ILoginViewModel, LoginViewModel>();
            this.context.Services.AddSingleton<IOpenModelViewModel, OpenModelViewModel>();
            this.context.ConfigureDevExpressBlazor();
            this.authorization = this.context.AddTestAuthorization();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyIndexPageNotAuthorized()
        {
            var renderer = this.context.RenderComponent<Index>();
            Assert.That(() => renderer.FindComponent<Login>(), Throws.Nothing);
        }

        [Test]
        public void VerifyIndexPageAuthorized()
        {
            this.authorization.SetAuthorized("User");
            this.sessionService.Setup(x => x.IsSessionOpen).Returns(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.DataSourceUri).Returns("http://localhost");
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            var renderer = this.context.RenderComponent<Index>();
            Assert.That(() => renderer.FindComponent<OpenModel>(), Throws.Nothing);
            this.sourceList.Add(new Iteration());
            Assert.That(() => renderer.FindComponent<Dashboard>(), Throws.Nothing);
        }
    }
}
