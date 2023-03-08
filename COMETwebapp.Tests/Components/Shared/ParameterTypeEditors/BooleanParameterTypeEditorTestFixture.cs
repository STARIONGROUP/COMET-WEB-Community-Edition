// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BooleanParameterTypeEditorTestFixture.cs" company="RHEA System S.A.">
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
    using COMETwebapp.Tests.Helpers;
    using COMETwebapp.ViewModels.Components.Shared.ParameterEditors;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class BooleanParameterTypeEditorTestFixture
    {
        private TestContext context;
        private IRenderedComponent<BooleanParameterTypeEditor> renderedComponent;
        private BooleanParameterTypeEditor editor;
        private bool eventCallbackCalled;
        private Mock<IParameterEditorBaseViewModel<BooleanParameterType>> viewModelMock;
        private EventCallback<IValueSet> eventCallback;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            var parameterValueSet = new ParameterValueSet()
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(new List<string>(){"-"}),
            };

            this.viewModelMock = new Mock<IParameterEditorBaseViewModel<BooleanParameterType>>();
            this.viewModelMock.Setup(x => x.ValueSet).Returns(parameterValueSet);
            this.viewModelMock.Setup(x => x.ValueArray).Returns(parameterValueSet.Manual);
            this.viewModelMock.Setup(x => x.IsReadOnly).Returns(false);

            this.eventCallback = new EventCallbackFactory().Create(this, (IValueSet _) =>
            {
                this.eventCallbackCalled = true;
            });

            this.viewModelMock.Setup(x => x.OnParameterValueChanged(It.IsAny<object>()))
                .Callback(() => this.eventCallback.InvokeAsync())
                .Returns(Task.CompletedTask);

            this.renderedComponent = this.context.RenderComponent<BooleanParameterTypeEditor>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
            });
            
            this.editor = this.renderedComponent.Instance;
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
                Assert.That(this.editor, Is.Not.Null);
                Assert.That(this.editor.ViewModel, Is.Not.Null);
            });
        }

        [Test]
        public async Task VerifyParameterValueChanged()
        {
            var textbox = this.renderedComponent.FindComponent<DxComboBox<string,string>>();
            Assert.That(textbox, Is.Not.Null);
            await this.renderedComponent.InvokeAsync(() => this.viewModelMock.Object.OnParameterValueChanged("false"));
            Assert.That(this.eventCallbackCalled, Is.True);
        }

        [Test]
        public void VerifyThatComponetCanBeReadOnly()
        {
            this.viewModelMock.Setup(x => x.IsReadOnly).Returns(true);

            this.renderedComponent.SetParametersAndRender(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModelMock.Object);
            });

            var textbox = this.renderedComponent.FindComponent<DxComboBox<string, string>>();
            this.editor.ViewModel.IsReadOnly = true;
            Assert.That(textbox.Instance.ReadOnly, Is.True);
        }
    }
}
