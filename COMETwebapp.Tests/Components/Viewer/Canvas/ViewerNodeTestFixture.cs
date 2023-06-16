// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewerNodeTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar
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

namespace COMETwebapp.Tests.Components.Viewer.Canvas
{
    using Bunit;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.Viewer;
    using COMETwebapp.ViewModels.Components.Shared;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ViewerNodeTestFixture
    {
        private TestContext context;
        private ViewerNode nodeComponent;
        private Mock<IBaseNodeViewModel> componentViewModel;
        private IRenderedComponent<ViewerNode> renderedComponent;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.componentViewModel = new Mock<IBaseNodeViewModel>();
            
            this.renderedComponent = this.context.RenderComponent<ViewerNode>(parameters 
                => parameters.Add(p=> p.ViewModel, this.componentViewModel.Object)
                             .Add(p=>p.Level, 1)
            );
            
            this.nodeComponent = this.renderedComponent.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.nodeComponent.ViewModel, Is.Not.Null);
                Assert.That(this.nodeComponent.Level, Is.EqualTo(1));
            });
        }
    }
}
