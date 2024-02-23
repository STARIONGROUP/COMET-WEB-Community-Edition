// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EnumerationParameterTypeEditorTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25
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
    public class EnumerationParameterTypeEditorTestFixture
    {
        private TestContext context;
        private IRenderedComponent<EnumerationParameterTypeEditor> renderedComponent;
        private EnumerationParameterTypeEditor editor;
        private bool eventCallbackCalled;
        private Mock<IParameterEditorBaseViewModel<EnumerationParameterType>> viewModelMock;
        private EventCallback<IValueSet> eventCallback;
        private EnumerationParameterType parameterType1;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            var enumerationValues = new List<string> { "cube", "sphere", "cylinder" };

            var parameterValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(enumerationValues)
            };

            var enumerationData = new OrderedItemList<EnumerationValueDefinition>(null)
            {
                new EnumerationValueDefinition
                {
                    Iid = Guid.NewGuid(),
                    Name = enumerationValues[0]
                },
                new EnumerationValueDefinition
                {
                    Iid = Guid.NewGuid(),
                    Name = enumerationValues[1]
                },
                new EnumerationValueDefinition
                {
                    Iid = Guid.NewGuid(),
                    Name = enumerationValues[2]
                }
            };

            var parameterType = new EnumerationParameterType
            {
                Iid = Guid.NewGuid()
            };

            this.parameterType1 = new EnumerationParameterType
            {
                Iid = Guid.NewGuid(),
                AllowMultiSelect = true
            };

            this.parameterType1.ValueDefinition.AddRange(enumerationData);

            parameterType.ValueDefinition.AddRange(enumerationData);

            this.viewModelMock = new Mock<IParameterEditorBaseViewModel<EnumerationParameterType>>();
            this.viewModelMock.As<IEnumerationParameterTypeEditorViewModel>();
            this.viewModelMock.As<IEnumerationParameterTypeEditorViewModel>().Setup(x => x.SelectedEnumerationValueDefinitions).Returns(enumerationValues);
            this.viewModelMock.As<IEnumerationParameterTypeEditorViewModel>().Setup(x => x.EnumerationValueDefinitions).Returns(enumerationData);
            this.viewModelMock.As<IEnumerationParameterTypeEditorViewModel>().Setup(x => x.SelectAllChecked).Returns(false);
            this.viewModelMock.As<IEnumerationParameterTypeEditorViewModel>().Setup(x => x.IsOnEditMode).Returns(true);
            this.viewModelMock.Setup(x => x.ParameterType).Returns(parameterType);
            this.viewModelMock.Setup(x => x.ValueSet).Returns(parameterValueSet);
            this.viewModelMock.Setup(x => x.ValueArray).Returns(parameterValueSet.Manual);

            this.eventCallback = new EventCallbackFactory().Create(this, (IValueSet _) => { this.eventCallbackCalled = true; });

            this.viewModelMock.Setup(x => x.OnParameterValueChanged(It.IsAny<object>()))
                .Callback(() => this.eventCallback.InvokeAsync());

            this.renderedComponent = this.context.RenderComponent<EnumerationParameterTypeEditor>(parameters => { parameters.Add(p => p.ViewModel, this.viewModelMock.Object); });

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
        public void VerifyMultipleSelection()
        {
            this.viewModelMock.Setup(x => x.ParameterType).Returns(this.parameterType1);

            this.renderedComponent.Render();

            var textBox = this.renderedComponent.FindComponent<DxTextBox>();

            Assert.Multiple(() =>
            {
                Assert.That(textBox, Is.Not.Null);
                Assert.That(textBox.Markup, Does.Contain("cube"));
            });

            var dropDownButton = this.renderedComponent.Find(".dropdownIcon");

            Assert.Multiple(() =>
            {
                Assert.That(dropDownButton, Is.Not.Null);
                Assert.That(this.viewModelMock.As<IEnumerationParameterTypeEditorViewModel>().Object.IsOnEditMode, Is.True);
            });

            var listBox = this.renderedComponent.FindComponent<DxListBox<string, string>>();
            Assert.That(listBox, Is.Not.Null);

            var confirmButton = this.renderedComponent.Find("#confirmButton");
            Assert.That(confirmButton, Is.Not.Null);
            confirmButton.Click();
            this.viewModelMock.As<IEnumerationParameterTypeEditorViewModel>().Object.OnConfirmButtonClick();

            var cancelButton = this.renderedComponent.Find("#cancelButton");
            Assert.That(cancelButton, Is.Not.Null);
            cancelButton.Click();
            this.viewModelMock.As<IEnumerationParameterTypeEditorViewModel>().Object.OnCancelButtonClick();

            var checkBox = this.renderedComponent.FindComponent<DxCheckBox<bool>>();
            Assert.That(checkBox, Is.Not.Null);

#pragma warning disable BL0005
            checkBox.Instance.Checked = true;
#pragma warning restore BL0005

            Assert.That(this.viewModelMock.As<IEnumerationParameterTypeEditorViewModel>().Object.SelectedEnumerationValueDefinitions, Has.Count.EqualTo(3));

            this.renderedComponent.InvokeAsync(() => this.viewModelMock.As<IEnumerationParameterTypeEditorViewModel>().Object.OnSelectAllChanged(true));
        }

        [Test]
        public async Task VerifyParameterValueChanged()
        {
            var comboBox = this.renderedComponent.FindComponent<DxComboBox<string, string>>();
            Assert.That(comboBox, Is.Not.Null);
            await this.renderedComponent.InvokeAsync(() => this.viewModelMock.Object.OnParameterValueChanged("value"));
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
