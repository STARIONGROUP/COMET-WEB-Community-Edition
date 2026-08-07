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
