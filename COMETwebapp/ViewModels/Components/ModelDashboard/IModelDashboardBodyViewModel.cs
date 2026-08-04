// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IModelDashboardBodyViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.ViewModels.Components.ModelDashboard
{
    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    using COMETwebapp.ViewModels.Components.ModelDashboard.Elements;
    using COMETwebapp.ViewModels.Components.ModelDashboard.ParameterValues;

    /// <summary>
    /// View Model that handle the logic for the Model Dashboard application
    /// </summary>
    public interface IModelDashboardBodyViewModel : ISingleIterationApplicationBaseViewModel
    {
        /// <summary>
        /// Gets the <see cref="IMultiOptionSelectorViewModel" /> driving the option filter. An empty selection
        /// means no option filtering is applied.
        /// </summary>
        IMultiOptionSelectorViewModel OptionSelector { get; }

        /// <summary>
        /// Gets the <see cref="IMultiFiniteStateSelectorViewModel" /> driving the finite state filter. An empty
        /// selection means no state filtering is applied.
        /// </summary>
        IMultiFiniteStateSelectorViewModel FiniteStateSelector { get; }

        /// <summary>
        /// Gets the <see cref="IMultiParameterTypeSelectorViewModel" /> driving the parameter-type filter. An empty
        /// selection means no parameter-type filtering is applied.
        /// </summary>
        IMultiParameterTypeSelectorViewModel ParameterTypeSelector { get; }

        /// <summary>
        /// The <see cref="IParameterDashboardViewModel" />
        /// </summary>
        IParameterDashboardViewModel ParameterDashboard { get; }

        /// <summary>
        /// Gets the <see cref="IElementDashboardViewModel" />
        /// </summary>
        IElementDashboardViewModel ElementDashboard { get; }

        /// <summary>
        /// Update the dashboard view models properties
        /// </summary>
        void UpdateDashboards();
    }
}
