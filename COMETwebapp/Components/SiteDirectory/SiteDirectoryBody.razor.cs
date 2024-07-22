// --------------------------------------------------------------------------------------------------------------------
//  <copyright file="SiteDirectoryBody.razor.cs" company="Starion Group S.A.">
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

namespace COMETwebapp.Components.SiteDirectory
{
    using COMETwebapp.Components.SiteDirectory.EngineeringModel;
    using COMETwebapp.Components.SiteDirectory.Roles;
    using COMETwebapp.Components.SiteDirectory.UserManagement;

    using DevExpress.Blazor;

    /// <summary>
    /// Core component for the Server Admin (old Site Directory) body application
    /// </summary>
    public partial class SiteDirectoryBody
    {
        /// <summary>
        /// A map with all the available components and their names
        /// </summary>
        private readonly Dictionary<Type, string> mapOfComponentsAndNames = new()
        {
            { typeof(EngineeringModelsTable), "Models" },
            { typeof(DomainsOfExpertiseTable), "Domains" },
            { typeof(OrganizationsTable), "Organizations" },
            { typeof(UserManagementTable), "User Management" },
            { typeof(PersonRolesTable), "Person Roles" },
            { typeof(ParticipantRolesTable), "Participant Roles" }
        };

        /// <summary>
        /// The selected component type
        /// </summary>
        public Type SelectedComponent { get; private set; }

        /// <summary>
        /// Method invoked when the component is ready to start, having received its
        /// initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.SelectedComponent = this.mapOfComponentsAndNames.First().Key;
        }

        /// <summary>
        /// Initializes values of the component and of the ViewModel based on parameters provided from the url
        /// </summary>
        /// <param name="parameters">A <see cref="Dictionary{TKey,TValue}" /> for parameters</param>
        protected override void InitializeValues(Dictionary<string, string> parameters)
        {
        }

        /// <summary>
        /// Method invoked to set the selected component from toolbar
        /// </summary>
        /// <param name="e"></param>
        private void OnItemClick(ToolbarItemClickEventArgs e)
        {
            this.SelectedComponent = this.mapOfComponentsAndNames.First(x => x.Value == e.ItemName).Key;
        }
    }
}
