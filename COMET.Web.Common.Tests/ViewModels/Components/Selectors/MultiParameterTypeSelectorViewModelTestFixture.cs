// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MultiParameterTypeSelectorViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
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
    public class MultiParameterTypeSelectorViewModelTestFixture
    {
        private MultiParameterTypeSelectorViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            this.viewModel = new MultiParameterTypeSelectorViewModel();
        }

        [TearDown]
        public void TearDown()
        {
            this.viewModel.Dispose();
        }

        [Test]
        public void VerifyInitialState()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableParameterTypes, Is.Empty);
                Assert.That(this.viewModel.SelectedParameterTypes, Is.Empty);
                Assert.That(this.viewModel.CurrentIteration, Is.Null);
            });
        }

        [Test]
        public void VerifyUpdatePropertiesAndFilter()
        {
            var booleanParameterType = new BooleanParameterType { Name = "bool", Iid = Guid.NewGuid() };
            var textParameterType = new TextParameterType { Name = "text", Iid = Guid.NewGuid() };
            var booleanParameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = booleanParameterType };
            var textParameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = textParameterType };
            var element = new ElementDefinition { Iid = Guid.NewGuid(), Parameter = { textParameter, booleanParameter } };
            var iteration = new Iteration { Iid = Guid.NewGuid(), Element = { element } };
            this.viewModel.CurrentIteration = iteration;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableParameterTypes.Count(), Is.EqualTo(2));
                Assert.That(this.viewModel.SelectedParameterTypes, Is.Empty);
            });

            this.viewModel.SelectedParameterTypes = new List<ParameterType> { booleanParameterType, textParameterType };
            this.viewModel.FilterAvailableParameterTypes(new List<Guid> { textParameterType.Iid });

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableParameterTypes.Count(), Is.EqualTo(1));
                Assert.That(this.viewModel.SelectedParameterTypes, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.SelectedParameterTypes.Single(), Is.SameAs(textParameterType));
            });

            this.viewModel.FilterAvailableParameterTypes(new List<Guid> { textParameterType.Iid, booleanParameterType.Iid });
            this.viewModel.SelectedParameterTypes = new List<ParameterType> { booleanParameterType, textParameterType };

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableParameterTypes.Count(), Is.EqualTo(2));
                Assert.That(this.viewModel.SelectedParameterTypes.Count(), Is.EqualTo(2));
            });
        }

        [Test]
        public void VerifyExcludeAvailableParameterTypes()
        {
            var booleanParameterType = new BooleanParameterType { Name = "bool", Iid = Guid.NewGuid() };
            var textParameterType = new TextParameterType { Name = "text", Iid = Guid.NewGuid() };
            var booleanParameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = booleanParameterType };
            var textParameter = new Parameter { Iid = Guid.NewGuid(), ParameterType = textParameterType };
            var element = new ElementDefinition { Iid = Guid.NewGuid(), Parameter = { textParameter, booleanParameter } };
            var iteration = new Iteration { Iid = Guid.NewGuid(), Element = { element } };
            this.viewModel.CurrentIteration = iteration;
            this.viewModel.SelectedParameterTypes = new List<ParameterType> { booleanParameterType, textParameterType };

            this.viewModel.ExcludeAvailableParameterTypes(new List<Guid> { booleanParameterType.Iid });

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.AvailableParameterTypes.Count(), Is.EqualTo(1));
                Assert.That(this.viewModel.AvailableParameterTypes.Single(), Is.SameAs(textParameterType));
                Assert.That(this.viewModel.SelectedParameterTypes, Has.Count.EqualTo(1));
                Assert.That(this.viewModel.SelectedParameterTypes.Single(), Is.SameAs(textParameterType));
            });
        }

        [Test]
        public void VerifySettingNullSelectionFallsBackToEmptyCollection()
        {
            this.viewModel.SelectedParameterTypes = null;
            Assert.That(this.viewModel.SelectedParameterTypes, Is.Not.Null);
            Assert.That(this.viewModel.SelectedParameterTypes, Is.Empty);
        }
    }
}
