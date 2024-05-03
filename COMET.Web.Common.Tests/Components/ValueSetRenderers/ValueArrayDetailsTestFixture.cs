// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ValueArrayDetailsTestFixture.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.Tests.Components.ValueSetRenderers
{
    using Bunit;

    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMET.Web.Common.Components.ValueSetRenderers;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ValueArrayDetailsTestFixture
    {
        private TestContext context;
        private static readonly string[] SfptValuesArray = ["1", "10", "2", "20"];

        [SetUp]
        public void Setup()
        {
            this.context = new TestContext();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.Dispose();
        }

        [Test]
        public void VerifyWithScalarParameterType()
        {
            var mass = new SimpleQuantityKind()
            {
                ShortName = "mass"
            };

            var scale = new RatioScale()
            {
                ShortName = "kg"
            };

            var valueArray = new ValueArray<string>(new []{"45"});

            var renderer = this.context.RenderComponent<ValueArrayDetails>(parameters =>
            {
                parameters.Add(p => p.ParameterType, mass);
                parameters.Add(p => p.Value,valueArray);
                parameters.Add(p => p.Scale, scale);
            });

            var div = renderer.Find("div");

            Assert.That(div.TextContent, Is.EqualTo("45 [kg]"));
        }

        [Test]
        public void VerifyWithCompoundParameterType()
        {
            var booleanParameterType = new BooleanParameterType();
            var compoundParameterType = new CompoundParameterType();

            compoundParameterType.Component.Add(new ParameterTypeComponent()
            {
                ParameterType = booleanParameterType
            });

            compoundParameterType.Component.Add(new ParameterTypeComponent()
            {
                ParameterType = booleanParameterType
            });

            var valueArray = new ValueArray<string>(new[] { "-", "true" });

            var renderer = this.context.RenderComponent<ValueArrayDetails>(parameters =>
            {
                parameters.Add(p => p.ParameterType, compoundParameterType);
                parameters.Add(p => p.Value, valueArray);
            });

            Assert.That(renderer.FindComponents<ScalarParameter>(), Has.Count.EqualTo(2));
        }

        [Test]
        public void VerifyWithTwoDimensionsArrayParameterType()
        {
            var booleanParameterType = new BooleanParameterType();
            var arrayParameterType = new ArrayParameterType();
            arrayParameterType.Dimension.Add(2);
            arrayParameterType.Dimension.Add(2);

            for (var componentIndex = 0; componentIndex < arrayParameterType.Dimension.Sum(); componentIndex++)
            {
                arrayParameterType.Component.Add(new ParameterTypeComponent
                {
                    ParameterType = booleanParameterType
                });
            }

            var valueArray = new ValueArray<string>(new[]{"-", "false", "true", "-"});
            
            var renderer = this.context.RenderComponent<ValueArrayDetails>(parameters =>
            {
                parameters.Add(p => p.ParameterType, arrayParameterType);
                parameters.Add(p => p.Value, valueArray);
            });

            Assert.That(renderer.FindComponents<ScalarParameter>(), Has.Count.EqualTo(4));
        }

        [Test]
        public void VerifyWithThreeDimensionsArrayParameterType()
        {
            var booleanParameterType = new BooleanParameterType();
            var arrayParameterType = new ArrayParameterType();
            arrayParameterType.Dimension.Add(2);
            arrayParameterType.Dimension.Add(2);
            arrayParameterType.Dimension.Add(2);

            for (var componentIndex = 0; componentIndex < arrayParameterType.Dimension.Sum(); componentIndex++)
            {
                arrayParameterType.Component.Add(new ParameterTypeComponent
                {
                    ParameterType = booleanParameterType
                });
            }

            var valueArray = new ValueArray<string>(new[] { "-", "false", "true", "-", "-", "false", "true", "true" });

            var renderer = this.context.RenderComponent<ValueArrayDetails>(parameters =>
            {
                parameters.Add(p => p.ParameterType, arrayParameterType);
                parameters.Add(p => p.Value, valueArray);
            });

            Assert.That(renderer.FindComponents<ScalarParameter>(), Has.Count.EqualTo(8));
        }

        [Test]
        public void VerifyWithSfpt()
        {
            var dependentType = new SimpleQuantityKind();
            var independentType = new SimpleQuantityKind();
            var sfpt = new SampledFunctionParameterType();

            sfpt.DependentParameterType.Add(new DependentParameterTypeAssignment()
            {
                ParameterType = dependentType
            });

            sfpt.IndependentParameterType.Add(new IndependentParameterTypeAssignment()
            {
                ParameterType = independentType
            });

            var valueArray = new ValueArray<string>(SfptValuesArray);

            var renderer = this.context.RenderComponent<ValueArrayDetails>(parameters =>
            {
                parameters.Add(p => p.ParameterType, sfpt);
                parameters.Add(p => p.Value, valueArray);
            });

            var sftpComponent = renderer.FindComponent<SampledFunctionParameterComponent>();
            Assert.That(sftpComponent.Instance.ParameterTypeAssignments, Has.Count.EqualTo(2));

            var columns = sftpComponent.FindAll("th");

            //table rows are the sum of all sfpt rows + header row
            var tableRows = sftpComponent.FindAll("tr");

            Assert.Multiple(() =>
            {
                Assert.That(columns, Has.Count.EqualTo(2));
                Assert.That(tableRows, Has.Count.EqualTo(3));
            });
        }
    }
}
