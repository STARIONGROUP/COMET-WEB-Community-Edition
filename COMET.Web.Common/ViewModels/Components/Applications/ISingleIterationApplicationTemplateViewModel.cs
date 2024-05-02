// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ISingleIterationApplicationTemplateViewModel.cs" company="Starion Group S.A.">
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

namespace COMET.Web.Common.ViewModels.Components.Applications
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.Model;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    /// <summary>
    /// ViewModel that will englobe all applications where only one <see cref="Iteration" /> needs to be selected
    /// </summary>
    public interface ISingleIterationApplicationTemplateViewModel : ISingleThingApplicationTemplateViewModel<Iteration>
    {
        /// <summary>
        /// The <see cref="IterationData" /> that will be used
        /// </summary>
        IterationData SelectedIterationData { get; set; }

        /// <summary>
        /// Gets the <see cref="IIterationSelectorViewModel"/>
        /// </summary>
        IIterationSelectorViewModel IterationSelectorViewModel { get; }
    }
}
