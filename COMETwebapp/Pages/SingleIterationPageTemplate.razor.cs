// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SingleIterationPageTemplate.razor.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Pages
{
	using CDP4Common.EngineeringModelData;

	using COMETwebapp.Extensions;

	using Microsoft.AspNetCore.Components;

	/// <summary>
	/// Base abstract component for any page that should use only one <see cref="Iteration" />
	/// </summary>
	public abstract partial class SingleIterationPageTemplate
	{
		/// <summary>
		/// The <see cref="Guid" /> of an <see cref="Iteration" /> as a short string
		/// </summary>
		[Parameter]
		[SupplyParameterFromQuery]
		public string IterationId { get; set; }

		/// <summary>
		/// The <see cref="Guid" /> of the requested <see cref="Iteration" />
		/// </summary>
		protected Guid RequestedIteration { get; private set; }

		/// <summary>
		/// Method invoked when the component has received parameters from its parent in
		/// the render tree, and the incoming values have been assigned to properties.
		/// </summary>
		protected override void OnParametersSet()
		{
			base.OnParametersSet();

			if (!string.IsNullOrEmpty(this.IterationId))
			{
				this.RequestedIteration = this.IterationId.FromShortGuid();
			}
		}
	}
}
