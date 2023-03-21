// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EnumerationParameterTypeEditorViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using System.Linq;
    using System.Threading.Tasks;

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
    public class EnumerationParameterTypeEditorViewModelTestFixture
    {
        private IEnumerationParameterTypeEditorViewModel viewModel;
        private EnumerationParameterType parameterType;

        [SetUp]
        public void SetUp()
        {
            var enumerationValues = new List<string> { "cube", "sphere", "cylinder" };

            var parameterValueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(enumerationValues),
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

            var subscriptionService = new Mock<ISubscriptionService>();

            this.viewModel = new EnumerationParameterTypeEditorViewModel(this.parameterType ,parameterValueSet, false);
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
        public async Task VerifySelectAll()
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

        [Test]
        public async Task VerifyCancelSelection()
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
        public async Task VerifyConfirmSelection()
        {
            this.viewModel.OnSelectAllChanged(true);

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.SelectedEnumerationValueDefinitions, Is.Not.Null);
                Assert.That(this.viewModel.SelectedEnumerationValueDefinitions.ToList(), Has.Count.EqualTo(3));
                Assert.That(this.viewModel.IsOnEditMode, Is.False);
                Assert.That(async () => this.viewModel.OnConfirmButtonClick(), Throws.Nothing);
            });
        }
    }
}
