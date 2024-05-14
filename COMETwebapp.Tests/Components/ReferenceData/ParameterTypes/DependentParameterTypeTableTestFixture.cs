// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DependentParameterTypeTableTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Tests.Components.ReferenceData.ParameterTypes
{
    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ReferenceData.ParameterTypes;

    using DevExpress.Blazor;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class DependentParameterTypeTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<DependentParameterTypeTable> renderer;
        private SampledFunctionParameterType parameterType;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.parameterType = new SampledFunctionParameterType
            {
                DependentParameterType =
                {
                    new DependentParameterTypeAssignment
                    {
                        MeasurementScale = new OrdinalScale(),
                        ParameterType = new SimpleQuantityKind
                        {
                            PossibleScale = [new OrdinalScale { Name = "scale" }],
                            Name = "parameter"
                        }
                    },

                    new DependentParameterTypeAssignment { MeasurementScale = new OrdinalScale(), ParameterType = new SimpleQuantityKind() }
                }
            };

            this.renderer = this.context.RenderComponent<DependentParameterTypeTable>(parameters =>
            {
                parameters.Add(p => p.ParameterTypes, [new SimpleQuantityKind(), new SpecializedQuantityKind()]);
                parameters.Add(p => p.ParameterType, this.parameterType);
                parameters.Add(p => p.Enabled, true);
            });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public async Task VerifyComponentsEdit()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.Item, Is.Null);
                Assert.That(this.parameterType.DependentParameterType, Has.Count.EqualTo(2));
            });

            var editDependentParameterTypeButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editDependentParameterTypeButton");
            await this.renderer.InvokeAsync(editDependentParameterTypeButton.Instance.Click.InvokeAsync);
            Assert.That(this.renderer.Instance.Item, Is.Not.Null);
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance, Is.Not.Null);
                Assert.That(this.renderer.Instance.ParameterTypes.Count(), Is.EqualTo(2));
                Assert.That(this.renderer.Instance.ParameterType, Is.Not.Null);
                Assert.That(this.renderer.Instance.OrderedItemsList, Is.EqualTo(this.parameterType.DependentParameterType));
                Assert.That(this.renderer.Markup, Does.Contain(this.parameterType.DependentParameterType.First().ParameterType.Name));
            });
        }
    }
}
