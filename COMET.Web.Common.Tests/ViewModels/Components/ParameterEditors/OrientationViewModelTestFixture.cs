// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OrientationViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Tests.ViewModels.Components.ParameterEditors
{
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using Microsoft.AspNetCore.Components;

    using NUnit.Framework;

    [TestFixture]
    public class OrientationViewModelTestFixture
    {
        private OrientationViewModel viewModel;
        private EventCallback<(IValueSet,int)> onParameterValueSetChanged;

        [SetUp]
        public void SetUp()
        {
            var valueSet = new ParameterValueSet()
            {
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(new List<string>() { "1", "0", "0", "0", "1", "0", "0", "0", "1" })
            };

            this.onParameterValueSetChanged = new EventCallback<(IValueSet, int)>();
            this.viewModel = new OrientationViewModel(valueSet,this.onParameterValueSetChanged);
        }

        [Test]
        public void VerifyViewModel()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Orientation, Is.Not.Null);
                Assert.That(this.viewModel.AngleFormat, Is.EqualTo(AngleFormat.Degrees));
                Assert.That(this.viewModel.CurrentValueSet, Is.Not.Null);
                Assert.That(this.viewModel.ParameterValueChanged, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyOnEulerAnglesChanged()
        {
            var previousMatrix = Orientation.Identity().Matrix;
            this.viewModel.OnEulerAnglesChanged("Rx", "1.0");
            this.viewModel.OnEulerAnglesChanged("Ry", "0.5");
            this.viewModel.OnEulerAnglesChanged("Rz", "1.5");

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Orientation.X, Is.Not.EqualTo(0));
                Assert.That(this.viewModel.Orientation.Y, Is.Not.EqualTo(0));
                Assert.That(this.viewModel.Orientation.Z, Is.Not.EqualTo(0));
                Assert.That(this.viewModel.Orientation.Matrix, Is.Not.EquivalentTo(previousMatrix));
            });
        }

        [Test]
        public void VerifyOnMatrixValuesChanged()
        {
            var previousMatrix = Orientation.Identity().Matrix;
            this.viewModel.OnMatrixValuesChanged(0, "1.0");
            this.viewModel.OnMatrixValuesChanged(3, "0.5");
            this.viewModel.OnMatrixValuesChanged(6, "1.5");

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Orientation.X, Is.Not.EqualTo(0));
                Assert.That(this.viewModel.Orientation.Y, Is.Not.EqualTo(0));
                Assert.That(this.viewModel.Orientation.Z, Is.Not.EqualTo(0));
                Assert.That(this.viewModel.Orientation.Matrix, Is.Not.EquivalentTo(previousMatrix));
            });
        }

        [Test]
        public void VerifyOnAngleFormatChanged()
        {
            var previousMatrix = Orientation.Identity().Matrix;
            this.viewModel.OnEulerAnglesChanged("Rx", "10");
            this.viewModel.OnEulerAnglesChanged("Ry", "5");
            this.viewModel.OnEulerAnglesChanged("Rz", "15");
            this.viewModel.OnAngleFormatChanged(AngleFormat.Radians);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.Orientation.X, Is.Not.EqualTo(0));
                Assert.That(this.viewModel.Orientation.Y, Is.Not.EqualTo(0));
                Assert.That(this.viewModel.Orientation.Z, Is.Not.EqualTo(0));
                Assert.That(this.viewModel.Orientation.Matrix, Is.Not.EquivalentTo(previousMatrix));
            });
        }
    }
}
