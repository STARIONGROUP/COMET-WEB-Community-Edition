// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DonutDashboard.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.ModelDashboard.ParameterValues
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Model;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component that provide information through a Donut chart
    /// </summary>
    public partial class DonutDashboard
    {
        /// <summary>
        /// A collection of <see cref="ParameterValueSetBase"/> to display
        /// </summary>
        [Parameter]
        public IEnumerable<ParameterValueSetBase> ValueSets { get; set; }

        /// <summary>
        /// A collection of <see cref="DomainOfExpertise"/>
        /// </summary>
        [Parameter]
        public IEnumerable<DomainOfExpertise> Domains { get; set; }

        /// <summary>
        /// A collection of <see cref="DataChart"/> to display
        /// </summary>
        private IEnumerable<DataChart> dataCharts = Enumerable.Empty<DataChart>();

        /// <summary>
        /// Set the color appearence for the donut chart
        /// </summary>
        /// <param name="settings">The <see cref="ChartSeriesPointCustomizationSettings"/> associated with the series point</param>
        private static void CustomizeSeriesPoint(ChartSeriesPointCustomizationSettings settings)
        {
            var argument = settings.Point.Argument.ToString();

            settings.PointAppearance.Color = argument switch
            {
                "Published Parameters" => System.Drawing.Color.SteelBlue,
                "Publishable Parameters" => System.Drawing.Color.LightCoral,
                "Missing Values" => System.Drawing.Color.DarkSeaGreen,
                "Complete Values" => System.Drawing.Color.LightSlateGray,
                _ => settings.PointAppearance.Color
            };
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            this.dataCharts = this.GetChartData();
        }

        /// <summary>
        /// Initialize <see cref="DataChart"/> list from data to represent in the grpah
        /// </summary>
        /// <returns>A collection of <see cref="DataChart"/> used in the graph</returns>
        private List<DataChart> GetChartData()
        {
            var donutData = new List<DataChart>();

            foreach (var domain in this.Domains)
            {
                var domainParameterValueSets = this.ValueSets.Where(x => x.Owner.Iid == domain.Iid).ToList();
                var defaultValues = domainParameterValueSets.Count(p => p.Published.Any(v =>  v is "-"));

                donutData.Add(new DataChart
                {
                    Value = defaultValues,
                    Argument = "Missing Values",
                    Domain = domain.ShortName
                });

                donutData.Add(new DataChart()
                {
                    Value = domainParameterValueSets.Count - defaultValues,
                    Argument = "Complete Values",
                    Domain = domain.ShortName
                });

                var publishedParameters = domainParameterValueSets.Count(p => p.Published.SequenceEqual(p.ActualValue));
                
                donutData.Add(new DataChart()
                {
                    Value = publishedParameters,
                    Argument = "Published Parameters",
                    Domain = domain.ShortName
                });

                donutData.Add(new DataChart()
                {
                    Value = domainParameterValueSets.Count - publishedParameters,
                    Argument = "Publishable Parameters",
                    Domain = domain.ShortName
                });
            }
            
            return donutData;
        }
    }
}
