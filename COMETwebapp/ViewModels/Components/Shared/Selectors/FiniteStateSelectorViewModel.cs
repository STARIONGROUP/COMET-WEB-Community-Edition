// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ActualFiniteStateSelectorViewModel.cs" company="RHEA System S.A.">
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
	using CDP4Common.EngineeringModelData;

	using ReactiveUI;

	/// <summary>
	/// View Model that enables the user to select an <see cref="ActualFiniteState"/>
	/// </summary>
	public class FiniteStateSelectorViewModel: BelongsToIterationSelectorViewModel, IFiniteStateSelectorViewModel
	{
		/// <summary>
		/// Backing field for <see cref="SelectedActualFiniteState"/>
		/// </summary>
		private ActualFiniteState selectedActualFiniteState;

		/// <summary>
		/// A collection of available <see cref="ActualFiniteState" />
		/// </summary>
		public IEnumerable<ActualFiniteState> AvailableFiniteStates { get; private set; } = Enumerable.Empty<ActualFiniteState>();

		/// <summary>
		/// The currently selected <see cref="ActualFiniteState"/>
		/// </summary>
		public ActualFiniteState SelectedActualFiniteState
		{
			get => this.selectedActualFiniteState;
			set => this.RaiseAndSetIfChanged(ref this.selectedActualFiniteState,value);
		}

		/// <summary>
		/// Updates this view model properties
		/// </summary>
		protected override void UpdateProperties()
		{
			this.SelectedActualFiniteState = null;

			this.AvailableFiniteStates = this.CurrentIteration?.ActualFiniteStateList.OrderBy(x => x.Name).SelectMany(x => x.ActualState)
				.OrderBy(x => x.Name) ?? Enumerable.Empty<ActualFiniteState>();
		}
	}
}
