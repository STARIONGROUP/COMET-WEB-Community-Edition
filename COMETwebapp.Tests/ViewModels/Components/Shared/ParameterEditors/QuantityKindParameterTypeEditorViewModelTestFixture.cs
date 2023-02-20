// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="QuantityKindParameterTypeEditorViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.ViewModels.Components.Shared.ParameterEditors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;
    
    using COMETwebapp.ViewModels.Components.Shared.ParameterEditors;
    
    using NUnit.Framework;
    
    using System.Collections.Generic;
    using System;
    
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using COMETwebapp.Model;
    
    [TestFixture]
    public class QuantityKindParameterTypeEditorViewModelTestFixture
    {
        private QuantityKindParameterTypeEditorViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            var parameterValueSet = new ParameterValueSet()
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(new List<string>() { "0" }),
            };

            var textParameterType = new SimpleQuantityKind()
            {
                Iid = Guid.NewGuid(),
            };

            this.viewModel = new QuantityKindParameterTypeEditorViewModel(textParameterType, parameterValueSet);
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

        [Test]
        public void VerifyThatSwitchEventChangeData()
        {
            Assert.That(this.viewModel.ValueSet.ValueSwitch, Is.EqualTo(ParameterSwitchKind.MANUAL));
            CDPMessageBus.Current.SendMessage(new SwitchEvent(Guid.NewGuid(), ParameterSwitchKind.COMPUTED, false));
            
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ValueSet.ValueSwitch, Is.EqualTo(ParameterSwitchKind.COMPUTED));
                Assert.That(this.viewModel.IsReadOnly, Is.False);
            });

            CDPMessageBus.Current.SendMessage(new SwitchEvent(Guid.NewGuid(), ParameterSwitchKind.REFERENCE, false));

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ValueSet.ValueSwitch, Is.EqualTo(ParameterSwitchKind.REFERENCE));
                Assert.That(this.viewModel.IsReadOnly, Is.True);
            });
        }
    }
}
