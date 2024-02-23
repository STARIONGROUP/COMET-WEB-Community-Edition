// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="HaveElementDefinitionData.razor.cs" company="RHEA System S.A.">
//     Copyright (c) 2023-2024 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of CDP4-COMET WEB Community Edition
//     The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Components.ModelDashboard.Elements
{
	using CDP4Common.EngineeringModelData;
	using CDP4Common.SiteDirectoryData;

	using Microsoft.AspNetCore.Components;

	/// <summary>
	/// Base component for component that provide information related to <see cref="ElementDefinition" /> for the
	/// <see cref="ElementDashboard" />
	/// </summary>
	public abstract partial class HaveElementDefinitionData: HaveChartData
	{
		/// <summary>
		/// A collection of <see cref="ElementDefinition" />
		/// </summary>
		[Parameter]
		public IEnumerable<ElementDefinition> Elements { get; set; }

		/// <summary>
		/// The <see cref="DomainOfExpertise" />
		/// </summary>
		[Parameter]
		public DomainOfExpertise CurrentDomain { get; set; }

		/// <summary>
		/// Filter a collection of <see cref="ElementDefinition" /> based on the <see cref="DomainOfExpertise" />
		/// </summary>
		/// <param name="elements">The collection of <see cref="ElementDefinition" /></param>
		/// <returns>A filtered collection of <see cref="ElementDefinition" /></returns>
		protected IEnumerable<ElementDefinition> FilterOnDomain(IEnumerable<ElementDefinition> elements)
		{
			return elements.Where(x => x.Owner.Iid == this.CurrentDomain.Iid);
		}
	}
}
