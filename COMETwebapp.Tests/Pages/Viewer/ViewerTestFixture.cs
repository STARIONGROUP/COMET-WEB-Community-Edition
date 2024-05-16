﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewerTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Pages.Viewer
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Components.Selectors;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Model.Configuration;
    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Services.StringTableService;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.Utilities;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Pages.Viewer;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Viewer;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ViewerTestFixture
    {
        private TestContext context;
        private ISingleIterationApplicationTemplateViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private SourceList<Iteration> openedIterations;
        private Mock<ISession> session;
        private Iteration firstIteration;
        private Iteration secondIteration;
        private ICDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.sessionService = new Mock<ISessionService>();
            this.openedIterations = new SourceList<Iteration>();
            this.sessionService.Setup(x => x.OpenIterations).Returns(this.openedIterations);
            this.viewModel = new SingleIterationApplicationTemplateViewModel(this.sessionService.Object, new IterationSelectorViewModel());
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.DataSourceUri).Returns("http://localhost:5000");
            this.sessionService.Setup(x => x.Session).Returns(this.session.Object);
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(new DomainOfExpertise { Iid = Guid.NewGuid() });

            this.firstIteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup
                {
                    Iid = Guid.NewGuid(),
                    IterationNumber = 1,
                    Container = new EngineeringModelSetup
                    {
                        Iid = Guid.NewGuid(),
                        Name = "EnVision"
                    }
                }
            };

            this.secondIteration = new Iteration
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup
                {
                    Iid = Guid.NewGuid(),
                    IterationNumber = 4,
                    Container = new EngineeringModelSetup
                    {
                        Iid = Guid.NewGuid(),
                        Name = "Loft"
                    }
                }
            };

            var mockConfigurationService = new Mock<IConfiguration>();
            mockConfigurationService.Setup(x => x.GetSection(ConfigurationKeys.ServerConfigurationKey).Get<ServerConfiguration>()).Returns(new ServerConfiguration());
            this.messageBus = new CDPMessageBus();

            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddSingleton(this.viewModel);
            this.context.Services.AddSingleton(this.sessionService.Object);
            this.context.Services.AddSingleton<IOpenModelViewModel, OpenModelViewModel>();
            this.context.Services.AddSingleton<IViewerBodyViewModel, ViewerBodyViewModel>();
            this.context.Services.AddSingleton<ISubscriptionService, SubscriptionService>();
            this.context.Services.AddSingleton(mockConfigurationService.Object);
            this.context.Services.AddSingleton<ISelectionMediator, SelectionMediator>();
            this.context.Services.AddSingleton<IBabylonInterop, BabylonInterop>();
            this.context.Services.AddSingleton<IActualFiniteStateSelectorViewModel, ActualFiniteStateSelectorViewModel>();
            this.context.Services.AddSingleton(this.messageBus);

            var configurationService = new Mock<IStringTableService>();
            configurationService.Setup(x => x.GetText(It.IsAny<string>())).Returns("something");
            this.context.Services.AddSingleton(configurationService.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyIterationPreselection()
        {
            this.openedIterations.AddRange(new List<Iteration> { this.firstIteration });

            this.context.RenderComponent<Viewer>(parameters => { parameters.Add(p => p.IterationId, this.firstIteration.Iid.ToShortGuid()); });

            var navigation = this.context.Services.GetService<NavigationManager>();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedThing, Is.EqualTo(this.firstIteration));
                Assert.That(navigation.Uri.Contains("server"), Is.True);
                Assert.That(navigation.Uri.Contains(this.firstIteration.Iid.ToShortGuid()), Is.True);
            });

            this.viewModel.SelectedThing = null;

            this.context.RenderComponent<Viewer>(parameters => { parameters.Add(p => p.IterationId, this.secondIteration.Iid.ToShortGuid()); });

            Assert.That(this.viewModel.SelectedThing, Is.EqualTo(this.firstIteration));
        }

        [Test]
        public async Task VerifyIterationSelection()
        {
            this.openedIterations.AddRange(new List<Iteration> { this.firstIteration, this.secondIteration });
            var renderer = this.context.RenderComponent<Viewer>();

            Assert.That(this.viewModel.IterationSelectorViewModel.AvailableIterations.ToList(), Has.Count.EqualTo(2));
            this.viewModel.IterationSelectorViewModel.SelectedIteration = this.viewModel.IterationSelectorViewModel.AvailableIterations.Last();
            var iterationSelector = renderer.FindComponent<IterationSelector>();
            var submitButton = iterationSelector.FindComponent<DxButton>();

            await renderer.InvokeAsync(() => submitButton.Instance.Click.InvokeAsync(new MouseEventArgs()));
            var navigation = this.context.Services.GetService<NavigationManager>();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedThing, Is.Not.Null);
                Assert.That(navigation.Uri.Contains("server"), Is.True);
                Assert.That(navigation.Uri.Contains(this.secondIteration.Iid.ToShortGuid()), Is.True);
            });
        }

        [Test]
        public void VerifyOpenModelPresent()
        {
            var renderer = this.context.RenderComponent<Viewer>();
            Assert.That(() => renderer.FindComponent<OpenModel>(), Throws.Nothing);
        }
    }
}
