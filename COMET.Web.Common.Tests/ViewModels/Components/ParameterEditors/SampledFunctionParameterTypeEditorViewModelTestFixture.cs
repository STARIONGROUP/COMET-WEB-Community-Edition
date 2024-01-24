// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SampledFunctionParameterTypeEditorViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Tests.ViewModels.Components.ParameterEditors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using NUnit.Framework;

    public class SampledFunctionParameterTypeEditorViewModelTestFixture
    {
        private SampledFunctionParameterTypeEditorViewModel viewModel;
        private ICDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            var textParameterType = new TextParameterType();
            var booleanParameterType = new BooleanParameterType();
            var measurementScale = new CyclicRatioScale();
            var quantityKindParameterType = new SimpleQuantityKind() { DefaultScale = measurementScale};

            var sampledFunctionParameterType = new SampledFunctionParameterType();

            sampledFunctionParameterType.IndependentParameterType.Add(new IndependentParameterTypeAssignment()
            {
                ParameterType = quantityKindParameterType,
                MeasurementScale = measurementScale
            });

            sampledFunctionParameterType.DependentParameterType.Add(new DependentParameterTypeAssignment()
            {
                ParameterType = booleanParameterType
            });

            sampledFunctionParameterType.DependentParameterType.Add(new DependentParameterTypeAssignment()
            {
                ParameterType = textParameterType
            });

            var valueSet = new ParameterValueSet()
            {
                Manual = new ValueArray<string>(new[]{"-","-","-"}),
                Reference = new ValueArray<string>(new[] { "-", "-", "-" }),
                Formula = new ValueArray<string>(new[] { "-", "-", "-" }),
                Computed = new ValueArray<string>(new[] { "-", "-", "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            this.messageBus = new CDPMessageBus();
            this.viewModel = new SampledFunctionParameterTypeEditorViewModel(sampledFunctionParameterType, valueSet,false, this.messageBus);
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyRowsUpdate()
        {
            Assert.That(this.viewModel.ValueArray, Has.Count.EqualTo(3));
            this.viewModel.AddRow();
            Assert.That(this.viewModel.ValueArray, Has.Count.EqualTo(6));
            this.viewModel.RemoveRow();
            Assert.That(this.viewModel.ValueArray, Has.Count.EqualTo(3));
            this.viewModel.RemoveRow();
            Assert.That(this.viewModel.ValueArray, Has.Count.EqualTo(3));

            this.viewModel.UpdateParameterSwitchKind(ParameterSwitchKind.COMPUTED);

            Assert.That(this.viewModel.ValueArray, Has.Count.EqualTo(3));
            this.viewModel.AddRow();
            Assert.That(this.viewModel.ValueArray, Has.Count.EqualTo(3));
        }

        [Test]
        public async Task VerifyCreateContainedViewModel()
        {
            var selectors = new List<IParameterTypeEditorSelectorViewModel>();

            for (var numberOfValuesIndex = 0; numberOfValuesIndex < this.viewModel.ParameterType.NumberOfValues; numberOfValuesIndex++)
            {
                selectors.Add(this.viewModel.CreateParameterTypeEditorSelectorViewModel(numberOfValuesIndex));
            }

            Assert.That(selectors[0].Scale, Is.Not.Null);

            var quantityKindEditorViewModel = selectors[0].CreateParameterEditorViewModel<QuantityKind>();
            await quantityKindEditorViewModel.OnParameterValueChanged("5");
            Assert.That(this.viewModel.ValueArray[0], Is.EqualTo("5"));
            this.viewModel.ResetChanges();
            Assert.That(this.viewModel.ValueArray[0], Is.EqualTo("-"));
        }

        [Test]
        public void VerifyOnParameterValueChanged()
        {
            Assert.That(async () => await this.viewModel.OnParameterValueChanged(null), Throws.Nothing);
        }
    }
}
