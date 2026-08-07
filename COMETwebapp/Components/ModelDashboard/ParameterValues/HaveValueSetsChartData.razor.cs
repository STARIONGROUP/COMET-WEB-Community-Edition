// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="HaveValueSetsChartData.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ModelDashboard.ParameterValues
{
	using CDP4Common.EngineeringModelData;

	using DevExpress.Blazor;

	using Microsoft.AspNetCore.Components;

	/// <summary>
	/// Base class for component that have <see cref="DxChart"/> related to <see cref="ParameterValueSetBase"/>
	/// </summary>
	public abstract partial class HaveValueSetsChartData: HaveChartData
	{
		/// <summary>
		/// A collection of <see cref="ParameterValueSetBase"/>
		/// </summary>
		[Parameter]
		public IEnumerable<ParameterValueSetBase> ValueSets { get; set; }

		/// <summary>
		/// Resolves the full name of the domain of expertise behind an axis argument (its short name),
		/// so the acronyms shown on the chart axis can be spelled out on hover (see issue #892).
		/// </summary>
		/// <param name="argument">The argument shown on the axis, i.e. the domain short name</param>
		/// <returns>The domain's full name, or the argument itself when it cannot be resolved</returns>
		public string GetDomainName(object argument)
		{
			return this.ValueSets.FirstOrDefault(d => d.Owner.ShortName.Equals(argument))?.Owner.Name ?? argument?.ToString();
		}

		/// <summary>
		/// Computes the share, in percent, that the given series-point count represents of all value sets owned by the
		/// domain behind an axis argument. Returned as <see cref="double.NaN"/> when that domain owns no value sets, so
		/// the tooltip can fall back to a dash rather than dividing by zero.
		/// </summary>
		/// <param name="argument">The argument shown on the axis, i.e. the domain short name</param>
		/// <param name="count">The series-point count for that argument</param>
		/// <returns>The rounded percentage, or <see cref="double.NaN"/> when the domain owns no value sets</returns>
		public double GetDomainPercentage(object argument, int count)
		{
			var total = this.ValueSets.Count(d => d.Owner.ShortName.Equals(argument));

			return total == 0 ? double.NaN : Math.Round((double)count / total * 100);
		}
	}
}
