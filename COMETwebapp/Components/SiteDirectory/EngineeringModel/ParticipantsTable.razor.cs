// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParticipantsTable.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.SiteDirectory.EngineeringModel
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="ParticipantsTable" />
    /// </summary>
    public partial class ParticipantsTable : SelectedDataItemBase<Participant, ParticipantRowViewModel>
    {
        /// <summary>
        /// The <see cref="IParticipantsTableViewModel" /> for this component
        /// </summary>
        [Parameter]
        public IParticipantsTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the active domains details text
        /// </summary>
        private string AssignedDomainsPopupText { get; set; }

        /// <summary>
        /// Gets the available persons. If the user is editing an existing participant, only the selected person should be
        /// retrieved
        /// </summary>
        private IEnumerable<Person> Persons => this.ShouldCreateThing ? this.ViewModel.Persons : [this.ViewModel.CurrentThing.Person];

        /// <summary>
        /// Method that is invoked when the edit/add thing form is being saved
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnEditThingSaving()
        {
            await this.ViewModel.CreateOrEditParticipant(this.ShouldCreateThing);
        }

        /// <summary>
        /// Method invoked when creating a new thing
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        protected override void CustomizeEditThing(GridCustomizeEditModelEventArgs e)
        {
            base.CustomizeEditThing(e);

            var dataItem = (ParticipantRowViewModel)e.DataItem;
            this.ShouldCreateThing = e.IsNew;
            this.ViewModel.CurrentThing = dataItem == null ? new Participant() : dataItem.Thing.Clone(true);
            e.EditModel = this.ViewModel.CurrentThing;
        }

        /// <summary>
        /// Opens the assigned domain details popup
        /// </summary>
        /// <param name="text">The text to show inside the popup</param>
        private void OpenAssignedDomainDetailsPopup(string text)
        {
            this.AssignedDomainsPopupText = text;
            this.IsOnEditMode = true;
        }

        /// <summary>
        /// Sets the selected values for the <see cref="Participant" /> creation and submits the form
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task SetSelectedValuesAndSubmit()
        {
            this.ViewModel.UpdateSelectedDomains();
            await this.Grid.SaveChangesAsync();
        }
    }
}
