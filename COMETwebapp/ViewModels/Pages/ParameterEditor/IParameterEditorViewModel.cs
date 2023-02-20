// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IParameterTableViewModel.cs" company="RHEA System S.A.">
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

namespace COMETwebapp.ViewModels.Pages.ParameterEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Services.SessionManagement;
    using COMETwebapp.Services.SubscriptionService;

    using DynamicData;

    /// <summary>
    /// ViewModel for the <see cref="COMETwebapp.Pages.ParameterEditor.ParameterEditor"/>
    /// </summary>
    public interface IParameterEditorViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ISubscriptionService"/>
        /// </summary>
        ISubscriptionService SubscriptionService { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISessionService"/>
        /// </summary>
        ISessionService SessionService { get; set; }

        /// <summary>
        /// The selected <see cref="ElementBase"/> to filter
        /// </summary>
        ElementBase SelectedElementFilter { get; set; }

        /// <summary>
        /// All <see cref="ElementBase"/> of the iteration
        /// </summary>
        List<ElementBase> Elements { get; set; }

        /// <summary>
        /// Gets or sets the filtered <see cref="ElementBase"/>
        /// </summary>
        SourceList<ElementBase> FilteredElements { get; set; }

        /// <summary>
        /// Sets if only parameters owned by the active domain are shown
        /// </summary>
        bool IsOwnedParameters { get; set; }

        /// <summary>
        /// Name of the parameter type selected
        /// </summary>
        ParameterType SelectedParameterTypeFilter { get; set; }

        /// <summary>
        /// Name of the option selected
        /// </summary>
        Option SelectedOptionFilter { get; set; }

        /// <summary>
        /// Name of the state selected
        /// </summary>
        ActualFiniteState SelectedStateFilter { get; set; }

        /// <summary>
        /// All ParameterType names in the model
        /// </summary>
        List<ParameterType> ParameterTypes { get; set; }

        /// <summary>
        /// Initializes the <see cref="ParameterEditorViewModel"/>
        /// </summary>
        void InitializeViewModel();

        /// <summary>
        /// Apply all the filters selected in the <param name="elements"/>
        /// </summary>
        /// <param name="elements">the elements to filter</param>
        /// <returns>the filtered elements</returns>
        void ApplyFilters(IEnumerable<ElementBase> elements);
    }
}
