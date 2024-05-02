// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OptionSelectorViewModelTestFixture.cs" company="Starion Group S.A.">
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
    public class OptionSelectorViewModelTestFixture
    {
        private OptionSelectorViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            this.viewModel = new OptionSelectorViewModel();
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
                Assert.That(this.viewModel.AllowNullOption, Is.True);
                Assert.That(this.viewModel.SelectedOption, Is.Null);
                Assert.That(this.viewModel.AvailableOptions, Is.Empty);
            });
        }

        [Test]
        public void VerifyUpdateProperties()
        {
            var iteration = new Iteration();

            var option = new Option()
            {
                Name = "b"
            };

            var option2 = new Option()
            {
                Name = "a"
            };
            
            iteration.Option.Add(option);
            iteration.Option.Add(option2);

            this.viewModel.CurrentIteration = iteration;
            
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableOptions.Count(), Is.EqualTo(2));
                Assert.That(this.viewModel.AvailableOptions.First(), Is.EqualTo(option2));
                Assert.That(this.viewModel.SelectedOption, Is.Null);
            });

            this.viewModel.Dispose();
            this.viewModel = new OptionSelectorViewModel(false);

            this.viewModel.CurrentIteration = iteration;
            Assert.That(this.viewModel.SelectedOption, Is.EqualTo(option));

            iteration.DefaultOption = option2;
            this.viewModel.CurrentIteration = null;
            this.viewModel.CurrentIteration = iteration;
            Assert.That(this.viewModel.SelectedOption, Is.EqualTo(option2));
        }
    }
}
