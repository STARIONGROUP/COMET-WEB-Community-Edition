// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewerPageTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
//
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Viewer
{
    using Bunit;
    using COMETwebapp.Components.CanvasComponent;
    using COMETwebapp.Interoperability;
    using COMETwebapp.IterationServices;
    using COMETwebapp.Pages.Viewer;
    using COMETwebapp.Primitives;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Tests.Utilities;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    public class ViewerPageTestFixture
    {
        private TestContext context;
        private ViewerPage viewerPage;
        private IRenderedComponent<ViewerPage> renderedComponent;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.AddDevExpressBlazorTesting();
            this.context.ConfigureDevExpressBlazor();

            this.context.Services.AddSingleton<IIterationService, IterationService>();

            Mock<ISessionAnchor> sessionAnchor = new Mock<ISessionAnchor>();

            this.context.Services.AddSingleton<ISessionAnchor, SessionAnchor>();
            this.context.Services.AddSingleton<IJSInterop,JSInterop>();
            this.context.Services.AddSingleton<IShapeFactory, ShapeFactory>();

            this.context.JSInterop.SetupVoid("AddPrimitive", _ => true);

            var canvasRenderer = this.context.RenderComponent<BabylonCanvas>();

            this.renderedComponent = this.context.RenderComponent<ViewerPage>(parameters =>
            {
                parameters.Add(p => p.CanvasComponentReference, canvasRenderer.Instance);
            });
            this.viewerPage = this.renderedComponent.Instance;            
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewerPage.JSInterop, Is.Not.Null);
                Assert.That(this.viewerPage.CanvasComponentReference, Is.Not.Null);
                Assert.That(this.viewerPage.FilterOption, Is.Null);
                Assert.That(this.viewerPage.Elements, Is.Not.Null);
                Assert.That(this.viewerPage.ElementUsagesOnScreen, Is.Not.Null);
                Assert.That(this.viewerPage.OptionSelected, Is.Null);
                Assert.That(this.viewerPage.StateSelected, Is.Null);
                Assert.That(this.viewerPage.IterationService, Is.Not.Null);
                Assert.That(this.viewerPage.Options, Is.Not.Null);
                Assert.That(this.viewerPage.States, Is.Null);
                Assert.That(this.viewerPage.ListActualFiniteStateLists, Is.Null);
                Assert.That(this.viewerPage.RootNode, Is.Null);
            });
        }
    }
}
