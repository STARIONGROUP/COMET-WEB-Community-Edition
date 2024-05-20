// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="QuantityKindFactorsTableTestFixture.cs" company="Starion Group S.A.">
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
    public class QuantityKindFactorsTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<QuantityKindFactorsTable> renderer;
        private DerivedQuantityKind parameterType;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.parameterType = new DerivedQuantityKind
            {
                QuantityKindFactor =
                {
                    new QuantityKindFactor { Exponent = "exp1", QuantityKind = new SimpleQuantityKind() },
                    new QuantityKindFactor { Exponent = "exp2", QuantityKind = new SimpleQuantityKind() }
                }
            };

            this.renderer = this.context.RenderComponent<QuantityKindFactorsTable>(parameters =>
            {
                parameters.Add(p => p.QuantityKindParameterTypes, [new SimpleQuantityKind(), new SpecializedQuantityKind()]);
                parameters.Add(p => p.Thing, this.parameterType);
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
                Assert.That(this.parameterType.QuantityKindFactor, Has.Count.EqualTo(2));
            });

            var addQuantityKindFactorButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addQuantityKindFactorButton");
            await this.renderer.InvokeAsync(addQuantityKindFactorButton.Instance.Click.InvokeAsync);
            Assert.That(this.renderer.Instance.Item, Is.Not.Null);

            var firstFactor = this.parameterType.QuantityKindFactor[0];

            var moveUpButton = this.renderer.FindComponents<DxButton>().Last(x => x.Instance.Id == "moveUpButton");
            await this.renderer.InvokeAsync(moveUpButton.Instance.Click.InvokeAsync);
            Assert.That(this.parameterType.QuantityKindFactor[0], Is.Not.EqualTo(firstFactor));

            var moveDownButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "moveDownButton");
            await this.renderer.InvokeAsync(moveDownButton.Instance.Click.InvokeAsync);
            Assert.That(this.parameterType.QuantityKindFactor[0], Is.EqualTo(firstFactor));

            var removeQuantityKindFactorButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeQuantityKindFactorButton");
            await this.renderer.InvokeAsync(removeQuantityKindFactorButton.Instance.Click.InvokeAsync);
            Assert.That(this.parameterType.QuantityKindFactor, Has.Count.EqualTo(1));

            var grid = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(grid.Instance.EditModelSaving.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.OrderedItemsList, Has.Count.EqualTo(2));
                Assert.That(this.renderer.Instance.ShouldCreate, Is.EqualTo(true));
            });

            var editQuantityKindFactorButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editQuantityKindFactorButton");
            await this.renderer.InvokeAsync(editQuantityKindFactorButton.Instance.Click.InvokeAsync);
            await this.renderer.InvokeAsync(grid.Instance.EditModelSaving.InvokeAsync);
            Assert.That(this.renderer.Instance.ShouldCreate, Is.EqualTo(false));
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance, Is.Not.Null);
                Assert.That(this.renderer.Instance.QuantityKindParameterTypes.Count(), Is.EqualTo(2));
                Assert.That(this.renderer.Instance.Thing, Is.Not.Null);
                Assert.That(this.renderer.Instance.OrderedItemsList, Is.EqualTo(this.parameterType.QuantityKindFactor));
                Assert.That(this.renderer.Markup, Does.Contain(this.parameterType.QuantityKindFactor.First().Exponent));
            });
        }
    }
}
