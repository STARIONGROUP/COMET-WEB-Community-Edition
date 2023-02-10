// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IterationSelectorViewModel.cs" company="RHEA System S.A.">
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

	using COMETwebapp.Model;

	using Microsoft.AspNetCore.Components;

	/// <summary>
	/// View model that enable the user to select one selected <see cref="Iteration" />
	/// </summary>
	public class IterationSelectorViewModel : IIterationSelectorViewModel
	{
		/// <summary>
		/// The collection of available <see cref="Iteration" />
		/// </summary>
		private IEnumerable<Iteration> iterations;

		/// <summary>
		/// The selected <see cref="IterationData" />
		/// </summary>
		public IterationData SelectedIteration { get; set; }

		/// <summary>
		/// A collection of available <see cref="IterationData" />
		/// </summary>
		public IEnumerable<IterationData> AvailableIterations { get; set; }

		/// <summary>
		/// <see cref="EventCallback{TValue}" /> to call when the <see cref="Iteration" /> has been selected
		/// </summary>
		public EventCallback<Iteration> OnSubmit { get; set; }

		/// <summary>
		/// Updates this view model properties
		/// </summary>
		/// <param name="availableIterations">A collection of available <see cref="Iteration" /></param>
		public void UpdateProperties(IEnumerable<Iteration> availableIterations)
		{
			this.iterations = availableIterations;
			this.AvailableIterations = this.iterations.Select(x => new IterationData(x.IterationSetup, true));
			this.SelectedIteration = null;
		}

		/// <summary>
		/// Submit the selection of the <see cref="SelectedIteration" />
		/// </summary>
		/// <returns>A <see cref="Task" /></returns>
		public Task Submit()
		{
			var iteration = this.iterations.First(x => x.IterationSetup.Iid == this.SelectedIteration.IterationSetupId);
			return this.OnSubmit.InvokeAsync(iteration);
		}
	}
}
