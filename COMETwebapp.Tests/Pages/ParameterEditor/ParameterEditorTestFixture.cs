// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterEditorTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Pages.ParameterEditor
{
    using System.Collections.Generic;

    using Bunit;

    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Pages.ParameterEditor;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.ViewModels.Pages.ParameterEditor;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ParameterEditorTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ParameterEditor> renderedComponent;
        private ParameterEditor editor;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.Services.AddDevExpressBlazor();

            var parameterEditorViewModel = new Mock<IParameterEditorViewModel>();

            parameterEditorViewModel.Setup(x => x.Elements).Returns(new List<ElementBase>()
            {
                new ElementDefinition()
                {
                    Name = "Element1"
                }, 
                new ElementDefinition()
                {
                    Name = "Element2"
                }, 
                new ElementDefinition()
                {
                    Name = "Element3"
                }
            });

            this.context.Services.AddSingleton(parameterEditorViewModel.Object);
            this.context.Services.AddSingleton<ISessionService, SessionService>();
            
            this.renderedComponent = this.context.RenderComponent<ParameterEditor>();
            this.editor = this.renderedComponent.Instance;
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.editor, Is.Not.Null);
                Assert.That(this.renderedComponent, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyComponentUI()
        {
            var elementFilterCombo = this.renderedComponent.Find(".element-filter");
        }
    }
}
