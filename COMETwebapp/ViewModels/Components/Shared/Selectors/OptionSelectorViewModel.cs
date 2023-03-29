// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="OptionSelectorViewModel.cs" company="RHEA System S.A.">
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
	/// View Model that provide capability to select an <see cref="Option" />
	/// </summary>
	public class OptionSelectorViewModel : BelongsToIterationSelectorViewModel, IOptionSelectorViewModel
	{
		/// <summary>
		/// Backing field for <see cref="SelectedOption" />
		/// </summary>
		private Option selectedOption;

		/// <summary>
		/// Value asserting that the current <see cref="Option"/> can be set to null or not
		/// </summary>
        public bool AllowNullOption { get; }

        /// <summary>
        /// Initializes a new <see cref="OptionSelectorViewModel" />
        /// </summary>
        /// <param name="allowNullOption">Value asserting that the current <see cref="Option"/> can be set to null or not</param>
        public OptionSelectorViewModel(bool allowNullOption = true)
        {
            this.AllowNullOption = allowNullOption;
        }

        /// <summary>
		/// The currently selected <see cref="Option" />
		/// </summary>
		public Option SelectedOption
		{
			get => this.selectedOption;
			set => this.RaiseAndSetIfChanged(ref this.selectedOption, value);
		}

		/// <summary>
		/// A collection of available <see cref="Option" />
		/// </summary>
		public IEnumerable<Option> AvailableOptions { get; private set; } = Enumerable.Empty<Option>();

		/// <summary>
		/// Updates this view model properties
		/// </summary>
		protected override void UpdateProperties()
		{
			this.AvailableOptions = this.CurrentIteration?.Option.OrderBy(x => x.Name)?? Enumerable.Empty<Option>();

            if (this.AllowNullOption || this.CurrentIteration == null)
            {
                this.SelectedOption = null;
            }
            else
            {
				this.selectedOption = this.CurrentIteration.DefaultOption ?? this.CurrentIteration.Option.FirstOrDefault();
            }
        }
	}
}
