// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ComponentsTableTestFixture.cs" company="Starion Group S.A.">
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
    public class ComponentsTableTestFixture
    {
        private TestContext context;
        private IRenderedComponent<ComponentsTable> renderer;
        private CompoundParameterType parameterType;

        [SetUp]
        public void SetUp()
        {
            this.context = new TestContext();
            this.context.ConfigureDevExpressBlazor();

            this.parameterType = new CompoundParameterType
            {
                Component =
                {
                    new ParameterTypeComponent
                    {
                        ShortName = "comp1",
                        ParameterType = new SimpleQuantityKind
                        {
                            PossibleScale = [new OrdinalScale { Name = "scale" }]
                        }
                    },

                    new ParameterTypeComponent { ShortName = "comp2", ParameterType = new SimpleQuantityKind() }
                }
            };

            this.renderer = this.context.RenderComponent<ComponentsTable>(parameters =>
            {
                parameters.Add(p => p.ParameterTypes, [new SimpleQuantityKind(), new SpecializedQuantityKind()]);
                parameters.Add(p => p.ParameterType, this.parameterType);
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
                Assert.That(this.parameterType.Component, Has.Count.EqualTo(2));
            });

            var editParameterTypeComponentButton = this.renderer.FindComponents<DxButton>().First(x => x.Instance.Id == "editParameterTypeComponentButton");
            await this.renderer.InvokeAsync(editParameterTypeComponentButton.Instance.Click.InvokeAsync);
            Assert.That(this.renderer.Instance.Item, Is.Not.Null);
        }

        [Test]
        public async Task VerifyDimensionUpdate()
        {
            this.renderer.SetParametersAndRender(p => p.Add(x => x.ParameterType, new ArrayParameterType()));

            var dimensionTextBox = this.renderer.FindComponents<DxTextBox>().First(x => x.Instance.Id == "dimensionTextBox");
            await this.renderer.InvokeAsync(() => dimensionTextBox.Instance.TextChanged.InvokeAsync("1,2,3"));
            Assert.That(this.renderer.Instance.Dimension, Is.EqualTo("1,2,3"));

            var arrayParameterTypeWithDimensions = new ArrayParameterType();
            arrayParameterTypeWithDimensions.Dimension.AddRange([1, 2, 3, 4, 5]);

            this.renderer.SetParametersAndRender(p => { p.Add(x => x.ParameterType, arrayParameterTypeWithDimensions); });

            Assert.That(((ArrayParameterType)this.renderer.Instance.ParameterType).Dimension, Has.Count.EqualTo(5));
            await this.renderer.InvokeAsync(() => dimensionTextBox.Instance.TextChanged.InvokeAsync("1,2,3"));

            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance.Dimension, Is.EqualTo("1,2,3"));
                Assert.That(((ArrayParameterType)this.renderer.Instance.ParameterType).Dimension, Has.Count.EqualTo(3));
            });
        }

        [Test]
        public void VerifyOnInitialized()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance, Is.Not.Null);
                Assert.That(this.renderer.Instance.ParameterTypes.Count(), Is.EqualTo(2));
                Assert.That(this.renderer.Instance.ParameterType, Is.Not.Null);
                Assert.That(this.renderer.Instance.OrderedItemsList, Is.EqualTo(this.parameterType.Component));
                Assert.That(this.renderer.Markup, Does.Contain(this.parameterType.Component.First().ShortName));
            });
        }
    }
}
