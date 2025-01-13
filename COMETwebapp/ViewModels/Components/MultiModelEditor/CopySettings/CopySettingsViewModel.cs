// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CopySettingsViewModel.cs" company="Starion Group S.A.">
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

    using COMET.Web.Common.Enumerations;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.Cache;
    using COMET.Web.Common.Utilities.DisposableObject;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// View model for the <see cref="CopySettingsViewModel" /> component
    /// </summary>
    public class CopySettingsViewModel : DisposableObject, ICopySettingsViewModel
    {
        /// <summary>
        /// Holds a reference to the Injected <see cref="ICacheService"/>
        /// </summary>
        private readonly ICacheService cacheService;

        /// <summary>
        /// Gets a collection of <see cref="OperationKind"/> instance that can be selected as Copy Operation
        /// </summary>
        public CopyOperationKinds AvailableOperationKinds { get; } = new();

        /// <summary>
        /// Gets or sets the selected <see cref="OperationKind"/>
        /// </summary>
        public OperationKind SelectedOperationKind { get; set; } = OperationKind.Copy;

        /// <summary>
        /// The selected copy <see cref="OperationKind"/>'s descriptive text
        /// </summary>
        public string SelectedOperationKindDescription => this.AvailableOperationKinds.Single(x => x.Value == this.SelectedOperationKind).Key;

        /// <summary>
        /// The callback executed when the method <see cref="SaveSettings" /> was executed
        /// </summary>
        public EventCallback OnSaveSettings { get; set; }

        /// <summary>
        /// Create a new instance of the <see cref="ICacheService"/> class
        /// </summary>
        /// <param name="cacheService">The Injected <see cref="ICacheService"/></param>
        public CopySettingsViewModel(ICacheService cacheService)
        {
            this.cacheService = cacheService;

            if (this.cacheService.TryGetOrAddBrowserSessionSetting(BrowserSessionSettingKey.CopyElementDefinitionOperationKind, OperationKind.Copy, out var selectedOperationKind) && selectedOperationKind is OperationKind operationKind)
            {
                this.SelectedOperationKind = operationKind;
            }
        }

        /// <summary>
        /// Initializes the current view model
        /// </summary>
        public void InitializeViewModel()
        {
        }

        /// <summary>
        /// Saves the Settings
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        public async Task SaveSettings()
        {
            this.cacheService.AddOrUpdateBrowserSessionSetting(BrowserSessionSettingKey.CopyElementDefinitionOperationKind, this.SelectedOperationKind);
            await this.OnSaveSettings.InvokeAsync();
        }
    }

    /// <summary>
    /// Defines a collection of descriptive strings that belongs to an <see cref="OperationKind"/>
    /// </summary>
    public class CopyOperationKinds : Dictionary<string, OperationKind>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CopyOperationKinds"/>
        /// </summary>
        public CopyOperationKinds()
        {
            this.Add("Set Parameter values to default, keep original owner", OperationKind.Copy);
            this.Add("Set Parameter values to default, change owner to active domain", OperationKind.CopyDefaultValuesChangeOwner);
            this.Add("Keep parameter values, keep original owner", OperationKind.CopyKeepValues);
            this.Add("Keep parameter values, change owner to active domain", OperationKind.CopyKeepValuesChangeOwner);
        }
    }
}
