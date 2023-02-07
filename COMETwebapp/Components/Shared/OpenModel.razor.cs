// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OpenModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Components.Shared
{
	using CDP4Common.EngineeringModelData;

	using COMETwebapp.ViewModels.Components.Shared;

	using Microsoft.AspNetCore.Components;

	using ReactiveUI;

	/// <summary>
	/// Component used to open an <see cref="EngineeringModel" /> and select an <see cref="Iteration" />
	/// </summary>
	public partial class OpenModel
	{
		/// <summary>
		/// Value asserting that selectors for <see cref="EngineeringModel"/> and <see cref="Iteration"/> are enabled
		/// </summary>
		private bool selectorEnabled;

		/// <summary>
		/// The <see cref="IOpenModelViewModel" />
		/// </summary>
		[Inject]
		public IOpenModelViewModel ViewModel { get; set; }

		/// <summary>
		/// The <see cref="Guid" /> of a requested <see cref="EngineeringModel" />
		/// </summary>
		[Parameter]
		public Guid ModelId { get; set; }

		/// <summary>
		/// The <see cref="Guid" /> of a requested <see cref="Iteration" />
		/// </summary>
		[Parameter]
		public Guid IterationId { get; set; }

		/// <summary>
		/// Value asserting that the button is enabled
		/// </summary>
		public bool ButtonEnabled => this.AreRequiredFieldSelected() && !this.ViewModel.IsOpeningSession;

		/// <summary>
		/// The display text of the button
		/// </summary>
		public string ButtonText => this.ViewModel.IsOpeningSession ? "Opening" : "Open";

		/// <summary>
		/// Method invoked when the component is ready to start, having received its
		/// initial parameters from its parent in the render tree.
		/// </summary>
		protected override void OnInitialized()
		{
			base.OnInitialized();

			this.ViewModel.InitializesProperties();

			this.Disposables.Add(this.ViewModel);

			this.Disposables.Add(this.WhenAnyValue(x => x.ViewModel.SelectedEngineeringModel,
                    x => x.ViewModel.SelectedIterationSetup,
                    x => x.ViewModel.SelectedDomainOfExpertise,
                    x => x.ViewModel.IsOpeningSession)
				.Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
		}

		/// <summary>
		/// Method invoked when the component has received parameters from its parent in
		/// the render tree, and the incoming values have been assigned to properties.
		/// </summary>
		protected override void OnParametersSet()
		{
			base.OnParametersSet();

			this.selectorEnabled = true;

			if (this.ModelId != Guid.Empty && this.IterationId != Guid.Empty)
			{
				this.selectorEnabled = false;
				this.ViewModel.PreSelectIteration(this.ModelId, this.IterationId);
			}
		}

		/// <summary>
		/// Verifies that all required field are selected
		/// </summary>
		/// <returns>True if all required field are selected</returns>
		private bool AreRequiredFieldSelected()
		{
			return this.ViewModel.SelectedEngineeringModel != null
			       && this.ViewModel.SelectedDomainOfExpertise != null
			       && this.ViewModel.SelectedIterationSetup != null;
		}
	}
}
