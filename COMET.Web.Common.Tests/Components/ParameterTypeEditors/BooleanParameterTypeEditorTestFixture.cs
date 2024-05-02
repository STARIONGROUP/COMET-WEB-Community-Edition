// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BooleanParameterTypeEditorTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
//    Annex A and Annex C.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMET.Web.Common.Tests.Components.ParameterTypeEditors
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMET.Web.Common.Components.ParameterTypeEditors;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

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

            var parameterValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(new List<string> { "-" })
            };

            this.viewModelMock = new Mock<IParameterEditorBaseViewModel<BooleanParameterType>>();
            this.viewModelMock.Setup(x => x.ValueSet).Returns(parameterValueSet);
            this.viewModelMock.Setup(x => x.ValueArray).Returns(parameterValueSet.Manual);
            this.viewModelMock.Setup(x => x.IsReadOnly).Returns(false);

            this.eventCallback = new EventCallbackFactory().Create(this, (IValueSet _) => { this.eventCallbackCalled = true; });

            this.viewModelMock.Setup(x => x.OnParameterValueChanged(It.IsAny<object>()))
                .Callback(() => this.eventCallback.InvokeAsync())
                .Returns(Task.CompletedTask);

            this.renderedComponent = this.context.RenderComponent<BooleanParameterTypeEditor>(parameters => { parameters.Add(p => p.ViewModel, this.viewModelMock.Object); });

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
            var textbox = this.renderedComponent.FindComponent<DxComboBox<string, string>>();
            Assert.That(textbox, Is.Not.Null);
            await this.renderedComponent.InvokeAsync(() => this.viewModelMock.Object.OnParameterValueChanged("false"));
            Assert.That(this.eventCallbackCalled, Is.True);
        }

        [Test]
        public void VerifyThatComponetCanBeReadOnly()
        {
            this.viewModelMock.Setup(x => x.IsReadOnly).Returns(true);

            this.renderedComponent.SetParametersAndRender(parameters => { parameters.Add(p => p.ViewModel, this.viewModelMock.Object); });

            var textbox = this.renderedComponent.FindComponent<DxComboBox<string, string>>();
            this.editor.ViewModel.IsReadOnly = true;
            Assert.That(textbox.Instance.ReadOnly, Is.True);
        }
    }
}
