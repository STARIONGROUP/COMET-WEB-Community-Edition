// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PublishedParameters.razor.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// <see cref="HaveValueSetsChartData"/> for published <see cref="ParameterValueSetBase"/>
    /// </summary>
    public partial class PublishedParameters
	{
		/// <summary>
		/// Get the number of <see cref="ParameterValueSetBase"/> represented in the selected area depending on criteria and domain
		/// </summary>
		/// <param name="criteria">The criteria represented in the selected area</param>
		/// <param name="domain">The associated domain of the selected area</param>
		/// <returns>The number of <see cref="ParameterValueSetBase"/> represented in the selected area</returns>
		protected int CountValueSetPerDomain(string criteria, object domain)
		{
			return criteria.Equals("Published Parameters", StringComparison.InvariantCultureIgnoreCase)
				? this.ValueSets.Count(d => d.Owner.ShortName.Equals(domain) && d.Published.SequenceEqual(d.ActualValue))
				: this.ValueSets.Count(d => d.Owner.ShortName.Equals(domain) && !d.Published.SequenceEqual(d.ActualValue));
		}
	}
}
