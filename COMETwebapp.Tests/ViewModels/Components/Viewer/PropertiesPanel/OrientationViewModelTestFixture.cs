// -------------------------------------------------------------------------------------------------------------------- 
// <copyright file="OrientationViewModelTestFixture.cs" company="RHEA System S.A."> 
//    Copyright (c) 2023 RHEA System S.A. 
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar 
// 
//    This file is part of COMET WEB Community Edition 
//    The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C. 
// 
//    The COMET WEB Community Edition is free software; you can redistribute it and/or 
//    modify it under the terms of the GNU Affero General Public 
//    License as published by the Free Software Foundation; either 
//    version 3 of the License, or (at your option) any later version. 
// 
//    The COMET WEB Community Edition is distributed in the hope that it will be useful, 
//    but WITHOUT ANY WARRANTY; without even the implied warranty of 
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU 
//    Affero General Public License for more details. 
// 
//    You should have received a copy of the GNU Affero General Public License 
//    along with this program.  If not, see <http://www.gnu.org/licenses/>. 
// </copyright> 
// -------------------------------------------------------------------------------------------------------------------- 

namespace COMETwebapp.Tests.ViewModels.Components.Viewer.PropertiesPanel
{
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using COMETwebapp.Enumerations;
    using COMETwebapp.Model;
    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;
    
    using Microsoft.AspNetCore.Components;
    
    using NUnit.Framework;
    
    [TestFixture]
    public class OrientationViewModelTestFixture
    {
        private IOrientationViewModel viewModel;
        private EventCallback<IValueSet> onParameterValueSetChanged;

        [SetUp]
        public void SetUp()
        {
            var valueSet = new ParameterValueSet()
            {
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(new List<string>() { "1", "0", "0", "0", "1", "0", "0", "0", "1" })
            };

            this.onParameterValueSetChanged = new EventCallback<IValueSet>();
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
