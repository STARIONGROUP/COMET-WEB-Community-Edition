// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementChartsTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.ModelDashboard.Elements
{
    using System;
    using System.Collections.Generic;

    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ModelDashboard.Elements;

    using DevExpress.Blazor;

    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    [TestFixture]
    public class ElementChartsTestFixture
    {
        private BunitContext context;
        private DomainOfExpertise domain;
        private List<ElementDefinition> elements;

        [SetUp]
        public void Setup()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
            this.context.Services.AddAntDesign();

            this.domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };

            this.elements =
            [
                new ElementDefinition { Iid = Guid.NewGuid(), Owner = this.domain },
                new ElementDefinition { Iid = Guid.NewGuid(), Owner = this.domain }
            ];
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        /// <summary>
        /// Verifies the Unused Elements chart plots counts, i.e. it no longer renders the misleading normalized
        /// full-stacked bars but count-based stacked bars.
        /// </summary>
        [Test]
        public void VerifyUnusedElementsPlotsCountsNotPercentages()
        {
            var renderer = this.context.Render<UnusedElements>(parameters =>
            {
                parameters.Add(p => p.Elements, this.elements);
                parameters.Add(p => p.CurrentDomain, this.domain);
                parameters.Add(p => p.UnusedElementDefinitions, [this.elements[0]]);
            });

            Assert.Multiple(() =>
            {
                Assert.That(renderer.FindComponents<DxChartFullStackedBarSeries<ElementDefinition, string, int>>(), Is.Empty);
                Assert.That(renderer.FindComponents<DxChartStackedBarSeries<ElementDefinition, string, int>>(), Is.Not.Empty);
            });
        }

        /// <summary>
        /// Verifies the Unreferenced Elements chart plots counts, i.e. it no longer renders the misleading normalized
        /// full-stacked bars but count-based stacked bars.
        /// </summary>
        [Test]
        public void VerifyUnreferencedElementsPlotsCountsNotPercentages()
        {
            var renderer = this.context.Render<UnreferencedElements>(parameters =>
            {
                parameters.Add(p => p.Elements, this.elements);
                parameters.Add(p => p.CurrentDomain, this.domain);
                parameters.Add(p => p.UnreferencedElementDefinitions, [this.elements[0]]);
            });

            Assert.Multiple(() =>
            {
                Assert.That(renderer.FindComponents<DxChartFullStackedBarSeries<ElementDefinition, string, int>>(), Is.Empty);
                Assert.That(renderer.FindComponents<DxChartStackedBarSeries<ElementDefinition, string, int>>(), Is.Not.Empty);
            });
        }
    }
}
