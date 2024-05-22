// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IndexTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Pages
{
    using Bunit;
    using Bunit.TestDoubles;

    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Model.DTO;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components;

    using COMETwebapp.Pages;
    using COMETwebapp.Utilities;

    using DynamicData;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using Result = FluentResults.Result;
    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class IndexTestFixture
    {
        private Mock<IIndexViewModel> viewModel;
        private TestContext context;
        private Mock<ISessionService> sessionService;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.context.AddTestAuthorization();
            this.context.ConfigureDevExpressBlazor();

            this.sessionService = new Mock<ISessionService>();
            this.sessionService.Setup(x => x.OpenIterations).Returns(new SourceList<Iteration>());

            this.viewModel = new Mock<IIndexViewModel>();
            this.viewModel.Setup(x => x.SessionService).Returns(this.sessionService.Object);

            var loginViewModel = new Mock<ILoginViewModel>();
            loginViewModel.Setup(x => x.ServerConnectionService.ServerConfiguration).Returns(new ServerConfiguration());
            loginViewModel.Setup(x => x.AuthenticationResult).Returns(new Result());
            loginViewModel.Setup(x => x.AuthenticationDto).Returns(new AuthenticationDto());

            this.context.Services.AddSingleton(loginViewModel.Object);
            this.context.Services.AddSingleton(this.viewModel.Object);
            this.context.Services.AddSingleton(this.sessionService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public void VerifyIndexPageNotAuthorized()
        {
            var renderer = this.context.RenderComponent<Index>();
            Assert.That(() => renderer.FindComponent<Login>(), Throws.Nothing);
        }
    }
}
