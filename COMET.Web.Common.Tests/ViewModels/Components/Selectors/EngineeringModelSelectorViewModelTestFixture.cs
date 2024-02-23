// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EngineeringModelSelectorViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMET.Web.Common.Tests.ViewModels.Components.Selectors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    using NUnit.Framework;

    [TestFixture]
    public class EngineeringModelSelectorViewModelTestFixture
    {
        private EngineeringModelSelectorViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            this.viewModel = new EngineeringModelSelectorViewModel();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableEngineeringModelSetups, Is.Null);
                Assert.That(this.viewModel.SelectedEngineeringModelSetup, Is.Null);
                Assert.That(this.viewModel.OnSubmit.HasDelegate, Is.False);
            });

            this.viewModel.UpdateProperties(new List<EngineeringModel> { new() { EngineeringModelSetup = new EngineeringModelSetup() } });

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableEngineeringModelSetups, Is.Not.Empty);
                Assert.That(this.viewModel.SelectedEngineeringModelSetup, Is.Null);
            });

            this.viewModel.SelectedEngineeringModelSetup = this.viewModel.AvailableEngineeringModelSetups.First();
            Assert.That(this.viewModel.SelectedEngineeringModelSetup, Is.Not.Null);
        }

        [Test]
        public async Task VerifySubmit()
        {
            EngineeringModel engineeringModel = default;
            this.viewModel.OnSubmit = new EventCallbackFactory().Create<EngineeringModel>(this, x => { engineeringModel = x; });

            Assert.That( () => this.viewModel.Submit(), Throws.Exception);
            var engineeringModelId = Guid.NewGuid();

            this.viewModel.UpdateProperties(new List<EngineeringModel>
            {
                new()
                {
                    Iid = engineeringModelId, EngineeringModelSetup = new EngineeringModelSetup
                    {
                        EngineeringModelIid = engineeringModelId
                    }
                }
            });

            this.viewModel.SelectedEngineeringModelSetup = this.viewModel.AvailableEngineeringModelSetups.First();
            await this.viewModel.Submit();
            Assert.That(engineeringModel.Iid, Is.EqualTo(this.viewModel.SelectedEngineeringModelSetup.EngineeringModelIid));
        }
    }
}
