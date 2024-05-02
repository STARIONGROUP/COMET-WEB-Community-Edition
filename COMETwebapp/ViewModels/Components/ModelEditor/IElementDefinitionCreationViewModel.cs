// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IElementDefinitionCreationViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.ViewModels.Components.ModelEditor
{
	using CDP4Common.EngineeringModelData;
	using CDP4Common.SiteDirectoryData;

	using Microsoft.AspNetCore.Components;

	/// <summary>
	///     Interface definition for <see cref="ElementDefinitionCreationViewModel" />
	/// </summary>
	public interface IElementDefinitionCreationViewModel
	{
		/// <summary>
		/// A collection of available <see cref="Category" />s
		/// </summary>
		IEnumerable<Category> AvailableCategories { get; set; }

		/// <summary>
		/// A collection of available <see cref="DomainOfExpertise" />s
		/// </summary>
		IEnumerable<DomainOfExpertise> AvailableDomains { get; set; }

		/// <summary>
		///     An <see cref="EventCallback" /> to invoke on form submit
		/// </summary>
		EventCallback OnValidSubmit { get; set; }

		/// <summary>
		/// Value indicating if the <see cref="ElementDefinition"/> is top element
		/// </summary>
		bool IsTopElement { get; set; }

		/// <summary>
		/// The <see cref="ElementDefinition" /> to create or edit
		/// </summary>
		ElementDefinition ElementDefinition { get; set; }

		/// <summary>
		/// Selected <see cref="Category" />
		/// </summary>
		IEnumerable<Category> SelectedCategories { get; set; }

		/// <summary>
		/// Method invoked when the component is ready to start, having received its
		/// initial parameters from its parent in the render tree.
		/// Override this method if you will perform an asynchronous operation and
		/// want the component to refresh when that operation is completed.
		/// </summary>
		void OnInitialized();
	}
}
