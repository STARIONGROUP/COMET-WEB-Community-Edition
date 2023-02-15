// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterSwitchKindComponentTestFixture.cs" company="RHEA System S.A.">
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

    using DevExpress.Blazor;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    public class ParameterSwitchKindComponentTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ParameterSwitchKindComponent> renderedComponent;
        private ParameterSwitchKindComponent parameterSwitch;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            var sessionMock = new Mock<ISessionService>();

            var viewModel = new ParameterSwitchKindComponentViewModel(sessionMock.Object, null);

            this.renderedComponent = this.context.RenderComponent<ParameterSwitchKindComponent>(parameters =>
            {
                parameters.Add(p => p.SwitchValue, ParameterSwitchKind.MANUAL);
                parameters.Add(p => p.ViewModel, viewModel);
            });

            this.parameterSwitch = this.renderedComponent.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyComponent()
        {
            var combo = this.renderedComponent.FindComponent<DxComboBox<ParameterSwitchKind, ParameterSwitchKind>>();

            Assert.Multiple(() =>
            {
                Assert.That(this.parameterSwitch, Is.Not.Null);
                Assert.That(this.renderedComponent, Is.Not.Null);
                Assert.That(combo, Is.Not.Null);
                Assert.That(combo.Instance.Value, Is.EqualTo(ParameterSwitchKind.MANUAL));
            });
        }
    }
}
