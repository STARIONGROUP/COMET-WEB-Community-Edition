// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertiesComponentTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Components.PropertiesPanel
{
    using Bunit;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;
    using COMETwebapp.Components.Canvas;
    using COMETwebapp.Components.PopUps;
    using COMETwebapp.Components.PropertiesPanel;
    using COMETwebapp.IterationServices;
    using COMETwebapp.SessionManagement;
    using COMETwebapp.Utilities;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Concurrent;
    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class PropertiesComponentTestFixture
    {
        private TestContext context;
        private IRenderedComponent<PropertiesComponent> renderedComponent;
        private PropertiesComponent propertiesComponent;

        private Mock<CanvasComponent> canvasComponentMock;
        private Mock<ConfirmChangeSelectionPopUp> popUpMock;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Uri uri = new Uri("http://www.rheagroup.com");

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.Services.AddSingleton<IIterationService, IterationService>();
            this.context.Services.AddSingleton<ISessionAnchor, SessionAnchor>();
            this.context.Services.AddSingleton<ISelectionMediator,SelectionMediator>();

            this.canvasComponentMock = new Mock<CanvasComponent>();
            this.popUpMock = new Mock<ConfirmChangeSelectionPopUp>();

            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            var parameter1 = new Parameter(Guid.NewGuid(), this.cache, this.uri);
            var parameter2 = new Parameter(Guid.NewGuid(), this.cache, this.uri);
            
            this.renderedComponent = this.context.RenderComponent<PropertiesComponent>(parameters =>
            {
                parameters.Add(p => p.Canvas, this.canvasComponentMock.Object);
                parameters.Add(p => p.ConfirmChangeSelectionPopUp, this.popUpMock.Object);
                parameters.Add(p => p.ParametersInUse, new System.Collections.Generic.List<ParameterBase>() { parameter1, parameter2 });
            });
            this.propertiesComponent = this.renderedComponent.Instance;
        }

        [Test]
        public void VerifyComponent()
        {
            
        }

        [Test]
        public void VerifyOnSubmitWorks()
        {
            var button = this.renderedComponent.Find(".submit-button");
            button.Click();

        }

        [Test]
        public void VerifyGetChangedParametersWorks()
        {

        }

        [Test]
        public void VerifyParameterChangedWorks()
        {
            Assert.That(this.propertiesComponent.SelectedParameter, Is.Null);
            var parameter = this.renderedComponent.Find(".parameter-item");
            parameter.Click();
            
        }
    }
}
