// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParticipantRoleDetails.razor.cs" company="RHEA System S.A.">
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

    using COMET.Web.Common.Components;

    using COMETwebapp.ViewModels.Components.SiteDirectory.Roles;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="ParticipantRoleDetails"/>
    /// </summary>
    public partial class ParticipantRoleDetails : DisposableComponent
    {
        /// <summary>
        /// The <see cref="IParticipantRolesTableViewModel" /> for this component
        /// </summary>
        [Parameter, Required]
        public IParticipantRolesTableViewModel ViewModel { get; set; }

        /// <summary>
        /// Method that is executed when the current edit form is submitted
        /// </summary>
        [Parameter]
        public EventCallback OnSubmit { get; set; }

        /// <summary>
        /// Method that is executed when the current edit form is canceled
        /// </summary>
        [Parameter]
        public EventCallback OnCancel { get; set; }

        /// <summary>
        /// Method that executes the default creation method in case the property <see cref="OnSubmit"/> is not set
        /// </summary>
        private async Task OnValidSubmit()
        {
            if (this.OnSubmit.HasDelegate)
            {
                await this.OnSubmit.InvokeAsync();
                return;
            }

            await this.ViewModel.CreateOrEditParticipantRole(false);
        }
    }
}
