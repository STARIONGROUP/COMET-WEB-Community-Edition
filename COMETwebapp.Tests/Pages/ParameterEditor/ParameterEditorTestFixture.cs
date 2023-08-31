// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterEditorTestFixture.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.Tests.Pages.ParameterEditor
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMET.Web.Common.Components;
    using COMET.Web.Common.Components.Selectors;
    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Services.ConfigurationService;
    using COMET.Web.Common.Services.NotificationService;
	using COMET.Web.Common.Services.ServerConnexionService;
	using COMET.Web.Common.Services.SessionManagement;
	using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.Pages.ParameterEditor;
    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.ViewModels.Components.ParameterEditor;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ParameterEditorTestFixture
    {
        private TestContext context;
        private ISingleIterationApplicationTemplateViewModel viewModel;
        private Mock<ISessionService> sessionService;
        private SourceList<Iteration> openedIterations;
        private Mock<ISession> session;
        private Iteration firstIteration;
        private Iteration secondIteration;

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
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(new SiteDirectory() { Iid = Guid.NewGuid() });
            this.sessionService.Setup(x => x.Session).Returns(this.session.Object);
            this.sessionService.Setup(x => x.GetDomainOfExpertise(It.IsAny<Iteration>())).Returns(new DomainOfExpertise() { Iid = Guid.NewGuid() });

            this.firstIteration = new Iteration()
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup()
                {
                    Iid = Guid.NewGuid(),
                    IterationNumber = 1,
                    Container = new EngineeringModelSetup()
                    {
                        Iid = Guid.NewGuid(),
                        Name = "EnVision"
                    }
                }
            };

            this.secondIteration = new Iteration()
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup()
                {
                    Iid = Guid.NewGuid(),
                    IterationNumber = 4,
                    Container = new EngineeringModelSetup()
                    {
                        Iid = Guid.NewGuid(),
                        Name = "Loft"
                    }
                }
            };

            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddSingleton(this.viewModel);
            this.context.Services.AddSingleton(this.sessionService.Object);
            this.context.Services.AddSingleton<IOpenModelViewModel, OpenModelViewModel>();
            this.context.Services.AddSingleton<IParameterEditorBodyViewModel, ParameterEditorBodyViewModel>();
            this.context.Services.AddSingleton<ISubscriptionService, SubscriptionService>();
            this.context.Services.AddSingleton<IParameterTableViewModel, ParameterTableViewModel>();
            this.context.Services.AddSingleton<INotificationService, NotificationService>();
			this.context.Services.AddSingleton<IServerConnexionService, ServerConnexionService>();

			var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(x => x.GetText(It.IsAny<string>())).Returns("something");
            this.context.Services.AddSingleton(configurationService.Object);
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyOpenModelPresent()
        {
            var renderer = this.context.RenderComponent<ParameterEditor>();
            Assert.That(() => renderer.FindComponent<OpenModel>(), Throws.Nothing);
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyIterationSelection()
        {
            this.openedIterations.AddRange(new List<Iteration> { this.firstIteration, this.secondIteration });
            var renderer = this.context.RenderComponent<ParameterEditor>();

            Assert.That(this.viewModel.IterationSelectorViewModel.AvailableIterations.ToList(), Has.Count.EqualTo(2));
            this.viewModel.IterationSelectorViewModel.SelectedIteration = this.viewModel.IterationSelectorViewModel.AvailableIterations.Last();
            var iterationSelector = renderer.FindComponent<IterationSelector>();
            var submitButton = iterationSelector.FindComponent<DxButton>();

            await renderer.InvokeAsync(() => submitButton.Instance.Click.InvokeAsync(new MouseEventArgs()));
            var navigation = this.context.Services.GetService<NavigationManager>();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedIteration, Is.Not.Null);
                Assert.That(navigation.Uri.Contains("server"), Is.True);
                Assert.That(navigation.Uri.Contains(this.secondIteration.Iid.ToShortGuid()), Is.True);
            });
        }

        [Test]
        public void VerifyIterationPreselection()
        {
            this.openedIterations.AddRange(new List<Iteration> { this.firstIteration });

            this.context.RenderComponent<ParameterEditor>(parameters =>
            {
                parameters.Add(p => p.IterationId, this.firstIteration.Iid.ToShortGuid());
            });

            var navigation = this.context.Services.GetService<NavigationManager>();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedIteration, Is.EqualTo(this.firstIteration));
                Assert.That(navigation.Uri.Contains("server"), Is.True);
                Assert.That(navigation.Uri.Contains(this.firstIteration.Iid.ToShortGuid()), Is.True);
            });

            this.viewModel.SelectedIteration = null;

            this.context.RenderComponent<ParameterEditor>(parameters =>
            {
                parameters.Add(p => p.IterationId, this.secondIteration.Iid.ToShortGuid());
            });

            Assert.That(this.viewModel.SelectedIteration, Is.Null);
        }
    }
}
