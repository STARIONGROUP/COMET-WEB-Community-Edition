// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParticipantsForm.razor.cs" company="Starion Group S.A.">
//    Copyright (c) 2023-2026 Starion Group S.A.
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the Starion Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET WEB Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-COMET WEB Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMETwebapp.Components.SiteDirectory.EngineeringModel
{
    using System.ComponentModel.DataAnnotations;

    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.SiteDirectory.EngineeringModels;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="ParticipantsForm" />
    /// </summary>
    public partial class ParticipantsForm : SelectedDataItemForm
    {
        /// <summary>
        /// The <see cref="IParticipantsTableViewModel" /> for this component
        /// </summary>
        [Parameter]
        [Required]
        public IParticipantsTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Gets the available persons. If the user is editing an existing participant, only the selected person should be
        /// retrieved
        /// </summary>
        private IEnumerable<Person> Persons => this.ShouldCreate
            ? this.ViewModel.Persons.Where(p => this.ViewModel.Rows.Items.All(r => r.Thing.Person.Iid != p.Iid))
            : [this.ViewModel.CurrentThing.Person];

        /// <summary>
        /// Method that is executed when there is a valid submit
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnValidSubmit()
        {
            this.ViewModel.UpdateSelectedDomains();
            await this.ViewModel.CreateOrEditParticipant(this.ShouldCreate);
            await base.OnValidSubmit();
        }

        /// <summary>
        /// Callback executed when the selected domains of expertise change in the listbox
        /// </summary>
        /// <param name="selectedDomains">The updated collection of <see cref="DomainOfExpertise"/> items</param>
        private void OnSelectedDomainsChanged(IEnumerable<DomainOfExpertise> selectedDomains)
        {
            this.ViewModel.SelectedDomains = selectedDomains;
            this.ViewModel.UpdateSelectedDomains();
        }
    }
}
