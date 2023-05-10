// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="QuantityKindParameterTypeEditorViewModelTestFixture.cs" company="RHEA System S.A.">
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

    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using NUnit.Framework;

    [TestFixture]
    public class QuantityKindParameterTypeEditorViewModelTestFixture
    {
        private QuantityKindParameterTypeEditorViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            var parameterValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(new List<string> { "0" })
            };

            var textParameterType = new SimpleQuantityKind
            {
                Iid = Guid.NewGuid()
            };

            this.viewModel = new QuantityKindParameterTypeEditorViewModel(textParameterType, parameterValueSet, false, ParameterSwitchKind.MANUAL);
        }

        [Test]
        public void VerifyThatSwitchEventChangeData()
        {
            Assert.That(this.viewModel.CurrentParameterSwitchKind, Is.EqualTo(ParameterSwitchKind.MANUAL));
            this.viewModel.UpdateParameterSwitchKind(ParameterSwitchKind.COMPUTED);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CurrentParameterSwitchKind, Is.EqualTo(ParameterSwitchKind.COMPUTED));
                Assert.That(this.viewModel.IsReadOnly, Is.True);
            });

            this.viewModel.UpdateParameterSwitchKind(ParameterSwitchKind.REFERENCE);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CurrentParameterSwitchKind, Is.EqualTo(ParameterSwitchKind.REFERENCE));
                Assert.That(this.viewModel.IsReadOnly, Is.False);
            });
        }

        [Test]
        public void VerifyViewModel()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ParameterType, Is.Not.Null);
                Assert.That(this.viewModel.ValueSet, Is.Not.Null);
                Assert.That(this.viewModel.IsReadOnly, Is.False);
                Assert.That(this.viewModel.ParameterValueChanged, Is.Not.Null);
            });
        }
    }
}
