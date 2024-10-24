// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="UnitFactorsTableTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.ReferenceData
{
    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ReferenceData.MeasurementUnits;

    using DevExpress.Blazor;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class UnitFactorsTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<UnitFactorsTable> renderer;
        private DerivedUnit derivedUnit;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.derivedUnit = new DerivedUnit
            {
                UnitFactor =
                {
                    new UnitFactor
                    {
                        Unit = new SimpleUnit { ShortName = "simple" },
                        Exponent = "exp"
                    }
                }
            };

            var measurementUnits = new List<MeasurementUnit>
            {
                new SimpleUnit()
            };

            this.renderer = this.context.RenderComponent<UnitFactorsTable>(p =>
            {
                p.Add(x => x.Thing, this.derivedUnit);
                p.Add(x => x.MeasurementUnits, measurementUnits);
            });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
            this.context.Dispose();
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance, Is.Not.Null);
                Assert.That(this.renderer.Instance.ShouldCreate, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.Thing, Is.Not.Null);
                Assert.That(this.renderer.Instance.MeasurementUnits, Is.Not.Null);
            });
        }

        [Test]
        public async Task VerifyUnitFactorsTable()
        {
            var editUnitFactorButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editUnitFactorButton");
            await this.renderer.InvokeAsync(editUnitFactorButton.Instance.Click.InvokeAsync);

            var form = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(form.Instance.EditModelSaving.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreate, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.Thing.UnitFactor, Has.Count.EqualTo(1));
            });

            var addUnitFactorButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addUnitFactorButton");
            await this.renderer.InvokeAsync(addUnitFactorButton.Instance.Click.InvokeAsync);
            await this.renderer.InvokeAsync(form.Instance.EditModelSaving.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreate, Is.EqualTo(true));
                Assert.That(this.renderer.Instance.Thing.UnitFactor, Has.Count.EqualTo(2));
            });

            var removeUnitFactorButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeUnitFactorButton");
            await this.renderer.InvokeAsync(removeUnitFactorButton.Instance.Click.InvokeAsync);

            Assert.That(this.renderer.Instance.Thing.UnitFactor, Has.Count.EqualTo(1));
        }
    }
}
