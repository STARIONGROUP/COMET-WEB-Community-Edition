// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PublicationRowViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.ViewModels.Components.Publications.Rows
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    
    using COMET.Web.Common.ViewModels.Components.Publications.Rows;

    using NUnit.Framework;

    [TestFixture]
    public class PublicationRowViewModelTestFixture
    {
        private PublicationRowViewModel viewModel;
        private Parameter parameter;
        private DomainOfExpertise doe;
        private ElementDefinition elementBase;
        private ParameterValueSet valueSet;

        [SetUp]
        public void SetUp()
        {
            this.doe = new DomainOfExpertise
            {
                Iid = Guid.NewGuid(),
                Name = "Sys"
            };

            this.elementBase = new ElementDefinition
            {
                Iid = Guid.NewGuid()
            };

            this.valueSet = new ParameterValueSet
            {
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(new[] { "-" })
            };

            this.parameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = this.doe,
                ValueSet =
                {
                    this.valueSet
                },
                ParameterType = new BooleanParameterType
                {
                    Iid = Guid.NewGuid(),
                    Name = "IsDeprecated"
                }
            };

            this.elementBase.Parameter.Add(this.parameter);

            this.viewModel = new PublicationRowViewModel(this.parameter, this.parameter.ValueSet.First());
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.ParameterOrOverride, Is.EqualTo(this.parameter));
                Assert.That(this.viewModel.Domain, Is.EqualTo(this.doe.Name));
                Assert.That(this.viewModel.ElementShortName, Is.EqualTo(this.elementBase.ShortName));
                Assert.That(this.viewModel.ModelCode, Is.EqualTo(this.parameter.ModelCode()));
                Assert.That(this.viewModel.NewValue, Is.EqualTo(this.valueSet.ActualValue.ToString()));
                Assert.That(this.viewModel.OldValue, Is.EqualTo(this.valueSet.Published.ToString()));
                Assert.That(this.viewModel.ParameterType, Is.EqualTo(this.parameter.ParameterType.Name));
            });
        }
    }
}
