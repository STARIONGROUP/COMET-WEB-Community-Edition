// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EnumerationParameterTypeEditorViewModelTestFixture.cs" company="RHEA System S.A.">
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
    public class EnumerationParameterTypeEditorViewModelTestFixture
    {
        private EnumerationParameterTypeEditorViewModel viewModel;
        private EnumerationParameterType parameterType;

        [SetUp]
        public void SetUp()
        {
            var enumerationValues = new List<string> { "cube", "sphere", "cylinder" };

            var parameterValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(enumerationValues)
            };

            var enumerationData = new OrderedItemList<EnumerationValueDefinition>(null)
            {
                new EnumerationValueDefinition
                {
                    Iid = Guid.NewGuid(),
                    Name = enumerationValues[0]
                },
                new EnumerationValueDefinition
                {
                    Iid = Guid.NewGuid(),
                    Name = enumerationValues[1]
                },
                new EnumerationValueDefinition
                {
                    Iid = Guid.NewGuid(),
                    Name = enumerationValues[2]
                }
            };

            this.parameterType = new EnumerationParameterType
            {
                Iid = Guid.NewGuid(),
                AllowMultiSelect = true
            };

            this.parameterType.ValueDefinition.AddRange(enumerationData);

            var sessionService = new Mock<ISessionService>();
            var session = new Mock<ISession>();
            var permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            session.Setup(x => x.PermissionService).Returns(permissionService.Object);
            sessionService.Setup(x => x.Session).Returns(session.Object);

            this.viewModel = new EnumerationParameterTypeEditorViewModel(this.parameterType, parameterValueSet, false);
        }

        [Test]
        public void VerifyCancelSelection()
        {
            this.viewModel.OnCancelButtonClick();

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedEnumerationValueDefinitions, Is.Not.Null);
                Assert.That(this.viewModel.SelectedEnumerationValueDefinitions.ToList(), Has.Count.EqualTo(0));
                Assert.That(this.viewModel.IsOnEditMode, Is.False);
            });
        }

        [Test]
        public void VerifyComponent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedEnumerationValueDefinitions, Is.Not.Null);
                Assert.That(this.viewModel.SelectedEnumerationValueDefinitions.ToList(), Has.Count.EqualTo(0));
                Assert.That(this.viewModel.EnumerationValueDefinitions, Is.Not.Null);
                Assert.That(this.viewModel.EnumerationValueDefinitions.ToList(), Has.Count.EqualTo(3));
                Assert.That(this.viewModel.IsOnEditMode, Is.False);
                Assert.That(this.viewModel.SelectAllChecked, Is.False);
            });
        }

        [Test]
        public void VerifyConfirmSelection()
        {
            this.viewModel.OnSelectAllChanged(true);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedEnumerationValueDefinitions, Is.Not.Null);
                Assert.That(this.viewModel.SelectedEnumerationValueDefinitions.ToList(), Has.Count.EqualTo(3));
                Assert.That(this.viewModel.IsOnEditMode, Is.False);
                Assert.That(() => this.viewModel.OnConfirmButtonClick(), Throws.Nothing);
            });
        }

        [Test]
        public void VerifySelectAll()
        {
            this.viewModel.OnSelectAllChanged(true);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedEnumerationValueDefinitions, Is.Not.Null);
                Assert.That(this.viewModel.SelectedEnumerationValueDefinitions.ToList(), Has.Count.EqualTo(3));
                Assert.That(this.viewModel.SelectAllChecked, Is.True);
            });

            this.viewModel.OnSelectAllChanged(false);

            Assert.That(this.viewModel.SelectedEnumerationValueDefinitions.ToList(), Has.Count.EqualTo(0));
        }
    }
}
