// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewerTests.cs" company="RHEA System S.A.">
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

    using COMETwebapp.Components.Viewer;
    using COMETwebapp.IterationServices;
    using COMETwebapp.Primitives;
    using COMETwebapp.SessionManagement;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    /// <summary>
    /// viewer tests that verifies the correct behavior 
    /// </summary>
    [TestFixture]
    public class ViewerTests
    {
        private Pages.Viewer.Viewer viewer;
        private TestContext context;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.JsRuntime = context.JSInterop.JSRuntime;

            this.context.Services.AddSingleton<ISessionAnchor, SessionAnchor>();
            this.context.Services.AddSingleton<IShapeFactory, ShapeFactory>();
            this.context.Services.AddTransient<ISceneProvider, SceneProvider>();
            this.context.Services.AddTransient<IIterationService, IterationService>();

            var renderer = this.context.RenderComponent<BabylonCanvas>();

            var viewerRender = this.context.RenderComponent<Pages.Viewer.Viewer>(parameters =>
                parameters.Add(p => p.CanvasComponentReference, renderer.Instance)
            );

            this.viewer = viewerRender.Instance;
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.IsNotNull(this.viewer.CanvasComponentReference);
            Assert.IsNull(this.viewer.FilterOption);
            Assert.IsNotNull(this.viewer.Elements);
            Assert.IsTrue(this.viewer.Elements.Count == 0);
            Assert.IsNull(this.viewer.OptionSelected);
            Assert.IsNotNull(this.viewer.SessionAnchor);
            Assert.IsNotNull(this.viewer.IterationService);
            Assert.IsNotNull(this.viewer.Options);
            Assert.IsTrue(this.viewer.Options.Count == 0);
            Assert.IsNull(this.viewer.States);
            Assert.IsNull(this.viewer.ListActualFiniteStateLists);
            Assert.IsNull(this.viewer.RootNode);
        }
    }
}
