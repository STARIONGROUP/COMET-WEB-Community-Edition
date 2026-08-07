// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IOpenTabViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2023-2026 Starion Group S.A.
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

namespace COMETwebapp.ViewModels.Components.Common.OpenTab
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.ViewModels.Components;

    using COMETwebapp.Model;

    /// <summary>
    /// View Model that enables a user to open an <see cref="EngineeringModel" />
    /// </summary>
    public interface IOpenTabViewModel : IOpenModelViewModel
    {
        /// <summary>
        /// The selected <see cref="TabbedApplication" />
        /// </summary>
        TabbedApplication SelectedApplication { get; set; }

        /// <summary>
        /// Gets the collection of participant models
        /// </summary>
        IEnumerable<EngineeringModelSetup> EngineeringModelSetups { get; }

        /// <summary>
        /// Gets the condition to check if the current selected iteration is already opened
        /// </summary>
        bool IsCurrentIterationOpened { get; }

        /// <summary>
        /// Gets a value indicating whether a tab for the selected application and model is already open
        /// </summary>
        bool HasOpenTab { get; }

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise" /> from the <see cref="OpenModelViewModel.SelectedIterationSetup" />
        /// </summary>
        DomainOfExpertise SelectedIterationDomainOfExpertise { get; }

        /// <summary>
        /// Opens the <see cref="EngineeringModel" /> based on the selected field
        /// </summary>
        /// <param name="panel">The <see cref="TabPanelInformation"/> for which the new tab will be opened</param>
        /// <returns>A <see cref="Task" /></returns>
        Task OpenTab(TabPanelInformation panel);

        /// <summary>
        /// Navigates to an already open tab for the selected engineering model or iteration if it exists
        /// </summary>
        /// <returns>True if a tab was navigated to; otherwise, false.</returns>
        bool NavigateToOpenTab();
    }
}
