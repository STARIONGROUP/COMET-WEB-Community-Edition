// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ITabsViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Group Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Pages
{
    using CDP4Common.EngineeringModelData;

    using COMETwebapp.Model;

    /// <summary>
    /// The <see cref="ITabsViewModel" /> contains logic and behavior that are required to support multi-tabs application
    /// </summary>
    public interface ITabsViewModel
    {
        /// <summary>
        /// Gets the collection of available <see cref="TabbedApplication" />
        /// </summary>
        IEnumerable<TabbedApplication> AvailableApplications { get; }

        /// <summary>
        /// Gets or sets the current selected <see cref="TabbedApplication" />
        /// </summary>
        TabbedApplication SelectedApplication { get; set; }

        /// <summary>
        /// Gets the side tab panel information
        /// </summary>
        TabPanelInformation SidePanel { get; }

        /// <summary>
        /// Gets the main tab panel information
        /// </summary>
        TabPanelInformation MainPanel { get; }

        /// <summary>
        /// Creates a new tab and sets it to current
        /// </summary>
        /// <param name="application">The <see cref="TabbedApplication" /> for which the tab will be created</param>
        /// <param name="objectOfInterestId">
        /// The id of the object of interest, which can be an <see cref="Iteration" /> or an
        /// <see cref="EngineeringModel" />
        /// </param>
        /// <param name="panel">The panel to open the new tab in</param>
        void CreateNewTab(TabbedApplication application, Guid objectOfInterestId, TabPanelInformation panel);
    }
}
