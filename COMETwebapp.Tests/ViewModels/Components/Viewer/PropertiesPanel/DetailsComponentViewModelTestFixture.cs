// -------------------------------------------------------------------------------------------------------------------- 
// <copyright file="DetailsComponentViewModelTestFixture.cs" company="RHEA System S.A."> 
//    Copyright (c) 2023 RHEA System S.A. 
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar 
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
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

    using Microsoft.AspNetCore.Components;

    using NUnit.Framework;

    [TestFixture]
    public class DetailsComponentViewModelTestFixture
    {
        private IDetailsComponentViewModel viewModel;
        private EventCallback<Dictionary<ParameterBase, IValueSet>> eventCallback;
        private ParameterBase selectedParameter;
        private Dictionary<ParameterBase, IValueSet> parameterValueSetRelations;

        [SetUp]
        public void SetUp()
        {
            this.eventCallback = new EventCallback<Dictionary<ParameterBase, IValueSet>>();
            this.selectedParameter = new Parameter();
            this.parameterValueSetRelations = new Dictionary<ParameterBase, IValueSet>();

            this.viewModel = new DetailsComponentViewModel(true, this.selectedParameter, this.parameterValueSetRelations, this.eventCallback);
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsVisible, Is.True);
                Assert.That(this.viewModel.SelectedParameter, Is.Not.Null);
                Assert.That(this.viewModel.OnParameterValueChanged, Is.Not.Null);
                Assert.That(this.viewModel.CurrentValueSet, Is.Null);
            });
        }

        [Test]
        public void VerifyOnParameterValueChange()
        {
            this.viewModel.CurrentValueSet = new ParameterValueSet()
            {
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(new List<string> { "1", "1" })
            };
            
            Dictionary<ParameterBase, IValueSet> result = null;
            
            this.viewModel.OnParameterValueChanged = new EventCallbackFactory().Create(this, (Dictionary<ParameterBase, IValueSet> parameterValueSetRelations) =>
            {
                result = parameterValueSetRelations;
            });

            this.viewModel.OnParameterValueChange(0, "2.1");

            Assert.That(result, Is.Not.Null);
        }
    }
}
