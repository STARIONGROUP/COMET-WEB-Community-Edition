// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DeprecatableDataItemTableViewModel.cs" company="Starion Group S.A.">
//     Copyright (c) 2024 Starion Group S.A.
// 
//     Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
// 
//     This file is part of COMET WEB Community Edition
//     The COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.ViewModels.Components.Common.DeprecatableDataItemTable
{
    using CDP4Common.CommonData;

    using CDP4Dal;

    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.Services.ShowHideDeprecatedThingsService;
    using COMETwebapp.ViewModels.Components.Common.BaseDataItemTable;
    using COMETwebapp.ViewModels.Components.Common.Rows;

    using ReactiveUI;

    /// <summary>
    /// View model that provides the basic functionalities for a reference data item
    /// </summary>
    public abstract class DeprecatableDataItemTableViewModel<T, TRow> : BaseDataItemTableViewModel<T, TRow>, IDeprecatableDataItemTableViewModel<T, TRow> where T : Thing, IShortNamedThing, INamedThing, IDeprecatableThing where TRow : DeprecatableDataItemRowViewModel<T>
    {
        /// <summary>
        /// Backing field for <see cref="IsOnDeprecationMode" />
        /// </summary>
        private bool isOnDeprecationMode;

        /// <summary>
        /// Creates a new instance of the <see cref="DeprecatableDataItemTableViewModel{T,TRow}"/>
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService"/></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="showHideDeprecatedThingsService">The <see cref="IShowHideDeprecatedThingsService"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        protected DeprecatableDataItemTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, IShowHideDeprecatedThingsService showHideDeprecatedThingsService,
            ILogger<DeprecatableDataItemTableViewModel<T, TRow>> logger) : base(sessionService, messageBus, logger)
        {
            this.ShowHideDeprecatedThingsService = showHideDeprecatedThingsService;
        }

        /// <summary>
        /// Injected property to get access to <see cref="IShowHideDeprecatedThingsService" />
        /// </summary>
        public IShowHideDeprecatedThingsService ShowHideDeprecatedThingsService { get; }

        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        public bool IsOnDeprecationMode
        {
            get => this.isOnDeprecationMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnDeprecationMode, value);
        }

        /// <summary>
        /// Gets or sets the popup message dialog
        /// </summary>
        public string PopupDialog { get; set; }

        /// <summary>
        /// Method invoked when confirming the deprecation/un-deprecation of the current thing
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task OnConfirmPopupButtonClick()
        {
            await this.DeprecateOrUnDeprecateThing();
            this.IsOnDeprecationMode = false;
        }

        /// <summary>
        /// Method invoked when canceling the deprecation/un-deprecation of the current thing
        /// </summary>
        public void OnCancelPopupButtonClick()
        {
            this.IsOnDeprecationMode = false;
        }

        /// <summary>
        /// Action invoked when the deprecate or undeprecate button is clicked
        /// </summary>
        /// <param name="thing"> The thing to deprecate or undeprecate </param>
        public void OnDeprecateUnDeprecateButtonClick(T thing)
        {
            this.Thing = thing;
            this.PopupDialog = this.Thing.IsDeprecated ? $"You are about to un-deprecate the {typeof(T).Name}: {thing.Name}" : $"You are about to deprecate the {typeof(T).Name}: {thing.Name}";
            this.IsOnDeprecationMode = true;
        }

        /// <summary>
        /// Tries to deprecate or undeprecate the current thing
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task DeprecateOrUnDeprecateThing()
        {
            this.IsLoading = true;

            var siteDirectoryClone = this.SessionService.GetSiteDirectory().Clone(false);
            var clonedThing = this.Thing.Clone(false);

            if (clonedThing is IDeprecatableThing deprecatableThing)
            {
                deprecatableThing.IsDeprecated = !deprecatableThing.IsDeprecated;
                this.Thing.IsDeprecated = deprecatableThing.IsDeprecated;
            }

            try
            {
                await this.SessionService.CreateOrUpdateThings(siteDirectoryClone, [clonedThing]);
                await this.SessionService.RefreshSession();
            }
            catch (Exception exception)
            {
                this.Logger.LogError(exception, "An error has occurred while trying to deprecating or un-deprecating the {thingType} {thingName}", typeof(T), ((IShortNamedThing)clonedThing).ShortName);
            }

            this.IsLoading = false;
        }
    }
}
