// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CompoundParameterTypeEditorViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Services.SubscriptionService;
    using COMETwebapp.ViewModels.Components.Shared.ParameterEditors;

    using Moq;

    using NUnit.Framework;
    
    [TestFixture]
    public class CompoundParameterTypeEditorViewModelTestFixture
    {
        private ICompoundParameterTypeEditorViewModel viewModel;
        private CompoundParameterType parameterType;

        [SetUp]
        public void SetUp()
        {
            var compoundValues = new List<string> { "1", "0", "3" };

            var parameterValueSet = new ParameterValueSet()
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(compoundValues),
            };

            var compoundData = new OrderedItemList<ParameterTypeComponent>(null)
            {
                new ParameterTypeComponent
                {
                    Iid = Guid.NewGuid(),
                    ShortName = "firstValue",
                    Scale = new OrdinalScale()
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    },
                    ParameterType = new SimpleQuantityKind()
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    }
                },
                new ParameterTypeComponent
                {
                    Iid = Guid.NewGuid(),
                    ShortName = "secondValue",
                    Scale = new OrdinalScale()
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    },
                    ParameterType = new SimpleQuantityKind()
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    }
                },
                new ParameterTypeComponent
                {
                    Iid = Guid.NewGuid(),
                    ShortName = "thirdValue",
                    Scale = new OrdinalScale()
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    },
                    ParameterType = new SimpleQuantityKind()
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    }
                }
            };

            this.parameterType = new CompoundParameterType()
            {
                Iid = Guid.NewGuid(),
            };

            parameterType.Component.AddRange(compoundData);

            var sessionService = new Mock<ISessionService>();
            var session = new Mock<ISession>();
            var permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            session.Setup(x => x.PermissionService).Returns(permissionService.Object);
            sessionService.Setup(x => x.Session).Returns(session.Object);

            var subscriptionService = new Mock<ISubscriptionService>();

            this.viewModel = new CompoundParameterTypeEditorViewModel(parameterType, parameterValueSet, false);
        }

        [Test]
        public void VerifyCreateOrientationViewModel()
        {
            var orientationViewModel = this.viewModel.CreateOrientationViewModel();

            Assert.Multiple(() =>
            {
                Assert.That(orientationViewModel, Is.Not.Null);
                Assert.That(orientationViewModel.CurrentValueSet, Is.Not.Null);
            });
        }

        [Test]
        public void VerifyOnComponentSelected()
        {
            this.viewModel.OnComponentSelected();

            Assert.That(this.viewModel.IsOnEditMode, Is.True);
        }

        [Test]
        public void VerifyCreateParameterTypeEditorSelectorViewModel()
        {
            var parameterTypeEditorSelectorViewModel = this.viewModel.CreateParameterTypeEditorSelectorViewModel(this.parameterType, 0);

            Assert.Multiple(() =>
            {
                Assert.That(parameterTypeEditorSelectorViewModel, Is.Not.Null);
                Assert.That(parameterTypeEditorSelectorViewModel.ParameterType, Is.Not.Null);
                Assert.That(parameterTypeEditorSelectorViewModel.ParameterType, Is.EqualTo(this.parameterType));
            });
        }
    }
}
