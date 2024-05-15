// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ScaleValueDefinitionsTableTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.ReferenceData.MeasurementScales
{
    using Bunit;

    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ReferenceData.MeasurementScales;

    using DevExpress.Blazor;

    using NUnit.Framework;

    using TestContext = Bunit.TestContext;

    [TestFixture]
    public class ScaleValueDefinitionsTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ScaleValueDefinitionsTable> renderer;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            var scale = new OrdinalScale()
            {
                ValueDefinition = { new ScaleValueDefinition() }
            };

            this.renderer = this.context.RenderComponent<ScaleValueDefinitionsTable>(parameters => { parameters.Add(p => p.MeasurementScale, scale); });
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
            });
        }

        [Test]
        public async Task VerifyUnitFactorsTable()
        {
            var timesScaleValueDefinitionsChanged = 0;

            this.renderer.SetParametersAndRender(p => { p.Add(parameters => parameters.MeasurementScaleChanged, () => { timesScaleValueDefinitionsChanged++; }); });

            var editScaleValueDefinitionButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editScaleValueDefinitionButton");
            await this.renderer.InvokeAsync(editScaleValueDefinitionButton.Instance.Click.InvokeAsync);

            var form = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(form.Instance.EditModelSaving.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreate, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.MeasurementScale.ValueDefinition, Has.Count.EqualTo(1));
                Assert.That(timesScaleValueDefinitionsChanged, Is.EqualTo(1));
            });

            var addScaleValueDefinitionButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addScaleValueDefinitionButton");
            await this.renderer.InvokeAsync(addScaleValueDefinitionButton.Instance.Click.InvokeAsync);
            await this.renderer.InvokeAsync(form.Instance.EditModelSaving.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreate, Is.EqualTo(true));
                Assert.That(this.renderer.Instance.MeasurementScale.ValueDefinition, Has.Count.EqualTo(2));
                Assert.That(timesScaleValueDefinitionsChanged, Is.EqualTo(2));
            });

            var removeScaleValueDefinitionButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeScaleValueDefinitionButton");
            await this.renderer.InvokeAsync(removeScaleValueDefinitionButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.MeasurementScale.ValueDefinition, Has.Count.EqualTo(1));
                Assert.That(timesScaleValueDefinitionsChanged, Is.EqualTo(3));
            });
        }
    }
}
