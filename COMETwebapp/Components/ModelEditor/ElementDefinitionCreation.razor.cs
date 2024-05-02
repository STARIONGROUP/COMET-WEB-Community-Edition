// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionCreation.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2024 Starion Group S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Nabil Abbar
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace COMETwebapp.Components.ModelEditor
{
	using COMETwebapp.ViewModels.Components.ModelEditor;

	using Microsoft.AspNetCore.Components;

	using ReactiveUI;

	/// <summary>
	///     Partial class for the component <see cref="ElementDefinitionCreation"/>
	/// </summary>
	public partial class ElementDefinitionCreation
	{
		/// <summary>
		///     The <see cref="IElementDefinitionCreationViewModel" /> for the component
		/// </summary>
		[Parameter]
		public IElementDefinitionCreationViewModel ViewModel { get; set; }

		/// <summary>
		/// Method invoked when the component is ready to start, having received its
		/// initial parameters from its parent in the render tree.
		/// </summary>
		protected override void OnInitialized()
		{
			base.OnInitialized();

			this.ViewModel.OnInitialized();
		}
	}
}
