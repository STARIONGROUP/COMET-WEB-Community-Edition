﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementBaseSelectorViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
// 
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25
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

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using NUnit.Framework;

    [TestFixture]
    public class ElementBaseSelectorViewModelTestFixture
    {
        private ElementBaseSelectorViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            this.viewModel = new ElementBaseSelectorViewModel();
        }

        [TearDown]
        public void TearDown()
        {
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableElements, Is.Empty);
                Assert.That(this.viewModel.SelectedElementBase, Is.Null);
                Assert.That(this.viewModel.CurrentIteration, Is.Null);
            });
        }

        [Test]
        public void VerifyUpdateProperties()
        {
            var topElement = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "box"
            };

            var elementDefinition = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "accelerometer"
            };

            var elementUsage = new ElementUsage()
            {
                Iid = Guid.NewGuid(),
                ElementDefinition = elementDefinition,
                Name = elementDefinition.Name
            };

            topElement.ContainedElement.Add(elementUsage);

            var iteration = new Iteration()
            {
                Element = { topElement, elementDefinition },
                Option = { new Option() }
            };

            this.viewModel.CurrentIteration = iteration;
            Assert.That(this.viewModel.AvailableElements, Is.Empty);

            iteration.TopElement = topElement;
            this.viewModel.CurrentIteration = null;
            this.viewModel.CurrentIteration = iteration;
            Assert.That(this.viewModel.AvailableElements.Count(), Is.EqualTo(2));
        }
    }
}
