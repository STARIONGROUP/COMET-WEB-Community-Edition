// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SampledFunctionParameterTypeEditorTestFixture.cs" company="RHEA System S.A.">
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

    using COMET.Web.Common.Components.ParameterTypeEditors;
    using COMET.Web.Common.Test.Helpers;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    using Moq;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class SampledFunctionParameterTypeEditorTestFixture
    {
        private Mock<IParameterEditorBaseViewModel<SampledFunctionParameterType>> viewModel;
        private TestContext context;

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            var sfpt = new SampledFunctionParameterType();

            sfpt.IndependentParameterType.Add(new IndependentParameterTypeAssignment()
            {
                ParameterType = new SimpleQuantityKind()
                {
                    Name = "mass"
                },
                MeasurementScale = new RatioScale()
                {
                    Name = "kg"
                }
            });

            sfpt.DependentParameterType.Add(new DependentParameterTypeAssignment()
            {
                ParameterType = new BooleanParameterType()
                {
                    Name = "bool"
                }
            });

            var valueSet = new ParameterValueSet()
            {
                Manual = new ValueArray<string>(new[] { "-", "-" }),
                Computed = new ValueArray<string>(new[] { "-", "-" }),
                Reference = new ValueArray<string>(new[] { "-", "-" }),
                Formula = new ValueArray<string>(new[] { "-", "-" }),
            };

            this.viewModel = new Mock<IParameterEditorBaseViewModel<SampledFunctionParameterType>>();
            this.viewModel.As<ISampledFunctionParameterTypeEditorViewModel>();
            this.viewModel.Setup(x => x.ParameterType).Returns(sfpt);
            this.viewModel.Setup(x => x.ValueSet).Returns(valueSet);
            this.viewModel.Setup(x => x.ValueArray).Returns(valueSet.Manual);

            var quantityKind = new Mock<IParameterEditorBaseViewModel<QuantityKind>>();
            quantityKind.Setup(x => x.ValueArray).Returns(valueSet.Manual);
            quantityKind.Setup(x => x.ValueArrayIndex).Returns(0);

            var firstSelector = new Mock<IParameterTypeEditorSelectorViewModel>();
            firstSelector.Setup(x => x.ParameterType).Returns(sfpt.IndependentParameterType[0].ParameterType);
            firstSelector.Setup(x => x.Scale).Returns(sfpt.IndependentParameterType[0].MeasurementScale);
            firstSelector.Setup(x => x.CreateParameterEditorViewModel<QuantityKind>()).Returns(quantityKind.Object);

            var booleanViewModel = new Mock<IParameterEditorBaseViewModel<BooleanParameterType>>();
            booleanViewModel.Setup(x => x.ValueArray).Returns(valueSet.Manual);
            booleanViewModel.Setup(x => x.ValueArrayIndex).Returns(1);

            var secondSelector = new Mock<IParameterTypeEditorSelectorViewModel>();
            secondSelector.Setup(x => x.ParameterType).Returns(sfpt.DependentParameterType[0].ParameterType);
            secondSelector.Setup(x => x.Scale).Returns(sfpt.DependentParameterType[0].MeasurementScale);
            secondSelector.Setup(x => x.CreateParameterEditorViewModel<BooleanParameterType>()).Returns(booleanViewModel.Object);

            this.viewModel.As<ISampledFunctionParameterTypeEditorViewModel>().Setup(x => x.CreateParameterTypeEditorSelectorViewModel(0))
                .Returns(firstSelector.Object);

            this.viewModel.As<ISampledFunctionParameterTypeEditorViewModel>().Setup(x => x.CreateParameterTypeEditorSelectorViewModel(1))
                .Returns(secondSelector.Object);

            this.viewModel.As<ISampledFunctionParameterTypeEditorViewModel>().Setup(x => x.ParameterTypeAssignments).Returns(sfpt.IndependentParameterType
                .Union(sfpt.DependentParameterType.OfType<IParameterTypeAssignment>()).ToList());
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyNonEditMode()
        {
            var renderer = this.context.RenderComponent<SampledFunctionParameterTypeEditor>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
            });

            var button = renderer.FindComponent<DxButton>();
            await renderer.InvokeAsync(button.Instance.Click.InvokeAsync);
            this.viewModel.As<ISampledFunctionParameterTypeEditorViewModel>().Verify(x=> x.OnComponentSelected(), Times.Once);
        }

        [Test]
        public async Task VerifyEditMode()
        {
            var callbackCount = 0;
            var callback = new EventCallbackFactory().Create(this, () => callbackCount++);
            this.viewModel.Setup(x => x.IsReadOnly).Returns(true);

            var renderer = this.context.RenderComponent<SampledFunctionParameterTypeEditor>(parameters =>
            {
                parameters.Add(p => p.ViewModel, this.viewModel.Object);
                parameters.Add(p => p.IsOnEditMode, true);
                parameters.Add(p => p.OnAfterCancel, callback);
                parameters.Add(p => p.OnAfterEdit, callback);
            });

            var buttons = renderer.FindComponents<DxButton>();
            Assert.That(buttons, Has.Count.EqualTo(2));
            this.viewModel.Setup(x => x.IsReadOnly).Returns(false);

            renderer.Render();
            buttons = renderer.FindComponents<DxButton>();
            Assert.That(buttons, Has.Count.EqualTo(4));

            var addRowButton = buttons[0];
            await renderer.InvokeAsync(addRowButton.Instance.Click.InvokeAsync);
            this.viewModel.As<ISampledFunctionParameterTypeEditorViewModel>().Verify(x => x.AddRow(), Times.Once);
            var removeRowButton = buttons[1];
            Assert.That(removeRowButton.Instance.Enabled, Is.False);

            this.viewModel.As<ISampledFunctionParameterTypeEditorViewModel>().Setup(x => x.CanRemoveRow).Returns(true);
            renderer.Render();
            buttons = renderer.FindComponents<DxButton>();
            removeRowButton = buttons[1];
            Assert.That(removeRowButton.Instance.Enabled, Is.True);
            await renderer.InvokeAsync(removeRowButton.Instance.Click.InvokeAsync);
            this.viewModel.As<ISampledFunctionParameterTypeEditorViewModel>().Verify(x => x.RemoveRow(), Times.Once);

            await renderer.InvokeAsync(buttons[2].Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                this.viewModel.Verify(x => x.OnParameterValueChanged(null), Times.Once);
                Assert.That(callbackCount, Is.EqualTo(1));
            });

            await renderer.InvokeAsync(buttons[3].Instance.Click.InvokeAsync);
            Assert.That(callbackCount, Is.EqualTo(2));
        }
    }
}
