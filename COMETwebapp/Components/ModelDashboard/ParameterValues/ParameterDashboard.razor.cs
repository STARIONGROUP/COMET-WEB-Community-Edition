// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParameterDashboard.razor.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.Components.ModelDashboard.ParameterValues
{
	using CDP4Common.EngineeringModelData;

    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.ModelDashboard.ParameterValues;

	using DevExpress.Blazor;

	using Microsoft.AspNetCore.Components;

	/// <summary>
	/// Component used to provides information related to <see cref="ParameterValueSetBase" />
	/// </summary>
	public partial class ParameterDashboard
	{
		/// <summary>
		/// A reference to the <see cref="DxPopup" />
		/// </summary>
		private DxPopup detailsPopup;

		/// <summary>
		/// A collection of <see cref="ParameterValueSetBase" /> for the details
		/// </summary>
		private IEnumerable<ParameterValueSetBase> parameterDetailsValues = new List<ParameterValueSetBase>();

		/// <summary>
		/// The <see cref="IParameterDashboardViewModel" />
		/// </summary>
		[Parameter]
		public IParameterDashboardViewModel ViewModel { get; set; }

		/// <summary>
		/// The <see cref="EventCallback" /> to call when the user clicks on the to do table
		/// </summary>
		[Parameter]
		public EventCallback OnToDoClick { get; set; }

		/// <summary>
		/// Method invoked when the component has received parameters from its parent in
		/// the render tree, and the incoming values have been assigned to properties.
		/// </summary>
		protected override void OnParametersSet()
		{
			base.OnParametersSet();

			this.Disposables.Add(this.ViewModel.ValueSets.CountChanged.Subscribe(_ => this.InvokeAsync(this.StateHasChanged)));
		}

		/// <summary>
		/// Provide information related to specific click data
		/// </summary>
		/// <param name="valueSet">Information to get access to the requested data</param>
		/// <returns>A <see cref="Task"/></returns>
		public Task OnAccessData((string serieName, object argument) valueSet)
		{
			var valueSetsToDisplay = this.ViewModel.ValueSets.Items
				.Where(x => ReferenceEquals(x.Owner.ShortName, valueSet.argument))
				.ToList();

			this.parameterDetailsValues = valueSet.serieName switch
			{
				WebAppConstantValues.PublishedParameters => valueSetsToDisplay.Where(p => p.Published.SequenceEqual(p.ActualValue)),
				WebAppConstantValues.PublishableParameters => valueSetsToDisplay.Where(p => !p.Published.SequenceEqual(p.ActualValue)),
				WebAppConstantValues.ParametersWithMissingValues => valueSetsToDisplay.Where(p => p.Published.All(v => v == "-")),
                WebAppConstantValues.ParametersWithValues => valueSetsToDisplay.Where(p => p.Published.Any(v => v != "-")),
				_ => this.parameterDetailsValues
			};

			return this.InvokeAsync(async () => await this.detailsPopup.ShowAsync());
		}
	}
}
