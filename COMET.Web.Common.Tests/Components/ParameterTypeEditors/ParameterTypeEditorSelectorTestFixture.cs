// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeEditorSelectorTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of COMET WEB Community Edition
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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

    using CDP4Dal;

    using COMET.Web.Common.Components.ParameterTypeEditors;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using Microsoft.AspNetCore.Components;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ParameterTypeEditorSelectorTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ParameterTypeEditorSelector> renderedComponent;
        private ParameterTypeEditorSelector editorSelector;
        private Mock<IParameterTypeEditorSelectorViewModel> viewModelMock;
        private ParameterValueSet valueSet;
        private Mock<ICDPMessageBus> messageBus;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.viewModelMock = new Mock<IParameterTypeEditorSelectorViewModel>();

            this.renderedComponent = this.context.RenderComponent<ParameterTypeEditorSelector>(parameters => { parameters.Add(p => p.ViewModel, this.viewModelMock.Object); });
            this.editorSelector = this.renderedComponent.Instance;
         
            this.valueSet = new ParameterValueSet()
            {
                Manual = new ValueArray<string>(["-"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            this.messageBus = new Mock<ICDPMessageBus>();
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
                Assert.That(this.editorSelector, Is.Not.Null);
                Assert.That(this.editorSelector.ViewModel, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyThatBooleanParameterTypeEditorIsRendered()
        {
            var parameterType = new BooleanParameterType
            {
                Iid = Guid.NewGuid()
            };

            this.viewModelMock.Setup(x => x.CreateParameterEditorViewModel<BooleanParameterType>())
                .Returns(new BooleanParameterTypeEditorViewModel(parameterType, this.valueSet, false));

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);
            this.renderedComponent.Render();

            var editor = this.renderedComponent.FindComponent<BooleanParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }

        [Test]
        public void VerifyThatCompoundParameterTypeEditorIsRendered()
        {
            var parameterType = new CompoundParameterType
            {
                Iid = Guid.NewGuid()
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);
            
            this.viewModelMock.Setup(x => x.CreateParameterEditorViewModel<CompoundParameterType>())
                .Returns(new CompoundParameterTypeEditorViewModel(parameterType, this.valueSet, false, this.messageBus.Object));

            this.renderedComponent.Render();

            var editor = this.renderedComponent.FindComponent<CompoundParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }

        [Test]
        public void VerifyThatDateParameterTypeEditorIsRendered()
        {
            var parameterType = new DateParameterType
            {
                Iid = Guid.NewGuid()
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.viewModelMock.Setup(x => x.CreateParameterEditorViewModel<DateParameterType>())
                .Returns(new DateParameterTypeEditorViewModel(parameterType, this.valueSet, false));

            this.renderedComponent.Render();

            var editor = this.renderedComponent.FindComponent<DateParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }

        [Test]
        public void VerifyThatDateTimeParameterTypeEditorIsRendered()
        {
            var parameterType = new DateTimeParameterType
            {
                Iid = Guid.NewGuid()
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.viewModelMock.Setup(x => x.CreateParameterEditorViewModel<DateTimeParameterType>())
                .Returns(new DateTimeParameterTypeEditorViewModel(parameterType, this.valueSet, false));

            this.renderedComponent.Render();
            var editor = this.renderedComponent.FindComponent<DateTimeParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }

        [Test]
        public void VerifyThatEnumerationParameterTypeEditorIsRendered()
        {
            var parameterType = new EnumerationParameterType
            {
                Iid = Guid.NewGuid()
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.viewModelMock.Setup(x => x.CreateParameterEditorViewModel<EnumerationParameterType>())
                .Returns(new EnumerationParameterTypeEditorViewModel(parameterType, this.valueSet, false));
            
            this.renderedComponent.Render();

            var editor = this.renderedComponent.FindComponent<EnumerationParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }

        [Test]
        public void VerifyThatQuantityKindParameterTypeEditorIsRendered()
        {
            var parameterType = new SimpleQuantityKind
            {
                Iid = Guid.NewGuid()
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.viewModelMock.Setup(x => x.CreateParameterEditorViewModel<QuantityKind>())
                .Returns(new QuantityKindParameterTypeEditorViewModel(parameterType, this.valueSet, false));
            
            this.renderedComponent.Render();
            var editor = this.renderedComponent.FindComponent<QuantityKindParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }

        [Test]
        public void VerifyThatTextParameterTypeEditorIsRendered()
        {
            var parameterType = new TextParameterType
            {
                Iid = Guid.NewGuid()
            };

            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.viewModelMock.Setup(x => x.CreateParameterEditorViewModel<TextParameterType>())
                .Returns(new TextParameterTypeEditorViewModel(parameterType, this.valueSet, false));
            
            this.renderedComponent.Render();

            var editor = this.renderedComponent.FindComponent<TextParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }

        [Test]
        public void VerifyThatTimeOfDayParameterTypeEditorIsRendered()
        {
            var parameterType = new TimeOfDayParameterType
            {
                Iid = Guid.NewGuid()
            };

            this.viewModelMock.Setup(x => x.ParameterValueChanged).Returns(new EventCallback<(IValueSet, int)>());
            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);

            this.viewModelMock.Setup(x => x.CreateParameterEditorViewModel<TimeOfDayParameterType>())
                .Returns(new TimeOfDayParameterTypeEditorViewModel(parameterType, this.valueSet, false));
            
            this.renderedComponent.Render();

            var editor = this.renderedComponent.FindComponent<TimeOfDayParameterTypeEditor>();

            Assert.That(editor, Is.Not.Null);
        }
    }
}
