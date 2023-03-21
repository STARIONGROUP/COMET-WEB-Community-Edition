// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EnumerationParameterTypeEditorTestFixture.cs" company="RHEA System S.A.">
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
                Manual = new ValueArray<string>(enumerationValues),
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

            this.eventCallback = new EventCallbackFactory().Create(this, (IValueSet _) =>
            {
                this.eventCallbackCalled = true;
            });

            this.viewModelMock.Setup(x => x.OnParameterValueChanged(It.IsAny<object>()))
                .Callback(() => this.eventCallback.InvokeAsync());

            this.renderedComponent = this.context.RenderComponent<EnumerationParameterTypeEditor>(parameters =>
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
            var comboBox = this.renderedComponent.FindComponent<DxComboBox<string,string>>();
            Assert.That(comboBox, Is.Not.Null);
            await this.renderedComponent.InvokeAsync(() => this.viewModelMock.Object.OnParameterValueChanged("value"));
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

            var checkBox = renderedComponent.FindComponent<DxCheckBox<bool>>();
            Assert.That(checkBox, Is.Not.Null);

            checkBox.Instance.Checked = true;

            Assert.That(this.viewModelMock.As<IEnumerationParameterTypeEditorViewModel>().Object.SelectedEnumerationValueDefinitions, Has.Count.EqualTo(3));
        }
    }
}
