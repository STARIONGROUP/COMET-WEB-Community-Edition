// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="TopMenuTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMETwebapp.Components.Shared;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Shared;
    using COMETwebapp.Shared.TopMenuEntry;
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.ViewModels.Shared.TopMenuEntry;

    using DynamicData;

    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class TopMenuTestFixture
    {
        private TestContext context;
        private CometWebAuthStateProvider stateProvider;
        private Mock<ISessionService> sessionService;
        private Mock<IAutoRefreshService> autoRefreshService;
        private Mock<IAuthenticationService> authenticationService;
        private SourceList<Iteration> sourceList;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.sessionService = new Mock<ISessionService>();
            this.stateProvider = new CometWebAuthStateProvider(this.sessionService.Object);
            this.authenticationService = new Mock<IAuthenticationService>();
            this.autoRefreshService = new Mock<IAutoRefreshService>();
            this.sourceList = new SourceList<Iteration>();
            this.sessionService.Setup(x => x.OpenIterations).Returns(this.sourceList);

            this.context.Services.AddSingleton<AuthenticationStateProvider>(this.stateProvider);
            this.context.Services.AddSingleton(this.sessionService.Object);
            this.context.Services.AddSingleton(this.authenticationService.Object);
            this.context.Services.AddSingleton(this.autoRefreshService.Object);
            this.context.Services.AddSingleton<ISessionMenuViewModel, SessionMenuViewModel>();
            this.context.Services.AddSingleton<IModelMenuViewModel, ModelMenuViewModel>();
            this.context.Services.AddSingleton<IAuthorizedMenuEntryViewModel, AuthorizedMenuEntryViewModel>();
            this.context.ConfigureDevExpressBlazor();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyTopMenu()
        {
            var renderer = this.context.RenderComponent<TopMenu>();
            var authorizedMenuEntries = renderer.FindComponents<AuthorizedMenuEntry>();

            Assert.That(authorizedMenuEntries.All(x => !x.Instance.AuthorizedMenuEntryViewModel.IsAuthenticated), Is.True);

            var session = new Mock<ISession>();
            session.Setup(x => x.ActivePerson).Returns(new Person() { GivenName = "User" , ShortName = "User"});

            this.sessionService.Setup(x => x.IsSessionOpen).Returns(true);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.stateProvider.NotifyAuthenticationStateChanged();
            Assert.That(authorizedMenuEntries.All(x => x.Instance.AuthorizedMenuEntryViewModel.IsAuthenticated), Is.True);

            var sessionMenuEntry = authorizedMenuEntries[2];
            var sessionMenuInstance = (SessionMenu)sessionMenuEntry.Instance;
            sessionMenuInstance.Expanded = true;
            await renderer.InvokeAsync(sessionMenuInstance.Logout);
            
            Assert.Multiple(() =>
            {
                this.authenticationService.Verify(x => x.Logout(), Times.Once);
                Assert.That(sessionMenuInstance.Expanded, Is.False);
            });

            var iteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup
                {
                    Iid = Guid.NewGuid(),
                    IterationNumber = 4,
                    Container = new EngineeringModelSetup
                    {
                        Name = "Envision",
                        Iid = Guid.NewGuid()
                    }
                }
            };

            this.sessionService.Setup(x => x.IsSessionOpen).Returns(true);
            this.sessionService.Setup(x => x.Session).Returns(session.Object);
            this.sessionService.Setup(x => x.DefaultIteration).Returns(iteration);

            var systemDomain = new DomainOfExpertise
            {
                Iid = Guid.NewGuid(),
                Name = "System"
            };

            var thermoDomain = new DomainOfExpertise
            {
                Iid = Guid.NewGuid(),
                Name = "Thermodynamic"
            };

            this.sessionService.Setup(x => x.GetModelDomains(It.IsAny<EngineeringModelSetup>()))
                .Returns(new List<DomainOfExpertise>
                {
                    systemDomain, thermoDomain
                });

            this.sessionService.Setup(x => x.GetDomainOfExpertise(iteration)).Returns(systemDomain);
            this.sourceList.Add(iteration);

            var modelMenuEntry = authorizedMenuEntries[1];
            var modelMenuInstance = (ModelMenu)modelMenuEntry.Instance;
            var modelMenuViewModel = modelMenuInstance.ViewModel;

            var modelRow = renderer.FindComponent<ModelMenuRow>();
            await renderer.InvokeAsync(modelRow.Instance.ViewModel.SwitchDomain);
            Assert.That(modelMenuViewModel.IsOnSwitchDomainMode, Is.True);
            var switchDomainComponent = renderer.FindComponent<SwitchDomain>();
            switchDomainComponent.Instance.ViewModel.SelectedDomainOfExpertise = switchDomainComponent.Instance.ViewModel.AvailableDomains.Last();
            await renderer.InvokeAsync(switchDomainComponent.Instance.ViewModel.OnSubmit.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(modelMenuViewModel.IsOnSwitchDomainMode, Is.False);

                this.sessionService.Verify(x => x.SwitchDomain(It.IsAny<Iteration>(),
                    It.IsAny<DomainOfExpertise>()), Times.Once);
            });

            await renderer.InvokeAsync(modelRow.Instance.ViewModel.CloseIteration);
            Assert.That(modelMenuViewModel.ConfirmCancelViewModel.IsVisible, Is.True);

            await renderer.InvokeAsync(modelMenuViewModel.ConfirmCancelViewModel.OnCancel.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(modelMenuViewModel.ConfirmCancelViewModel.IsVisible, Is.False);
                this.sessionService.Verify(x => x.CloseIteration(It.IsAny<Iteration>()), Times.Never);
            });

            await renderer.InvokeAsync(modelRow.Instance.ViewModel.CloseIteration);
            await renderer.InvokeAsync(modelMenuViewModel.ConfirmCancelViewModel.OnConfirm.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(modelMenuViewModel.ConfirmCancelViewModel.IsVisible, Is.False);
                this.sessionService.Verify(x => x.CloseIteration(It.IsAny<Iteration>()), Times.Once);
            });
        }
    }
}
