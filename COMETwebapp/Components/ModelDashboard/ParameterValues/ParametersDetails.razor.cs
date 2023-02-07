// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParametersDetails.razor.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.Components.ModelDashboard.ParameterValues
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.ViewModels.Components.ModelDashboard.ParameterValues;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Component that provide additional details on <see cref="Parameter" />s
    /// </summary>
    public partial class ParametersDetails
    {
        /// <summary>
        /// A collection of <see cref="ParameterValueSetBaseRowViewModel" />
        /// </summary>
        private IEnumerable<ParameterValueSetBaseRowViewModel> rows;

        /// <summary>
        /// A collection of <see cref="ParameterValueSetBase" />
        /// </summary>
        [Parameter]
        public IEnumerable<ParameterValueSetBase> ValueSets { get; set; }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            this.rows = this.ValueSets != null
                ? this.ValueSets.Select(x => new ParameterValueSetBaseRowViewModel(x))
                : Enumerable.Empty<ParameterValueSetBaseRowViewModel>();
        }
    }
}
