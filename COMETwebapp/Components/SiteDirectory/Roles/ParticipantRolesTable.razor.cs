// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParticipantRolesTable.razor.cs" company="RHEA System S.A.">
//    Copyright (c) 2023-2024 RHEA System S.A.
//
//    Authors: Sam Gerené, Alex Vorobiev, Alexander van Delft, Jaime Bernar, Théate Antoine, João Rua
//
//    This file is part of CDP4-COMET WEB Community Edition
//    The CDP4-COMET WEB Community Edition is the RHEA Web Application implementation of ECSS-E-TM-10-25 Annex A and Annex C.
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

namespace COMETwebapp.Components.SiteDirectory.Roles
{
    using System.ComponentModel.DataAnnotations;

    using CDP4Common.SiteDirectoryData;

    using COMETwebapp.Components.Common;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Roles;
    using COMETwebapp.ViewModels.Components.SiteDirectory.Rows;

    using DevExpress.Blazor;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="ParticipantRolesTable"/>
    /// </summary>
    public partial class ParticipantRolesTable : SelectedDeprecatableDataItemBase<ParticipantRole, ParticipantRoleRowViewModel>
    {
        /// <summary>
        /// The <see cref="IParticipantRolesTableViewModel" /> for this component
        /// </summary>
        [Parameter, Required]
        public IParticipantRolesTableViewModel ViewModel { get; set; }

        /// <summary>
        /// The callback for when a participant role is selected
        /// </summary>
        [Parameter]
        public EventCallback<ParticipantRole> OnRoleSelected { get; set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Initialize(this.ViewModel);
        }

        /// <summary>
        /// Method that is invoked when the edit/add thing form is being saved
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        protected override async Task OnEditThingSaving()
        {
            await this.ViewModel.CreateOrEditParticipantRole(this.ShouldCreateThing);
        }

        /// <summary>
        /// Method invoked when creating a new thing
        /// </summary>
        /// <param name="e">A <see cref="GridCustomizeEditModelEventArgs" /></param>
        protected override void CustomizeEditThing(GridCustomizeEditModelEventArgs e)
        {
            base.CustomizeEditThing(e);

            var dataItem = (ParticipantRoleRowViewModel)e.DataItem;
            this.ShouldCreateThing = e.IsNew;

            if (dataItem == null)
            {
                this.ViewModel.Thing = new ParticipantRole();
                e.EditModel = this.ViewModel.Thing;
                return;
            }

            this.ViewModel.Thing = dataItem.Thing.Clone(true);
            e.EditModel = this.ViewModel.Thing;
        }

        /// <summary>
        /// Metgid invoked everytime a row is selected
        /// </summary>
        /// <param name="row">The selected row</param>
        private async Task OnSelectedDataItemChanged(ParticipantRoleRowViewModel row)
        {
            this.ViewModel.Thing = row.Thing.Clone(true);
            await this.OnRoleSelected.InvokeAsync(row.Thing);
        }
    }
}
