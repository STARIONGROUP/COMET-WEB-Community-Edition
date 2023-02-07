// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterTypeSelectorViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Components.Shared.Selectors
{
	using CDP4Common.SiteDirectoryData;

	/// <summary>
	/// Interface definition for <see cref="ParameterTypeSelectorViewModel" />
	/// </summary>
	public interface IParameterTypeSelectorViewModel: IBelongsToIterationSelectorViewModel
	{
		/// <summary>
		/// The currently selected <see cref="ParameterType" />
		/// </summary>
		ParameterType SelectedParameterType { get; set; }

		/// <summary>
		/// A collection of available <see cref="ParameterType" />
		/// </summary>
		IEnumerable<ParameterType> AvailableParameterTypes { get; }
	}
}
