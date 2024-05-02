// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ElementDashboard.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.ModelDashboard.Elements
{
	using CDP4Common.EngineeringModelData;

    using COMETwebapp.Utilities;
    using COMETwebapp.ViewModels.Components.ModelDashboard.Elements;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

	/// <summary>
	/// Component used to provides information related to <see cref="ElementBase" />
	/// </summary>
	public partial class ElementDashboard
	{
        /// <summary>
        /// A reference to the <see cref="DxPopup" />
        /// </summary>
        private DxPopup detailsPopup;

        /// <summary>
        /// A collection of <see cref="ElementDefinition"/> for the details
        /// </summary>
        private IEnumerable<ElementDefinition> elementsDetailsValues = new List<ElementDefinition>();

        /// <summary>
		/// The <see cref="IElementDashboardViewModel"/>
		/// </summary>
		[Parameter]
		public IElementDashboardViewModel ViewModel { get; set; }

		/// <summary>
		/// Provide information related to specific click data
		/// </summary>
		/// <param name="valueSet">Information to get access to the requested data</param>
		/// <returns>A <see cref="Task"/></returns>
        public Task OnAccessData((string serieName, object argument) valueSet)
        {
            var elementDefinitions = this.ViewModel.ElementDefinitions.Items
                .Where(x => ReferenceEquals(x.Owner.ShortName, valueSet.argument))
                .ToList();

            this.elementsDetailsValues = valueSet.serieName switch
            {
                WebAppConstantValues.UnusedElements => elementDefinitions.FindAll(e => this.ViewModel.UnusedElements.Any(x => x.Iid == e.Iid)),
                WebAppConstantValues.UsedElements => elementDefinitions.FindAll(e => this.ViewModel.UnusedElements.All(x => x.Iid != e.Iid)),
                WebAppConstantValues.UnreferencedElements => elementDefinitions.FindAll(e => this.ViewModel.UnreferencedElements.Any(x => x.Iid == e.Iid)),
                WebAppConstantValues.ReferencedElements => elementDefinitions.FindAll(e => this.ViewModel.UnreferencedElements.All(x => x.Iid != e.Iid)),
                _ => this.elementsDetailsValues
            };

            this.elementsDetailsValues = this.elementsDetailsValues.OrderBy(x => x.Name);
            return this.InvokeAsync(async () => await this.detailsPopup.ShowAsync());
        }
    }
}
