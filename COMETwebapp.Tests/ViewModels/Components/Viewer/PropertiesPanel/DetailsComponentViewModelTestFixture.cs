// -------------------------------------------------------------------------------------------------------------------- 
// <copyright file="DetailsComponentViewModelTestFixture.cs" company="RHEA System S.A."> 
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
    using System;
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMETwebapp.ViewModels.Components.Viewer.PropertiesPanel;

    using Microsoft.AspNetCore.Components;

    using NUnit.Framework;

    [TestFixture]
    public class DetailsComponentViewModelTestFixture
    {
        private IDetailsComponentViewModel viewModel;
        private EventCallback<(IValueSet, int)> eventCallback;
        private ParameterType parameterType;
        private ParameterValueSet valueSet;

        [SetUp]
        public void SetUp()
        {
            this.eventCallback = new EventCallback<(IValueSet, int)>();

            this.parameterType = new TextParameterType()
            {
                Iid = Guid.NewGuid(),
            };

            this.valueSet = new ParameterValueSet()
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(new List<string>() { "1", "2", "3" })
            };

            this.viewModel = new DetailsComponentViewModel(true, this.parameterType, this.valueSet, this.eventCallback);
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.IsVisible, Is.True);
                Assert.That(this.viewModel.ParameterType, Is.Not.Null);
            });
        }
    }
}
