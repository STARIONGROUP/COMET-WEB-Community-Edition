// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScaleValueDefinitionsTableTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Antoine Théate, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.ReferenceData.MeasurementScales
{
    using System.Linq;
    using System.Threading.Tasks;

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
            context = new TestContext();
            context.ConfigureDevExpressBlazor();

            renderer = context.RenderComponent<ScaleValueDefinitionsTable>(parameters =>
            {
                parameters.Add(p => p.ScaleValueDefinitions, [new ScaleValueDefinition()]);
            });
        }

        [TearDown]
        public void Teardown()
        {
            context.CleanContext();
            context.Dispose();
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance, Is.Not.Null);
                Assert.That(renderer.Instance.ShouldCreate, Is.EqualTo(false));
            });
        }

        [Test]
        public async Task VerifyUnitFactorsTable()
        {
            var timesScaleValueDefinitionsChanged = 0;

            renderer.SetParametersAndRender(p =>
            {
                p.Add(parameters => parameters.ScaleValueDefinitionsChanged, () => { timesScaleValueDefinitionsChanged++; });
            });

            var editScaleValueDefinitionButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editScaleValueDefinitionButton");
            await renderer.InvokeAsync(editScaleValueDefinitionButton.Instance.Click.InvokeAsync);

            var form = renderer.FindComponent<DxGrid>();
            await renderer.InvokeAsync(form.Instance.EditModelSaving.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.ShouldCreate, Is.EqualTo(false));
                Assert.That(renderer.Instance.ScaleValueDefinitions, Has.Count.EqualTo(1));
                Assert.That(timesScaleValueDefinitionsChanged, Is.EqualTo(1));
            });

            var addScaleValueDefinitionButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "addScaleValueDefinitionButton");
            await renderer.InvokeAsync(addScaleValueDefinitionButton.Instance.Click.InvokeAsync);
            await renderer.InvokeAsync(form.Instance.EditModelSaving.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.ShouldCreate, Is.EqualTo(true));
                Assert.That(renderer.Instance.ScaleValueDefinitions, Has.Count.EqualTo(2));
                Assert.That(timesScaleValueDefinitionsChanged, Is.EqualTo(2));
            });

            var removeScaleValueDefinitionButton = renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "removeScaleValueDefinitionButton");
            await renderer.InvokeAsync(removeScaleValueDefinitionButton.Instance.Click.InvokeAsync);

            Assert.Multiple(() =>
            {
                Assert.That(renderer.Instance.ScaleValueDefinitions, Has.Count.EqualTo(1));
                Assert.That(timesScaleValueDefinitionsChanged, Is.EqualTo(3));
            });
        }
    }
}
