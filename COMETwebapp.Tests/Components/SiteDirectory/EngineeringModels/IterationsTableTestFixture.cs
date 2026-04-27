// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IterationsTableTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.SiteDirectory.EngineeringModels
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.SiteDirectory.EngineeringModel;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DynamicData;

    using NUnit.Framework;


    [TestFixture]
    public class IterationsTableTestFixture
    {
        private BunitContext context;
        private IRenderedComponent<IterationsTable> renderer;
        private EngineeringModelSetup model;
        private Iteration iteration1;

        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();

            this.model = new EngineeringModelSetup
            {
                Name = "model",
                ShortName = "model"
            };

            this.iteration1 = new Iteration
            {
                IterationSetup = new IterationSetup { Container = this.model },
                Container = new EngineeringModel { EngineeringModelSetup = this.model }
            };

            var rows = new SourceList<IterationRowViewModel>();
            rows.Add(new IterationRowViewModel(this.iteration1));
            this.context.ConfigureDevExpressBlazor();

            this.renderer = this.context.Render<IterationsTable>(p =>
            {
                p.Add(parameter => parameter.EngineeringModelSetup, this.model);
                p.Add(parameter => parameter.IterationRows, rows.Items);
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
                Assert.That(this.renderer.Instance.ShouldCreateThing, Is.EqualTo(false));
                Assert.That(this.renderer.Instance.EngineeringModelSetup, Is.EqualTo(this.model));
                Assert.That(this.renderer.Markup, Does.Contain(this.model.ShortName));
            });
        }
    }
}
