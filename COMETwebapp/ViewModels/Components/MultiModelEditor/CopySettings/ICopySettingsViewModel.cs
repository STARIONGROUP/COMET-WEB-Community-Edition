// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ICopySettingsViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.MultiModelEditor.CopySettings
{
    using CDP4Dal.Operations;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Interface for the <see cref="CopySettingsViewModel" />
    /// </summary>
    public interface ICopySettingsViewModel
    {
        /// <summary>
        /// Gets a collection of <see cref="OperationKind"/> instance that can be selected as Copy Operation
        /// </summary>
        CopyOperationKinds AvailableOperationKinds { get; }

        /// <summary>
        /// Gets the selected <see cref="OperationKind"/>
        /// </summary>
        OperationKind SelectedOperationKind { get; set; }

        /// <summary>
        /// The callback executed when the method <see cref="SaveSettings" /> was executed
        /// </summary>
        EventCallback OnSaveSettings { get; set; }

        /// <summary>
        /// The selected copy <see cref="OperationKind"/>'s descriptive text
        /// </summary>
        string SelectedOperationKindDescription { get; }

        /// <summary>
        /// Initializes the current view model
        /// </summary>
        void InitializeViewModel();

        /// <summary>
        /// Saves the Settings
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        Task SaveSettings();
    }
}
