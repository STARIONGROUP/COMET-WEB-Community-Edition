// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MappingToReferenceScalesTableTestFixture.cs" company="Starion Group S.A.">
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
    public class MappingToReferenceScalesTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<MappingToReferenceScalesTable> renderer;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            var mappingToReferenceScale = new MappingToReferenceScale
            {
                ReferenceScaleValue = new ScaleValueDefinition(),
                DependentScaleValue = new ScaleValueDefinition()
            };

            var scale = new OrdinalScale()
            {
                MappingToReferenceScale = { mappingToReferenceScale }
            };

            this.renderer = this.context.RenderComponent<MappingToReferenceScalesTable>(parameters =>
            {
                parameters.Add(p => p.MeasurementScale, scale);
                parameters.Add(p => p.DependentScaleValueDefinitions, [new ScaleValueDefinition()]);
                parameters.Add(p => p.ReferenceScaleValueDefinitions, [new ScaleValueDefinition()]);
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
                Assert.That(this.renderer.Instance.DependentScaleValueDefinitions, Is.Not.Null);
                Assert.That(this.renderer.Instance.DependentScaleValueDefinitions, Is.Not.Null);
                Assert.That(this.renderer.Instance.ShouldCreate, Is.EqualTo(false));
            });
        }

        [Test]
        public async Task VerifyUnitFactorsTable()
        {
            var timesMeasurementScaleChanged = 0;

            this.renderer.SetParametersAndRender(p => { p.Add(parameters => parameters.MeasurementScaleChanged, () => { timesMeasurementScaleChanged++; }); });

            var editMappingToReferenceScaleButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editMappingToReferenceScaleButton");
            await this.renderer.InvokeAsync(editMappingToReferenceScaleButton.Instance.Click.InvokeAsync);

            var form = this.renderer.FindComponent<DxGrid>();
            await this.renderer.InvokeAsync(form.Instance.EditModelSaving.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreate, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.MeasurementScale.MappingToReferenceScale, Has.Count.EqualTo(1));
                Assert.That(timesMeasurementScaleChanged, Is.EqualTo(1));
            });

            var addMappingToReferenceScaleButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addMappingToReferenceScaleButton");
            await this.renderer.InvokeAsync(addMappingToReferenceScaleButton.Instance.Click.InvokeAsync);
            await this.renderer.InvokeAsync(form.Instance.EditModelSaving.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.ShouldCreate, Is.EqualTo(true));
                Assert.That(this.renderer.Instance.MeasurementScale.MappingToReferenceScale, Has.Count.EqualTo(2));
                Assert.That(timesMeasurementScaleChanged, Is.EqualTo(2));
            });

            var removeMappingToReferenceScaleButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeMappingToReferenceScaleButton");
            await this.renderer.InvokeAsync(removeMappingToReferenceScaleButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.MeasurementScale.MappingToReferenceScale, Has.Count.EqualTo(1));
                Assert.That(timesMeasurementScaleChanged, Is.EqualTo(3));
            });
        }
    }
}
