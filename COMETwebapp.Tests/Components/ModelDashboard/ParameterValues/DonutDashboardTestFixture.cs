// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DonutDashboardTestFixture.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or (at your option) any later version.
// 
//     The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//     Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//   --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Tests.Components.ModelDashboard.ParameterValues
{
    using Bunit;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using COMET.Web.Common.Test.Helpers;

    using COMETwebapp.Components.ModelDashboard.ParameterValues;
    using COMETwebapp.Utilities;

    using NUnit.Framework;

    /// <summary>
    /// Test fixture for <see cref="DonutDashboard" /> component.
    /// </summary>
    [TestFixture]
    public class DonutDashboardTestFixture
    {
        /// <summary>
        /// The <see cref="BunitContext" /> used for rendering components.
        /// </summary>
        private BunitContext context;

        /// <summary>
        /// The collection of <see cref="DomainOfExpertise" /> used for testing.
        /// </summary>
        private List<DomainOfExpertise> domains;

        /// <summary>
        /// The collection of <see cref="ParameterValueSetBase" /> used for testing.
        /// </summary>
        private List<ParameterValueSetBase> valueSets;

        /// <summary>
        /// Sets up the test context and test data before each test execution.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.context = new BunitContext();
            this.context.ConfigureDevExpressBlazor();

            var domain = new DomainOfExpertise
            {
                Iid = Guid.NewGuid(),
                ShortName = "SYS"
            };

            this.domains = [domain];

            var parameter1 = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = domain
            };

            var valueSet1 = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Published = new ValueArray<string>(["-"]),
                Manual = new ValueArray<string>(["10"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            parameter1.ValueSet.Add(valueSet1);

            var parameter2 = new Parameter
            {
                Iid = Guid.NewGuid(),
                Owner = domain
            };

            var valueSet2 = new ParameterValueSet
            {
                Iid = Guid.NewGuid(),
                Published = new ValueArray<string>(["10"]),
                Manual = new ValueArray<string>(["10"]),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            parameter2.ValueSet.Add(valueSet2);

            this.valueSets = [valueSet1, valueSet2];
        }

        /// <summary>
        /// Cleans up the test context after each test execution.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.context.CleanContext();
        }

        /// <summary>
        /// Verifies that <see cref="DonutDashboard" /> renders correctly with given value sets and domains in default and split
        /// view modes.
        /// </summary>
        [Test]
        public void VerifyDonutDashboardRendering()
        {
            var renderedComponent = this.context.Render<DonutDashboard>(parameters =>
            {
                parameters.Add(p => p.Domains, this.domains);
                parameters.Add(p => p.ValueSets, this.valueSets);
            });

            var chartContainer = renderedComponent.Find("[data-testid=\"dashboard-chart\"]");

            var splitViewRenderedComponent = this.context.Render<DonutDashboard>(parameters =>
            {
                parameters.AddCascadingValue(WebAppConstantValues.IsSplitViewCascadingValueName, true);
                parameters.Add(p => p.Domains, this.domains);
                parameters.Add(p => p.ValueSets, this.valueSets);
            });

            var splitChartContainer = splitViewRenderedComponent.Find("[data-testid=\"dashboard-chart\"]");

            Assert.Multiple(() =>
            {
                Assert.That(renderedComponent.Instance, Is.Not.Null);
                Assert.That(chartContainer, Is.Not.Null);
                Assert.That(chartContainer.ClassList, Does.Contain("col-xxl-8"));
                Assert.That(renderedComponent.Instance.IsSplitView, Is.False);
                Assert.That(splitViewRenderedComponent.Instance, Is.Not.Null);
                Assert.That(splitChartContainer, Is.Not.Null);
                Assert.That(splitChartContainer.ClassList, Does.Contain("col-12"));
                Assert.That(splitViewRenderedComponent.Instance.IsSplitView, Is.True);
            });
        }
    }
}
