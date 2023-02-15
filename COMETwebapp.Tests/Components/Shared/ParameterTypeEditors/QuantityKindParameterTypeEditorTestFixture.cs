// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="QuantityKindParameterTypeEditorTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Components.Shared.ParameterTypeEditors
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.Components.Shared.ParameterTypeEditors;
    using COMETwebapp.ViewModels.Components.Shared.ParameterEditors;

    using DevExpress.Blazor;
    using DevExpress.Blazor.Popup.Internal;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class QuantityKindParameterTypeEditorTestFixture
    {
        private TestContext context;
        private IRenderedComponent<QuantityKindParameterTypeEditor> renderedComponent;
        private QuantityKindParameterTypeEditor editor;
        private bool EventCallbackCalled = false;
        private Mock<IParameterEditorBaseViewModel<QuantityKind>> viewModelMock;
        private EventCallback<IValueSet> eventCallback;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.Services.AddDevExpressBlazor();
            this.context.JSInterop.SetupVoid("DxBlazor.Input.loadModule");

            var parameterValueSet = new ParameterValueSet()
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(new List<string>(){"0","0","0"}),
            };

            this.viewModelMock = new Mock<IParameterEditorBaseViewModel<QuantityKind>>();
            this.viewModelMock.Setup(x => x.ValueSet).Returns(parameterValueSet);
            
            this.eventCallback = new EventCallbackFactory().Create(this, (IValueSet valueSet) =>
            {
                this.EventCallbackCalled = true;
            });

            this.renderedComponent = this.context.RenderComponent<QuantityKindParameterTypeEditor>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
                parameters.Add(p => p.ParameterValueChanged, this.eventCallback);
            });
            
            this.editor = this.renderedComponent.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            this.context.Dispose();
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderedComponent, Is.Not.Null);
                Assert.That(this.editor, Is.Not.Null);
                Assert.That(this.editor.ViewModel, Is.Not.Null);
                Assert.That(this.editor.ParameterValueChanged, Is.Not.Null);
            });
        }

        [Test]
        public async Task VerifyParameterValueChanged()
        {
            var textbox = this.renderedComponent.FindComponent<DxTextBox>();
            Assert.That(textbox, Is.Not.Null);
            await this.editor.ParameterValueChanged.InvokeAsync();
            Assert.That(this.EventCallbackCalled, Is.True);
        }

        [Test]
        public void VerifyThatComponetCanBeReadOnly()
        {
            this.viewModelMock.Setup(x => x.IsReadOnly).Returns(true);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
                parameters.Add(p => p.ParameterValueChanged, this.eventCallback);
            });

            var textbox = this.renderedComponent.FindComponent<DxTextBox>();
            this.editor.ViewModel.IsReadOnly = true;
            Assert.That(textbox.Instance.ReadOnly, Is.True);
        }
    }
}
