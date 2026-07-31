// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DependentParameterTypeFormTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

    using NUnit.Framework;

    [TestFixture]
    public class DependentParameterTypeFormTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<DependentParameterTypeForm> renderer;
        private DependentParameterTypeAssignment item;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            this.item = new DependentParameterTypeAssignment
            {
                MeasurementScale = new OrdinalScale(),
                ParameterType = new SimpleQuantityKind
                {
                    PossibleScale = [new OrdinalScale { Name = "scale" }],
                    Name = "parameter"
                }
            };

            this.renderer = this.context.Render<DependentParameterTypeForm>(parameters =>
            {
                parameters.Add(p => p.Item, this.item);
                parameters.Add(p => p.ParameterTypes, [new SimpleQuantityKind()]);
            });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyDependentParameterTypeFormRendering()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance, Is.Not.Null);
                Assert.That(this.renderer.Instance.Item, Is.EqualTo(this.item));
                Assert.That(this.renderer.Instance.ParameterTypes.Count(), Is.EqualTo(1));
            });
        }
    }
}
