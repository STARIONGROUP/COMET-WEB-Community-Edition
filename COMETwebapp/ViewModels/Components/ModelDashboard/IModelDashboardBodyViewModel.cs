﻿// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IModelDashboardBodyViewModel.cs" company="Starion Group S.A.">
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
        /// Gets the <see cref="IOptionSelectorViewModel" />
        /// </summary>
        IOptionSelectorViewModel OptionSelector { get; }

        /// <summary>
        /// Gets the <see cref="IFiniteStateSelectorViewModel" />
        /// </summary>
        IFiniteStateSelectorViewModel FiniteStateSelector { get; }

        /// <summary>
        /// Gets the <see cref="IParameterTypeSelectorViewModel" />
        /// </summary>
        IParameterTypeSelectorViewModel ParameterTypeSelector { get; }

        /// <summary>
        /// The <see cref="IParameterDashboardViewModel" />
        /// </summary>
        IParameterDashboardViewModel ParameterDashboard { get; }

        /// <summary>
        /// Gets the <see cref="IElementDashboardViewModel" />
        /// </summary>
        IElementDashboardViewModel ElementDashboard { get; }
    }
}
