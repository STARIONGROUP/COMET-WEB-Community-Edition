// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ISystemRepresentationBodyViewModel.cs" company="RHEA System S.A.">
//     Copyright (c) 2023 RHEA System S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, Nabil Abbar
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

namespace COMETwebapp.ViewModels.Components.SystemRepresentation
{
    using CDP4Common.EngineeringModelData;

    using COMET.Web.Common.ViewModels.Components.Applications;
    using COMET.Web.Common.ViewModels.Components.Selectors;

    /// <summary>
    /// View Model that handle the logic for the System Representation application
    /// </summary>
    public interface ISystemRepresentationBodyViewModel: ISingleIterationApplicationBaseViewModel
    {
        /// <summary>
        /// Gets the <see cref="IOptionSelectorViewModel" />
        /// </summary>
        public IOptionSelectorViewModel OptionSelector { get; }

        /// <summary>
        /// Represents the RootNode of the tree
        /// </summary>
        SystemNodeViewModel RootNode { get; set; }

        /// <summary>
        /// The <see cref="SystemRepresentationTreeViewModel" />
        /// </summary>
        SystemRepresentationTreeViewModel ProductTreeViewModel { get; }

        /// <summary>
        /// The <see cref="IElementDefinitionDetailsViewModel" />
        /// </summary>
        IElementDefinitionDetailsViewModel ElementDefinitionDetailsViewModel { get; }

        /// <summary>
        /// All <see cref="ElementBase" /> of the iteration
        /// </summary>
        List<ElementBase> Elements { get; set; }

        /// <summary>
        /// set the selected <see cref="SystemNodeViewModel" />
        /// </summary>
        /// <param name="selectedNode">The selected <see cref="SystemNodeViewModel" /></param>
        /// <returns>A <see cref="Task" /></returns>
        void SelectElement(SystemNodeViewModel selectedNode);

        /// <summary>
        /// Apply all the filters on the <see cref="SystemRepresentationTreeViewModel" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        Task ApplyFilters();
    }
}
