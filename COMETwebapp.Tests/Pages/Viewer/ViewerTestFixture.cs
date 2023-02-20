// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewerTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Pages.Viewer
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
    using COMETwebapp.Components.Shared.Selectors;
    using COMETwebapp.Extensions;
    using COMETwebapp.IterationServices;
    using COMETwebapp.Model;
    using COMETwebapp.Pages.ModelDashboard;
    using COMETwebapp.Pages.Viewer;
    using COMETwebapp.Services.Interoperability;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.ModelDashboard.ParameterValues;
    using COMETwebapp.ViewModels.Components.ModelDashboard;
    using COMETwebapp.ViewModels.Components.Shared;
    using COMETwebapp.ViewModels.Components.Shared.Selectors;
    using COMETwebapp.ViewModels.Components.Viewer;
    using COMETwebapp.ViewModels.Components.Viewer.Canvas;
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

    using DevExpress.Blazor;

    using DynamicData;

    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;
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

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.sessionService = new Mock<ISessionService>();
            this.openedIterations = new SourceList<Iteration>();
            this.sessionService.Setup(x => x.OpenIterations).Returns(this.openedIterations);
            this.viewModel = new SingleIterationApplicationTemplateViewModel(this.sessionService.Object, new IterationSelectorViewModel());
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.DataSourceUri).Returns("http://localhost:5000");
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
            this.context.Services.AddSingleton<IViewerBodyViewModel, ViewerBodyViewModel>();

            var selectionMediator = new Mock<ISelectionMediator>();
            var babylonInterop = new Mock<IBabylonInterop>();
            var iterationService = new Mock<IIterationService>();

            this.context.Services.AddSingleton(selectionMediator.Object);
            this.context.Services.AddSingleton(babylonInterop.Object);
            this.context.Services.AddSingleton(iterationService.Object);
            this.context.Services.AddSingleton<IActualFiniteStateSelectorViewModel, ActualFiniteStateSelectorViewModel>();
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyOpenModelPresent()
        {
            var renderer = this.context.RenderComponent<Viewer>();
            Assert.That(() => renderer.FindComponent<OpenModel>(), Throws.Nothing);
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
                Assert.That(this.viewModel.SelectedIteration, Is.Not.Null);
                Assert.That(navigation.Uri.Contains("server"), Is.True);
                Assert.That(navigation.Uri.Contains(this.secondIteration.Iid.ToShortGuid()), Is.True);
            });
        }

        [Test]
        public void VerifyIterationPreselection()
        {
            this.openedIterations.AddRange(new List<Iteration> { this.firstIteration });

            this.context.RenderComponent<Viewer>(parameters =>
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

            this.context.RenderComponent<Viewer>(parameters =>
            {
                parameters.Add(p => p.IterationId, this.secondIteration.Iid.ToShortGuid());
            });

            Assert.That(this.viewModel.SelectedIteration, Is.Null);
        }
    }
}
