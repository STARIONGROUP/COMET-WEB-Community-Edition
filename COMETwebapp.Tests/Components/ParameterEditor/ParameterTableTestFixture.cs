// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTableTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Components.ParameterEditor
{
    using Bunit;

    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Components.ParameterEditor;
    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.ViewModels.Components.ParameterEditor;

    using DynamicData;

    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ParameterTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ParameterTable> renderedComponent;
        private ParameterTable table;
        
        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddSingleton<ISessionService, SessionService>();
            this.context.Services.AddSingleton<IParameterTableViewModel, ParameterTableViewModel>();

            var element1 = new ElementDefinition();

            var element2 = new ElementUsage()
            {
                ElementDefinition = new ElementDefinition()
            };

            var elementList = new SourceList<ElementBase>();
            elementList.Add(element1);
            elementList.Add(element2);

            this.renderedComponent = this.context.RenderComponent<ParameterTable>(parameters =>
            {
                parameters.Add(p => p.Elements, elementList);
            });
            
            this.table = this.renderedComponent.Instance;
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
                Assert.That(this.renderedComponent, Is.Not.Null);
                Assert.That(this.table, Is.Not.Null);
                Assert.That(this.table.Elements, Is.Not.Null);
                Assert.That(this.table.ViewModel, Is.Not.Null);
            });
        }
    }
}
