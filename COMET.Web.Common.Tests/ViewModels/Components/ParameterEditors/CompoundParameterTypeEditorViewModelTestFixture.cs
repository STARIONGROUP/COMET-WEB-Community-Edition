// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CompoundParameterTypeEditorViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.ParameterEditors;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class CompoundParameterTypeEditorViewModelTestFixture
    {
        private ICompoundParameterTypeEditorViewModel viewModel;
        private CompoundParameterType parameterType;
        private ICDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            var compoundValues = new List<string> { "1", "0", "3" };

            var parameterValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(compoundValues)
            };

            var compoundData = new OrderedItemList<ParameterTypeComponent>(null)
            {
                new ParameterTypeComponent
                {
                    Iid = Guid.NewGuid(),
                    ShortName = "firstValue",
                    Scale = new OrdinalScale
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    },
                    ParameterType = new SimpleQuantityKind
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    }
                },
                new ParameterTypeComponent
                {
                    Iid = Guid.NewGuid(),
                    ShortName = "secondValue",
                    Scale = new OrdinalScale
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    },
                    ParameterType = new SimpleQuantityKind
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    }
                },
                new ParameterTypeComponent
                {
                    Iid = Guid.NewGuid(),
                    ShortName = "thirdValue",
                    Scale = new OrdinalScale
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    },
                    ParameterType = new SimpleQuantityKind
                    {
                        Iid = Guid.NewGuid(),
                        ShortName = "m"
                    }
                }
            };

            this.parameterType = new CompoundParameterType
            {
                Iid = Guid.NewGuid()
            };

            this.parameterType.Component.AddRange(compoundData);

            var sessionService = new Mock<ISessionService>();
            var session = new Mock<ISession>();
            var permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            session.Setup(x => x.PermissionService).Returns(permissionService.Object);
            sessionService.Setup(x => x.Session).Returns(session.Object);

            this.messageBus = new CDPMessageBus();
            this.viewModel = new CompoundParameterTypeEditorViewModel(this.parameterType, parameterValueSet, false, this.messageBus);
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
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
        public void VerifyCreateParameterTypeEditorSelectorViewModel()
        {
            var parameterTypeEditorSelectorViewModel = this.viewModel.CreateParameterTypeEditorSelectorViewModel(this.parameterType, 0, null, default);

            Assert.Multiple(() =>
            {
                Assert.That(parameterTypeEditorSelectorViewModel, Is.Not.Null);
                Assert.That(parameterTypeEditorSelectorViewModel.ParameterType, Is.Not.Null);
                Assert.That(parameterTypeEditorSelectorViewModel.ParameterType, Is.EqualTo(this.parameterType));
            });
        }

        [Test]
        public void VerifyOnComponentSelected()
        {
            Assert.That(() => this.viewModel.OnComponentSelected(), Throws.Nothing);
        }
    }
}
