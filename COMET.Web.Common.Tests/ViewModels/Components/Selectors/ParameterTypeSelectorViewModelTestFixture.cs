// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterTypeSelectorViewModelTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components.Selectors;

    using NUnit.Framework;

    [TestFixture]
    public class ParameterTypeSelectorViewModelTestFixture
    {
        private ParameterTypeSelectorViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            this.viewModel = new ParameterTypeSelectorViewModel();
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
                Assert.That(this.viewModel.AvailableParameterTypes, Is.Empty);
                Assert.That(this.viewModel.SelectedParameterType, Is.Null);
                Assert.That(this.viewModel.CurrentIteration, Is.Null);
            });
        }

        [Test]
        public void VerifyUpdatePropertiesAndFilter()
        {
            var booleanParameterType = new BooleanParameterType(){Name = "bool", Iid = Guid.NewGuid()};
            var textParameterType = new TextParameterType() { Name = "text", Iid = Guid.NewGuid() };
            var booleanParameter = new Parameter() { Iid = Guid.NewGuid(), ParameterType = booleanParameterType };
            var textParameter = new Parameter() { Iid = Guid.NewGuid(), ParameterType = textParameterType };
            var element = new ElementDefinition() { Iid = Guid.NewGuid(), Parameter = { textParameter, booleanParameter } };
            var iteration = new Iteration() { Iid = Guid.NewGuid(), Element = { element } };
            this.viewModel.CurrentIteration = iteration;
            
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableParameterTypes.Count(), Is.EqualTo(2));
                Assert.That(this.viewModel.SelectedParameterType, Is.Null);
            });

            this.viewModel.SelectedParameterType = booleanParameterType;
            this.viewModel.FilterAvailableParameterTypes(new List<Guid>{textParameterType.Iid});

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableParameterTypes.Count(), Is.EqualTo(1));
                Assert.That(this.viewModel.SelectedParameterType, Is.Null);
            });

            this.viewModel.SelectedParameterType = textParameterType;
            this.viewModel.FilterAvailableParameterTypes(new List<Guid> { textParameterType.Iid, booleanParameterType.Iid});

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableParameterTypes.Count(), Is.EqualTo(2));
                Assert.That(this.viewModel.SelectedParameterType, Is.Not.Null);
            });
        }
    }
}
