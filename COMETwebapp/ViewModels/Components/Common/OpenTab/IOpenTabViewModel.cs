// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IOpenTabViewModel.cs" company="Starion Group S.A.">
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
        /// Gets the condition to check if the current selected model is already opened
        /// </summary>
        bool IsCurrentModelOpened { get; }

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
    }
}
