﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RolesTables.razor.cs" company="RHEA System S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using COMET.Web.Common.Components;

    using COMETwebapp.ViewModels.Components.SiteDirectory.Roles;

    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Support class for the <see cref="RolesTables"/>
    /// </summary>
    public partial class RolesTables : DisposableComponent
    {
        /// <summary>
        /// The <see cref="IParticipantRolesTableViewModel" /> for this component
        /// </summary>
        [Inject]
        public IParticipantRolesTableViewModel ParticipantRolesViewModel { get; set; }

        /// <summary>
        /// The <see cref="IPersonRolesTableViewModel" /> for this component
        /// </summary>
        [Inject]
        public IPersonRolesTableViewModel PersonRolesViewModel { get; set; }

        /// <summary>
        /// The selected component type
        /// </summary>
        public Type SelectedComponent { get; private set; }

        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}" /> for the <see cref="DynamicComponent.Parameters" />
        /// </summary>
        private readonly Dictionary<string, object> parameters = [];

        /// <summary>
        /// A map with all the available detail components and their names
        /// </summary>
        private readonly Dictionary<Type, (Type, object)> mapOfRolesAndDetailsData = [];

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.mapOfRolesAndDetailsData.Clear();
            this.mapOfRolesAndDetailsData.Add(typeof(ParticipantRole), (typeof(ParticipantRoleDetails), this.ParticipantRolesViewModel));
            this.mapOfRolesAndDetailsData.Add(typeof(PersonRole), (typeof(PersonRoleDetails), this.PersonRolesViewModel));
        }

        /// <summary>
        /// Method that is executed everytime a role is selected
        /// </summary>
        /// <param name="role"></param>
        private void OnRoleSelected(Thing role)
        {
            var tupleOfDetailsData = this.mapOfRolesAndDetailsData.FirstOrDefault(x => x.Key == role.GetType()).Value;
            this.parameters["ViewModel"] = tupleOfDetailsData.Item2;

            this.parameters["OnCancel"] = new EventCallbackFactory().Create(this, async () =>
            {
                this.SelectedComponent = null;
                await this.InvokeAsync(this.StateHasChanged);
            });

            this.SelectedComponent = tupleOfDetailsData.Item1;
        }
    }
}