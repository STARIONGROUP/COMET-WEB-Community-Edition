// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IOpenModelViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine
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

namespace COMETwebapp.ViewModels.Components.Shared
{
	using CDP4Common.EngineeringModelData;
	using CDP4Common.SiteDirectoryData;

	using COMETwebapp.Model;

	/// <summary>
	/// Interface definition for <see cref="OpenModelViewModel" />
	/// </summary>
	public interface IOpenModelViewModel : IDisposableViewModel
	{
		/// <summary>
		/// The selected <see cref="EngineeringModelSetup" />
		/// </summary>
		EngineeringModelSetup SelectedEngineeringModel { get; set; }

		/// <summary>
		/// The selected <see cref="IterationData" />
		/// </summary>
		IterationData SelectedIterationSetup { get; set; }

		/// <summary>
		/// The selected <see cref="DomainOfExpertise" />
		/// </summary>
		DomainOfExpertise SelectedDomainOfExpertise { get; set; }

		/// <summary>
		/// A collection of available <see cref="EngineeringModelSetup" />
		/// </summary>
		IEnumerable<EngineeringModelSetup> AvailableEngineeringModelSetups { get; set; }

		/// <summary>
		/// A collection of available <see cref="IterationData" />
		/// </summary>
		IEnumerable<IterationData> AvailableIterationSetups { get; set; }

		/// <summary>
		/// A collection of available <see cref="DomainOfExpertise" />
		/// </summary>
		IEnumerable<DomainOfExpertise> AvailablesDomainOfExpertises { get; set; }

		/// <summary>
		/// Value asserting that the session is on way to open
		/// </summary>
		bool IsOpeningSession { get; set; }

		/// <summary>
		/// Initializes this view model properties
		/// </summary>
		void InitializesProperties();

		/// <summary>
		/// Opens the <see cref="EngineeringModel" /> based on the selected field
		/// </summary>
		/// <returns></returns>
		Task OpenSession();

		/// <summary>
		/// Preselects the <see cref="Iteration" /> to open
		/// </summary>
		/// <param name="modelId">The <see cref="Guid" /> of the <see cref="EngineeringModel" /></param>
		/// <param name="iterationId">The <see cref="Guid" /> of the <see cref="Iteration" /> to open</param>
		void PreSelectIteration(Guid modelId, Guid iterationId);
	}
}
