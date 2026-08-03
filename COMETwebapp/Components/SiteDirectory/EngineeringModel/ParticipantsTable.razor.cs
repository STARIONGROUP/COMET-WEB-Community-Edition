// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ParticipantsTable.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.SiteDirectory.EngineeringModel
{
    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

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
        /// Gets or sets the assigned domains list to display in the popup
        /// </summary>
        private List<string> AssignedDomainsPopupList { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether the assigned domains popup is visible
        /// </summary>
        public bool IsAssignedDomainsPopupVisible { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its initial parameters
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Initialize(this.ViewModel);
        }

        /// <summary>
        /// Starts the creation flow for a new <see cref="Participant"/> item.
        /// </summary>
        public override void StartCreate()
        {
            base.StartCreate();
            this.ViewModel.CurrentThing = new Participant();
        }

        /// <summary>
        /// Starts the edit flow for the specified <paramref name="row"/>.
        /// </summary>
        /// <param name="row">The selected row to edit.</param>
        public override void StartEdit(ParticipantRowViewModel row)
        {
            if (row == null)
            {
                return;
            }

            this.OnSelectedDataItemChanged(row);
        }

        /// <summary>
        /// Method invoked every time a row is selected
        /// </summary>
        /// <param name="row">The selected row</param>
        protected override void OnSelectedDataItemChanged(ParticipantRowViewModel row)
        {
            base.OnSelectedDataItemChanged(row);
            this.ShouldCreateThing = false;
            this.ViewModel.CurrentThing = row.Thing.Clone(true);
        }

        /// <summary>
        /// Method invoked whenever a form is saved
        /// </summary>
        protected override void OnSaved()
        {
            base.OnSaved();
            this.IsOnEditMode = false;
        }

        /// <summary>
        /// Opens the assigned domain details popup
        /// </summary>
        /// <param name="row">The <see cref="ParticipantRowViewModel"/> row whose assigned domains to show</param>
        private void OpenAssignedDomainDetailsPopup(ParticipantRowViewModel row)
        {
            this.AssignedDomainsPopupList = row.Thing.Domain.Select(x => x.Name).ToList();
            this.IsAssignedDomainsPopupVisible = true;
        }
    }
}
