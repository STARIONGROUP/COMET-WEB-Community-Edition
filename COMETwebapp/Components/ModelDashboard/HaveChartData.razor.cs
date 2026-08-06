// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="HaveChartData.razor.cs" company="Starion Group S.A.">
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
//    Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.ModelDashboard
{
	using DevExpress.Blazor;

	using Microsoft.AspNetCore.Components;

	/// <summary>
	/// Base class for component that have <see cref="DxChart" />
	/// </summary>
	public abstract partial class HaveChartData: ComponentBase
	{
		/// <summary>
		/// <see cref="EventCallback" /> to call to access data on a clicked <see cref="DxChart" />
		/// </summary>
		[Parameter]
		public EventCallback<(string serieName, object argument)> OnAccessDataCallback { get; set; }

        // Each dashboard "feature" (Published Parameters, Missing Values, Unused Elements, Unreferenced Elements) owns ONE
        // green/red pair used consistently across ALL of its graphs (donut ring, bar chart and progress bar), and the four
        // features use four visually distinct pairs. Colors are hex strings (so they can feed the AntDesign Progress bars,
        // whose StrokeColor takes a lowercase "#rrggbb" string) with a matching System.Drawing.Color form for the DevExpress
        // chart series. See issue #892 - green means "done", red means "needs attention".

        /// <summary>
        /// The "done" color of the Published Parameters feature (published parameters). A light green.
        /// </summary>
        public const string PublishedColor = "#8bc34a";

        /// <summary>
        /// The "needs attention" color of the Published Parameters feature (publishable parameters). A light red.
        /// </summary>
        public const string PublishableColor = "#ef5350";

        /// <summary>
        /// The "done" color of the Missing Values feature (complete values). A sea green.
        /// </summary>
        public const string CompleteColor = "#2e8b57";

        /// <summary>
        /// The "needs attention" color of the Missing Values feature (missing values). An orange red.
        /// </summary>
        public const string MissingColor = "#d21f04";

        /// <summary>
        /// The "done" color of the Unused Elements feature (used elements). A vivid green.
        /// </summary>
        public const string UsedColor = "#00c853";

        /// <summary>
        /// The "needs attention" color of the Unused Elements feature (unused elements). A vivid red.
        /// </summary>
        public const string UnusedColor = "#ff1744";

        /// <summary>
        /// The "done" color of the Unreferenced Elements feature (referenced elements). A dark green.
        /// </summary>
        public const string ReferencedColor = "#1b5e20";

        /// <summary>
        /// The "needs attention" color of the Unreferenced Elements feature (unreferenced elements). A dark red.
        /// </summary>
        public const string UnreferencedColor = "#7f0000";

        /// <summary>
        /// The <see cref="System.Drawing.Color" /> form of <see cref="PublishedColor" /> for DevExpress chart series.
        /// </summary>
        public static System.Drawing.Color PublishedChartColor => System.Drawing.ColorTranslator.FromHtml(PublishedColor);

        /// <summary>
        /// The <see cref="System.Drawing.Color" /> form of <see cref="PublishableColor" /> for DevExpress chart series.
        /// </summary>
        public static System.Drawing.Color PublishableChartColor => System.Drawing.ColorTranslator.FromHtml(PublishableColor);

        /// <summary>
        /// The <see cref="System.Drawing.Color" /> form of <see cref="CompleteColor" /> for DevExpress chart series.
        /// </summary>
        public static System.Drawing.Color CompleteChartColor => System.Drawing.ColorTranslator.FromHtml(CompleteColor);

        /// <summary>
        /// The <see cref="System.Drawing.Color" /> form of <see cref="MissingColor" /> for DevExpress chart series.
        /// </summary>
        public static System.Drawing.Color MissingChartColor => System.Drawing.ColorTranslator.FromHtml(MissingColor);

        /// <summary>
        /// The <see cref="System.Drawing.Color" /> form of <see cref="UsedColor" /> for DevExpress chart series.
        /// </summary>
        public static System.Drawing.Color UsedChartColor => System.Drawing.ColorTranslator.FromHtml(UsedColor);

        /// <summary>
        /// The <see cref="System.Drawing.Color" /> form of <see cref="UnusedColor" /> for DevExpress chart series.
        /// </summary>
        public static System.Drawing.Color UnusedChartColor => System.Drawing.ColorTranslator.FromHtml(UnusedColor);

        /// <summary>
        /// The <see cref="System.Drawing.Color" /> form of <see cref="ReferencedColor" /> for DevExpress chart series.
        /// </summary>
        public static System.Drawing.Color ReferencedChartColor => System.Drawing.ColorTranslator.FromHtml(ReferencedColor);

        /// <summary>
        /// The <see cref="System.Drawing.Color" /> form of <see cref="UnreferencedColor" /> for DevExpress chart series.
        /// </summary>
        public static System.Drawing.Color UnreferencedChartColor => System.Drawing.ColorTranslator.FromHtml(UnreferencedColor);

        /// <summary>
        /// Access the data related to a <see cref="ChartSeriesPoint" />
        /// </summary>
        /// <param name="point">The <see cref="ChartSeriesPoint" /></param>
        /// <returns>A <see cref="Task" /></returns>
        protected Task AccessData(ChartSeriesPoint point)
		{
			return this.OnAccessDataCallback.InvokeAsync((point.SeriesName, point.Argument));
		}
	}
}
