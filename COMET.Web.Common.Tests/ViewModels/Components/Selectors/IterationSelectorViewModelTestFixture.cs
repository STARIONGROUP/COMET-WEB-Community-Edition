// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IterationSelectorViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace COMET.Web.Common.Tests.ViewModels.Components.Selectors
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using Microsoft.AspNetCore.Components;

    using NUnit.Framework;

    [TestFixture]
    public class IterationSelectorViewModelTestFixture
    {
        private IterationSelectorViewModel viewModel;
        private Iteration submittedIteration;

        [SetUp]
        public void Setup()
        {
            this.viewModel = new IterationSelectorViewModel()
            {
                OnSubmit = new EventCallbackFactory().Create<Iteration>(this, x => this.submittedIteration = x)
            };
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableIterations, Is.Null);
                Assert.That(this.viewModel.SelectedIteration, Is.Null);
                Assert.That(this.viewModel.OnSubmit.HasDelegate, Is.True);
            });
        }

        [Test]
        public async Task VerifyUpdateProperties()
        {
            var engineeringModelSetup = new EngineeringModelSetup()
            {
                Iid = Guid.NewGuid(),
                Name = "LOFT"
            };

            var iteration = new Iteration()
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup()
                {
                    Iid = Guid.NewGuid(),
                    IterationNumber = 1,
                    FrozenOn = DateTime.Now,
                    Container = engineeringModelSetup
                }
            };

            var iteration2 = new Iteration()
            {
                Iid = Guid.NewGuid(),
                IterationSetup = new IterationSetup()
                {
                    Iid = Guid.NewGuid(),
                    IterationNumber = 2,
                    Container = engineeringModelSetup
                }
            };

            var iterations = new List<Iteration> { iteration, iteration2 };
            this.viewModel.UpdateProperties(iterations);
            Assert.That(this.viewModel.AvailableIterations.Count(), Is.EqualTo(iterations.Count));
            this.viewModel.SelectedIteration = this.viewModel.AvailableIterations.First();
            Assert.That(this.viewModel.SelectedIteration, Is.Not.Null);
            await this.viewModel.Submit();
            Assert.That(this.submittedIteration, Is.EqualTo(iteration));
            this.viewModel.UpdateProperties(iterations);
            Assert.That(this.viewModel.SelectedIteration, Is.Null);
        }
    }
}
