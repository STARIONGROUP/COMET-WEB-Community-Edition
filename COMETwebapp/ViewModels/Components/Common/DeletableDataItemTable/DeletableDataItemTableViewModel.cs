// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DeletableDataItemTableViewModel.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.ViewModels.Components.Common.DeletableDataItemTable
{
    using AntDesign;

    using CDP4Common.CommonData;

    using CDP4Dal;

    using COMET.Web.Common.Extensions;
    using COMET.Web.Common.Model;
    using COMET.Web.Common.Services.SessionManagement;

    using COMETwebapp.ViewModels.Components.Common.BaseDataItemTable;
    using COMETwebapp.ViewModels.Components.Common.Rows;

    using ReactiveUI;

    /// <summary>
    /// View model that provides the basic functionalities for a deletable data item
    /// </summary>
    public abstract class DeletableDataItemTableViewModel<T, TRow> : BaseDataItemTableViewModel<T, TRow>, IDeletableDataItemTableViewModel<T, TRow> where T : Thing where TRow : BaseDataItemRowViewModel<T>
    {
        /// <summary>
        /// Backing field for <see cref="IsOnDeletionMode" />
        /// </summary>
        private bool isOnDeletionMode;

        /// <summary>
        /// Creates a new instance of the <see cref="DeletableDataItemTableViewModel{T,TRow}"/>
        /// </summary>
        /// <param name="sessionService">The <see cref="ISessionService"/></param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/></param>
        protected DeletableDataItemTableViewModel(ISessionService sessionService, ICDPMessageBus messageBus, ILogger<DeletableDataItemTableViewModel<T, TRow>> logger) 
            : base(sessionService, messageBus, logger)
        {
        }

        /// <summary>
        /// Indicates if confirmation popup is visible
        /// </summary>
        public bool IsOnDeletionMode
        {
            get => this.isOnDeletionMode;
            set => this.RaiseAndSetIfChanged(ref this.isOnDeletionMode, value);
        }

        /// <summary>
        /// Gets or sets the popup message dialog
        /// </summary>
        public string PopupDialog { get; set; }

        /// <summary>
        /// Method invoked when confirming the deletion of the current thing
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task OnConfirmPopupButtonClick()
        {
            await this.DeleteThing();
            this.IsOnDeletionMode = false;
        }

        /// <summary>
        /// Method invoked when canceling the deletion of the current thing
        /// </summary>
        public void OnCancelPopupButtonClick()
        {
            this.IsOnDeletionMode = false;
        }

        /// <summary>
        /// Action invoked when the delete button is clicked
        /// </summary>
        /// <param name="thingRow"> The row to delete </param>
        public void OnDeleteButtonClick(TRow thingRow)
        {
            this.CurrentThing = thingRow.Thing;
            this.PopupDialog = $"You are about to delete the {typeof(T).Name}: {thingRow.Name}";
            this.IsOnDeletionMode = true;
        }

        /// <summary>
        /// Tries to delete the current thing
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task DeleteThing()
        {
            var clonedContainer = this.CurrentThing.Container.Clone(false);

            try
            {
                await this.SessionService.DeleteThingsWithNotification(clonedContainer, [this.CurrentThing.Clone(false)], this.GetDeletionNotificationDescription());
            }
            catch (Exception exception)
            {
                this.Logger.LogError(exception, "An error has occurred while trying to delete the {thingType} with iid {thingIid}", typeof(T), this.CurrentThing.Iid);
            }
        }

        /// <summary>
        /// Gets the message for the success notification
        /// </summary>
        /// <returns>The message</returns>
        protected NotificationDescription GetDeletionNotificationDescription()
        {
            var notificationDescription = new NotificationDescription()
            {
                OnSuccess = $"The {typeof(T).Name} {this.CurrentThing.GetShortNameOrName()} was deleted!",
                OnError = $"Error while deleting The {typeof(T).Name} {this.CurrentThing.GetShortNameOrName()}"
            };

            return notificationDescription;
        }
    }
}
