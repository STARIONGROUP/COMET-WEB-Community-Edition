// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="BelongsToIterationSelectorViewModel.cs" company="RHEA System S.A.">
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
    using COMETwebapp.Utilities.DisposableObject;
    using DynamicData.Binding;

    using ReactiveUI;

    /// <summary>
    /// Abstract class for View Model selector that belongs to an <see cref="Iteration" />
    /// </summary>
    public abstract class BelongsToIterationSelectorViewModel : DisposableObject, IBelongsToIterationSelectorViewModel
	{
		/// <summary>
		/// Backing field for <see cref="CurrentIteration" />
		/// </summary>
		private Iteration currentIteration;

		/// <summary>
		/// Initializes a new <see cref="BelongsToIterationSelectorViewModel" />
		/// </summary>
		protected BelongsToIterationSelectorViewModel()
		{
			this.Disposables.Add(this.WhenAnyPropertyChanged(nameof(this.CurrentIteration))
				.Subscribe(_ => this.UpdateProperties()));
		}

		/// <summary>
		/// The current <see cref="Iteration" />
		/// </summary>
		public Iteration CurrentIteration
		{
			get => this.currentIteration;
			set => this.RaiseAndSetIfChanged(ref this.currentIteration, value);
		}

		/// <summary>
		/// Updates this view model properties
		/// </summary>
		protected abstract void UpdateProperties();
	}
}
