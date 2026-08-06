// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterValueChartsTestFixture.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Tests.Components.ModelDashboard.ParameterValues
{
    using System;
    using System.Collections.Generic;

    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ModelDashboard;
    using COMETwebapp.Components.ModelDashboard.ParameterValues;

    using DevExpress.Blazor;

    using NUnit.Framework;

    [TestFixture]
    public class ParameterValueChartsTestFixture
    {
        private BunitContext context;

        [SetUp]
        public void Setup()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();
        }

        [TearDown]
        public void Teardown()
        {
            this.context.CleanContext();
        }

        /// <summary>
        /// Builds a single <see cref="ParameterValueSetBase" /> owned by the given domain, with the supplied
        /// published values (its actual value being kept equal to the published one).
        /// </summary>
        /// <param name="owner">The owning <see cref="DomainOfExpertise" /></param>
        /// <param name="published">The published values</param>
        /// <returns>A contained <see cref="ParameterValueSetBase" /></returns>
        private static ParameterValueSetBase CreateValueSet(DomainOfExpertise owner, IEnumerable<string> published)
        {
            var values = new ValueArray<string>(published);

            var parameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = owner
            };

            var valueSet = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Published = values,
                Manual = values,
                Formula = new ValueArray<string>(["-"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            parameter.ValueSet.Add(valueSet);
            return valueSet;
        }

        /// <summary>
        /// Verifies the issue #892 palette: a "done" state (complete/published) reads as green and a state needing
        /// attention (missing/publishable) reads as red, with the publication axis using its own distinct hue pair.
        /// </summary>
        [Test]
        public void VerifyDonutPaletteFlagsDoneStatesAsGreen()
        {
            Assert.Multiple(() =>
            {
                Assert.That(DonutDashboard.GetSeriesColor("Complete Values"), Is.EqualTo(HaveChartData.CompleteChartColor));
                Assert.That(DonutDashboard.GetSeriesColor("Published Parameters"), Is.EqualTo(HaveChartData.PublishedChartColor));
                Assert.That(DonutDashboard.GetSeriesColor("Missing Values"), Is.EqualTo(HaveChartData.MissingChartColor));
                Assert.That(DonutDashboard.GetSeriesColor("Publishable Parameters"), Is.EqualTo(HaveChartData.PublishableChartColor));
                Assert.That(HaveChartData.CompleteChartColor.ToArgb(), Is.EqualTo(System.Drawing.Color.SeaGreen.ToArgb()));
                Assert.That(HaveChartData.MissingChartColor, Is.Not.EqualTo(HaveChartData.CompleteChartColor));
                Assert.That(HaveChartData.PublishedChartColor, Is.Not.EqualTo(HaveChartData.CompleteChartColor));
                Assert.That(HaveChartData.PublishableChartColor, Is.Not.EqualTo(HaveChartData.MissingChartColor));
            });
        }

        /// <summary>
        /// Verifies the Published Parameters chart plots counts, i.e. it no longer renders the misleading
        /// normalized full-stacked bars but count-based stacked bars.
        /// </summary>
        [Test]
        public void VerifyPublishedParametersPlotsCountsNotPercentages()
        {
            var domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "SYS", Name = "System" };
            IEnumerable<ParameterValueSetBase> valueSets = [CreateValueSet(domain, ["12"])];

            var renderer = this.context.Render<PublishedParameters>(parameters => parameters.Add(p => p.ValueSets, valueSets));

            Assert.Multiple(() =>
            {
                Assert.That(renderer.FindComponents<DxChartFullStackedBarSeries<ParameterValueSetBase, string, int>>(), Is.Empty);
                Assert.That(renderer.FindComponents<DxChartStackedBarSeries<ParameterValueSetBase, string, int>>(), Is.Not.Empty);
            });
        }

        /// <summary>
        /// Verifies the Missing Values chart plots counts, i.e. it no longer renders the misleading normalized
        /// full-stacked bars but count-based stacked bars.
        /// </summary>
        [Test]
        public void VerifyDefaultValuesPlotsCountsNotPercentages()
        {
            var domain = new DomainOfExpertise { Iid = Guid.NewGuid(), ShortName = "THE", Name = "Thermal" };
            IEnumerable<ParameterValueSetBase> valueSets = [CreateValueSet(domain, ["-"])];

            var renderer = this.context.Render<DefaultValues>(parameters => parameters.Add(p => p.ValueSets, valueSets));

            Assert.Multiple(() =>
            {
                Assert.That(renderer.FindComponents<DxChartFullStackedBarSeries<ParameterValueSetBase, string, int>>(), Is.Empty);
                Assert.That(renderer.FindComponents<DxChartStackedBarSeries<ParameterValueSetBase, string, int>>(), Is.Not.Empty);
            });
        }
    }
}
