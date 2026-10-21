// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="EnumerationValueDefinitionFormTestFixture.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ReferenceData.ParameterTypes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class EnumerationValueDefinitionFormTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<EnumerationValueDefinitionForm> renderer;
        private EnumerationValueDefinition item;
        private Mock<ISessionService> sessionService;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.sessionService = new Mock<ISessionService>();
            this.context.Services.AddSingleton(this.sessionService.Object);

            this.item = new EnumerationValueDefinition
            {
                Name = "Value 1",
                ShortName = "v1"
            };

            this.renderer = this.context.Render<EnumerationValueDefinitionForm>(parameters =>
            {
                parameters.Add(p => p.Item, this.item);
            });
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        [Test]
        public void VerifyEnumerationValueDefinitionFormRendering()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.renderer.Instance, Is.Not.Null);
                Assert.That(this.renderer.Instance.Item, Is.EqualTo(this.item));
            });
        }
    }
}
