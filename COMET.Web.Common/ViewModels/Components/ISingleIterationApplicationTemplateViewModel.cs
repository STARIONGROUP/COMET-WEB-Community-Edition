// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ISingleIterationApplicationTemplateViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMET.Web.Common.ViewModels.Components
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Services.SessionManagement;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    /// <summary>
    /// ViewModel that will englobe all applications where only one <see cref="Iteration" /> needs to be selected
    /// </summary>
    public interface ISingleIterationApplicationTemplateViewModel
	{
		/// <summary>
		/// Gets the <see cref="ISessionService" />
		/// </summary>
		ISessionService SessionService { get; }

		/// <summary>
		/// Gets the <see cref="IIterationSelectorViewModel" />
		/// </summary>
		IIterationSelectorViewModel IterationSelectorViewModel { get; }

		/// <summary>
		/// Value asserting that the user should select an <see cref="Iteration" />
		/// </summary>
		bool IsOnIterationSelectionMode { get; set; }

		/// <summary>
		/// The <see cref="Guid" /> of the <see cref="Iteration" /> that will be used
		/// </summary>
		Iteration SelectedIteration { get; set; }

		/// <summary>
		/// Selects an <see cref="Iteration" />
		/// </summary>
		/// <param name="iteration">The selected <see cref="Iteration" /></param>
		void SelectIteration(Iteration iteration);

		/// <summary>
		/// Asks the user to selects the <see cref="Iteration" /> that he wants to works with
		/// </summary>
		void AskToSelectIteration();
	}
}
