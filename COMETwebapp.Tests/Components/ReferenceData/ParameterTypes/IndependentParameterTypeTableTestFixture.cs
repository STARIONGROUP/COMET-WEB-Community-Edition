// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IndependentParameterTypeTableTestFixture.cs" company="Starion Group S.A.">
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
    using CDP4Common.Types;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ReferenceData.ParameterTypes;

    using DevExpress.Blazor;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class IndependentParameterTypeTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<IndependentParameterTypeTable> renderer;
        private SampledFunctionParameterType parameterType;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.parameterType = new SampledFunctionParameterType
            {
                IndependentParameterType =
                {
                    new IndependentParameterTypeAssignment
                    {
                        MeasurementScale = new OrdinalScale(),
                        ParameterType = new SimpleQuantityKind
                        {
                            PossibleScale = [new OrdinalScale { Name = "scale" }],
                            Name = "parameter"
                        }
                    },

                    new IndependentParameterTypeAssignment { MeasurementScale = new OrdinalScale(), ParameterType = new SimpleQuantityKind() }
                },
                InterpolationPeriod = new ValueArray<string>(["1", "2"])
            };

            this.renderer = this.context.RenderComponent<IndependentParameterTypeTable>(parameters =>
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
        public async Task VerifyGridActions()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.Item, Is.Null);
                Assert.That(this.parameterType.IndependentParameterType, Has.Count.EqualTo(2));
            });

            var addIndependentParameterTypeButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addIndependentParameterTypeButton");
            await this.renderer.InvokeAsync(addIndependentParameterTypeButton.Instance.Click.InvokeAsync);
            Assert.That(this.renderer.Instance.Item, Is.Not.Null);

            var firstFactor = this.parameterType.IndependentParameterType[0];

            var moveUpButton = this.renderer.FindComponents<DxButton>().Last(x => x.Instance.Id == "moveUpButton");
            await this.renderer.InvokeAsync(moveUpButton.Instance.Click.InvokeAsync);
            Assert.That(this.parameterType.IndependentParameterType[0], Is.Not.EqualTo(firstFactor));

            var moveDownButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "moveDownButton");
            await this.renderer.InvokeAsync(moveDownButton.Instance.Click.InvokeAsync);
            Assert.That(this.parameterType.IndependentParameterType[0], Is.EqualTo(firstFactor));

            var removeIndependentParameterTypeButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeIndependentParameterTypeButton");
            await this.renderer.InvokeAsync(removeIndependentParameterTypeButton.Instance.Click.InvokeAsync);
            Assert.That(this.parameterType.IndependentParameterType, Has.Count.EqualTo(1));

            var grid = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(grid.Instance.EditModelSaving.InvokeAsync);
            Assert.That(this.renderer.Instance.ShouldCreate, Is.EqualTo(true));

            var editIndependentParameterTypeButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editIndependentParameterTypeButton");
            await this.renderer.InvokeAsync(editIndependentParameterTypeButton.Instance.Click.InvokeAsync);
            await this.renderer.InvokeAsync(grid.Instance.EditModelSaving.InvokeAsync);
            Assert.That(this.renderer.Instance.ShouldCreate, Is.EqualTo(false));
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance, Is.Not.Null);
                Assert.That(this.renderer.Instance.ParameterTypes.Count(), Is.EqualTo(2));
                Assert.That(this.renderer.Instance.ParameterType, Is.Not.Null);
                Assert.That(this.renderer.Markup, Does.Contain(this.parameterType.IndependentParameterType.First().ParameterType.Name));
            });
        }
    }
}
