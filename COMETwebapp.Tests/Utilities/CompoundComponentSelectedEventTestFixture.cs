// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompoundComponentSelectedEventTestFixture.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Tests.Utilities
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using COMETwebapp.Components.Viewer.Canvas;
    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.Shared.ParameterEditors;
    using Microsoft.AspNetCore.Components;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Numerics;

    [TestFixture]
    public class CompoundComponentSelectedEventTestFixture
    {
        private ICompoundParameterTypeEditorViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            var parameterType = new CompoundParameterType()
            {
                Iid = Guid.NewGuid(),
            };
            
            var compoundValues = new List<string> { "1", "0", "3" };

            var parameterValueSet = new ParameterValueSet()
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(compoundValues),
            };


            this.viewModel = new CompoundParameterTypeEditorViewModel(parameterType, parameterValueSet, false);
        }

        [Test]
        public void VerifyCompoundComponentSelectedEventCreation()
        {
            var compoundComponentSelectedEvent = new CompoundComponentSelectedEvent((CompoundParameterTypeEditorViewModel)this.viewModel);

            Assert.Multiple(() =>
            {
                Assert.That(compoundComponentSelectedEvent.CompoundParameterTypeEditorViewModel, Is.Not.Null);
                Assert.That(compoundComponentSelectedEvent.CompoundParameterTypeEditorViewModel, Is.EqualTo(this.viewModel));
                Assert.That(compoundComponentSelectedEvent.CompoundParameterTypeEditorViewModel.ParameterType, Is.Not.Null);
            });
        }
    }
}
